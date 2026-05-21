/**
 * FE-07 — Seleção de Área (UC05)
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { LawAreaService, SessionService } from '../../services/api.js';
import Router from '../../utils/router.js';
import { showToast } from '../../components/ui/Toast.js';

export default class SelectAreaPage {
  constructor(container) {
    this.container = container;
    this.areas      = [];
    this.selected   = [];   // IDs selecionados
    this.count      = 20;
    this.exclude    = false;
  }

  async render() {
    const shell = new AppShell(this.container, { title: 'Iniciar Sessão', activeNav: 'practice' });
    shell.render('<p class="text-muted">Carregando áreas…</p>');
    try {
      this.areas = await LawAreaService.list();
    } catch {
      this.areas = [];
    }
    this._renderContent();
  }

  _renderContent() {
    const COUNTS = [5, 10, 20, 30, 50];
    document.getElementById('page-content').innerHTML = `
      <div style="max-width:800px">

        <div class="card card-lg mb-20">
          <h3 style="margin-bottom:4px">Escolha as Áreas</h3>
          <p class="text-sm text-muted" style="margin-bottom:16px">Selecione uma ou mais áreas do Direito</p>
          <div style="display:flex;gap:8px;flex-wrap:wrap;margin-bottom:16px">
            <button id="btn-all" class="btn btn-sm ${this.selected.length===0?'btn-primary':'btn-secondary'}">
              Todas as áreas
            </button>
          </div>
          <div style="display:grid;grid-template-columns:repeat(auto-fill,minmax(180px,1fr));gap:12px" id="areas-grid">
            ${this.areas.map(a => this._areaCard(a)).join('')}
          </div>
        </div>

        <div class="card card-lg mb-20">
          <h3 style="margin-bottom:16px">Configurações</h3>
          <div style="margin-bottom:18px">
            <div class="form-label">Número de questões</div>
            <div style="display:flex;gap:8px;flex-wrap:wrap">
              ${COUNTS.map(n => `
                <div data-count="${n}" class="count-opt"
                  style="padding:8px 16px;border-radius:8px;font-size:14px;font-weight:500;cursor:pointer;border:1.5px solid ${this.count===n?'var(--gold)':'var(--border)'};background:${this.count===n?'var(--gold-dim)':'var(--bg3)'};color:${this.count===n?'var(--gold)':'var(--text2)'};transition:all .15s">
                  ${n}
                </div>
              `).join('')}
            </div>
          </div>
          <label style="display:flex;align-items:center;gap:10px;cursor:pointer">
            <input type="checkbox" id="exclude" ${this.exclude?'checked':''} style="accent-color:var(--gold)">
            <span class="text-sm text-muted">Excluir questões já respondidas corretamente</span>
          </label>
        </div>

        <div style="display:flex;align-items:center;justify-content:space-between">
          <p class="text-sm text-muted">
            ${this.selected.length===0?'Todas as áreas':this.selected.length+' área(s)'} — ${this.count} questões
          </p>
          <button id="btn-start" class="btn btn-primary btn-lg">🚀 Iniciar Sessão</button>
        </div>
      </div>
    `;
    this._attachEvents();
  }

  _areaCard(a) {
    const sel = this.selected.includes(a.id);
    const pct = a.userAccuracyPct ?? 0;
    return `
      <div data-area="${a.id}" class="area-card"
        style="background:var(--bg2);border:1.5px solid ${sel?'var(--gold)':'var(--border)'};border-radius:12px;padding:16px;cursor:pointer;transition:all .2s;${sel?'background:var(--gold-dim)':''}">
        <div style="font-size:26px;margin-bottom:8px">${a.iconUrl||'⚖️'}</div>
        <div style="font-size:13px;font-weight:600;margin-bottom:3px">${a.name}</div>
        <div class="text-xs text-muted">${a.questionCount} questões</div>
        <div style="height:3px;background:var(--bg3);border-radius:2px;margin-top:10px;overflow:hidden">
          <div style="height:100%;width:${pct}%;background:${pct>=70?'var(--green)':pct>=55?'var(--gold)':'var(--red)'};border-radius:2px"></div>
        </div>
        <div style="font-size:11px;font-weight:700;margin-top:4px;color:${pct>=70?'var(--green)':pct>=55?'var(--gold)':'var(--red)'}">${Math.round(pct)}%</div>
      </div>
    `;
  }

  _attachEvents() {
    document.getElementById('btn-all').addEventListener('click', () => {
      this.selected = []; this._renderContent();
    });
    document.querySelectorAll('[data-area]').forEach(el => {
      el.addEventListener('click', () => {
        const id = Number(el.dataset.area);
        const idx = this.selected.indexOf(id);
        if (idx >= 0) this.selected.splice(idx, 1);
        else this.selected.push(id);
        this._renderContent();
      });
    });
    document.querySelectorAll('[data-count]').forEach(el => {
      el.addEventListener('click', () => { this.count = Number(el.dataset.count); this._renderContent(); });
    });
    document.getElementById('exclude').addEventListener('change', e => { this.exclude = e.target.checked; });
    document.getElementById('btn-start').addEventListener('click', () => this._start());
  }

  async _start() {
    const btn = document.getElementById('btn-start');
    btn.disabled = true; btn.textContent = 'Criando sessão…';
    try {
      const areas = this.selected.length > 0 ? this.selected : [9];
      const data = await SessionService.create(areas, this.count, this.exclude);
      Router.go(`/session/${data.sessionId}`);
    } catch (err) {
      showToast(err.message, 'error');
      btn.disabled = false; btn.textContent = '🚀 Iniciar Sessão';
    }
  }
}
