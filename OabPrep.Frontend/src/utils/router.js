/**
 * OAB Prep — Router
 * Roteador SPA baseado em hash (#/rota).
 * Integração com React Router v6 ou Expo Navigation é drop-in.
 */

import AuthStore from '../store/auth.js';

const ROUTES = {
  '/login':             () => import('../pages/auth/LoginPage.js'),
  '/register':          () => import('../pages/auth/RegisterPage.js'),
  '/forgot-password':   () => import('../pages/auth/ForgotPasswordPage.js'),
  '/reset-password':    () => import('../pages/auth/ResetPasswordPage.js'),
  '/confirm-email':     () => import('../pages/auth/ConfirmEmailPage.js'),
  '/':                  () => import('../pages/HomePage.js'),
  '/select-area':       () => import('../pages/study/SelectAreaPage.js'),
  '/session/:id':       () => import('../pages/study/SessionPage.js'),
  '/result/:id':        () => import('../pages/study/ResultPage.js'),
  '/performance':       () => import('../pages/performance/PerformancePage.js'),
  '/performance/:area': () => import('../pages/performance/AreaDetailPage.js'),
  '/history':           () => import('../pages/study/HistoryPage.js'),
  '/profile':           () => import('../pages/profile/ProfilePage.js'),
  '/privacy':           () => import('../pages/profile/PrivacyPage.js'),
  // Admin routes — protegidas por role=Admin
  '/admin/questions':   () => import('../pages/admin/QuestionsPage.js').then(m => ({ default: m.QuestionsPage })),
  '/admin/users':       () => import('../pages/admin/QuestionsPage.js').then(m => ({ default: m.UsersPage })),
  '/admin/reports':     () => import('../pages/admin/QuestionsPage.js').then(m => ({ default: m.ReportsPage })),
};

/** Rotas que não exigem autenticação */
const PUBLIC_ROUTES = ['/login', '/register', '/forgot-password', '/reset-password', '/confirm-email'];

/** Rotas exclusivas para Admin */
const ADMIN_ROUTES = ['/admin', '/admin/questions', '/admin/users', '/admin/reports'];

const Router = {
  go(path, params = {}) {
    const qs = Object.keys(params).length ? '?' + new URLSearchParams(params) : '';
    window.location.hash = '#' + path + qs;
  },

  getPath() {
    const hash = window.location.hash.slice(1).split('?')[0] || '/';
    return hash;
  },

  getParams() {
    const search = window.location.hash.split('?')[1] || '';
    return Object.fromEntries(new URLSearchParams(search));
  },

  /** Extrai segmentos dinâmicos, ex: /session/abc123 → { id: 'abc123' } */
  matchRoute(path) {
    for (const pattern of Object.keys(ROUTES)) {
      const regex = new RegExp('^' + pattern.replace(/:([^/]+)/g, '([^/]+)') + '$');
      const m = path.match(regex);
      if (m) {
        const keys = [...pattern.matchAll(/:([^/]+)/g)].map(x => x[1]);
        const values = m.slice(1);
        return { pattern, params: Object.fromEntries(keys.map((k, i) => [k, values[i]])) };
      }
    }
    return null;
  },

  /** Guard: redireciona se não autenticado ou sem permissão */
  guard(path) {
    if (PUBLIC_ROUTES.includes(path)) return true;
    if (!AuthStore.isAuthenticated()) { this.go('/login'); return false; }
    if (ADMIN_ROUTES.some(r => path.startsWith(r)) && !AuthStore.isAdmin()) {
      this.go('/'); return false;
    }
    return true;
  },

  async load() {
    const path = this.getPath();
    if (!this.guard(path)) return;

    const match = this.matchRoute(path);
    if (!match) { this.go('/'); return; }

    const loader = ROUTES[match.pattern];
    const { default: Page } = await loader();
    const app = document.getElementById('root');
    app.innerHTML = '';
    new Page(app, { ...match.params, ...this.getParams() }).render();
  },

  init() {
    window.addEventListener('hashchange', () => this.load());
    this.load();
  },
};

export default Router;
