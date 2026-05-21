/**
 * FE-14 — Desempenho (UC11)
 * Endpoint: GET /users/me/performance?period=7d|30d|all
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { UserService } from '../../services/api.js';

export default class PerformancePage {
  constructor(container) { this.container = container; this.period = '30d'; }

  async render() {
    const shell = new AppShell(this.container, { title: 'Desempenho', activeNav: 'performance' });
    shell.render('<p class="text-muted">Carregando desempenho…</p>');
    await this._loadAndRender();
  }

  async _loadAndRender() {
    try {
      const data = await UserService.getPerformance(this.period);
      document.getElementById('page-content').innerHTML = this._html(data);
      this._attachEvents();
    } catch (err) {
      document.getElementById('page-content').innerHTML = `<p style="color:var(--red)">${err.message}</p>`;
    }
  }

  _html(data) {
    const g = data.global || {};
    const byArea = data.byArea || [];
    const trend = data.trend || [];

    // Gráfico de linha simples via SVG
    const maxPct = 100;
    const pts = trend.map((t, i) => {
      const x = 30 + (i / Math.max(trend.length - 1, 1)) * 540;
      const y = 110 - (t.accuracyPct / maxPct) * 100;
      return `${x},${y}`;
    }).join(' ');

    return `
      <!-- Filtro de período -->
      <div style="display:flex;gap:8px;margin-bottom:24px">
        ${['7d','30d','all'].map(p => `
          <button data-period="${p}" class="btn btn-sm ${this.period===p?'btn-primary':'btn-secondary'}">
            ${p==='7d'?'7 dias':p==='30d'?'30 dias':'Todo o período'}
          </button>
        `).join('')}
      </div>

      <!-- Estatísticas globais -->
      <div class="stats-grid mb-24">
        <div class="stat-card" style="--stat-color:var(--gold)">
          <div class="stat-label">Total Respondidas</div>
          <div class="stat-value">${(g.totalAnswered ?? 0).toLocaleString('pt-BR')}</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--green)">
          <div class="stat-label">Taxa Global</div>
          <div class="stat-value">${Math.round(g.accuracyPct ?? 0)}%</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--blue)">
          <div class="stat-label">Sessões</div>
          <div class="stat-value">${g.totalSessions ?? 0}</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--purple)">
          <div class="stat-label">Tempo médio/questão</div>
          <div class="stat-value">${g.avgTimePerQuestion ?? 0}s</div>
        </div>
      </div>

      <!-- Gráfico de evolução -->
      ${trend.length > 1 ? `
        <div class="card mb-24">
          <div class="section-title mb-16">Evolução de Acertos (%)</div>
          <div style="position:relative;height:130px">
            <svg viewBox="0 0 600 120" style="width:100%;height:100%;overflow:visible">
              ${[0,25,50,75,100].map(v => `
                <line x1="30" y1="${110-(v/100*100)}" x2="570" y2="${110-(v/100*100)}"
                  stroke="rgba(255,255,255,0.05)" stroke-width="1"/>
                <text x="22" y="${114-(v/100*100)}" font-size="9" fill="#5e5c6b" text-anchor="end">${v}%</text>
              `).join('')}
              <polyline points="${pts}" fill="none" stroke="var(--gold)" stroke-width="2" stroke-linejoin="round"/>
              ${trend.map((t, i) => {
                const x = 30 + (i / Math.max(trend.length-1,1)) * 540;
                const y = 110 - (t.accuracyPct / maxPct) * 100;
                return `<circle cx="${x}" cy="${y}" r="4" fill="var(--gold)"/>`;
              }).join('')}
            </svg>
          </div>
        </div>
      ` : ''}

      <!-- Desempenho por área -->
      <div class="card">
        <div class="section-title mb-16">Por Área do Direito</div>
        ${byArea.length ? byArea.map(a => {
          const pct = Math.round(a.accuracyPct);
          const c = pct>=70?'var(--green)':pct>=55?'var(--gold)':'var(--red)';
          return `
            <div class="perf-row" style="cursor:pointer" onclick="window.__router.go('/performance/${a.areaId}')">
              <div class="perf-row-header">
                <div class="perf-row-name">${a.areaName}</div>
                <div style="display:flex;align-items:center;gap:12px">
                  <span class="text-xs text-muted">${a.totalCorrect}/${a.totalAnswered}</span>
                  <div class="perf-row-pct" style="color:${c}">${pct}%</div>
                  <span class="text-xs text-muted">→</span>
                </div>
              </div>
              <div class="perf-bar-track">
                <div class="perf-bar-fill" style="width:${pct}%;background:${c}"></div>
              </div>
            </div>
          `;
        }).join('') : '<p class="text-muted text-sm">Nenhum dado disponível. Comece a praticar!</p>'}
      </div>
    `;
  }

  _attachEvents() {
    document.querySelectorAll('[data-period]').forEach(btn => {
      btn.addEventListener('click', () => {
        this.period = btn.dataset.period;
        this._loadAndRender();
      });
    });
  }
}
