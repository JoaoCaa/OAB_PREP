/**
 * FE-16/17/18 — Painel Admin
 * Questões · Usuários · Relatórios
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { AdminQuestionService, AdminUserService, AdminReportService, LawAreaService } from '../../services/api.js';
import { showToast } from '../../components/ui/Toast.js';
// ⚠️  Admin Question IDs são int; Admin User IDs são Guid

// ── Questões ───────────────────────────────────────────────────────

export class QuestionsPage {
  constructor(container) { this.container = container; this.questions = []; this.areas = []; }

  async render() {
    const shell = new AppShell(this.container, { title: 'Admin — Questões', activeNav: 'admin-q' });
    shell.render('<p class="text-muted">Carregando questões…</p>');
    const [qs, areas] = await Promise.allSettled([
      AdminQuestionService.list({ page: 1, size: 20 }),
      LawAreaService.list(),
    ]);
    this.questions = qs.status === 'fulfilled' ? qs.value.items ?? qs.value : [];
    this.areas     = areas.status === 'fulfilled' ? areas.value : [];
    document.getElementById('page-content').innerHTML = this._html();
    this._attachEvents();
  }

  _html() {
    return `
      <div style="display:flex;align-items:center;gap:12px;margin-bottom:20px;flex-wrap:wrap">
        <div style="flex:1;min-width:200px;display:flex;align-items:center;gap:8px;background:var(--bg3);border:1px solid var(--border);border-radius:8px;padding:0 14px">
          <span style="color:var(--text3)">🔍</span>
          <input id="search" type="text" placeholder="Buscar questões…" style="flex:1;background:none;border:none;outline:none;padding:10px 0;font-size:14px;color:var(--text)">
        </div>
        <select id="filter-area" class="form-input" style="width:180px">
          <option value="">Todas as áreas</option>
          ${this.areas.map(a => `<option value="${a.id}">${a.name}</option>`).join('')}
        </select>
        <select id="filter-diff" class="form-input" style="width:140px">
          <option value="">Dificuldade</option>
          <option value="Easy">Fácil</option>
          <option value="Medium">Média</option>
          <option value="Hard">Difícil</option>
        </select>
        <button id="btn-new" class="btn btn-primary">+ Nova Questão</button>
        <button id="btn-import" class="btn btn-secondary">📤 Importar JSON</button>
        <input id="import-file" type="file" accept=".json" style="display:none">
      </div>

      <div class="card" style="padding:0;overflow:hidden">
        <div style="overflow-x:auto">
          <table style="width:100%;border-collapse:collapse">
            <thead>
              <tr>
                ${['ID','Área','Enunciado','Ano','Dificuldade','Status','Ações'].map(h =>
                  `<th style="padding:10px 14px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:.8px;color:var(--text3);border-bottom:1px solid var(--border)">${h}</th>`
                ).join('')}
              </tr>
            </thead>
            <tbody id="questions-tbody">
              ${this.questions.length ? this.questions.map(q => this._row(q)).join('') :
                `<tr><td colspan="7" style="padding:24px;text-align:center;color:var(--text2)">Nenhuma questão encontrada</td></tr>`}
            </tbody>
          </table>
        </div>
      </div>

      <!-- Modal criar/editar questão -->
      <div id="q-modal" class="modal-overlay hidden" onclick="if(event.target===this)this.classList.add('hidden')">
        <div class="modal" style="max-width:640px;max-height:90vh;overflow-y:auto">
          <button style="position:absolute;top:16px;right:16px;background:var(--bg3);border:1px solid var(--border);border-radius:6px;width:30px;height:30px;color:var(--text2);cursor:pointer"
            onclick="document.getElementById('q-modal').classList.add('hidden')">✕</button>
          <h3 id="modal-title" style="margin-bottom:4px">Nova Questão</h3>
          <p class="text-sm text-muted" style="margin-bottom:20px">Preencha todos os campos obrigatórios</p>
          <div class="form-group">
            <label class="form-label">Área do Direito</label>
            <select class="form-input" id="q-area">
              ${this.areas.map(a => `<option value="${a.id}">${a.name}</option>`).join('')}
            </select>
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr 1fr;gap:12px;margin-bottom:18px">
            <div>
              <label class="form-label">Ano</label>
              <input class="form-input" id="q-year" type="number" placeholder="2023">
            </div>
            <div>
              <label class="form-label">Edição</label>
              <input class="form-input" id="q-edition" placeholder="XXXV">
            </div>
            <div>
              <label class="form-label">Dificuldade</label>
              <select class="form-input" id="q-diff">
                <option value="Easy">Fácil</option>
                <option value="Medium" selected>Média</option>
                <option value="Hard">Difícil</option>
              </select>
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Enunciado</label>
            <textarea class="form-input" id="q-statement" rows="4" placeholder="Texto da questão…"></textarea>
          </div>
          <div class="form-group">
            <label class="form-label">Explicação geral</label>
            <textarea class="form-input" id="q-explanation" rows="3" placeholder="Explicação do gabarito…"></textarea>
          </div>
          <div class="section-title mt-16 mb-8">Alternativas (exatamente 1 correta)</div>
          ${['A','B','C','D','E'].map(l => `
            <div style="display:flex;align-items:flex-start;gap:10px;margin-bottom:10px">
              <input type="radio" name="correct-alt" value="${l}" style="accent-color:var(--gold);margin-top:13px;flex-shrink:0">
              <div style="flex:1">
                <div style="font-size:11px;color:var(--text3);margin-bottom:3px">Alternativa ${l}</div>
                <input class="form-input" id="alt-${l}" placeholder="Texto da alternativa ${l}…">
              </div>
            </div>
          `).join('')}
          <div style="margin-top:20px;display:flex;gap:10px;justify-content:flex-end">
            <button class="btn btn-secondary" onclick="document.getElementById('q-modal').classList.add('hidden')">Cancelar</button>
            <button id="btn-save-q" class="btn btn-primary">Salvar questão</button>
          </div>
        </div>
      </div>
    `;
  }

  _row(q) {
    const diffMap = { Easy:'badge-green', Medium:'badge-blue', Hard:'badge-red' };
    const diffLabel = { Easy:'Fácil', Medium:'Média', Hard:'Difícil' };
    return `
      <tr style="cursor:pointer;transition:background .1s" onmouseover="this.style.background='var(--bg3)'" onmouseout="this.style.background=''">
        <td style="padding:12px 14px;font-size:11px;color:var(--text3)">${q.id?.slice?.(0,8) ?? '—'}…</td>
        <td style="padding:12px 14px"><span class="badge badge-gold">${q.lawAreaName || '—'}</span></td>
        <td style="padding:12px 14px;max-width:280px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:13px;color:var(--text2)">${q.statement?.substring?.(0,80) ?? '—'}…</td>
        <td style="padding:12px 14px;font-size:13px">${q.year ?? '—'}</td>
        <td style="padding:12px 14px"><span class="badge ${diffMap[q.difficulty]??'badge-gray'}">${diffLabel[q.difficulty]??q.difficulty??'—'}</span></td>
        <td style="padding:12px 14px"><span style="width:6px;height:6px;border-radius:50%;background:${q.isActive?'var(--green)':'var(--red)'};display:inline-block;margin-right:6px"></span>${q.isActive?'Ativa':'Inativa'}</td>
        <td style="padding:12px 14px;display:flex;gap:4px">
          <button class="btn btn-ghost btn-sm" onclick="window.__adminQ.edit('${q.id}')">✏️</button>
          <button class="btn btn-ghost btn-sm" onclick="window.__adminQ.remove('${q.id}')">🗑️</button>
        </td>
      </tr>
    `;
  }

  _attachEvents() {
    document.getElementById('btn-new').addEventListener('click', () => {
      document.getElementById('q-modal').classList.remove('hidden');
    });
    document.getElementById('btn-import').addEventListener('click', () =>
      document.getElementById('import-file').click());
    document.getElementById('import-file').addEventListener('change', async e => {
      const file = e.target.files[0];
      if (!file) return;
      try {
        const res = await AdminQuestionService.importBatch(file);
        showToast(`✅ ${res.imported} importadas. ${res.failed} falharam.`, res.failed ? 'warning' : 'success');
      } catch (err) { showToast(err.message, 'error'); }
    });
    document.getElementById('btn-save-q').addEventListener('click', async () => {
      const correct = document.querySelector('input[name="correct-alt"]:checked')?.value;
      if (!correct) { showToast('Selecione a alternativa correta', 'error'); return; }
      const data = {
        lawAreaId: Number(document.getElementById('q-area').value),
        year: Number(document.getElementById('q-year').value),
        examEdition: document.getElementById('q-edition').value,
        difficulty: document.getElementById('q-diff').value,
        statement: document.getElementById('q-statement').value,
        explanation: document.getElementById('q-explanation').value,
        alternatives: ['A','B','C','D','E'].map(l => ({
          letter: l,
          text: document.getElementById(`alt-${l}`).value,
          isCorrect: l === correct,
          explanation: '',
        })),
      };
      try {
        await AdminQuestionService.create(data);
        showToast('Questão criada! ✓', 'success');
        document.getElementById('q-modal').classList.add('hidden');
      } catch (err) { showToast(err.message, 'error'); }
    });

    window.__adminQ = {
      edit: (id) => showToast(`Editando questão ${id}`, 'warning'),
      remove: async (id) => {
        if (!confirm('Desativar esta questão?')) return;
        try { await AdminQuestionService.remove(id); showToast('Questão desativada', 'success'); }
        catch (err) { showToast(err.message, 'error'); }
      },
    };
  }
}

// ── Usuários ───────────────────────────────────────────────────────

export class UsersPage {
  constructor(container) { this.container = container; }

  async render() {
    const shell = new AppShell(this.container, { title: 'Admin — Usuários', activeNav: 'admin-u' });
    shell.render('<p class="text-muted">Carregando usuários…</p>');
    try {
      const data = await AdminUserService.list({ page: 1, size: 20 });
      const users = data.items ?? data;
      document.getElementById('page-content').innerHTML = this._html(users);
      this._attachEvents(users);
    } catch (err) {
      document.getElementById('page-content').innerHTML = `<p style="color:var(--red)">${err.message}</p>`;
    }
  }

  _html(users) {
    return `
      <div style="display:flex;gap:12px;margin-bottom:20px">
        <div style="flex:1;display:flex;align-items:center;gap:8px;background:var(--bg3);border:1px solid var(--border);border-radius:8px;padding:0 14px">
          <span style="color:var(--text3)">🔍</span>
          <input id="user-search" type="text" placeholder="Buscar por nome ou e-mail…" style="flex:1;background:none;border:none;outline:none;padding:10px 0;font-size:14px;color:var(--text)">
        </div>
      </div>
      <div class="card" style="padding:0;overflow:hidden">
        <div style="overflow-x:auto">
          <table style="width:100%;border-collapse:collapse">
            <thead>
              <tr>
                ${['Usuário','Role','Status','Sessões','Questões','Último acesso','Ações'].map(h =>
                  `<th style="padding:10px 14px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:.8px;color:var(--text3);border-bottom:1px solid var(--border)">${h}</th>`
                ).join('')}
              </tr>
            </thead>
            <tbody>
              ${users.map(u => {
                const initials = (u.name||'?').split(' ').map(n=>n[0]).slice(0,2).join('').toUpperCase();
                return `
                  <tr style="transition:background .1s" onmouseover="this.style.background='var(--bg3)'" onmouseout="this.style.background=''">
                    <td style="padding:12px 14px">
                      <div style="display:flex;align-items:center;gap:10px">
                        <div style="width:32px;height:32px;border-radius:50%;background:linear-gradient(135deg,var(--purple),var(--blue));display:flex;align-items:center;justify-content:center;font-size:12px;font-weight:600;flex-shrink:0">${initials}</div>
                        <div>
                          <div style="font-size:13px;font-weight:500">${u.name}</div>
                          <div style="font-size:11px;color:var(--text3)">${u.email}</div>
                        </div>
                      </div>
                    </td>
                    <td style="padding:12px 14px"><span class="badge ${u.role==='Admin'?'badge-purple':'badge-blue'}">${u.role}</span></td>
                    <td style="padding:12px 14px"><span style="width:6px;height:6px;border-radius:50%;background:${u.isActive?'var(--green)':'var(--red)'};display:inline-block;margin-right:6px"></span>${u.isActive?'Ativo':'Bloqueado'}</td>
                    <td style="padding:12px 14px;font-size:13px">${u.totalSessions??0}</td>
                    <td style="padding:12px 14px;font-size:13px">${(u.totalAnswered??0).toLocaleString()}</td>
                    <td style="padding:12px 14px;font-size:13px;color:var(--text2)">${u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleDateString('pt-BR') : '—'}</td>
                    <td style="padding:12px 14px;display:flex;gap:4px">
                      <button class="btn btn-ghost btn-sm" onclick="window.__adminU.toggle('${u.id}',${u.isActive})">${u.isActive?'🔒':'🔓'}</button>
                      <button class="btn btn-ghost btn-sm" onclick="window.__adminU.role('${u.id}','${u.role}')">⚙️</button>
                    </td>
                  </tr>
                `;
              }).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  }

  _attachEvents(users) {
    window.__adminU = {
      toggle: async (id, isActive) => {
        if (!confirm(isActive ? 'Bloquear este usuário?' : 'Desbloquear este usuário?')) return;
        try {
          if (isActive) await AdminUserService.block(id);
          else          await AdminUserService.unblock(id);
          showToast(isActive ? 'Usuário bloqueado' : 'Usuário desbloqueado', isActive ? 'error' : 'success');
        } catch (err) { showToast(err.message, 'error'); }
      },
      role: async (id, current) => {
        const newRole = current === 'Admin' ? 'Student' : 'Admin';
        if (!confirm(`Alterar role para ${newRole}?`)) return;
        try {
          await AdminUserService.setRole(id, newRole);
          showToast(`Role alterado para ${newRole}`, 'success');
        } catch (err) { showToast(err.message, 'error'); }
      },
    };
  }
}

// ── Relatórios ─────────────────────────────────────────────────────

export class ReportsPage {
  constructor(container) { this.container = container; }

  async render() {
    const shell = new AppShell(this.container, { title: 'Admin — Relatórios', activeNav: 'admin-r' });
    shell.render('<p class="text-muted">Carregando relatórios…</p>');
    try {
      const data = await AdminReportService.getSummary();
      document.getElementById('page-content').innerHTML = this._html(data);
    } catch (err) {
      document.getElementById('page-content').innerHTML = `<p style="color:var(--red)">${err.message}</p>`;
    }
  }

  _html(d) {
    const regs = d.registrationsByMonth ?? [];
    const max  = Math.max(...regs.map(r => r.count), 1);
    return `
      <div class="stats-grid mb-24">
        ${[
          { label:'Total Usuários',      value:(d.totalUsers??0).toLocaleString(), color:'var(--blue)' },
          { label:'Ativos (30d)',         value:(d.activeUsersLast30d??0).toLocaleString(), color:'var(--green)' },
          { label:'Total Questões',       value:(d.totalQuestions??0).toLocaleString(), color:'var(--gold)' },
          { label:'Acerto Médio Global',  value:`${Math.round(d.avgAccuracyGlobal??0)}%`, color:'var(--purple)' },
        ].map(s => `
          <div class="stat-card" style="--stat-color:${s.color}">
            <div class="stat-label">${s.label}</div>
            <div class="stat-value">${s.value}</div>
          </div>
        `).join('')}
      </div>

      <div class="grid-2">
        <div class="card">
          <div class="section-title mb-12">Áreas com mais erros</div>
          ${(d.topWeakAreas??[]).map((a,i) => `
            <div style="display:flex;align-items:center;gap:10px;margin-bottom:10px">
              <span style="font-size:16px;color:var(--text3);font-weight:700;min-width:20px">${i+1}</span>
              <span style="flex:1;font-size:13px">${typeof a === 'string' ? a : a.areaName ?? a.name ?? JSON.stringify(a)}</span>
              <span class="badge badge-red">Alta taxa de erro</span>
            </div>
          `).join('')}
        </div>

        <div class="card">
          <div class="section-title mb-12">Novos usuários por mês</div>
          ${regs.map(r => `
            <div style="display:flex;align-items:center;gap:10px;margin-bottom:8px">
              <span style="font-size:12px;color:var(--text2);min-width:50px">${r.month}</span>
              <div style="flex:1;height:6px;background:var(--bg3);border-radius:3px;overflow:hidden">
                <div style="height:100%;width:${r.count/max*100}%;background:var(--blue);border-radius:3px"></div>
              </div>
              <span style="font-size:12px;font-weight:600;min-width:32px;text-align:right">${r.count}</span>
            </div>
          `).join('')}
        </div>
      </div>
    `;
  }
}

// Default exports para o router
export default QuestionsPage;
