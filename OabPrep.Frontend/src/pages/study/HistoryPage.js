/**
 * FE — Histórico de Sessões
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { SessionService, UserService } from '../../services/api.js';
import Router from '../../utils/router.js';

export default class HistoryPage {
  constructor(container) {
    this.container = container;
  }

  async render() {
    const shell = new AppShell(this.container, { title: 'Histórico', activeNav: 'history' });
    shell.render('<p class="text-muted">Carregando histórico…</p>');

    try {
      const perf = await UserService.getPerformance('all');
      document.getElementById('page-content').innerHTML = this._html(perf);
      this._attachEvents();
    } catch (err) {
      document.getElementById('page-content').innerHTML = `
        <p style="color:var(--red)">Erro ao carregar histórico: ${err.message}</p>
      `;
    }
  }

  _html(perf) {
    const global = perf?.global || {};
    const byArea = perf?.byArea || [];
    const trend  = perf?.trend  || [];

    return `
      <!-- Resumo geral -->
      <div class="stats-grid mb-24">
        <div class="stat-card" style="--stat-color:var(--gold)">
          <div class="stat-label">Total Respondidas</div>
          <div class="stat-value">${(global.totalAnswered ?? 0).toLocaleString('pt-BR')}</div>
          <div class="stat-sub">Em todas as sessões</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--green)">
          <div class="stat-label">Taxa de Acerto</div>
          <div class="stat-value">${Math.round(global.accuracyPct ?? 0)}%</div>
          <div class="stat-sub">Média geral</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--blue)">
          <div class="stat-label">Sessões Realizadas</div>
          <div class="stat-value">${global.totalSessions ?? 0}</div>
          <div class="stat-sub">Total histórico</div>
        </div>
        <div class="stat-card" style="--stat-color:var(--purple)">
          <div class="stat-label">Tempo Médio/Questão</div>
          <div class="stat-value">${global.avgTimePerQuestion ?? '—'}s</div>
          <div class="stat-sub">Por resposta</div>
        </div>
      </div>

      <div class="grid-2 mb-24">
        <!-- Desempenho por área -->
        <div class="card card-lg">
          <div class="section-title mb-16">Desempenho por Área</div>
          ${byArea.length ? byArea.map(a => {
            const pct = Math.round(a.accuracyPct ?? 0);
            const color = pct >= 70 ? 'var(--green)' : pct >= 55 ? 'var(--gold)' : 'var(--red)';
            const name = a.areaName ?? a.lawAreaName ?? 'Área';
            return `
              <div class="perf-row" onclick="window.__router.go('/performance')" style="cursor:pointer">
                <div class="perf-row-header">
                  <div class="perf-row-name">${name}</div>
                  <div class="perf-row-pct" style="color:${color}">${pct}%</div>
                </div>
                <div class="perf-bar-track">
                  <div class="perf-bar-fill" style="width:${pct}%;background:${color}"></div>
                </div>
                <div style="font-size:11px;color:var(--text3);margin-top:3px">${a.totalAnswered ?? 0} questões respondidas</div>
              </div>
            `;
          }).join('') : '<p class="text-muted text-sm">Nenhuma área estudada ainda.</p>'}
        </div>

        <!-- Tendência recente -->
        <div class="card card-lg">
          <div class="section-title mb-16">Atividade Recente</div>
          ${trend.length ? trend.slice(-10).reverse().map(t => {
            const pct = Math.round(t.accuracyPct ?? 0);
            const color = pct >= 70 ? 'var(--green)' : pct >= 55 ? 'var(--gold)' : 'var(--red)';
            const date = new Date(t.date).toLocaleDateString('pt-BR', { day:'2-digit', month:'short' });
            return `
              <div style="display:flex;align-items:center;gap:12px;margin-bottom:12px">
                <div style="font-size:12px;color:var(--text3);min-width:50px">${date}</div>
                <div style="flex:1;height:6px;background:var(--bg3);border-radius:3px;overflow:hidden">
                  <div style="height:100%;width:${pct}%;background:${color};border-radius:3px;transition:width .8s"></div>
                </div>
                <div style="font-size:12px;font-weight:600;color:${color};min-width:36px;text-align:right">${pct}%</div>
                <div style="font-size:11px;color:var(--text3);min-width:30px">${t.questionsAnswered ?? 0}q</div>
              </div>
            `;
          }).join('') : '<p class="text-muted text-sm">Nenhuma atividade recente.</p>'}

          <button class="btn btn-secondary btn-full" style="margin-top:16px"
            onclick="window.__router.go('/select-area')">
            🚀 Iniciar nova sessão
          </button>
        </div>
      </div>

      <!-- CTA -->
      <div class="card" style="text-align:center;padding:32px">
        <div style="font-size:36px;margin-bottom:12px">📊</div>
        <h3 style="margin-bottom:8px">Quer ver mais detalhes?</h3>
        <p class="text-muted text-sm" style="margin-bottom:20px">
          Acesse a tela de desempenho completo para ver gráficos e análises detalhadas por área.
        </p>
        <button class="btn btn-primary btn-lg" onclick="window.__router.go('/performance')">
          Ver Desempenho Completo →
        </button>
      </div>
    `;
  }

  _attachEvents() {
    // eventos adicionais se necessário
  }
}