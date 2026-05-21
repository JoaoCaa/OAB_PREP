/**
 * FE-03 — Tela de Login (UC02)
 * Endpoints: POST /api/v1/auth/login · POST /api/v1/auth/oauth/google
 */

import { AuthService } from '../../services/api.js';
import AuthStore from '../../store/auth.js';
import Router from '../../utils/router.js';
import { showToast } from '../../components/ui/Toast.js';

export default class LoginPage {
  constructor(container) { this.container = container; }

  render() {
    this.container.innerHTML = `
      <link rel="stylesheet" href="../../styles/design-system.css">
      <div class="auth-wrap">
        <div class="auth-card">
          <div style="text-align:center;margin-bottom:32px">
            <div class="auth-logo-box">⚖</div>
            <h2 style="margin-bottom:4px">Bem-vindo ao OAB Prep</h2>
            <p class="text-muted text-sm">Entre para continuar seus estudos</p>
          </div>

          <!-- Google OAuth -->
          <button id="btn-google" class="btn btn-secondary btn-full" style="margin-bottom:16px;justify-content:center;gap:10px">
            <svg width="18" height="18" viewBox="0 0 18 18" aria-hidden="true">
              <path fill="#4285F4" d="M17.64 9.2c0-.637-.057-1.251-.164-1.84H9v3.481h4.844a4.14 4.14 0 0 1-1.796 2.716v2.259h2.908C16.658 14.13 17.64 11.822 17.64 9.2z"/>
              <path fill="#34A853" d="M9 18c2.43 0 4.467-.806 5.956-2.18l-2.908-2.259c-.806.54-1.837.86-3.048.86-2.344 0-4.328-1.584-5.036-3.711H.957v2.332A8.997 8.997 0 0 0 9 18z"/>
              <path fill="#FBBC05" d="M3.964 10.71A5.41 5.41 0 0 1 3.682 9c0-.593.102-1.17.282-1.71V4.958H.957A8.996 8.996 0 0 0 0 9c0 1.452.348 2.827.957 4.042l3.007-2.332z"/>
              <path fill="#EA4335" d="M9 3.58c1.321 0 2.508.454 3.44 1.345l2.582-2.58C13.463.891 11.426 0 9 0A8.997 8.997 0 0 0 .957 4.958L3.964 6.29C4.672 4.163 6.656 3.58 9 3.58z"/>
            </svg>
            Entrar com Google
          </button>

          <div class="divider">
            <div class="divider-line"></div>
            <div class="divider-text">ou</div>
            <div class="divider-line"></div>
          </div>

          <!-- E-mail / Senha -->
          <div id="error-banner" class="hidden" style="padding:10px 14px;background:var(--red-dim);border:1px solid rgba(224,92,92,.2);border-radius:8px;font-size:13px;color:var(--red);margin-bottom:16px"></div>

          <div class="form-group">
            <label class="form-label" for="email">E-mail</label>
            <input class="form-input" id="email" type="email" placeholder="seu@email.com" autocomplete="email">
          </div>
          <div class="form-group">
            <label class="form-label" for="password">Senha</label>
            <div style="position:relative">
              <input class="form-input" id="password" type="password" placeholder="••••••••" autocomplete="current-password" style="padding-right:42px">
              <button id="toggle-pw" type="button" style="position:absolute;right:12px;top:50%;transform:translateY(-50%);color:var(--text3);font-size:16px">👁</button>
            </div>
          </div>

          <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:20px">
            <label style="display:flex;align-items:center;gap:8px;cursor:pointer">
              <input type="checkbox" id="remember" checked style="accent-color:var(--gold)">
              <span class="text-sm text-muted">Lembrar de mim</span>
            </label>
            <a href="#/forgot-password" class="text-sm text-gold">Esqueci a senha</a>
          </div>

          <button id="btn-login" class="btn btn-primary btn-full btn-lg">Entrar</button>

          <p class="text-sm text-muted" style="text-align:center;margin-top:16px">
            Não tem conta? <a href="#/register" class="text-gold">Criar conta</a>
          </p>
        </div>
      </div>
      <div id="toast-root"></div>
    `;
    this._attachEvents();
  }

  _attachEvents() {
    document.getElementById('btn-login').addEventListener('click', () => this._doLogin());
    document.getElementById('password').addEventListener('keydown', e => {
      if (e.key === 'Enter') this._doLogin();
    });
    document.getElementById('toggle-pw').addEventListener('click', () => {
      const inp = document.getElementById('password');
      inp.type = inp.type === 'password' ? 'text' : 'password';
    });
    document.getElementById('btn-google').addEventListener('click', () => this._doGoogle());
  }

  async _doLogin() {
    const email    = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;
    const remember = document.getElementById('remember').checked;
    const btn      = document.getElementById('btn-login');
    const banner   = document.getElementById('error-banner');

    if (!email || !password) {
      this._showError('Preencha e-mail e senha.'); return;
    }

    btn.disabled = true;
    btn.textContent = 'Entrando…';
    banner.classList.add('hidden');

    try {
      const data = await AuthService.login(email, password, remember);
      AuthStore.setTokens(data.accessToken, data.refreshToken);
      AuthStore.setUser({
        id:    data.userId,
        name:  data.name,
        email: data.email,
        role:  data.role,
        avatarUrl: data.avatarUrl ?? null,
      });
      Router.go('/');
    } catch (err) {
      if (err.status === 423) {
        this._showError(`Conta bloqueada temporariamente. ${err.message}`);
      } else {
        this._showError('Credenciais inválidas.');
      }
    } finally {
      btn.disabled = false;
      btn.textContent = 'Entrar';
    }
  }

  async _doGoogle() {
    // Em produção: usar expo-auth-session (RN) ou google.accounts.id (Web)
    showToast('Integração Google OAuth — configure o Client ID no .env', 'warning');
  }

  _showError(msg) {
    const el = document.getElementById('error-banner');
    el.textContent = msg;
    el.classList.remove('hidden');
  }
}
