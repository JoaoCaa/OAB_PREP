/**
 * OAB Prep — API Service Layer
 * =====================================================================
 * Contratos 100% alinhados com o backend .NET 8 (JoaoCaa/OAB_PREP)
 * Gerado a partir dos controllers reais em OabPrep.API/Controllers/
 *
 * Tipos de ID confirmados pelo código do colega:
 *   sessionId   → int   ({sessionId:int})
 *   questionId  → int   ({questionId:int})
 *   lawAreaId   → int   ({id:int})
 *   userId      → Guid  ({id:guid})
 *   adminId     → Guid  (claim NameIdentifier)
 *
 * Status codes reais observados:
 *   PUT /users/me          → 204 (não retorna body)
 *   PUT /users/me/password → 204
 *   DELETE /users/me       → 204
 *   POST /users/me/data-export → 202
 *   DELETE /admin/questions/:id → 204
 *   PATCH /admin/users/:id/block|unblock|role → 204
 *   POST /auth/logout → 204
 * =====================================================================
 */

import AuthStore from '../store/auth.js';
import Router from '../utils/router.js';

const BASE_URL = ((window.ENV?.API_URL ?? 'http://localhost:5000')).replace(/\/$/, '') + '/api/v1';

// ─── HTTP CLIENT ──────────────────────────────────────────────────

async function request(method, path, body = null, opts = {}) {
  const token = AuthStore.getAccessToken();
  const headers = {};

  // Não setar Content-Type em multipart/form-data (upload de avatar)
  if (!(body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  let res;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body instanceof FormData
        ? body
        : body !== null ? JSON.stringify(body) : undefined,
      signal: opts.signal,
    });
  } catch (networkErr) {
    throw new ApiError(0, 'Sem conexão com o servidor. Verifique se o backend está rodando.');
  }

  // 401 → tenta refresh automático uma vez (BE-06)
  if (res.status === 401 && !opts._retry) {
    const refreshed = await _tryRefresh();
    if (refreshed) return request(method, path, body, { ...opts, _retry: true });
    AuthStore.clear();
    Router.go('/login');
    throw new ApiError(401, 'Sessão expirada. Faça login novamente.');
  }

  // Respostas sem body
  if (res.status === 204 || res.status === 202) return null;

  const ct = res.headers.get('content-type') ?? '';
  let data;
  if (ct.includes('application/json')) {
    data = await res.json();
  } else {
    data = await res.text();
  }

  if (!res.ok) {
    // ValidationProblemDetails do ASP.NET (400) tem { title, errors: { field: [msgs] } }
    const message = data?.title ?? data?.message ?? data ?? `Erro ${res.status}`;
    const errors  = data?.errors ?? null;
    throw new ApiError(res.status, message, errors);
  }

  return data;
}

const get   = (path, opts)       => request('GET',    path, null, opts);
const post  = (path, body, opts) => request('POST',   path, body, opts);
const put   = (path, body, opts) => request('PUT',    path, body, opts);
const patch = (path, body, opts) => request('PATCH',  path, body, opts);
const del   = (path, opts)       => request('DELETE', path, null, opts);

/** Refresh silencioso sem passar pelo interceptor (evita loop). */
async function _tryRefresh() {
  try {
    const refreshToken = AuthStore.getRefreshToken();
    if (!refreshToken) return false;
    const res = await fetch(`${BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });
    if (!res.ok) return false;
    const data = await res.json();
    // RefreshTokenResponse { accessToken, refreshToken, expiresIn }
    AuthStore.setTokens(data.accessToken, data.refreshToken);
    return true;
  } catch {
    return false;
  }
}

export class ApiError extends Error {
  constructor(status, message, errors = null) {
    super(message);
    this.name   = 'ApiError';
    this.status = status;
    this.errors = errors; // { "fieldName": ["mensagem de erro"] }
  }
}

// ─── AUTH  ────────────────────────────────────────────────────────
// Controller: AuthController  →  /api/v1/auth
//
//  POST /auth/register        → 201 RegisterUserResponse  | 400
//  GET  /auth/confirm-email   → 302 redirect (não chamado via JS)
//  POST /auth/login           → 200 LoginResponse  | 401 | 423
//  POST /auth/forgot-password → 200 ForgotPasswordResponse | 429
//  POST /auth/reset-password  → 200 ResetPasswordResponse  | 400
//  POST /auth/refresh         → 200 RefreshTokenResponse   | 401
//  POST /auth/oauth/google    → 200 LoginResponse          | 401
//  POST /auth/logout [Auth]   → 204
// ─────────────────────────────────────────────────────────────────

export const AuthService = {
  /**
   * Cadastro de novo usuário (UC01)
   * Body: RegisterUserCommand { name, email, password, confirmPassword, acceptedTerms: true }
   * 201 → RegisterUserResponse { message }
   */
  register: (command) => post('/auth/register', command),

  /**
   * Login (UC02)
   * Body: LoginCommand { email, password, rememberMe }
   * 200 → LoginResponse { accessToken, refreshToken, expiresIn, user: { id, name, email, role } }
   * 401 → credenciais inválidas  |  423 → conta bloqueada temporariamente
   */
  login: (email, password, rememberMe = false) =>
    post('/auth/login', { email, password, rememberMe }),

  /**
   * Solicitar reset de senha (UC03)
   * Body: ForgotPasswordCommand { email }
   * 200 → ForgotPasswordResponse { message }  ← sempre igual (não revela existência do e-mail)
   * 429 → rate limit (máx 3 por 15 min)
   */
  forgotPassword: (email) => post('/auth/forgot-password', { email }),

  /**
   * Redefinir senha com token (UC03)
   * Body: ResetPasswordCommand { token, newPassword, confirmPassword }
   * 200 → ResetPasswordResponse { message }  |  400 token inválido/expirado
   */
  resetPassword: (token, newPassword, confirmPassword) =>
    post('/auth/reset-password', { token, newPassword, confirmPassword }),

  /**
   * Renovar access token (BE-06)
   * Body: RefreshTokenCommand { refreshToken }
   * 200 → RefreshTokenResponse { accessToken, refreshToken, expiresIn }
   * 401 → refresh token inválido/expirado
   */
  refresh: (refreshToken) => post('/auth/refresh', { refreshToken }),

  /**
   * OAuth Google (BE-26)
   * Body: OAuthGoogleCommand { idToken }
   * 200 → LoginResponse (mesmo formato do login)
   */
  loginGoogle: (idToken) => post('/auth/oauth/google', { idToken }),

  /**
   * Logout  [Requer Auth]
   * 204 No Content — invalida todos os refresh tokens do usuário
   */
  logout: () => post('/auth/logout'),

  /**
   * URL para link de confirmação de e-mail (GET com redirect 302)
   * Usada apenas para construir links em e-mail — não é chamada via fetch.
   */
  confirmEmailUrl: (token) =>
    `${BASE_URL}/auth/confirm-email?token=${encodeURIComponent(token)}`,
};

// ─── LAW AREAS  ───────────────────────────────────────────────────
// LawAreasController (público) → /api/v1/law-areas
// AdminLawAreasController      → /api/v1/admin/law-areas  [Admin]
//
//  GET    /law-areas              → 200 IList<LawAreaResponse>
//  GET    /law-areas/{id:int}     → 200 LawAreaResponse  | 404
//  POST   /admin/law-areas        → 201 LawAreaResponse  | 400
//  PUT    /admin/law-areas/{id}   → 200 LawAreaResponse  | 400 | 404
//  DELETE /admin/law-areas/{id}   → 204  | 404
// ─────────────────────────────────────────────────────────────────

export const LawAreaService = {
  /** Lista todas as áreas ativas (público — sem autenticação). */
  list: () => get('/law-areas'),

  /** Detalhe de uma área. id: int */
  getById: (id) => get(`/law-areas/${id}`),

  // ── Admin ──────────────────────────────────────────────────────
  /** Body: CreateLawAreaCommand { name, slug, description?, iconUrl?, sortOrder? } */
  create: (command) => post('/admin/law-areas', command),

  /** id vai na URL. Body: UpdateLawAreaCommand (mesmos campos, sem Id) */
  update: (id, command) => put(`/admin/law-areas/${id}`, command),

  /** Soft delete (IsActive=false). 204 */
  deactivate: (id) => del(`/admin/law-areas/${id}`),
};

// ─── SESSIONS  ────────────────────────────────────────────────────
// SessionsController → /api/v1/sessions  [Auth]
//
//  POST  /sessions                                           → 201 CreateSessionResponse
//  GET   /sessions/{sessionId:int}                           → 200 GetSessionResponse | 403 | 404
//  POST  /sessions/{sessionId}/answers                       → 200 SubmitAnswerResponse | 409
//  POST  /sessions/{sessionId}/finish                        → 200 FinishSessionResponse | 409
//  PATCH /sessions/{sessionId}/answers/{questionId:int}/review → 200 ToggleReviewMarkResponse
//  POST  /sessions/{sessionId}/questions/{questionId}/chat/messages → 200 | 429 | 503
//  GET   /sessions/{sessionId}/questions/{questionId}/chat          → 200
// ─────────────────────────────────────────────────────────────────

export const SessionService = {
  /**
   * Criar nova sessão de estudo (UC05/UC06)
   * Body: CreateSessionCommand { lawAreaIds: int[], questionCount: int, excludeAnswered: bool }
   *   lawAreaIds = [] → todas as áreas
   *   questionCount: 5-50 (RN11)
   * 201 → CreateSessionResponse {
   *   sessionId: int,
   *   questions: [{
   *     id: int, statement, lawAreaName, examEdition?, year,
   *     alternatives: [{ id: int, letter: 'A'..'E', text }]
   *     // ⚠️ isCorrect NÃO é retornado (RN17)
   *   }]
   * }
   */
  create: (lawAreaIds, questionCount, excludeAnswered = false) =>
    post('/sessions', { lawAreaIds, questionCount, excludeAnswered }),

  /**
   * Recuperar sessão (para retomada — BE-14)
   * 200 → GetSessionResponse {
   *   sessionId, status: 'InProgress'|'Completed'|'Abandoned',
   *   totalQuestions, answeredCount,
   *   questions: [{
   *     id: int, statement, lawAreaName,
   *     answeredAlternativeId: int|null,  ← null se não respondida ainda
   *     isMarkedForReview: bool,
   *     alternatives: [{ id, letter, text }]
   *     // ⚠️ isCorrect NÃO retornado para questões ainda não respondidas
   *   }]
   * }
   */
  get: (sessionId) => get(`/sessions/${sessionId}`),

  /**
   * Submeter resposta (UC07)
   * Body: SubmitAnswerCommand { questionId: int, selectedAlternativeId: int, timeSpentSeconds: int }
   * 200 → SubmitAnswerResponse {
   *   isCorrect: bool,
   *   correctAlternativeId: int,
   *   explanation: string,
   *   legalRefs: string[],
   *   alternatives: [{ id, letter, isCorrect, explanation }]
   * }
   * 409 → questão já respondida nesta sessão
   */
  submitAnswer: (sessionId, questionId, selectedAlternativeId, timeSpentSeconds) =>
    post(`/sessions/${sessionId}/answers`, {
      questionId,
      selectedAlternativeId,
      timeSpentSeconds,
    }),

  /**
   * Finalizar sessão (UC10)
   * 200 → FinishSessionResponse {
   *   sessionId, totalQuestions, correctAnswers, accuracyPct,
   *   timeSpentSeconds, avgTimePerQuestion,
   *   byArea: [{ areaName, total, correct, accuracyPct }],
   *   weakAreas: string[]   ← áreas com accuracyPct < 50%
   * }
   * 409 → sessão já foi finalizada
   */
  finish: (sessionId) => post(`/sessions/${sessionId}/finish`),

  /**
   * Marcar/desmarcar questão para revisão (UC08-FA1)
   * Body: ToggleReviewMarkCommand { marked: bool }
   * 200 → ToggleReviewMarkResponse { marked: bool }
   */
  toggleReview: (sessionId, questionId, marked) =>
    patch(`/sessions/${sessionId}/answers/${questionId}/review`, { marked }),

  /**
   * Histórico do chat contextualizado na questão (UC13)
   * 200 → GetChatHistoryResponse {
   *   questionContext: { statement, areaName, legalRefs },
   *   messages: [{ id, role: 'user'|'assistant', content, legalRefs, createdAt }],
   *   messageCount: int,
   *   maxMessages: 20
   * }
   */
  getChatHistory: (sessionId, questionId) =>
    get(`/sessions/${sessionId}/questions/${questionId}/chat`),

  /**
   * Enviar mensagem ao chat da sessão (UC14/UC15)
   * Body: SendSessionChatMessageCommand { message: string (max 500 chars) }
   * 200 → SendSessionChatMessageResponse { id, role, content, legalRefs, createdAt }
   * 429 → limite de 20 mensagens atingido para esta questão
   * 503 → API de LLM indisponível
   */
  sendChatMessage: (sessionId, questionId, message) =>
    post(`/sessions/${sessionId}/questions/${questionId}/chat/messages`, { message }),
};

// ─── CHAT STANDALONE  ─────────────────────────────────────────────
// ChatController → /api/v1/chat  [Auth]
// Rota independente de sessão — para contexto livre
//
//  POST /chat/messages → 200 SendChatMessageResponse | 400 | 503
// ─────────────────────────────────────────────────────────────────

export const ChatService = {
  /**
   * Enviar mensagem sem vínculo de sessão
   * Body: SendChatMessageCommand — ver definição no backend
   * 200 → SendChatMessageResponse
   * 503 → LLM indisponível
   */
  sendMessage: (command) => post('/chat/messages', command),
};

// ─── USERS  ───────────────────────────────────────────────────────
// UsersController → /api/v1/users  [Auth]
//
//  GET    /users/me                          → 200 GetProfileResponse
//  PUT    /users/me                          → 204  ⚠️ sem body de retorno
//  PUT    /users/me/password                 → 204  ⚠️ sem body de retorno
//  POST   /users/me/avatar (multipart)       → 200 UploadAvatarResponse  | 413
//  DELETE /users/me                          → 204
//  POST   /users/me/data-export              → 202 Accepted | 429
//  GET    /users/me/performance?period=      → 200 GetUserPerformanceResponse
//  GET    /users/me/performance/areas/{int}  → 200 GetAreaPerformanceResponse | 404
// ─────────────────────────────────────────────────────────────────

export const UserService = {
  /**
   * Perfil do usuário autenticado (UC04)
   * 200 → GetProfileResponse { id, name, email, avatarUrl, role, createdAt }
   */
  getProfile: () => get('/users/me'),

  /**
   * Atualizar nome (UC04)
   * Body: UpdateProfileCommand { name: string (3-150) }
   * 204 No Content — buscar perfil novamente para atualizar UI
   */
  updateProfile: (name) => put('/users/me', { name }),

  /**
   * Alterar senha (UC04)
   * Body: ChangePasswordCommand { currentPassword, newPassword, confirmPassword }
   * 204 No Content  |  400 senha atual incorreta
   */
  changePassword: (currentPassword, newPassword, confirmPassword) =>
    put('/users/me/password', { currentPassword, newPassword, confirmPassword }),

  /**
   * Upload de avatar
   * Multipart: campo "file" (image/jpeg ou image/png, máx 2MB)
   * Controller tem [RequestSizeLimit(2 * 1024 * 1024 + 4096)]
   * 200 → UploadAvatarResponse { avatarUrl: string }  |  413 arquivo grande demais
   */
  uploadAvatar: (file) => {
    const form = new FormData();
    form.append('file', file);
    return post('/users/me/avatar', form);
  },

  /**
   * Excluir conta (LGPD — soft delete + anonimização)
   * 204 No Content
   */
  deleteAccount: () => del('/users/me'),

  /**
   * Solicitar exportação de dados (LGPD Art. 18)
   * 202 Accepted — processamento assíncrono, e-mail enviado em até 24h
   * 429 → máx 1 solicitação por usuário a cada 30 dias
   */
  requestDataExport: () => post('/users/me/data-export'),

  /**
   * Desempenho global (UC11)
   * @param {'7d'|'30d'|'all'} period
   * 200 → GetUserPerformanceResponse {
   *   global: { totalAnswered, totalCorrect, accuracyPct, totalSessions, avgTimePerQuestion, streakDays },
   *   byArea: [{ areaId, areaName, totalAnswered, totalCorrect, accuracyPct }],
   *   trend:  [{ date, accuracyPct, questionsAnswered }]
   * }
   */
  getPerformance: (period = '30d') => get(`/users/me/performance?period=${period}`),

  /**
   * Desempenho detalhado por área (UC12)
   * areaId: int
   * 200 → GetAreaPerformanceResponse {
   *   areaName, totalAnswered, totalCorrect, accuracyPct,
   *   recentWrongQuestions: [{ questionId, statement, answeredAt }],
   *   evolution: [{ date, accuracyPct }]
   * }
   */
  getAreaPerformance: (areaId) => get(`/users/me/performance/areas/${areaId}`),
};

// ─── ADMIN — QUESTIONS  ───────────────────────────────────────────
// AdminQuestionsController → /api/v1/admin/questions  [Admin]
//
//  GET    /admin/questions                → 200 PagedResult<QuestionSummaryResponse>
//  GET    /admin/questions/{id:int}       → 200 QuestionDetailResponse  | 404
//  POST   /admin/questions               → 201 QuestionDetailResponse   | 400
//  PUT    /admin/questions/{id:int}       → 200 QuestionDetailResponse   | 400 | 404
//  DELETE /admin/questions/{id:int}       → 204 (soft delete)            | 404
// ─────────────────────────────────────────────────────────────────

export const AdminQuestionService = {
  /**
   * Lista paginada com filtros
   * Query params: areaId?, year?, difficulty?(int), search?, page=1, pageSize=20
   * 200 → PagedResult<QuestionSummaryResponse> { items: [], totalCount, page, pageSize }
   */
  list: ({ areaId, year, difficulty, search, page = 1, pageSize = 20 } = {}) => {
    const p = new URLSearchParams({ page, pageSize });
    if (areaId != null) p.set('areaId',     areaId);
    if (year   != null) p.set('year',       year);
    if (difficulty != null) p.set('difficulty', difficulty);
    if (search)         p.set('search',     search);
    return get(`/admin/questions?${p}`);
  },

  /** 200 → QuestionDetailResponse (com alternativas completas + explanations) */
  getById: (id) => get(`/admin/questions/${id}`),

  /**
   * Criar questão
   * Body: CreateQuestionCommand {
   *   lawAreaId: int,
   *   statement: string,
   *   year: int,
   *   examEdition?: string,       ex: 'XXXVII'
   *   explanation: string,
   *   legalRefs?: string[],
   *   difficulty: 'Easy'|'Medium'|'Hard',
   *   alternatives: [{            ← exatamente 5 (A-E), exatamente 1 isCorrect=true
   *     letter: 'A'|'B'|'C'|'D'|'E',
   *     text: string,
   *     isCorrect: bool,
   *     explanation: string       ← obrigatório
   *   }]
   * }
   * 201 → QuestionDetailResponse
   */
  create: (command) => post('/admin/questions', command),

  /**
   * Atualizar questão (id vai na URL via route, não no body)
   * Body: UpdateQuestionCommand (mesmos campos do Create, sem Id)
   * 200 → QuestionDetailResponse
   */
  update: (id, command) => put(`/admin/questions/${id}`, command),

  /**
   * Soft delete (IsActive=false)
   * 204 No Content
   */
  deactivate: (id) => del(`/admin/questions/${id}`),
};

// ─── ADMIN — USERS  ───────────────────────────────────────────────
// AdminUsersController → /api/v1/admin/users  [Admin]
// ⚠️  IDs aqui são Guid (não int!)
//
//  GET   /admin/users               → 200 PagedResult<AdminUserResponse>
//  GET   /admin/users/{id:guid}     → 200 AdminUserResponse  | 404
//  PATCH /admin/users/{id:guid}/block    → 204  | 409
//  PATCH /admin/users/{id:guid}/unblock  → 204  | 409
//  PATCH /admin/users/{id:guid}/role     → 204  | 409
//    Body: ChangeUserRoleCommand { role: 'Admin'|'Student' }
// ─────────────────────────────────────────────────────────────────

export const AdminUserService = {
  /**
   * Lista paginada  (query: search?, page=1, size=20)
   * 200 → PagedResult<AdminUserResponse> {
   *   items: [{ id, name, email, role, isActive, emailConfirmed,
   *             createdAt, lastLoginAt, totalSessions, totalAnswered }],
   *   totalCount, page, pageSize
   * }
   */
  list: ({ search, page = 1, size = 20 } = {}) => {
    const p = new URLSearchParams({ page, size });
    if (search) p.set('search', search);
    return get(`/admin/users?${p}`);
  },

  /** 200 → AdminUserResponse */
  getById: (id) => get(`/admin/users/${id}`),

  /** Bloquear (IsActive=false). 204 | 409 já bloqueado */
  block: (id) => patch(`/admin/users/${id}/block`),

  /** Desbloquear (IsActive=true). 204 | 409 já ativo */
  unblock: (id) => patch(`/admin/users/${id}/unblock`),

  /**
   * Alterar role
   * Body: ChangeUserRoleCommand { role: 'Admin'|'Student' }
   * 204 No Content
   */
  setRole: (id, role) => patch(`/admin/users/${id}/role`, { role }),
};

// ─── ADMIN — REPORTS  ─────────────────────────────────────────────
// AdminReportsController → /api/v1/admin/reports  [Admin]
//
//  GET /admin/reports/summary           → 200 SystemSummaryResponse
//  GET /admin/reports/questions?areaId= → 200 QuestionStatsResponse
// ─────────────────────────────────────────────────────────────────

export const AdminReportService = {
  /**
   * Resumo geral do sistema
   * 200 → SystemSummaryResponse {
   *   totalUsers, activeUsersLast30d, totalQuestions, totalSessions,
   *   avgAccuracyGlobal, topWeakAreas: string[],
   *   registrationsByMonth: [{ month, count }]
   * }
   */
  getSummary: () => get('/admin/reports/summary'),

  /**
   * Estatísticas de questões (questões com mais erros, tempo médio, etc.)
   * areaId: int — opcional
   * 200 → QuestionStatsResponse
   */
  getQuestionStats: (areaId) => {
    const qs = areaId != null ? `?areaId=${areaId}` : '';
    return get(`/admin/reports/questions${qs}`);
  },
};
