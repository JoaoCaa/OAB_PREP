/**
 * FE-02 — Tela de Cadastro (UC01)
 * Endpoint: POST /api/v1/auth/register
 */

import { AuthService } from '../../services/api.js';
import Router from '../../utils/router.js';
import { showToast } from '../../components/ui/Toast.js';

export default class RegisterPage {
  constructor(container) { this.container = container; }

  render() {
    this.container.innerHTML = `
      <link rel="stylesheet" href="/src/styles/design-system.css">
      <div class="auth-wrap">
        <div class="auth-card">
          <div style="text-align:center;margin-bottom:28px">
            <div class="auth-logo-box">⚖</div>
            <h2 style="margin-bottom:4px">Criar sua conta</h2>
            <p class="text-muted text-sm">Junte-se a milhares de candidatos à OAB</p>
          </div>

          <div id="error-banner" class="hidden" style="padding:10px 14px;background:var(--red-dim);border:1px solid rgba(224,92,92,.2);border-radius:8px;font-size:13px;color:var(--red);margin-bottom:16px"></div>

          <div class="form-group">
            <label class="form-label" for="name">Nome completo</label>
            <input class="form-input" id="name" type="text" placeholder="Seu nome completo">
            <div class="form-error hidden" id="err-name"></div>
          </div>
          <div class="form-group">
            <label class="form-label" for="email">E-mail</label>
            <input class="form-input" id="email" type="email" placeholder="seu@email.com">
            <div class="form-error hidden" id="err-email"></div>
          </div>
          <div class="form-group">
            <label class="form-label" for="password">Senha</label>
            <input class="form-input" id="password" type="password" placeholder="Mín. 8 caracteres">
            <!-- Indicador de força (FE-02 requisito) -->
            <div style="display:flex;gap:4px;margin-top:6px">
              <div class="seg" id="seg1" style="flex:1;height:3px;border-radius:2px;background:var(--bg3);transition:all .3s"></div>
              <div class="seg" id="seg2" style="flex:1;height:3px;border-radius:2px;background:var(--bg3);transition:all .3s"></div>
              <div class="seg" id="seg3" style="flex:1;height:3px;border-radius:2px;background:var(--bg3);transition:all .3s"></div>
              <div class="seg" id="seg4" style="flex:1;height:3px;border-radius:2px;background:var(--bg3);transition:all .3s"></div>
            </div>
            <div id="strength-label" style="font-size:11px;color:var(--text3);margin-top:3px">Digite uma senha</div>
          </div>
          <div class="form-group">
            <label class="form-label" for="confirm">Confirmar senha</label>
            <input class="form-input" id="confirm" type="password" placeholder="Repita a senha">
            <div class="form-error hidden" id="err-confirm"></div>
          </div>
          <div class="form-group">
            <label style="display:flex;align-items:flex-start;gap:10px;cursor:pointer">
              <input type="checkbox" id="terms" style="accent-color:var(--gold);margin-top:2px">
              <span class="text-sm text-muted">
                Li e aceito os <a href="#" class="text-gold">Termos de Uso</a> e a <a href="#" class="text-gold">Política de Privacidade</a>
              </span>
            </label>
            <div class="form-error hidden" id="err-terms"></div>
          </div>

          <button id="btn-register" class="btn btn-primary btn-full btn-lg">Criar conta</button>

          <p class="text-sm text-muted" style="text-align:center;margin-top:16px">
            Já tem conta? <a href="#/login" class="text-gold">Entrar</a>
          </p>
        </div>
      </div>
      <div id="toast-root"></div>
    `;
    this._attachEvents();
  }

  _attachEvents() {
    document.getElementById('password').addEventListener('input', e => this._checkStrength(e.target.value));
    document.getElementById('btn-register').addEventListener('click', () => this._doRegister());
  }

  _checkStrength(pass) {
    let score = 0;
    if (pass.length >= 8) score++;
    if (/[A-Z]/.test(pass)) score++;
    if (/[0-9]/.test(pass)) score++;
    if (/[^A-Za-z0-9]/.test(pass)) score++;
    const colors = ['var(--red)', 'var(--red)', 'var(--gold)', 'var(--green)'];
    const labels = ['Muito fraca', 'Fraca', 'Média', 'Forte'];
    for (let i = 1; i <= 4; i++) {
      const el = document.getElementById(`seg${i}`);
      el.style.background = i <= score ? colors[score - 1] : 'var(--bg3)';
    }
    const lbl = document.getElementById('strength-label');
    lbl.textContent = pass.length ? labels[score - 1] || 'Muito forte' : 'Digite uma senha';
    lbl.style.color = score > 0 ? colors[score - 1] : 'var(--text3)';
  }

  async _doRegister() {
    const name     = document.getElementById('name').value.trim();
    const email    = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;
    const confirm  = document.getElementById('confirm').value;
    const terms    = document.getElementById('terms').checked;

    // Validações client-side (espelham FluentValidation do backend)
    let valid = true;
    const showErr = (id, msg) => {
      const el = document.getElementById(id);
      el.textContent = msg; el.classList.remove('hidden'); valid = false;
    };
    const clearErr = id => document.getElementById(id).classList.add('hidden');

    clearErr('err-name'); clearErr('err-email'); clearErr('err-confirm'); clearErr('err-terms');

    if (name.length < 3)  showErr('err-name', 'Nome deve ter ao menos 3 caracteres.');
    if (!/\S+@\S+\.\S+/.test(email)) showErr('err-email', 'E-mail inválido.');
    if (password !== confirm) showErr('err-confirm', 'As senhas não coincidem.');
    if (!terms) showErr('err-terms', 'Você deve aceitar os termos.');
    if (!valid) return;

    const btn = document.getElementById('btn-register');
    btn.disabled = true; btn.textContent = 'Criando conta…';

    try {
      await AuthService.register({ name, email, password, confirmPassword: confirm, acceptedTerms: true });
      showToast('Conta criada! Verifique seu e-mail para confirmar. 📧', 'success');
      Router.go('/login');
    } catch (err) {
      // Mapeia erros campo-a-campo vindos do FluentValidation (BE-02)
      if (err.errors?.email) showErr('err-email', err.errors.email[0]);
      else document.getElementById('error-banner').textContent = err.message;
      document.getElementById('error-banner').classList.remove('hidden');
    } finally {
      btn.disabled = false; btn.textContent = 'Criar conta';
    }
  }
}
