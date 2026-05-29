const ICONS = {
  error: `<svg viewBox="0 0 20 20" fill="none"><circle cx="10" cy="10" r="8.5" stroke="currentColor" stroke-width="1.5"/><path d="M10 6v4.5M10 13.5v.5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round"/></svg>`,
  success: `<svg viewBox="0 0 20 20" fill="none"><circle cx="10" cy="10" r="8.5" stroke="currentColor" stroke-width="1.5"/><path d="M6.5 10l2.5 2.5 4.5-4.5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"/></svg>`,
  info: `<svg viewBox="0 0 20 20" fill="none"><circle cx="10" cy="10" r="8.5" stroke="currentColor" stroke-width="1.5"/><path d="M10 9v5M10 6.5v.5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round"/></svg>`,
  warning: `<svg viewBox="0 0 20 20" fill="none"><path d="M10 3L18 17H2L10 3z" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round"/><path d="M10 8v4M10 14v.5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round"/></svg>`,
};

const TITLES = { error: 'Ошибка', success: 'Успешно', info: 'Информация', warning: 'Предупреждение' };

function escapeHtml(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
  }[c]));
}

function buildToast({ type, title, message, actions }) {
  const el = document.createElement('div');
  el.className = `toast toast-${type}${actions?.length ? ' toast-actionable' : ''}`;
  const actionsHtml = actions?.length
    ? `<div class="toast-actions">${actions.map((a, i) =>
        `<button class="toast-action-btn toast-action-${a.variant || 'ghost'}" data-action-idx="${i}">${escapeHtml(a.label)}</button>`
      ).join('')}</div>`
    : '';

  el.innerHTML = `
    <span class="toast-icon">${ICONS[type]}</span>
    <div class="toast-body">
      <div class="toast-title">${escapeHtml(title)}</div>
      <div class="toast-msg">${message /* allow simple HTML like <strong> */}</div>
      ${actionsHtml}
    </div>
    <button class="toast-close" aria-label="Закрыть">
      <svg viewBox="0 0 14 14" fill="none"><path d="M1 1l12 12M13 1L1 13" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>
    </button>`;
  return el;
}

function showSimple(type, message, duration = 4500) {
  return showAction({ type, title: TITLES[type], message, duration });
}

function showAction({ type = 'info', title = TITLES[type] || '', message = '', duration = 4500, actions = [] }) {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const el = buildToast({ type, title, message, actions });
  container.appendChild(el);

  let timer = null;
  let dismissed = false;

  const dismiss = () => {
    if (dismissed) return;
    dismissed = true;
    clearTimeout(timer);
    el.classList.add('toast-out');
    el.addEventListener('animationend', () => el.remove(), { once: true });
  };

  el.querySelector('.toast-close').addEventListener('click', dismiss);

  // Action buttons
  if (actions?.length) {
    el.querySelectorAll('.toast-action-btn').forEach(btn => {
      btn.addEventListener('click', async () => {
        const idx = Number(btn.dataset.actionIdx);
        const action = actions[idx];
        if (!action?.onClick) { dismiss(); return; }
        // Disable all buttons to prevent double-click
        el.querySelectorAll('.toast-action-btn').forEach(b => b.disabled = true);
        try {
          await action.onClick(dismiss);
        } catch (e) {
          console.error('Toast action handler failed:', e);
        }
        // If handler didn't dismiss, re-enable so user can try again
        if (!dismissed) {
          el.querySelectorAll('.toast-action-btn').forEach(b => b.disabled = false);
        }
      });
    });
  }

  // Auto-dismiss only when duration > 0 and no actions (actionable toasts stay until clicked)
  if (duration > 0 && !actions?.length) {
    timer = setTimeout(dismiss, duration);
    el.addEventListener('mouseenter', () => clearTimeout(timer));
    el.addEventListener('mouseleave', () => { if (!dismissed) timer = setTimeout(dismiss, 1500); });
  }

  return { dismiss };
}

export const toast = {
  error:   (msg, d) => showSimple('error',   msg, d),
  success: (msg, d) => showSimple('success', msg, d),
  info:    (msg, d) => showSimple('info',    msg, d),
  warning: (msg, d) => showSimple('warning', msg, d),

  // Toast with action buttons — does NOT auto-dismiss when actions are present.
  // opts: { type, title, message, duration, actions: [{ label, variant, onClick(dismiss) }] }
  action: (opts) => showAction(opts),
};
