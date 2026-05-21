/**
 * FE-04 — Recuperação de Senha (UC03)
 * Tela 1: POST /api/v1/auth/forgot-password
 * Tela 2: POST /api/v1/auth/reset-password  (deep link ?token=xxx)
 */

import { AuthService } from '../../services/api.js';
import Router from '../../utils/router.js';
import { showToast } from '../../components/ui/Toast.js';

export class ForgotPasswordPage {
  constructor(container) { this.container = container; }

  render() {
    this.container.innerHTML = `
      <link rel="stylesheet" href="/src/styles/design-system.css">
      <div class="auth-wrap">
        <div class="auth-card">
          <div style="text-align:center;margin-bottom:28px">
            <div class="auth-logo-box" style="font-size:22px">🔑</div>
            <h2 style="margin-bottom:4px">Recuperar senha</h2>
            <p class="text-muted text-sm">Enviaremos instruções para o seu e-mail</p>
          </div>

          <div id="success-msg" class="hidden" style="padding:14px;background:var(--green-dim);border:1px solid rgba(76,175,125,.2);border-radius:8px;font-size:14px;color:var(--green);margin-bottom:16px;text-align:center">
            Se o e-mail existir, você receberá as instruções em breve.
          </div>

          <div id="form-area">
            <div class="form-group">
              <label class="form-label" for="email">E-mail cadastrado</label>
              <input class="form-input" id="email" type="email" placeholder="seu@email.com">
            </div>
            <button id="btn-send" class="btn btn-primary btn-full btn-lg">Enviar instruções</button>
          </div>

          <p style="text-align:center;margin-top:20px">
            <a href="#/login" class="text-sm text-gold">← Voltar ao login</a>
          </p>
        </div>
      </div>
      <div id="toast-root"></div>
    `;

    document.getElementById('btn-send').addEventListener('click', async () => {
      const email = document.getElementById('email').value.trim();
      if (!email) return;
      const btn = document.getElementById('btn-send');
      btn.disabled = true; btn.textContent = 'Enviando…';
      try {
        await AuthService.forgotPassword(email);
      } catch {} // sempre exibe mesma mensagem (RN09 — não revela existência do e-mail)
      document.getElementById('form-area').classList.add('hidden');
      document.getElementById('success-msg').classList.remove('hidden');
      btn.disabled = false; btn.textContent = 'Enviar instruções';
    });
  }
}

export class ResetPasswordPage {
  constructor(container, params) {
    this.container = container;
    this.token = params?.token || '';
  }

  render() {
    this.container.innerHTML = `
      <link rel="stylesheet" href="/src/styles/design-system.css">
      <div class="auth-wrap">
        <div class="auth-card">
          <div style="text-align:center;margin-bottom:28px">
            <div class="auth-logo-box" style="font-size:22px">🔒</div>
            <h2 style="margin-bottom:4px">Nova senha</h2>
            <p class="text-muted text-sm">Defina sua nova senha de acesso</p>
          </div>

          <div id="error-msg" class="hidden" style="padding:10px 14px;background:var(--red-dim);border:1px solid rgba(224,92,92,.2);border-radius:8px;font-size:13px;color:var(--red);margin-bottom:16px"></div>

          <div class="form-group">
            <label class="form-label">Nova senha</label>
            <input class="form-input" id="new-pass" type="password" placeholder="Mín. 8 caracteres">
          </div>
          <div class="form-group">
            <label class="form-label">Confirmar nova senha</label>
            <input class="form-input" id="confirm-pass" type="password" placeholder="Repita a senha">
          </div>
          <button id="btn-reset" class="btn btn-primary btn-full btn-lg">Redefinir senha</button>

          <p style="text-align:center;margin-top:20px">
            <a href="#/login" class="text-sm text-gold">← Voltar ao login</a>
          </p>
        </div>
      </div>
      <div id="toast-root"></div>
    `;

    document.getElementById('btn-reset').addEventListener('click', async () => {
      const newPassword     = document.getElementById('new-pass').value;
      const confirmPassword = document.getElementById('confirm-pass').value;
      const errEl = document.getElementById('error-msg');

      if (newPassword !== confirmPassword) {
        errEl.textContent = 'As senhas não coincidem.';
        errEl.classList.remove('hidden'); return;
      }
      const btn = document.getElementById('btn-reset');
      btn.disabled = true; btn.textContent = 'Redefinindo…';
      try {
        await AuthService.resetPassword(this.token, newPassword, confirmPassword);
        showToast('Senha redefinida com sucesso! 🔐', 'success');
        Router.go('/login');
      } catch (err) {
        errEl.textContent = err.message || 'Token inválido ou expirado.';
        errEl.classList.remove('hidden');
      } finally {
        btn.disabled = false; btn.textContent = 'Redefinir senha';
      }
    });
  }
}

// Alias de export default para o router
export default ForgotPasswordPage;
