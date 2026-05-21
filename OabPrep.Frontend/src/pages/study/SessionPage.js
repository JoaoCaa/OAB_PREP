/**
 * FE-07/08/09/10/11 — Sessão de Estudo
 */

import { AppShell } from '../../components/layout/AppShell.js';
import { SessionService } from '../../services/api.js';
import Router from '../../utils/router.js';
import { showToast } from '../../components/ui/Toast.js';

export default class SessionPage {
  constructor(container, params) {
    this.container = container;
    this.sessionId = parseInt(params.id);
    this.state = {
      session: null,
      questions: [],
      currentIdx: 0,
      answers: {},
      chatMessages: {},
      chatLoading: false,
      pendingAlt: null,
      timerSecs: 0,
    };
    this._timerInterval = null;
  }

  async render() {
    const shell = new AppShell(this.container, { title: 'Sessão de Estudo', activeNav: 'practice' });
    shell.render('<p class="text-muted">Carregando sessão…</p>');

    try {
      const data = await SessionService.get(this.sessionId);
      this.state.session   = data;
      this.state.questions = data.questions;
      // Restaurar respostas já enviadas
      data.questions.forEach(q => {
        if (q.answeredAlternativeId) {
          this.state.answers[q.questionId] = {
            selectedId: q.answeredAlternativeId,
            isCorrect: false, // será atualizado ao renderizar
          };
        }
      });
      this._renderQuestion();
      this._startTimer();
    } catch (err) {
      document.getElementById('page-content').innerHTML = `
        <p style="color:var(--red)">Erro ao carregar sessão: ${err.message}</p>
        <button class="btn btn-secondary" onclick="window.__router.go('/')">← Início</button>
      `;
    }
  }

  _renderQuestion() {
    const content = document.getElementById('page-content');
    if (!content) return;
    const q    = this.state.questions[this.state.currentIdx];
    const ans  = this.state.answers[q.questionId];
    const done = !!ans;
    const total = this.state.questions.length;
    const idx   = this.state.currentIdx;

    content.innerHTML = `
      <div class="progress-track mb-24">
        <div class="progress-fill" style="width:${Math.round((idx / total) * 100)}%"></div>
      </div>

      <div style="display:grid;grid-template-columns:1fr 340px;gap:20px;align-items:start">

        <div>
          <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap;margin-bottom:16px">
            <span class="badge badge-gold">${q.lawAreaName}</span>
            ${q.examEdition ? `<span class="badge badge-gray">OAB ${q.examEdition} (${q.year})</span>` : ''}
            <span style="margin-left:auto;font-size:13px;color:var(--text2)">⏱ <span id="timer-display">00:00</span></span>
            <button id="btn-review" class="btn btn-ghost btn-sm" style="${ans?.isMarkedForReview?'color:var(--gold)':''}">
              ${ans?.isMarkedForReview ? '⭐' : '☆'} Revisão
            </button>
            <button class="btn btn-ghost btn-sm" id="btn-nav">⊞ ${idx+1}/${total}</button>
          </div>

          <p style="font-size:15px;line-height:1.8;margin-bottom:22px">${q.statement}</p>

          <div id="alternatives" style="display:flex;flex-direction:column;gap:10px">
            ${q.alternatives.map(alt => {
              let cls = 'alt-item';
              if (done) {
                cls += ' disabled';
                if (alt.alternativeId === ans.correctAlternativeId) cls += ' correct';
                else if (ans.selectedId === alt.alternativeId) cls += ' wrong';
              }
              return `
                <div class="${cls}" data-alt="${alt.alternativeId}">
                  <div class="alt-letter">${alt.letter}</div>
                  <div class="alt-text">${alt.text}</div>
                  ${done && alt.alternativeId === ans.correctAlternativeId ? '<span style="margin-left:auto;color:var(--green)">✓</span>' : ''}
                  ${done && ans.selectedId===alt.alternativeId && !alt.isCorrect ? '<span style="margin-left:auto;color:var(--red)">✗</span>' : ''}
                </div>
              `;
            }).join('')}
          </div>

          ${!done ? `
            <div style="margin-top:20px">
              <button id="btn-confirm" class="btn btn-primary btn-lg" disabled>Confirmar Resposta</button>
            </div>
          ` : `
            <div id="feedback" style="margin-top:20px">
              <div style="padding:14px 18px;border-radius:12px;margin-bottom:16px;display:flex;align-items:center;gap:10px;font-weight:600;background:${ans.isCorrect?'var(--green-dim)':'var(--red-dim)'};border:1px solid ${ans.isCorrect?'rgba(76,175,125,.2)':'rgba(224,92,92,.2)'};color:${ans.isCorrect?'var(--green)':'var(--red)'}">
                ${ans.isCorrect ? '✅ Resposta correta!' : '❌ Resposta incorreta.'}
              </div>
              ${ans.explanation ? `
                <div style="background:var(--bg3);border-radius:12px;padding:16px;margin-bottom:14px">
                  <div class="section-title mb-8">Explicação</div>
                  <p class="text-sm" style="line-height:1.7;color:var(--text2)">${ans.explanation}</p>
                </div>
              ` : ''}
              ${ans.legalRefs?.length ? `
                <div style="display:flex;flex-wrap:wrap;gap:6px;margin-bottom:16px">
                  ${ans.legalRefs.map(r => `<span class="legal-chip">${r}</span>`).join('')}
                </div>
              ` : ''}
              <div style="display:flex;gap:10px;justify-content:flex-end;margin-top:16px">
                ${idx < total - 1
                  ? `<button id="btn-next" class="btn btn-primary btn-lg">Próxima questão →</button>`
                  : `<button id="btn-finish" class="btn btn-primary btn-lg">Ver resultado 🏆</button>`
                }
              </div>
            </div>
          `}
        </div>

        <div>
          <div class="card" style="padding:0;overflow:hidden">
            <div class="chat-header">
              <span style="font-size:16px">💬</span>
              <span style="font-size:13px;font-weight:600">Assistente Jurídico</span>
              <span class="text-xs text-muted" style="margin-left:auto" id="chat-count">
                ${(this.state.chatMessages[q.questionId]||[]).length}/20
              </span>
            </div>
            ${!done ? `
              <div style="padding:8px 16px;background:rgba(200,168,75,.08);border-bottom:1px solid var(--border);font-size:12px;color:var(--gold)">
                ⚠️ Responda a questão antes de perguntar sobre o gabarito
              </div>
            ` : ''}
            <div class="chat-msgs" id="chat-msgs">
              ${this._renderMessages(q.questionId)}
            </div>
            ${(this.state.chatMessages[q.questionId]||[]).length >= 20 ? `
              <div style="padding:10px;text-align:center;font-size:12px;color:var(--text2);border-top:1px solid var(--border)">
                Limite de 20 mensagens atingido
              </div>
            ` : `
              <div class="chat-input-row">
                <textarea class="chat-textarea" id="chat-input" placeholder="Tire sua dúvida…" rows="1"></textarea>
                <button class="chat-send-btn" id="chat-send">➤</button>
              </div>
            `}
          </div>
        </div>
      </div>

      <div id="qnav-modal" class="modal-overlay hidden" onclick="if(event.target===this)this.classList.add('hidden')">
        <div class="modal">
          <button style="position:absolute;top:16px;right:16px;background:var(--bg3);border:1px solid var(--border);border-radius:6px;width:30px;height:30px;color:var(--text2);cursor:pointer;font-size:14px"
            onclick="document.getElementById('qnav-modal').classList.add('hidden')">✕</button>
          <h3 style="margin-bottom:4px">Navegar Questões</h3>
          <p class="text-sm text-muted" style="margin-bottom:16px">Clique para ir diretamente a qualquer questão</p>
          <div style="display:grid;grid-template-columns:repeat(8,1fr);gap:6px">
            ${this.state.questions.map((q, i) => {
              const a = this.state.answers[q.questionId];
              let bg = 'var(--bg3)', border = 'var(--border)', color = 'var(--text2)';
              if (i === idx) { border = 'var(--gold)'; color = 'var(--gold)'; }
              else if (a) {
                if (a.isCorrect) { bg='var(--green-dim)'; border='var(--green)'; color='var(--green)'; }
                else             { bg='var(--red-dim)';   border='var(--red)';   color='var(--red)';   }
              }
              if (a?.isMarkedForReview) { bg='rgba(200,168,75,.1)'; border='var(--gold)'; color='var(--gold)'; }
              return `
                <div onclick="window.__session.goToQ(${i});document.getElementById('qnav-modal').classList.add('hidden')"
                  style="aspect-ratio:1;border-radius:6px;display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:700;cursor:pointer;background:${bg};border:1.5px solid ${border};color:${color};transition:all .15s">
                  ${i + 1}
                </div>
              `;
            }).join('')}
          </div>
          ${Object.keys(this.state.answers).length === total ? `
            <button class="btn btn-primary btn-full" style="margin-top:16px" id="btn-nav-finish">Finalizar Sessão 🏆</button>
          ` : ''}
        </div>
      </div>
    `;

    this._attachQuestionEvents(q);
    this._updateTimer();
  }

  _renderMessages(qid) {
    const msgs = this.state.chatMessages[qid] || [];
    if (!msgs.length) {
      return `<div style="text-align:center;color:var(--text3);font-size:13px;padding:24px 0;flex:1;display:flex;align-items:center;justify-content:center;margin:auto">
        🤖 Olá! Sou seu assistente jurídico.<br>Tire dúvidas sobre esta questão!
      </div>`;
    }
    return msgs.map(m => `
      <div class="chat-msg ${m.role}">
        <div class="msg-bubble">${m.content.replace(/\n/g, '<br>')}</div>
        <div class="msg-time">${m.time}</div>
      </div>
    `).join('');
  }

  _attachQuestionEvents(q) {
    document.querySelectorAll('[data-alt]').forEach(el => {
      el.addEventListener('click', () => {
        if (this.state.answers[q.questionId]) return;
        this.state.pendingAlt = el.dataset.alt;
        document.querySelectorAll('[data-alt]').forEach(e => e.classList.remove('selected'));
        el.classList.add('selected');
        document.getElementById('btn-confirm').disabled = false;
      });
    });

    document.getElementById('btn-confirm')?.addEventListener('click', () => {
      const currentQ = this.state.questions[this.state.currentIdx];
      this._submitAnswer(currentQ);
    });

    document.getElementById('btn-next')?.addEventListener('click', () => {
      clearInterval(this._timerInterval);
      this.state.currentIdx++;
      this.state.pendingAlt = null;
      this.state.timerSecs = 0;
      this._renderQuestion();
      this._startTimer();
    });
    document.getElementById('btn-finish')?.addEventListener('click', () => this._finishSession());
    document.getElementById('btn-nav-finish')?.addEventListener('click', () => this._finishSession());

    document.getElementById('btn-review')?.addEventListener('click', () => this._toggleReview(q));

    document.getElementById('btn-nav')?.addEventListener('click', () => {
      document.getElementById('qnav-modal').classList.remove('hidden');
    });

    document.getElementById('chat-send')?.addEventListener('click', () => this._sendChat(q));
    document.getElementById('chat-input')?.addEventListener('keydown', e => {
      if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this._sendChat(q); }
    });

    window.__session = { goToQ: (i) => {
      clearInterval(this._timerInterval);
      this.state.currentIdx = i;
      this.state.pendingAlt = null;
      this.state.timerSecs = 0;
      this._renderQuestion();
      this._startTimer();
    }};
  }

  async _submitAnswer(q) {
    if (!this.state.pendingAlt) return;
    const btn = document.getElementById('btn-confirm');
    btn.disabled = true; btn.textContent = 'Enviando…';

    try {
      const result = await SessionService.submitAnswer(
        this.sessionId,
        parseInt(q.questionId),
        parseInt(this.state.pendingAlt),
        parseInt(this.state.timerSecs)
      );
      this.state.answers[q.questionId] = {
        selectedId:  parseInt(this.state.pendingAlt),
        isCorrect:   result.isCorrect,
        explanation: result.explanation,
        legalRefs:   result.legalRefs,
        correctAlternativeId: result.correctAlternativeId, // ← adiciona
      };
      clearInterval(this._timerInterval);
      if (result.isCorrect) showToast('✅ Correto!', 'success');
      else showToast('❌ Incorreto. Veja a explicação!', 'error');
      this._renderQuestion();
    } catch (err) {
      showToast(err.message, 'error');
      btn.disabled = false; btn.textContent = 'Confirmar Resposta';
    }
  }

  async _toggleReview(q) {
    if (!this.state.answers[q.questionId]) {
      showToast('Responda a questão primeiro', 'warning'); return;
    }
    const current = this.state.answers[q.questionId].isMarkedForReview;
    try {
      await SessionService.toggleReview(this.sessionId, q.questionId, !current);
      this.state.answers[q.questionId].isMarkedForReview = !current;
      const btn = document.getElementById('btn-review');
      if (btn) { btn.textContent = (!current ? '⭐' : '☆') + ' Revisão'; btn.style.color = !current ? 'var(--gold)' : ''; }
    } catch {}
  }

  async _finishSession() {
    try {
      const result = await SessionService.finish(this.sessionId);
      Router.go(`/result/${this.sessionId}`);
    } catch (err) {
      showToast(err.message, 'error');
    }
  }

  async _sendChat(q) {
    const input = document.getElementById('chat-input');
    const msg = input?.value.trim();
    if (!msg) return;
    input.value = '';

    if (!this.state.chatMessages[q.questionId]) this.state.chatMessages[q.questionId] = [];
    const msgs = this.state.chatMessages[q.questionId];
    if (msgs.length >= 20) { showToast('Limite de 20 mensagens atingido', 'warning'); return; }

    const now = new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
    msgs.push({ role: 'user', content: msg, time: now });
    this._updateChatUI(q.questionId);

    try {
      const result = await SessionService.sendChatMessage(this.sessionId, q.questionId, msg);
      msgs.push({
        role: 'assistant',
        content: result.content,
        time: new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
      });
    } catch (err) {
      msgs.push({ role: 'assistant', content: `Erro: ${err.message}`, time: now });
    }
    this._updateChatUI(q.questionId);
  }

  _updateChatUI(qid) {
    const msgs = document.getElementById('chat-msgs');
    if (msgs) {
      msgs.innerHTML = this._renderMessages(qid);
      msgs.scrollTop = msgs.scrollHeight;
    }
    const count = document.getElementById('chat-count');
    if (count) count.textContent = `${(this.state.chatMessages[qid]||[]).length}/20`;
  }

  _startTimer() {
    this.state.timerSecs = 0;
    clearInterval(this._timerInterval);
    this._timerInterval = setInterval(() => {
      this.state.timerSecs++;
      this._updateTimer();
    }, 1000);
  }

  _updateTimer() {
    const el = document.getElementById('timer-display');
    if (el) {
      const m = String(Math.floor(this.state.timerSecs / 60)).padStart(2, '0');
      const s = String(this.state.timerSecs % 60).padStart(2, '0');
      el.textContent = `${m}:${s}`;
    }
  }
}