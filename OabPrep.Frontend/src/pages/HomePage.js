/**
 * FE-06 — Tela Home / Painel Principal
 * Endpoints: GET /users/me/performance?period=7d · GET /sessions?status=InProgress
 */

import { AppShell } from '../components/layout/AppShell.js';
import { UserService, SessionService } from '../services/api.js';
import AuthStore from '../store/auth.js';
import Router from '../utils/router.js';

export default class HomePage {
  constructor(container) {
    this.container = container;
    this.user = AuthStore.getUser();
  }

  async render() {
    const shell = new AppShell(this.container, { title: 'Início', activeNav: 'home' });

    // Skeleton inicial
    shell.render(this._skeleton());

    // Carrega dados em paralelo
    const [perf] = await Promise.allSettled([
      UserService.getPerformance('7d'),
    ]);
    const sessions = { status: 'fulfilled', value: null };

    const perfData    = perf.status    === 'fulfilled' ? perf.value    : null;
    const activeSession = sessions.status === 'fulfilled' ? sessions.value?.[0] : null;

    document.getElementById('page-content').innerHTML = this._html(perfData, activeSession);
    this._attachEvents(activeSession);
  }

  _skeleton() {
    return `<div style="opacity:.5">
      <div style="height:32px;width:280px;background:var(--bg3);border-radius:8px;margin-bottom:8px"></div>
      <div style="height:16px;width:200px;background:var(--bg3);border-radius:6px;margin-bottom:28px"></div>
      <div class="stats-grid">
        ${Array(4).fill('<div class="stat-card" style="height:90px"></div>').join('')}
      </div>
    </div>`;
  }

  _html(perf, activeSession) {
    const name  = this.user?.name?.split(' ')[0] || 'Advogado';
    const global = perf?.global || {};
    const byArea = perf?.byArea || [];

    const weakAreas = byArea.filter(a => a.accuracyPct < 65).slice(0, 4);

    return `
      <!-- Saudação -->
      <div style="margin-bottom:24px">
        <h1 style="margin-bottom:4px">Olá, <span style="color:var(--gold)">${name}</span>! 👋</h1>
        <p class="text-muted">Vamos estudar para a OAB hoje? Continue progredindo!</p>
      </div>

      <!-- Banner de sessão ativa -->
      ${activeSession ? `
        <div id="resume-banner" style="background:var(--gold-dim);border:1px solid rgba(200,168,75,.2);border-radius:12px;padding:14px 18px;margin-bottom:24px;display:flex;align-items:center;gap:14px;cursor:pointer;transition:all .15s">
          <span style="font-size:24px">▶️</span>
          <div style="flex:1">
            <div style="font-size:14px;font-weight:600;color:var(--gold)">Sessão em andamento</div>
            <div class="text-sm text-muted" style="margin-top:2px">${activeSession.answeredCount} de ${activeSession.totalQuestions} questões respondidas</div>
          </div>
          <button class="btn btn-sm btn-primary">Retomar →</button>
        </div>
      ` : ''}

      <!-- Stats -->
      <div class="stats-grid mb-24">
        <div class="stat-card" style="--stat-color:var(--gold)">
          <div class="stat-label">Questões Respondidas</div>
          <div class="stat-value">${(global.totalAnswered ?? 0).toLocaleString('pt-BR')}</div>
          <div class="stat-sub">+${global.todayAnswered ?? 0} hoje</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--green)">
          <div class="stat-label">Taxa de Acerto</div>
          <div class="stat-value">${Math.round(global.accuracyPct ?? 0)}%</div>
          <div class="stat-sub">↑ esta semana</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--blue)">
          <div class="stat-label">Sequência Atual</div>
          <div class="stat-value">${global.streakDays ?? 0} dias 🔥</div>
          <div class="stat-sub">Recorde: ${global.bestStreak ?? 0} dias</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--purple)">
          <div class="stat-label">Sessões</div>
          <div class="stat-value">${global.totalSessions ?? 0}</div>
          <div class="stat-sub">Este mês: ${global.sessionsThisMonth ?? 0}</div>
        </div>
      </div>

      <div class="grid-2 mb-24">
        <!-- CTA Praticar -->
        <div class="card card-lg" style="position:relative;overflow:hidden">
          <div style="position:absolute;right:20px;top:20px;font-size:48px;opacity:.15">⚖️</div>
          <h3 style="margin-bottom:6px">Praticar agora</h3>
          <p class="text-muted text-sm" style="margin-bottom:18px">Sessão personalizada para o seu nível atual</p>
          <button id="btn-start" class="btn btn-primary btn-lg">🚀 Iniciar Sessão</button>
        </div>

        <!-- Áreas fracas -->
        <div class="card card-lg">
          <div class="section-title">Áreas para reforçar</div>
          ${weakAreas.length ? weakAreas.map(a => `
            <div style="display:flex;align-items:center;gap:10px;margin-bottom:10px">
              <div class="text-sm text-muted" style="flex:1">${a.areaName ?? a.lawAreaName ?? 'Área'}</div>
              <span class="badge badge-red">${Math.round(a.accuracyPct)}%</span>
            </div>
          `).join('') : '<p class="text-muted text-sm">Nenhuma área fraca detectada 🎉</p>'}
          <button onclick="window.__router.go('/performance')" class="btn btn-secondary btn-sm btn-full" style="margin-top:12px">Ver desempenho completo →</button>
        </div>
      </div>

      <!-- Sessões recentes -->
      <div class="section-title">Sessões Recentes</div>
      <div id="sessions-list">
        <p class="text-muted text-sm">Carregando histórico…</p>
      </div>
    `;
  }

  _attachEvents(activeSession) {
    document.getElementById('btn-start')?.addEventListener('click', () => Router.go('/select-area'));
    document.getElementById('resume-banner')?.addEventListener('click', () => {
      Router.go(`/session/${activeSession.sessionId}`);
    });
    this._loadHistory();
  }

  async _loadHistory() {
    const container = document.getElementById('sessions-list');
    if (!container) return;
    // O backend não tem endpoint de listagem de sessões do usuário público —
    // o histórico vem dos dados de performance. Mostramos placeholder amigável.
    container.innerHTML = `
      <p class="text-muted text-sm" style="text-align:center;padding:16px 0">
        Complete uma sessão para ver seu histórico aqui.
      </p>
    `;
  }
}
