/**
 * OAB Prep — AppShell
 * Sidebar + header wrapper para todas as páginas autenticadas.
 * Uso: new AppShell(container, { title, navItem }).render(contentHTML)
 */

import AuthStore from '../../store/auth.js';
import Router from '../../utils/router.js';
import { showToast } from '../ui/Toast.js';

const NAV_ITEMS = [
  { id: 'home',        icon: '🏠', label: 'Início',       path: '/' },
  { id: 'practice',   icon: '📚', label: 'Praticar',     path: '/select-area' },
  { id: 'performance',icon: '📊', label: 'Desempenho',   path: '/performance' },
  { id: 'history',    icon: '🕐', label: 'Histórico',    path: '/history' },
];

const ADMIN_ITEMS = [
  { id: 'admin-q',   icon: '📝', label: 'Questões',  path: '/admin/questions', badge: 'ADM' },
  { id: 'admin-u',   icon: '👥', label: 'Usuários',  path: '/admin/users',     badge: 'ADM' },
  { id: 'admin-r',   icon: '📊', label: 'Relatórios',path: '/admin/reports',   badge: 'ADM' },
];

const ACCOUNT_ITEMS = [
  { id: 'profile', icon: '👤', label: 'Perfil',      path: '/profile' },
  { id: 'privacy', icon: '🔒', label: 'Privacidade', path: '/privacy' },
];

export class AppShell {
  constructor(container, { title = '', activeNav = '' } = {}) {
    this.container = container;
    this.title = title;
    this.activeNav = activeNav;
    this.user = AuthStore.getUser();
    this.isAdmin = AuthStore.isAdmin();
  }

  render(contentHTML) {
    const initials = (this.user?.name || 'U')
      .split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();

    this.container.innerHTML = `
      <div class="app-shell">
        <nav class="sidebar" id="sidebar">
          <!-- Logo -->
          <div style="padding:22px 20px 18px;border-bottom:1px solid var(--border);display:flex;align-items:center;gap:10px">
            <div style="width:36px;height:36px;border-radius:10px;background:linear-gradient(135deg,var(--gold),#a87c2a);display:flex;align-items:center;justify-content:center;font-size:18px;font-weight:800;color:#1a1200;flex-shrink:0">⚖</div>
            <div>
              <div style="font-size:15px;font-weight:600">OAB Prep</div>
              <div style="font-size:10px;color:var(--text2)">Plataforma de Estudos</div>
            </div>
          </div>

          <!-- Nav principal -->
          <div style="padding:12px 10px 0">
            <div style="font-size:10px;text-transform:uppercase;letter-spacing:1px;color:var(--text3);padding:0 10px;margin-bottom:6px">Menu</div>
            ${NAV_ITEMS.map(i => this._navItem(i)).join('')}
          </div>

          <!-- Admin -->
          ${this.isAdmin ? `
            <div style="padding:12px 10px 0">
              <div style="font-size:10px;text-transform:uppercase;letter-spacing:1px;color:var(--text3);padding:0 10px;margin-bottom:6px">Admin</div>
              ${ADMIN_ITEMS.map(i => this._navItem(i)).join('')}
            </div>
          ` : ''}

          <!-- Conta -->
          <div style="padding:12px 10px 0">
            <div style="font-size:10px;text-transform:uppercase;letter-spacing:1px;color:var(--text3);padding:0 10px;margin-bottom:6px">Conta</div>
            ${ACCOUNT_ITEMS.map(i => this._navItem(i)).join('')}
          </div>

          <!-- Footer -->
          <div style="margin-top:auto;padding:16px;border-top:1px solid var(--border)">
            <div style="display:flex;align-items:center;gap:10px">
              <div style="width:34px;height:34px;border-radius:50%;background:linear-gradient(135deg,#9b7de8,#5b8dee);display:flex;align-items:center;justify-content:center;font-size:13px;font-weight:600;flex-shrink:0">${initials}</div>
              <div style="flex:1;overflow:hidden">
                <div style="font-size:13px;font-weight:500;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">${this.user?.name || 'Usuário'}</div>
                <div style="font-size:11px;color:var(--text2)">${this.isAdmin ? 'Administrador' : 'Estudante'}</div>
              </div>
              <button onclick="window.__logout()" style="padding:6px;border-radius:6px;color:var(--text3);font-size:16px;transition:all .15s" title="Sair">🚪</button>
            </div>
          </div>
        </nav>

        <div class="main-area">
          <!-- Header -->
          <header class="page-header">
            <div style="font-size:16px;font-weight:600">${this.title}</div>
            <div style="display:flex;align-items:center;gap:10px">
              <button style="width:36px;height:36px;border-radius:8px;background:var(--bg3);border:1px solid var(--border);display:flex;align-items:center;justify-content:center;font-size:16px;color:var(--text2);cursor:pointer" title="Notificações">🔔</button>
            </div>
          </header>

          <!-- Page content -->
          <div class="page-content fade-in" id="page-content">
            ${contentHTML}
          </div>
        </div>
      </div>

      <div id="toast-root"></div>
      <nav class="mobile-nav">
        <div class="mobile-nav-item ${this.activeNav==='home'?'active':''}" onclick="window.__router.go('/')">
          <span>🏠</span><span>Início</span>
        </div>
        <div class="mobile-nav-item ${this.activeNav==='practice'?'active':''}" onclick="window.__router.go('/select-area')">
          <span>📚</span><span>Praticar</span>
        </div>
        <div class="mobile-nav-item ${this.activeNav==='performance'?'active':''}" onclick="window.__router.go('/performance')">
          <span>📊</span><span>Desempenho</span>
        </div>
        <div class="mobile-nav-item" onclick="window.__router.go('/profile')">
          <span>👤</span><span>Perfil</span>
        </div>
      </nav>
    `;

    // Logout global
    window.__logout = async () => {
      try {
        const { AuthService } = await import('../../services/api.js');
        await AuthService.logout();
      } catch {}
      AuthStore.clear();
      Router.go('/login');
      showToast('Sessão encerrada', 'success');
    };
  }

  _navItem({ id, icon, label, path, badge }) {
    const active = this.activeNav === id;
    return `
      <div onclick="window.__router.go('${path}')"
        style="display:flex;align-items:center;gap:10px;padding:9px 10px;border-radius:8px;cursor:pointer;font-size:13px;margin-bottom:2px;transition:all .15s;
               ${active ? 'background:rgba(200,168,75,.12);color:var(--gold)' : 'color:var(--text2)'}">
        <span style="font-size:18px;width:20px;text-align:center">${icon}</span>
        <span>${label}</span>
        ${badge ? `<span style="margin-left:auto;background:var(--gold);color:#1a1200;font-size:10px;font-weight:700;padding:2px 6px;border-radius:20px">${badge}</span>` : ''}
      </div>
    `;
  }
}
