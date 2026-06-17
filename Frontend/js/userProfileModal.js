import { profileApi, PUBLIC_PROFILE_FIELDS } from './profileApi.js';

// Лёгкая модалка просмотра чужого профиля. Создаётся один раз, переиспользуется.
// Запрашивает только публичные поля (через ?fields=...).
// Опционально показывает кнопку действия (например, «Пригласить в задачу»).

let overlay = null;

function escHtml(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => ({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;',
  }[c]));
}

function fullName(p) {
  return [p.lastName, p.firstName, p.secondName].filter(Boolean).join(' ')
      || p.username || '—';
}
function initials(p) {
  const parts = [p.firstName, p.lastName].filter(Boolean).map(s => s[0].toUpperCase());
  return parts.join('') || '?';
}

// Нормализуем ответ: бэкенд при ?fields= отдаёт Dictionary, без fields — UserProfile.
// Оба — плоские объекты с теми же ключами, так что используем как есть.
function normalize(raw) {
  return raw || {};
}

function ensureOverlay() {
  if (overlay) return overlay;

  // Самодостаточные стили модалки (классы up-* нигде больше не определены).
  // Главное здесь — .up-row как flex с gap, чтобы между лейблом («Email»,
  // «Телефон») и значением был отступ и текст не слипался.
  if (!document.getElementById('up-styles')) {
    const style = document.createElement('style');
    style.id = 'up-styles';
    style.textContent = `
      /* Профиль всегда поверх любой другой модалки (детали задачи и т.п.) */
      #modal-user-profile { z-index: 11500; }
      #modal-user-profile .up-avatar {
        width: 64px; height: 64px; border-radius: 50%;
        background: rgba(255,255,255,.2); color: #fff;
        display: grid; place-items: center;
        font-family: 'Instrument Serif', serif; font-size: 1.5rem;
        backdrop-filter: blur(4px);
      }
      #modal-user-profile .up-body, #modal-user-profile #up-body { gap: .25rem; }
      #modal-user-profile .up-loading {
        padding: 1.5rem 0; text-align: center; color: var(--text-sec); font-size: .9rem;
      }
      #modal-user-profile .up-section-title {
        font-size: .78rem; font-weight: 700; text-transform: uppercase;
        letter-spacing: .04em; color: var(--text-sec);
        margin: 1rem 0 .35rem;
      }
      #modal-user-profile .up-row {
        display: flex; align-items: baseline;
        gap: .75rem; padding: .5rem 0;
        border-bottom: 1px solid var(--surface);
      }
      #modal-user-profile .up-row:last-child { border-bottom: none; }
      #modal-user-profile .up-row-label {
        flex: 0 0 38%; max-width: 38%;
        font-size: .82rem; font-weight: 600; color: var(--text-sec);
      }
      #modal-user-profile .up-row-value {
        flex: 1; min-width: 0;
        font-size: .9rem; font-weight: 500; color: var(--text);
        word-break: break-word;
      }
      #modal-user-profile .up-row-value a { color: var(--primary); font-weight: 600; }
      #modal-user-profile .up-empty {
        padding: 1rem 0; text-align: center; color: var(--text-sec);
        font-size: .85rem; opacity: .8;
      }`;
    document.head.appendChild(style);
  }

  overlay = document.createElement('div');
  overlay.className = 'modal-overlay';
  overlay.id = 'modal-user-profile';
  overlay.hidden = true;
  overlay.innerHTML = `
    <div class="modal modal-detail-inner" style="max-width:480px;">
      <button class="modal-close" id="up-close" aria-label="Закрыть">
        <svg viewBox="0 0 20 20" fill="none"><path d="M4 4l12 12M16 4L4 16" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>
      </button>
      <div class="detail-hero" id="up-hero">
        <div class="up-avatar" id="up-avatar">?</div>
        <h2 class="detail-title" id="up-name" style="margin-top:.75rem;">Загрузка…</h2>
        <div class="detail-meta" id="up-sub"></div>
      </div>
      <div class="detail-body" id="up-body">
        <div class="up-loading">Загрузка профиля…</div>
      </div>
      <div class="detail-footer" id="up-footer" hidden></div>
    </div>`;
  document.body.appendChild(overlay);

  overlay.querySelector('#up-close').addEventListener('click', close);
  overlay.addEventListener('click', e => { if (e.target === overlay) close(); });
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape' && !overlay.hidden) close();
  });
  return overlay;
}

function close() {
  if (!overlay) return;
  overlay.hidden = true;
  document.body.style.overflow = '';
}

function row(label, value, isLink = false) {
  if (!value) return '';
  const v = isLink
    ? `<a href="${escHtml(value)}" target="_blank" rel="noopener">${escHtml(value)}</a>`
    : escHtml(value);
  return `<div class="up-row"><span class="up-row-label">${escHtml(label)}</span><span class="up-row-value">${v}</span></div>`;
}

// Мессенджеры: значение может быть ником или ссылкой. Оставляем как текст,
// но для tg/vk пытаемся собрать ссылку, если это похоже на ник/handle.
function messengerRow(label, value, kind) {
  if (!value) return '';
  let href = null;
  const v = String(value).trim();
  if (/^https?:\/\//i.test(v)) {
    href = v;
  } else if (kind === 'tg') {
    href = `https://t.me/${v.replace(/^@/, '')}`;
  } else if (kind === 'vk') {
    href = `https://vk.com/${v.replace(/^@/, '')}`;
  }
  const inner = href
    ? `<a href="${escHtml(href)}" target="_blank" rel="noopener">${escHtml(v)}</a>`
    : escHtml(v);
  return `<div class="up-row"><span class="up-row-label">${escHtml(label)}</span><span class="up-row-value">${inner}</span></div>`;
}

/**
 * Открыть модалку профиля пользователя.
 * @param {string} userId
 * @param {object} [opts]
 * @param {{label:string, onClick:(close:Function)=>any}} [opts.action] — кнопка в футере
 * @param {string} [opts.fallbackName] — имя, пока грузится
 */
export async function openUserProfile(userId, opts = {}) {
  if (!userId) return;
  const el = ensureOverlay();
  el.hidden = false;
  document.body.style.overflow = 'hidden';

  const nameEl   = el.querySelector('#up-name');
  const subEl    = el.querySelector('#up-sub');
  const avatarEl = el.querySelector('#up-avatar');
  const bodyEl   = el.querySelector('#up-body');
  const footerEl = el.querySelector('#up-footer');

  nameEl.textContent = opts.fallbackName || 'Загрузка…';
  subEl.innerHTML = '';
  avatarEl.textContent = '?';
  bodyEl.innerHTML = '<div class="up-loading">Загрузка профиля…</div>';
  footerEl.hidden = true;
  footerEl.innerHTML = '';

  let profile = null;
  try {
    profile = normalize(await profileApi.getById(userId, PUBLIC_PROFILE_FIELDS));
  } catch (e) {
    bodyEl.innerHTML = `<div class="up-error">Не удалось загрузить профиль</div>`;
    return;
  }

  nameEl.textContent = fullName(profile);
  avatarEl.textContent = initials(profile);
  subEl.innerHTML = profile.city ? `<span>${escHtml(profile.city)}</span>` : '';

  const contacts = [
    row('Email', profile.email),
    row('Телефон', profile.phone),
  ].join('');

  const messengers = [
    messengerRow('Telegram', profile.tg, 'tg'),
    messengerRow('VK', profile.vk, 'vk'),
    messengerRow('MAX', profile.max, 'max'),
  ].join('');

  const education = [
    row('Учебное заведение', profile.educationPlace),
    row('Специальность', profile.educationField),
    row('Годы обучения',
      [profile.educationStartYear, profile.educationEndYear].filter(Boolean).join(' — ')),
  ].join('');

  const about = profile.about
    ? `<div class="up-about">${escHtml(profile.about)}</div>` : '';

  bodyEl.innerHTML = `
    ${about}
    ${contacts ? `<div class="up-section"><div class="up-section-title">Контакты</div>${contacts}</div>` : ''}
    ${messengers ? `<div class="up-section"><div class="up-section-title">Мессенджеры</div>${messengers}</div>` : ''}
    ${education ? `<div class="up-section"><div class="up-section-title">Образование</div>${education}</div>` : ''}
    ${!contacts && !messengers && !education && !about
      ? '<div class="up-empty">Пользователь не указал дополнительных данных</div>' : ''}
  `;

    if (opts.action || opts.secondAction) {
    footerEl.hidden = false;
    footerEl.style.cssText = 'display:flex;gap:.75rem;flex-wrap:wrap;';
    footerEl.innerHTML = `
      ${opts.action ? `
        <button class="btn-primary" id="up-action" style="width:auto;padding:0 1.5rem;flex:1;">
          <span class="btn-label">${escHtml(opts.action.label)}</span>
          <span class="btn-spinner" hidden></span>
        </button>` : ''}
      ${opts.secondAction ? `
        <button class="btn-secondary" id="up-second-action" style="width:auto;padding:0 1.5rem;flex:1;">
          <span class="btn-label">${escHtml(opts.secondAction.label)}</span>
          <span class="btn-spinner" hidden></span>
        </button>` : ''}`;

    function bindActionBtn(id, handler) {
      const btn = footerEl.querySelector(`#${id}`);
      if (!btn) return;
      btn.addEventListener('click', async () => {
        const lbl = btn.querySelector('.btn-label');
        const sp  = btn.querySelector('.btn-spinner');
        btn.disabled = true;
        if (lbl) lbl.hidden = true;
        if (sp)  sp.hidden  = false;
        try { await handler(close); }
        finally {
          btn.disabled = false;
          if (lbl) lbl.hidden = false;
          if (sp)  sp.hidden  = true;
        }
      });
    }

    if (opts.action)       bindActionBtn('up-action',        opts.action.onClick);
    if (opts.secondAction) bindActionBtn('up-second-action', opts.secondAction.onClick);
  }
}

export const userProfileModal = { open: openUserProfile, close };
