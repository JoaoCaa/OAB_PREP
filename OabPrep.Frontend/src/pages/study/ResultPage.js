/**
 * FE-12 — Resultado da Sessão (UC10)
 * Endpoint: POST /sessions/:id/finish  (chamado ao entrar na tela)
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { SessionService } from '../../services/api.js';
import Router from '../../utils/router.js';

export default class ResultPage {
  constructor(container, params) {
    this.container = container;
    this.sessionId = params.id;
  }

  async render() {
    const shell = new AppShell(this.container, { title: 'Resultado da Sessão' });
    shell.render('<p class="text-muted">Calculando resultado…</p>');
    try {
      const r = await SessionService.finish(this.sessionId);
      document.getElementById('page-content').innerHTML = this._html(r);
    } catch (err) {
      // 409 = sessão já finalizada, tenta buscar os dados mesmo assim
      if (err.status === 409) {
        try {
          const session = await SessionService.get(this.sessionId);
          document.getElementById('page-content').innerHTML = this._htmlFromSession(session);
        } catch (e) {
          document.getElementById('page-content').innerHTML = `<p style="color:var(--red)">Erro: ${e.message}</p>`;
        }
      } else {
        document.getElementById('page-content').innerHTML = `<p style="color:var(--red)">Erro: ${err.message}</p>`;
      }
    }
  }

  _htmlFromSession(session) {
  const correct = session.correctAnswers ?? 0;
  const total = session.totalQuestions ?? 0;
  const pct = total > 0 ? Math.round((correct / total) * 100) : 0;
  return this._html({
    accuracyPct: pct,
    totalQuestions: total,
    correctAnswers: correct,
    avgTimePerQuestion: null,
    byArea: [],
    weakAreas: []
  });
}

  _html(r) {
    const pct = r.accuracyPct > 1 ? Math.round(r.accuracyPct) : Math.round(r.accuracyPct * 100);
    const color = pct >= 75 ? 'var(--green)' : pct >= 60 ? 'var(--gold)' : 'var(--red)';
    const radius = 54, circ = 2 * Math.PI * radius;
    const fill = circ - (circ * pct / 100);

    return `
      <div style="max-width:680px;margin:0 auto;text-align:center">
        <div style="font-size:36px;margin-bottom:8px">${pct>=80?'🏆':pct>=60?'⭐':'📚'}</div>
        <h1 style="margin-bottom:4px">${pct>=80?'Excelente!':pct>=60?'Bom resultado!':'Continue praticando!'}</h1>
        <p class="text-muted" style="margin-bottom:32px">Sessão concluída com sucesso</p>

        <!-- Círculo de resultado -->
        <div style="position:relative;width:160px;height:160px;margin:0 auto 28px">
          <svg viewBox="0 0 120 120" width="160" height="160" style="transform:rotate(-90deg)">
            <circle cx="60" cy="60" r="${radius}" fill="none" stroke="var(--bg3)" stroke-width="10"/>
            <circle cx="60" cy="60" r="${radius}" fill="none" stroke="${color}"
              stroke-width="10" stroke-dasharray="${circ}" stroke-dashoffset="${fill}"
              stroke-linecap="round" style="transition:stroke-dashoffset 1s ease"/>
          </svg>
          <div style="position:absolute;inset:0;display:flex;flex-direction:column;align-items:center;justify-content:center">
            <div style="font-size:34px;font-weight:800;color:${color}">${pct}%</div>
            <div class="text-xs text-muted">acertos</div>
          </div>
        </div>

        <!-- Stats -->
        <div class="stats-grid mb-24" style="grid-template-columns:repeat(3,1fr)">
          <div class="stat-card" style="--stat-color:var(--blue);text-align:center">
            <div class="stat-label">Questões</div>
            <div class="stat-value" style="font-size:22px">${r.totalQuestions}</div>
          </div>
          <div class="stat-card" style="--stat-color:var(--green);text-align:center">
            <div class="stat-label">Corretas</div>
            <div class="stat-value" style="font-size:22px;color:var(--green)">${r.correctAnswers}</div>
          </div>
          <div class="stat-card" style="--stat-color:var(--purple);text-align:center">
            <div class="stat-label">Tempo médio</div>
            <div class="stat-value" style="font-size:22px">${r.avgTimePerQuestion ?? '—'}s</div>
          </div>
        </div>

        <!-- Por área -->
        ${r.byArea?.length ? `
          <div class="card mb-20" style="text-align:left">
            <div class="section-title mb-16">Resultado por Área</div>
            ${r.byArea.map(a => {
              const ap = Math.round(a.accuracyPct);
              const ac = ap>=70?'var(--green)':ap>=55?'var(--gold)':'var(--red)';
              return `
                <div class="perf-row">
                  <div class="perf-row-header">
                    <div class="perf-row-name">${a.areaName}</div>
                    <div class="perf-row-pct" style="color:${ac}">${ap}%</div>
                  </div>
                  <div class="perf-bar-track">
                    <div class="perf-bar-fill" style="width:${ap}%;background:${ac}"></div>
                  </div>
                </div>
              `;
            }).join('')}
          </div>
        ` : ''}

        <!-- Áreas fracas -->
        ${r.weakAreas?.length ? `
          <div class="card mb-20" style="text-align:left;border-color:rgba(224,92,92,.2)">
            <div style="font-size:14px;font-weight:600;color:var(--red);margin-bottom:8px">⚠️ Reforce estas áreas</div>
            <div style="display:flex;gap:8px;flex-wrap:wrap">
              ${r.weakAreas.map(a => `<span class="badge badge-red">${a}</span>`).join('')}
            </div>
          </div>
        ` : ''}

        <!-- Ações -->
        <div style="display:flex;gap:12px;flex-wrap:wrap;justify-content:center">
          <button class="btn btn-primary btn-lg" onclick="window.__router.go('/select-area')">🔄 Praticar Novamente</button>
          <button class="btn btn-secondary btn-lg" onclick="window.__router.go('/performance')">📊 Ver Desempenho</button>
        </div>
      </div>
    `;
  }
}
