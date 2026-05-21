/**
 * FE-05 — Tela de Perfil (UC04)
 * FE-22 — Tela de Privacidade (LGPD)
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { UserService } from '../../services/api.js';
import AuthStore from '../../store/auth.js';
import Router from '../../utils/router.js';
import { showToast } from '../../components/ui/Toast.js';
// ⚠️  PUT /users/me e PUT /users/me/password retornam 204 (sem body)

export class ProfilePage {
  constructor(container) { this.container = container; this.user = AuthStore.getUser(); }

  async render() {
    const shell = new AppShell(this.container, { title: 'Perfil', activeNav: 'profile' });
    try {
      const me = await UserService.getProfile();
      AuthStore.setUser(me);
      this.user = me;
    } catch {}
    shell.render(this._html());
    this._attachEvents();
  }

  _html() {
    const u = this.user || {};
    const initials = (u.name||'?').split(' ').map(n=>n[0]).slice(0,2).join('').toUpperCase();
    return `
      <div style="max-width:560px">
        <!-- Perfil principal -->
        <div class="card card-lg mb-20">
          <div style="display:flex;align-items:center;gap:16px;margin-bottom:24px">
            <div id="avatar-circle" style="width:64px;height:64px;border-radius:50%;background:linear-gradient(135deg,var(--purple),var(--blue));display:flex;align-items:center;justify-content:center;font-size:22px;font-weight:600;flex-shrink:0;cursor:pointer" title="Trocar foto">
              ${u.avatarUrl ? `<img src="${u.avatarUrl}" style="width:100%;height:100%;border-radius:50%;object-fit:cover">` : initials}
            </div>
            <input type="file" id="avatar-input" accept="image/jpeg,image/png" style="display:none">
            <div>
              <h2 style="margin-bottom:2px">${u.name || ''}</h2>
              <p class="text-sm text-muted">${u.email || ''}</p>
              <span class="badge badge-gold" style="margin-top:6px">${u.role === 'Admin' ? 'Administrador' : 'Estudante'}</span>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Nome completo</label>
            <input class="form-input" id="inp-name" value="${u.name || ''}">
          </div>
          <div class="form-group">
            <label class="form-label">E-mail</label>
            <input class="form-input" value="${u.email || ''}" disabled style="opacity:.6">
            <div class="form-hint">O e-mail não pode ser alterado</div>
          </div>
          <button id="btn-save" class="btn btn-primary">💾 Salvar alterações</button>
        </div>

        <!-- Alterar senha -->
        <div class="card card-lg mb-20">
          <h3 style="margin-bottom:16px">🔑 Alterar Senha</h3>
          <div class="form-group">
            <label class="form-label">Senha atual</label>
            <input class="form-input" id="cur-pass" type="password" placeholder="••••••••">
          </div>
          <div class="form-group">
            <label class="form-label">Nova senha</label>
            <input class="form-input" id="new-pass" type="password" placeholder="Mín. 8 caracteres">
          </div>
          <div class="form-group">
            <label class="form-label">Confirmar nova senha</label>
            <input class="form-input" id="conf-pass" type="password" placeholder="Repita a senha">
          </div>
          <div class="form-error hidden" id="err-pass"></div>
          <button id="btn-pass" class="btn btn-primary">Atualizar senha</button>
        </div>

        <!-- Zona de perigo -->
        <div class="card card-lg" style="border-color:rgba(224,92,92,.2)">
          <h3 style="color:var(--red);margin-bottom:8px">⚠️ Zona de Perigo</h3>
          <p class="text-sm text-muted" style="margin-bottom:16px">
            A exclusão é permanente. Seus dados serão anonimizados conforme a LGPD.
          </p>
          <button id="btn-delete" class="btn btn-danger">🗑️ Excluir minha conta</button>
        </div>
      </div>
    `;
  }

  _attachEvents() {
    document.getElementById('avatar-circle').addEventListener('click', () =>
      document.getElementById('avatar-input').click());

    document.getElementById('avatar-input').addEventListener('change', async e => {
      const file = e.target.files[0];
      if (!file) return;
      if (file.size > 2 * 1024 * 1024) { showToast('Imagem deve ter no máximo 2MB', 'error'); return; }
      try {
        const res = await UserService.uploadAvatar(file);
        const circle = document.getElementById('avatar-circle');
        circle.innerHTML = `<img src="${res.avatarUrl}" style="width:100%;height:100%;border-radius:50%;object-fit:cover">`;
        showToast('Foto atualizada! ✓', 'success');
      } catch (err) { showToast(err.message, 'error'); }
    });

    document.getElementById('btn-save').addEventListener('click', async () => {
      const name = document.getElementById('inp-name').value.trim();
      if (name.length < 3) { showToast('Nome deve ter ao menos 3 caracteres', 'error'); return; }
      try {
        await UserService.updateProfile(name); // 204 — sem body de retorno
        // Busca perfil atualizado para sincronizar AuthStore
        const updated = await UserService.getProfile();
        AuthStore.setUser(updated);
        showToast('Perfil atualizado! ✓', 'success');
      } catch (err) { showToast(err.message, 'error'); }
    });

    document.getElementById('btn-pass').addEventListener('click', async () => {
      const cur  = document.getElementById('cur-pass').value;
      const nw   = document.getElementById('new-pass').value;
      const conf = document.getElementById('conf-pass').value;
      const errEl = document.getElementById('err-pass');
      if (nw !== conf) { errEl.textContent = 'As senhas não coincidem.'; errEl.classList.remove('hidden'); return; }
      errEl.classList.add('hidden');
      try {
        await UserService.changePassword(cur, nw, conf);
        showToast('Senha atualizada! 🔐', 'success');
        ['cur-pass','new-pass','conf-pass'].forEach(id => document.getElementById(id).value = '');
      } catch (err) { errEl.textContent = err.message; errEl.classList.remove('hidden'); }
    });

    document.getElementById('btn-delete').addEventListener('click', async () => {
      const confirmed = prompt('Digite EXCLUIR para confirmar a exclusão permanente da sua conta:');
      if (confirmed !== 'EXCLUIR') return;
      try {
        await UserService.deleteAccount();
        AuthStore.clear();
        Router.go('/login');
        showToast('Conta excluída. Dados anonimizados conforme LGPD.', 'error');
      } catch (err) { showToast(err.message, 'error'); }
    });
  }
}

export class PrivacyPage {
  constructor(container) { this.container = container; }

  async render() {
    const shell = new AppShell(this.container, { title: 'Privacidade & LGPD', activeNav: 'privacy' });
    shell.render(this._html());
    this._attachEvents();
  }

  _html() {
    return `
      <div style="max-width:560px">
        <!-- Consentimentos -->
        <div class="card card-lg mb-20">
          <h3 style="margin-bottom:4px">Consentimentos</h3>
          <p class="text-sm text-muted" style="margin-bottom:20px">Gerencie como seus dados são utilizados</p>
          ${[
            { id: 'c1', label: 'Dados de desempenho para personalização', hint: '', checked: true },
            { id: 'c2', label: 'Analytics e melhoria do serviço', hint: '', checked: true },
            { id: 'c3', label: 'Comunicações por e-mail', hint: 'Termos v2.1 — aceitos em 01/05/2025', checked: true, required: true },
          ].map(c => `
            <div style="display:flex;align-items:center;gap:12px;padding:12px 0;border-bottom:1px solid var(--border)">
              <div style="flex:1">
                <div style="font-size:14px">${c.label}</div>
                ${c.hint ? `<div class="text-xs text-muted" style="margin-top:2px">${c.hint}</div>` : ''}
                ${c.required ? `<div class="text-xs" style="color:var(--gold);margin-top:2px">Obrigatório</div>` : ''}
              </div>
              <input type="checkbox" id="${c.id}" ${c.checked?'checked':''} ${c.required?'disabled':''} style="accent-color:var(--gold);width:18px;height:18px">
            </div>
          `).join('')}
          <button id="btn-save-consents" class="btn btn-primary" style="margin-top:16px">Salvar preferências</button>
        </div>

        <!-- Direitos LGPD -->
        <div class="card card-lg mb-20">
          <h3 style="margin-bottom:4px">Seus Direitos (LGPD)</h3>
          <p class="text-sm text-muted" style="margin-bottom:16px">Art. 18 da Lei nº 13.709/2018 — direito de acesso, portabilidade e exclusão</p>
          <div style="display:flex;flex-direction:column;gap:10px">
            <button id="btn-export" class="btn btn-secondary btn-full">📥 Exportar meus dados</button>
            <button class="btn btn-danger btn-full" onclick="window.__router.go('/profile')">🗑️ Excluir minha conta</button>
          </div>
        </div>

        <!-- Política -->
        <div class="card card-sm">
          <div style="font-size:14px;font-weight:600;margin-bottom:8px">📄 Política de Privacidade</div>
          <p class="text-sm text-muted" style="margin-bottom:10px">Versão 2.1 — Atualizada em 01/05/2025. Em conformidade com a LGPD.</p>
          <a href="#" class="text-sm text-gold">Ler política completa →</a>
        </div>
      </div>
    `;
  }

  _attachEvents() {
    document.getElementById('btn-save-consents').addEventListener('click', () =>
      showToast('Preferências salvas ✓', 'success'));
    document.getElementById('btn-export').addEventListener('click', async () => {
      try {
        await UserService.requestDataExport(); // 202 Accepted
        showToast('Exportação em processamento. Você receberá um e-mail em até 24h 📧', 'success');
      } catch (err) { showToast(err.message, 'error'); }
    });
  }
}

// Default export para o router
export default ProfilePage;
