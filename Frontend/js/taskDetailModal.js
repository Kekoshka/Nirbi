import { tasksApi }        from './tasksApi.js';
import { usersApi }        from './api.js';
import { openUserProfile } from './userProfileModal.js';

// ─────────────────────────────────────────────────────────────────────────────
// Лёгкая read-only модалка деталей задачи. Переиспользуется на страницах, где
// не нужен полный функционал (например, на странице подтверждений): показывает
// название, описание, фото (с лайтбоксом), вознаграждение/волонтёров/координаты,
// мини-карту и организатора (клик → профиль). Без действий отклика/редактирования.
//
// Переиспользует CSS-классы detail-* из tasks.css (они есть на всех страницах,
// которые подключают tasks.css). Свои стили инжектит только для мелочей.
// ─────────────────────────────────────────────────────────────────────────────

let overlay = null;
let miniMap = null;
let ownerNameCache = new Map();   // userId → имя организатора

function escHtml(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => ({
    '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;',
  }[c]));
}

function getOwnerId(task) {
  return task.consumerId ?? task.ownerId ?? task.creatorId
      ?? task.userId     ?? task.organizerId ?? null;
}

function statusLabel(s) {
  if (!s) return '';
  const n = (s.name || s).toString().toLowerCase();
  if (n.includes('открыт') || n.includes('open') || n.includes('search')) return 'Открытая';
  if (n.includes('работ')  || n.includes('progress')) return 'В работе';
  if (n.includes('заверш') || n.includes('done') || n.includes('complet')) return 'Завершена';
  return s.name || String(s);
}

function formatReward(v) {
  if (!v && v !== 0) return '—';
  return Number(v).toLocaleString('ru-RU', { style: 'currency', currency: 'RUB', maximumFractionDigits: 0 });
}

function ensureOverlay() {
  if (overlay) return overlay;

  if (!document.getElementById('tdm-styles')) {
    const style = document.createElement('style');
    style.id = 'tdm-styles';
    style.textContent = `
      #modal-task-detail .tdm-owner-link {
        background: none; border: none; padding: 0; cursor: pointer;
        color: #fff; font: inherit; text-decoration: underline;
        text-underline-offset: 2px;
      }
      #modal-task-detail .tdm-loading {
        padding: 2rem 0; text-align: center; color: var(--text-sec); font-size: .9rem;
      }
      #modal-task-detail #tdm-map { height: 180px; }
      #modal-task-detail .detail-gallery img { cursor: zoom-in; }
      /* Лайтбокс для фото внутри этой модалки */
      #tdm-lightbox {
        position: fixed; inset: 0; z-index: 11000;
        background: rgba(15,23,42,.92);
        display: flex; align-items: center; justify-content: center; padding: 2rem;
        backdrop-filter: blur(4px);
      }
      #tdm-lightbox[hidden] { display: none; }
      #tdm-lightbox img { max-width: 100%; max-height: 100%; object-fit: contain; border-radius: var(--radius); }
      #tdm-lightbox .tdm-lightbox-close {
        position: absolute; top: 1.5rem; right: 1.5rem;
        width: 44px; height: 44px; border-radius: 50%;
        background: rgba(255,255,255,.15); color: #fff;
        display: grid; place-items: center; border: none; cursor: pointer;
      }`;
    document.head.appendChild(style);
  }

  overlay = document.createElement('div');
  overlay.className = 'modal-overlay';
  overlay.id = 'modal-task-detail';
  overlay.hidden = true;
  overlay.innerHTML = `
    <div class="modal modal-detail-inner">
      <button class="modal-close" id="tdm-close" aria-label="Закрыть">
        <svg viewBox="0 0 20 20" fill="none"><path d="M4 4l12 12M16 4L4 16" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>
      </button>
      <div class="detail-hero">
        <div class="detail-badge" id="tdm-status"></div>
        <h2 class="detail-title" id="tdm-title">Загрузка…</h2>
        <div class="detail-meta" id="tdm-meta"></div>
      </div>
      <div class="detail-body" id="tdm-body">
        <div class="tdm-loading">Загрузка задачи…</div>
      </div>
    </div>`;
  document.body.appendChild(overlay);

  overlay.querySelector('#tdm-close').addEventListener('click', close);
  overlay.addEventListener('click', e => { if (e.target === overlay) close(); });
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape' && !overlay.hidden) close();
  });

  return overlay;
}

function openLightbox(src) {
  let lb = document.getElementById('tdm-lightbox');
  if (!lb) {
    lb = document.createElement('div');
    lb.id = 'tdm-lightbox';
    lb.hidden = true;
    lb.innerHTML = `
      <button class="tdm-lightbox-close" aria-label="Закрыть">
        <svg viewBox="0 0 24 24" fill="none"><path d="M6 6l12 12M18 6L6 18" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>
      </button>
      <img src="" alt="" />`;
    document.body.appendChild(lb);
    lb.querySelector('.tdm-lightbox-close').addEventListener('click', () => { lb.hidden = true; });
    lb.addEventListener('click', e => { if (e.target === lb) lb.hidden = true; });
  }
  lb.querySelector('img').src = src;
  lb.hidden = false;
}

function close() {
  if (!overlay) return;
  overlay.hidden = true;
  document.body.style.overflow = '';
  if (miniMap) { try { miniMap.remove(); } catch {} miniMap = null; }
}

async function resolveOwnerName(ownerId) {
  if (!ownerId) return null;
  const key = String(ownerId);
  if (ownerNameCache.has(key)) return ownerNameCache.get(key);
  try {
    const map = await usersApi.getUsernameMap([ownerId]);
    const name = map.get(key) || null;
    ownerNameCache.set(key, name);
    return name;
  } catch { return null; }
}

/**
 * Открыть модалку деталей задачи (read-only).
 * @param {string} taskId
 */
export async function openTaskDetail(taskId) {
  if (!taskId || taskId === '00000000-0000-0000-0000-000000000000') return;

  ensureOverlay();
  overlay.hidden = false;
  document.body.style.overflow = 'hidden';

  // сброс в состояние загрузки
  overlay.querySelector('#tdm-title').textContent = 'Загрузка…';
  overlay.querySelector('#tdm-status').textContent = '';
  overlay.querySelector('#tdm-meta').innerHTML = '';
  overlay.querySelector('#tdm-body').innerHTML = '<div class="tdm-loading">Загрузка задачи…</div>';

  let task = null;
  try {
    task = await tasksApi.getById(taskId);
  } catch {
    overlay.querySelector('#tdm-body').innerHTML =
      '<div class="tdm-loading">Не удалось загрузить задачу</div>';
    return;
  }
  if (!task) {
    overlay.querySelector('#tdm-body').innerHTML =
      '<div class="tdm-loading">Задача не найдена</div>';
    return;
  }

  // Заголовок
  overlay.querySelector('#tdm-status').textContent = statusLabel(task.status);
  overlay.querySelector('#tdm-title').textContent = task.name || 'Без названия';

  // Организатор — имя подгружаем, по клику открываем профиль
  const ownerId = getOwnerId(task);
  const ownerName = await resolveOwnerName(ownerId);
  const ownerLabel = escHtml(ownerName || 'Организатор');
  const metaEl = overlay.querySelector('#tdm-meta');
  metaEl.innerHTML = `
    <span>Организатор: ${ownerId
      ? `<button type="button" class="tdm-owner-link" id="tdm-owner"><strong>${ownerLabel}</strong></button>`
      : `<strong>${ownerLabel}</strong>`}</span>
    <span>Волонтёров: ${task.numberVolunteers || 0}</span>
    <span>Вознаграждение: ${formatReward(task.encouragement)}</span>`;
  if (ownerId) {
    metaEl.querySelector('#tdm-owner')?.addEventListener('click', () => {
      openUserProfile(String(ownerId), { fallbackName: ownerName || '' });
    });
  }

  // Тело: описание + галерея + статы + мини-карта
  const imgs = Array.isArray(task.images) ? task.images.filter(i => i?.data) : [];
  const galleryHtml = imgs.length
    ? `<div class="detail-gallery">${imgs
        .slice().sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))
        .map(img => `<img class="tdm-img" src="data:${img.contentType || 'image/jpeg'};base64,${img.data}" alt="${escHtml(img.originalFileName || '')}" />`)
        .join('')}</div>`
    : '';

  const coords = task.latitude
    ? `${Number(task.latitude).toFixed(4)}, ${Number(task.longitude).toFixed(4)}`
    : '—';

  overlay.querySelector('#tdm-body').innerHTML = `
    <div class="detail-description">${escHtml(task.description || 'Описание отсутствует')}</div>
    ${galleryHtml}
    <div class="detail-stats">
      <div class="stat-card">
        <span class="stat-label">Вознаграждение</span>
        <span class="stat-value reward-val">${formatReward(task.encouragement)}</span>
      </div>
      <div class="stat-card">
        <span class="stat-label">Волонтёров нужно</span>
        <span class="stat-value">${task.numberVolunteers || 0}</span>
      </div>
      <div class="stat-card">
        <span class="stat-label">Координаты</span>
        <span class="stat-value coords-val">${coords}</span>
      </div>
    </div>
    ${task.latitude ? `<div class="detail-map-mini"><div id="tdm-map"></div></div>` : ''}`;

  // Лайтбокс на фото
  overlay.querySelectorAll('.tdm-img').forEach(im => {
    im.addEventListener('click', () => openLightbox(im.src));
  });

  // Мини-карта (если Leaflet подключён на странице)
  if (task.latitude && task.longitude && window.L) {
    // Задержка побольше — даём модалке отрендериться и завершить анимацию,
    // иначе Leaflet измеряет нулевой размер контейнера и карта серая.
    setTimeout(() => {
      if (miniMap) { try { miniMap.remove(); } catch {} miniMap = null; }
      const el = overlay.querySelector('#tdm-map');
      if (!el) return;
      miniMap = window.L.map(el, { zoomControl: false, dragging: false, scrollWheelZoom: false })
        .setView([task.latitude, task.longitude], 14);
      window.L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap',
      }).addTo(miniMap);
      window.L.marker([task.latitude, task.longitude]).addTo(miniMap);
      // Принудительный пересчёт размеров — ключевой фикс «серой» карты в модалке
      setTimeout(() => { try { miniMap.invalidateSize(); } catch {} }, 120);
    }, 200);
  }
}
