import { toast }                              from './toast.js';
import { tokenStore }                         from './tokenStore.js';
import { authApi }                            from './api.js';
import { tasksApi }                           from './tasksApi.js';
import { dataApi }                            from './dataApi.js';
import { confirmationsApi }                   from './confirmationsApi.js';
import { startNotifications, onNotification } from './notifications.js';
import { CONFIRMATION_TYPES,
         CONFIRMATION_DEFAULT_EXPIRATION_HOURS } from './config.js';

// ── Guard: redirect to login if no session ───────────────────────────────────
if (!tokenStore.hasSession()) {
  window.location.href = 'index.html#login';
}

// ── State ────────────────────────────────────────────────────────────────────
let allTasks    = [];
let statuses    = [];
let mode        = 'all';       // 'all' | 'my'
let editingId   = null;        // task id being edited (null = create)
let deletingId  = null;
let mainMap     = null;
let detailMap   = null;
let formMap     = null;
let formMarker  = null;
let mainMarkers = [];
let removedImageIds = new Set();  // IDs картинок, помеченных на удаление в форме редактирования
let editingTask    = null;         // полные данные задачи при редактировании (включая images)

// Бэкенд использует статус "Created" как начальный, а не "Pending".
// Нормализуем оба варианта — иначе кнопки действий не появятся.
function isPending(status) {
  const s = (status || '').toLowerCase();
  return s === 'pending' || s === 'created';
}

const currentUserId = tokenStore.getUserId();

// ── DOM refs ─────────────────────────────────────────────────────────────────
const tasksGrid      = document.getElementById('tasks-grid');
const emptyState     = document.getElementById('empty-state');
const countLabel     = document.getElementById('tasks-count-label');
const searchInput    = document.getElementById('search-input');
const filterStatus   = document.getElementById('filter-status');
const filterSort     = document.getElementById('filter-sort');
const modeToggle     = document.getElementById('mode-toggle');

// Modals
const modalDetail    = document.getElementById('modal-detail');
const modalForm      = document.getElementById('modal-form');
const modalConfirm   = document.getElementById('modal-confirm');

// ── Helpers ───────────────────────────────────────────────────────────────────
function statusLabel(s) {
  if (!s) return 'Неизвестно';
  const n = (s.name || s).toLowerCase();
  if (n.includes('открыт') || n.includes('open'))   return 'Открытая';
  if (n.includes('работ') || n.includes('progress')) return 'В работе';
  if (n.includes('заверш') || n.includes('done') || n.includes('complet')) return 'Завершена';
  return s.name || s;
}
function statusClass(s) {
  if (!s) return 'badge-default';
  const n = (s.name || s).toLowerCase();
  if (n.includes('открыт') || n.includes('open'))   return 'badge-open';
  if (n.includes('работ') || n.includes('progress')) return 'badge-progress';
  if (n.includes('заверш') || n.includes('done') || n.includes('complet')) return 'badge-done';
  return 'badge-default';
}
function markerColor(s) {
  if (!s) return '#014BAA';
  const n = (s.name || s).toLowerCase();
  if (n.includes('работ') || n.includes('progress')) return '#D97706';
  if (n.includes('заверш') || n.includes('done'))    return '#059669';
  return '#014BAA';
}
function makeIcon(color) {
  return L.divIcon({
    className: '',
    html: `<svg width="28" height="36" viewBox="0 0 28 36" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M14 0C6.268 0 0 6.268 0 14c0 9.333 14 22 14 22S28 23.333 28 14C28 6.268 21.732 0 14 0z" fill="${color}"/>
      <circle cx="14" cy="14" r="6" fill="white"/>
    </svg>`,
    iconSize: [28, 36], iconAnchor: [14, 36], popupAnchor: [0, -38],
  });
}

function formatReward(v) {
  if (!v && v !== 0) return '—';
  return Number(v).toLocaleString('ru-RU', { style: 'currency', currency: 'RUB', maximumFractionDigits: 0 });
}

// Возможные имена поля владельца в ответе бэкенда — пробуем по очереди
function getOwnerId(task) {
  return task.consumerId ?? task.ownerId ?? task.creatorId
      ?? task.userId     ?? task.organizerId ?? null;
}

function isOwner(task) {
  const ownerId = getOwnerId(task);
  return ownerId && String(ownerId) === String(currentUserId);
}

function ownerBadge(task) {
  return isOwner(task) ? 'Вы' : 'Организатор';
}

// ── Image lightbox ──────────────────────────────────────────────────────────
function openLightbox(src) {
  const lb  = document.getElementById('image-lightbox');
  const img = document.getElementById('lightbox-img');
  if (!lb || !img) return;
  img.src = src;
  lb.hidden = false;
  document.body.style.overflow = 'hidden';
}
function closeLightbox() {
  const lb = document.getElementById('image-lightbox');
  if (!lb) return;
  lb.hidden = true;
  document.body.style.overflow = '';
}

// Wire up close handlers once
(() => {
  const lb = document.getElementById('image-lightbox');
  if (!lb) return;
  document.getElementById('lightbox-close')?.addEventListener('click', closeLightbox);
  lb.addEventListener('click', e => {
    if (e.target === lb) closeLightbox();
  });
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape' && !lb.hidden) closeLightbox();
  });
})();

function setLoading(btnId, loading) {
  const btn = document.getElementById(btnId);
  if (!btn) return;
  btn.disabled = loading;
  const l = btn.querySelector('.btn-label');
  const s = btn.querySelector('.btn-spinner');
  if (l) l.hidden = loading;
  if (s) s.hidden = !loading;
}

// ── Map initialisation ────────────────────────────────────────────────────────
function initMainMap() {
  mainMap = L.map('tasks-map', { zoomControl: true }).setView([55.7558, 37.6176], 10);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap',
    maxZoom: 19,
  }).addTo(mainMap);
}

function refreshMapMarkers(tasks) {
  mainMarkers.forEach(m => m.remove());
  mainMarkers = [];
  tasks.forEach(task => {
    if (!task.latitude && !task.longitude) return;
    const marker = L.marker([task.latitude, task.longitude], {
      icon: makeIcon(markerColor(task.status)),
    }).addTo(mainMap);
    marker.bindPopup(`<strong>${task.name}</strong><br/>${statusLabel(task.status)}`);
    marker.on('click', () => openDetail(task.id));
    mainMarkers.push(marker);
  });

  if (tasks.length > 0 && tasks.some(t => t.latitude)) {
    const pts = tasks.filter(t => t.latitude).map(t => [t.latitude, t.longitude]);
    mainMap.fitBounds(L.latLngBounds(pts).pad(0.15), { maxZoom: 14 });
  }
}

// ── Data loading ──────────────────────────────────────────────────────────────
async function loadStatuses() {
  try {
    statuses = await tasksApi.getStatuses() || [];
    filterStatus.innerHTML = '<option value="">Все статусы</option>';
    statuses.forEach(s => {
      const opt = document.createElement('option');
      // Фильтруем по имени статуса (t.status = строка), а не по UUID
      opt.value = s.name;
      opt.textContent = s.name;
      filterStatus.appendChild(opt);
    });
  } catch {}
}

async function loadTasks() {
  try {
    tasksGrid.innerHTML = '<div class="task-card skeleton"></div>'.repeat(6);
    emptyState.hidden = true;

    const data = await tasksApi.getAll(100) || [];
    allTasks = Array.isArray(data) ? data : [];
    renderTasks();
  } catch (e) {
    toast.error('Не удалось загрузить задачи');
    tasksGrid.innerHTML = '';
    emptyState.hidden = false;
  }
}

// ── Render ────────────────────────────────────────────────────────────────────
function getFiltered() {
  let tasks = [...allTasks];

  // mode filter
  if (mode === 'my') {
    tasks = tasks.filter(t => isOwner(t));
  }

  // search
  const q = searchInput.value.trim().toLowerCase();
  if (q) tasks = tasks.filter(t => (t.name || '').toLowerCase().includes(q));

  // status — сравниваем строку t.status с именем статуса из select
  const sid = filterStatus.value;
  if (sid) tasks = tasks.filter(t => (t.status || '') === sid);

  // sort
  const sort = filterSort.value;
  if (sort === 'reward')     tasks.sort((a, b) => (b.encouragement || 0) - (a.encouragement || 0));
  if (sort === 'volunteers') tasks.sort((a, b) => (b.numberVolunteers || 0) - (a.numberVolunteers || 0));

  return tasks;
}

function renderTasks() {
  const tasks = getFiltered();

  countLabel.textContent = `${tasks.length} ${tasks.length === 1 ? 'задача' : tasks.length < 5 ? 'задачи' : 'задач'}`;
  refreshMapMarkers(tasks);

  if (!tasks.length) {
    tasksGrid.innerHTML = '';
    emptyState.hidden = false;
    return;
  }
  emptyState.hidden = true;

  tasksGrid.innerHTML = tasks.map((t, i) => {
    // previewImageData: base64 строка (без префикса data:)
    // previewImageContentType: image/jpeg, image/png ...
    const preview = t.previewImageData
      ? `<div class="card-preview"><img src="data:${t.previewImageContentType || 'image/jpeg'};base64,${t.previewImageData}" alt="" /></div>`
      : '';
    return `
    <div class="task-card ${preview ? 'has-preview' : ''}" data-id="${t.id}" style="animation-delay:${i * 30}ms">
      ${preview}
      <div class="card-top">
        <span class="card-title">${t.name || 'Без названия'}</span>
        <span class="status-badge ${statusClass(t.status)}">${statusLabel(t.status)}</span>
      </div>
      <p class="card-desc">${t.description || 'Описание отсутствует'}</p>
      <div class="card-footer">
        <div class="card-chips">
          <span class="chip chip-reward">
            <svg viewBox="0 0 16 16" fill="none"><circle cx="8" cy="8" r="6.5" stroke="currentColor" stroke-width="1.25"/><path d="M8 5v6M6 7h3.5" stroke="currentColor" stroke-width="1.25" stroke-linecap="round"/></svg>
            ${formatReward(t.encouragement)}
          </span>
          <span class="chip">
            <svg viewBox="0 0 16 16" fill="none"><circle cx="8" cy="6" r="2.5" stroke="currentColor" stroke-width="1.25"/><path d="M3 14c0-2.761 2.239-4.5 5-4.5s5 1.739 5 4.5" stroke="currentColor" stroke-width="1.25" stroke-linecap="round"/></svg>
            ${t.numberVolunteers || 0}
          </span>
        </div>
        ${isOwner(t)
          ? '<span class="card-owner-badge owner-self">Вы</span>'
          : '<span class="card-owner-badge owner-other">Организатор</span>'}
      </div>
    </div>`;
  }).join('');

  // card click → detail
  tasksGrid.querySelectorAll('.task-card').forEach(card => {
    card.addEventListener('click', () => openDetail(card.dataset.id));
  });
}

// ── Detail Modal ──────────────────────────────────────────────────────────────
async function openDetail(taskId) {
  // Список задач не содержит полный массив Images — всегда подгружаем детально.
  // Если getById недоступен (например, сеть упала) — упадём на кэш из allTasks.
  let task = null;
  try {
    task = await tasksApi.getById(taskId);
  } catch {
    task = allTasks.find(t => String(t.id) === String(taskId));
  }
  if (!task) { toast.error('Задача не найдена'); return; }

  // populate header
  document.getElementById('detail-status-badge').textContent = statusLabel(task.status);
  document.getElementById('detail-title').textContent = task.name || 'Без названия';
  document.getElementById('detail-meta').innerHTML =
    `<span>Организатор: <strong>${ownerBadge(task)}</strong></span>
     <span>Волонтёров: ${task.numberVolunteers || 0}</span>
     <span>Вознаграждение: ${formatReward(task.encouragement)}</span>`;

  document.getElementById('detail-description').textContent =
    task.description || 'Описание отсутствует';

  // Image gallery — MinorTaskDetailResponse.Images: FileMetadataDto[] с base64 в .data
  const gallery = document.getElementById('detail-gallery');
  const imgs = Array.isArray(task.images) ? task.images : [];
  if (imgs.length) {
    gallery.hidden = false;
    gallery.innerHTML = imgs
      .slice()
      .sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))
      .map(img => {
        if (!img.data) return '';
        const ct = img.contentType || 'image/jpeg';
        return `<img class="clickable-img" src="data:${ct};base64,${img.data}" alt="${img.originalFileName || ''}" />`;
      }).join('');
    // Bind click → lightbox
    gallery.querySelectorAll('img.clickable-img').forEach(im => {
      im.addEventListener('click', () => openLightbox(im.src));
    });
  } else {
    gallery.hidden = true;
    gallery.innerHTML = '';
  }

  document.getElementById('detail-reward').textContent = formatReward(task.encouragement);
  document.getElementById('detail-volunteers').textContent = task.numberVolunteers || 0;
  document.getElementById('detail-coords').textContent =
    task.latitude ? `${Number(task.latitude).toFixed(4)}, ${Number(task.longitude).toFixed(4)}` : '—';

  // footer buttons
  const footer = document.getElementById('detail-footer');
  footer.innerHTML = '';

  if (isOwner(task)) {
    footer.innerHTML = `
      <button class="btn-outline-primary" id="detail-btn-edit">Редактировать</button>
      <button class="btn-danger" id="detail-btn-delete">
        <span class="btn-label">Удалить</span><span class="btn-spinner" hidden></span>
      </button>`;
    document.getElementById('detail-btn-edit').addEventListener('click', () => {
      closeModal(modalDetail);
      openForm(task);
    });
    document.getElementById('detail-btn-delete').addEventListener('click', () => {
      deletingId = task.id;
      closeModal(modalDetail);
      openModal(modalConfirm);
    });
  } else {
    // Проверяем, не отправлял ли уже текущий пользователь заявку на эту задачу
    const myApplication = confirmations.find(c =>
      String(c.initiatorId) === String(currentUserId) &&
      String(c.entityId)    === String(task.id)      &&
      (c.confirmationType || '') === CONFIRMATION_TYPES.RESPOND_TO_MINOR_TASK
    );
    const status = (myApplication?.status || '').toLowerCase();

    if (myApplication && isPending(myApplication.status)) {
      // Ожидает решения — кнопка "Отозвать"
      footer.innerHTML = `
        <div class="detail-apply-info detail-apply-pending">
          <svg viewBox="0 0 20 20" fill="none"><circle cx="10" cy="10" r="8.5" stroke="currentColor" stroke-width="1.5"/><path d="M10 6v4l3 2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/></svg>
          <span>Вы уже откликнулись на задачу. Ожидайте ответа организатора.</span>
        </div>
        <button class="btn-outline-primary" id="detail-btn-revoke">
          <span class="btn-label">Отозвать заявку</span>
          <span class="btn-spinner" hidden></span>
        </button>`;
      document.getElementById('detail-btn-revoke').addEventListener('click', async () => {
        setLoading('detail-btn-revoke', true);
        try {
          await confirmationsApi.revoke(myApplication.id, currentUserId);
          toast.info('Заявка отозвана');
          closeModal(modalDetail);
          await loadConfirmations();
        } catch (e) {
          toast.error(e.message || 'Не удалось отозвать заявку');
        } finally {
          setLoading('detail-btn-revoke', false);
        }
      });
    } else if (myApplication && status === 'accepted') {
      footer.innerHTML = `
        <div class="detail-apply-info detail-apply-accepted">
          <svg viewBox="0 0 20 20" fill="none"><circle cx="10" cy="10" r="8.5" stroke="currentColor" stroke-width="1.5"/><path d="M6.5 10l2.5 2.5 4.5-4.5" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"/></svg>
          <span>Вы участвуете в этой задаче</span>
        </div>`;
    } else {
      // Не подавал, либо предыдущая заявка отклонена/отозвана/истекла — можно подать новую
      const wasRejected = myApplication && (status === 'rejected' || status === 'expired' || status === 'revoked');
      footer.innerHTML = `
        ${wasRejected ? `<div class="detail-apply-info detail-apply-rejected">
          <span>Предыдущая заявка ${status === 'rejected' ? 'была отклонена' : status === 'expired' ? 'истекла' : 'была отозвана'}. Вы можете попробовать снова.</span>
        </div>` : ''}
        <button class="btn-primary" id="detail-btn-join" style="width:auto;padding:0 2rem;">
          <span class="btn-label">Откликнуться на задачу</span>
          <span class="btn-spinner" hidden></span>
        </button>`;
      document.getElementById('detail-btn-join').addEventListener('click', async () => {
        const ownerId = getOwnerId(task);
        if (!ownerId) {
          toast.error('Не удалось определить организатора задачи');
          return;
        }
        setLoading('detail-btn-join', true);
        try {
          await confirmationsApi.create({
            confirmationType: CONFIRMATION_TYPES.RESPOND_TO_MINOR_TASK,
            entityId:         task.id,
            reviewerId:       ownerId,
            expirationHours:  CONFIRMATION_DEFAULT_EXPIRATION_HOURS,
            metaData: {
              taskName:           task.name || '',
              applicantUsername:  tokenStore.getUsername() || '',
            },
          });
          toast.success('Заявка отправлена организатору');
          closeModal(modalDetail);
          await loadConfirmations();
        } catch (e) {
          toast.error(e.message || 'Не удалось отправить отклик');
        } finally {
          setLoading('detail-btn-join', false);
        }
      });
    }
  }

  openModal(modalDetail);

  // mini map
  setTimeout(() => {
    if (detailMap) { detailMap.remove(); detailMap = null; }
    if (task.latitude && task.longitude) {
      detailMap = L.map('detail-map', { zoomControl: false, dragging: false, scrollWheelZoom: false })
        .setView([task.latitude, task.longitude], 14);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(detailMap);
      L.marker([task.latitude, task.longitude], { icon: makeIcon(markerColor(task.status)) }).addTo(detailMap);
    }
  }, 50);
}

// ── Create / Edit Form ────────────────────────────────────────────────────────
function openForm(task = null) {
  editingId   = task ? task.id : null;
  editingTask = task ?? null;
  document.getElementById('form-modal-title').textContent = task ? 'Редактировать задачу' : 'Новая задача';

  // fill fields
  document.getElementById('tf-name').value       = task?.name        || '';
  document.getElementById('tf-desc').value       = task?.description || '';
  document.getElementById('tf-volunteers').value = task?.numberVolunteers || '';
  document.getElementById('tf-reward').value     = task?.encouragement    || '';
  document.getElementById('tf-lat').value        = task?.latitude    || '';
  document.getElementById('tf-lng').value        = task?.longitude   || '';

  // reset image input
  const imgInput = document.getElementById('tf-images');
  if (imgInput) { imgInput.value = ''; }
  const imgPreview = document.getElementById('tf-images-preview');
  if (imgPreview) imgPreview.innerHTML = '';
  const uploadText = document.getElementById('file-upload-text');
  if (uploadText) uploadText.textContent = 'Нажмите для выбора изображений';

  // Reset list of images user wants removed
  removedImageIds = new Set();

  // Existing images — показываем только в режиме редактирования
  const existingWrap = document.getElementById('tf-existing-wrap');
  const existingImages = document.getElementById('tf-existing-images');
  if (task && existingWrap && existingImages) {
    // Если в переданной задаче уже есть массив images с base64 — используем сразу;
    // иначе подгружаем через API (список задач не содержит полных images).
    const renderExisting = (imgs) => {
      const arr = Array.isArray(imgs) ? imgs.filter(i => i?.data) : [];
      if (arr.length) {
        existingWrap.hidden = false;
        existingImages.innerHTML = arr
          .slice()
          .sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))
          .map(img => `
            <div class="preview-img-wrap" data-file-id="${img.id}">
              <img class="preview-img clickable-img" src="data:${img.contentType || 'image/jpeg'};base64,${img.data}" alt="${img.originalFileName || ''}" />
              <button type="button" class="preview-img-remove" data-file-id="${img.id}" aria-label="Удалить">
                <svg viewBox="0 0 14 14" fill="none"><path d="M1 1l12 12M13 1L1 13" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>
              </button>
            </div>`)
          .join('');

        // Wire up clicks: × → remove, image → lightbox
        existingImages.querySelectorAll('.preview-img-remove').forEach(btn => {
          btn.addEventListener('click', e => {
            e.stopPropagation();
            const id = btn.dataset.fileId;
            removedImageIds.add(id);
            btn.closest('.preview-img-wrap')?.remove();
            if (!existingImages.querySelector('.preview-img-wrap')) {
              existingWrap.hidden = true;
            }
          });
        });
        existingImages.querySelectorAll('img.clickable-img').forEach(im => {
          im.addEventListener('click', () => openLightbox(im.src));
        });
      } else {
        existingWrap.hidden = true;
        existingImages.innerHTML = '';
      }
    };

    if (Array.isArray(task.images) && task.images.some(i => i?.data)) {
      renderExisting(task.images);
    } else {
      existingWrap.hidden = true;
      existingImages.innerHTML = '';
      // Подгружаем полные детали с картинками
      tasksApi.getById(task.id).then(full => {
        // Не перерисовываем, если форма уже закрыта или мы перешли к другой задаче
        if (modalForm.hidden || editingId !== task.id) return;
        editingTask = full;   // обновляем — теперь есть images[].fileCollectionId
        renderExisting(full?.images);
      }).catch(() => { /* молча — фото просто не покажутся */ });
    }
  } else if (existingWrap) {
    existingWrap.hidden = true;
    existingImages.innerHTML = '';
  }

  // clear errors
  ['name','desc','volunteers','reward','coords'].forEach(f => {
    document.getElementById(`tf-${f}-error`).textContent = '';
  });
  ['tf-name','tf-desc','tf-volunteers','tf-reward'].forEach(id => {
    document.getElementById(id)?.classList.remove('error');
  });

  openModal(modalForm);

  // init form map
  setTimeout(() => {
    if (formMap) { formMap.remove(); formMap = null; formMarker = null; }
    const lat = task?.latitude  || 55.7558;
    const lng = task?.longitude || 37.6176;
    formMap = L.map('form-map').setView([lat, lng], 11);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(formMap);

    if (task?.latitude) {
      formMarker = L.marker([task.latitude, task.longitude], { icon: makeIcon('#014BAA'), draggable: true }).addTo(formMap);
      formMarker.on('dragend', () => updateFormCoords(formMarker.getLatLng()));
    }

    formMap.on('click', e => {
      if (formMarker) formMarker.remove();
      formMarker = L.marker(e.latlng, { icon: makeIcon('#014BAA'), draggable: true }).addTo(formMap);
      formMarker.on('dragend', () => updateFormCoords(formMarker.getLatLng()));
      updateFormCoords(e.latlng);
    });
  }, 80);
}

// ── Image upload preview ─────────────────────────────────────────────────────
document.getElementById('tf-images').addEventListener('change', function () {
  const preview = document.getElementById('tf-images-preview');
  const label   = document.getElementById('file-upload-text');
  const files   = Array.from(this.files).slice(0, 5);
  preview.innerHTML = '';
  if (files.length) {
    label.textContent = `Выбрано файлов: ${files.length}`;
    files.forEach(file => {
      const url = URL.createObjectURL(file);
      const img = document.createElement('img');
      img.src = url;
      img.className = 'preview-img';
      img.onload = () => URL.revokeObjectURL(url);
      preview.appendChild(img);
    });
  } else {
    label.textContent = 'Нажмите для выбора изображений';
  }
});

function updateFormCoords({ lat, lng }) {
  document.getElementById('tf-lat').value = lat.toFixed(6);
  document.getElementById('tf-lng').value = lng.toFixed(6);
  document.getElementById('tf-coords-error').textContent = '';
}

function validateForm() {
  let ok = true;
  const set = (id, msg) => {
    const el = document.getElementById(`tf-${id}-error`);
    const inp = document.getElementById(`tf-${id}`);
    if (el) el.textContent = msg;
    if (inp) inp.classList.toggle('error', !!msg);
    if (msg) ok = false;
  };

  const name = document.getElementById('tf-name').value.trim();
  if (!name) set('name', 'Введите название');
  else if (name.length < 3) set('name', 'Минимум 3 символа');
  else set('name', '');

  const desc = document.getElementById('tf-desc').value.trim();
  if (!desc) set('desc', 'Введите описание');
  else set('desc', '');

  const vol = Number(document.getElementById('tf-volunteers').value);
  if (!vol || vol < 1) set('volunteers', 'Укажите кол-во волонтёров (мин. 1)');
  else set('volunteers', '');

  const lat = document.getElementById('tf-lat').value;
  const lng = document.getElementById('tf-lng').value;
  if (!lat || !lng) {
    document.getElementById('tf-coords-error').textContent = 'Выберите место на карте';
    ok = false;
  } else {
    document.getElementById('tf-coords-error').textContent = '';
  }

  return ok;
}

document.getElementById('task-form').addEventListener('submit', async e => {
  e.preventDefault();
  if (!validateForm()) return;

  setLoading('btn-submit-form', true);
  try {
    const payload = {
      name:             document.getElementById('tf-name').value.trim(),
      description:      document.getElementById('tf-desc').value.trim(),
      numberVolunteers: Number(document.getElementById('tf-volunteers').value),
      encouragement:    Number(document.getElementById('tf-reward').value) || 0,
      latitude:         Number(document.getElementById('tf-lat').value),
      longitude:        Number(document.getElementById('tf-lng').value),
    };

    if (editingId) {
      await tasksApi.update(editingId, payload);

      // Загружаем новые изображения в существующую коллекцию
      const imageInput = document.getElementById('tf-images');
      const newImages  = imageInput ? Array.from(imageInput.files) : [];
      if (newImages.length) {
        // fileCollectionId берём из первого изображения (все в одной коллекции)
        const collectionId = editingTask?.images?.[0]?.fileCollectionId
                          ?? editingTask?.fileCollectionId;
        if (collectionId) {
          const uploadResults = await Promise.allSettled(
            newImages.map(f => dataApi.uploadToCollection(collectionId, f))
          );
          const failed = uploadResults.filter(r => r.status === 'rejected').length;
          if (failed) toast.warning(`${failed} из ${newImages.length} изображений не удалось загрузить`);
        } else {
          toast.warning('Не удалось добавить изображения: коллекция не определена. Попробуйте пересоздать задачу.');
        }
      }

      // Удаляем картинки, помеченные ×
      if (removedImageIds.size) {
        const ids = [...removedImageIds];
        const results = await Promise.allSettled(ids.map(id => dataApi.deleteFile(id)));
        const failed = results.filter(r => r.status === 'rejected').length;
        if (failed) toast.warning(`Часть изображений (${failed}) не удалось удалить`);
        removedImageIds.clear();
      }

      toast.success('Задача обновлена');
    } else {
      const imageInput = document.getElementById('tf-images');
      const images = imageInput ? Array.from(imageInput.files) : [];
      await tasksApi.create(payload, images);
      toast.success('Задача создана');
    }

    closeModal(modalForm);
    await loadTasks();
  } catch (err) {
    toast.error(err.message);
  } finally {
    setLoading('btn-submit-form', false);
  }
});

// ── Delete ────────────────────────────────────────────────────────────────────
document.getElementById('btn-confirm-delete').addEventListener('click', async () => {
  if (!deletingId) return;
  setLoading('btn-confirm-delete', true);
  try {
    await tasksApi.delete(deletingId);
    toast.success('Задача удалена');
    closeModal(modalConfirm);
    await loadTasks();
  } catch (e) {
    toast.error(e.message);
  } finally {
    setLoading('btn-confirm-delete', false);
    deletingId = null;
  }
});
document.getElementById('btn-cancel-delete').addEventListener('click', () => closeModal(modalConfirm));

// ── Modal helpers ─────────────────────────────────────────────────────────────
function openModal(el) {
  el.hidden = false;
  document.body.style.overflow = 'hidden';
}
function closeModal(el) {
  el.hidden = true;
  document.body.style.overflow = '';
}

// close on overlay click
[modalDetail, modalForm, modalConfirm].forEach(m => {
  m.addEventListener('click', e => { if (e.target === m) closeModal(m); });
});
document.getElementById('btn-close-detail').addEventListener('click', () => closeModal(modalDetail));
document.getElementById('btn-close-form').addEventListener('click',   () => closeModal(modalForm));
document.getElementById('btn-cancel-form').addEventListener('click',  () => closeModal(modalForm));

// Esc key
document.addEventListener('keydown', e => {
  if (e.key === 'Escape') [modalDetail, modalForm, modalConfirm].forEach(closeModal);
});

// ── Controls ──────────────────────────────────────────────────────────────────
document.getElementById('btn-create-task').addEventListener('click', () => openForm());

modeToggle.querySelectorAll('.toggle-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    modeToggle.querySelectorAll('.toggle-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    mode = btn.dataset.mode;
    renderTasks();
  });
});

searchInput.addEventListener('input', () => renderTasks());
filterStatus.addEventListener('change', () => renderTasks());
filterSort.addEventListener('change',   () => renderTasks());

// Logout
document.getElementById('btn-logout').addEventListener('click', async () => {
  const refresh = tokenStore.getRefresh();
  tokenStore.clear();
  if (refresh) await authApi.logout(refresh).catch(() => {});
  window.location.href = 'index.html#login';
});

// ── Notifications panel ──────────────────────────────────────────────────────
const notifBtn    = document.getElementById('btn-notifications');
const notifPanel  = document.getElementById('notif-panel');
const notifList   = document.getElementById('notif-list');
const notifBadge  = document.getElementById('notif-badge');
const notifClear  = document.getElementById('btn-notif-clear');

let confirmations = [];   // unified list: incoming + outgoing
let dismissedIds  = new Set(JSON.parse(localStorage.getItem('nirbi_dismissed_confirmations') || '[]'));

function saveDismissed() {
  localStorage.setItem('nirbi_dismissed_confirmations', JSON.stringify([...dismissedIds]));
}

function unreadCount() {
  return confirmations.filter(c => {
    if (dismissedIds.has(c.id)) return false;
    // Только входящие (где я reviewer) — это то, что требует моей реакции
    if (String(c.reviewerId) !== String(currentUserId)) return false;
    const status = (c.status || '').toLowerCase();
    return isPending(c.status);
  }).length;
}

function refreshBadge() {
  const n = unreadCount();
  if (n > 0) {
    notifBadge.hidden = false;
    notifBadge.textContent = n > 99 ? '99+' : String(n);
  } else {
    notifBadge.hidden = true;
  }
}

function formatNotifDate(d) {
  try {
    return new Date(d).toLocaleString('ru-RU', {
      day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit',
    });
  } catch { return ''; }
}

function renderNotifications() {
  if (!confirmations.length) {
    notifList.innerHTML = '<div class="notif-empty">Нет уведомлений</div>';
    refreshBadge();
    return;
  }

  // В панели — только то, где я reviewer (входящие заявки на МОИ задачи).
  // Свои отправленные/полученные ответы пользователь смотрит на /confirmations.html
  const incoming = confirmations.filter(c => String(c.reviewerId) === String(currentUserId));

  if (!incoming.length) {
    notifList.innerHTML = '<div class="notif-empty">Нет уведомлений</div>';
    refreshBadge();
    return;
  }

  // Сортируем новые сверху
  const sorted = [...incoming].sort((a, b) =>
    new Date(b.createdAt || 0) - new Date(a.createdAt || 0)
  );

  notifList.innerHTML = sorted.map(c => {
    const status    = (c.status || '').toLowerCase();
    const taskName  = c.metaData?.taskName || 'задача';
    const applicant = c.metaData?.applicantUsername || '';

    let title = '';
    let body  = '';
    let actions = '';

    if (isPending(c.status)) {
      title = 'Новая заявка на участие';
      body  = `${applicant ? `<strong>${applicant}</strong> хочет` : 'Кто-то хочет'} присоединиться к задаче «${taskName}»`;
      actions = `
        <button class="notif-btn notif-btn-accept" data-action="accept" data-id="${c.id}">Принять</button>
        <button class="notif-btn notif-btn-reject" data-action="reject" data-id="${c.id}">Отклонить</button>`;
    } else if (status === 'revoked') {
      title = 'Отклик отозван';
      body  = `${applicant ? `<strong>${applicant}</strong> отозвал` : 'Пользователь отозвал'} заявку на «${taskName}»`;
    } else if (status === 'accepted') {
      title = 'Вы приняли заявку';
      body  = `${applicant ? `<strong>${applicant}</strong> — задача` : 'Задача'} «${taskName}»`;
    } else if (status === 'rejected') {
      title = 'Вы отклонили заявку';
      body  = `Задача «${taskName}»`;
    } else if (status === 'expired') {
      title = 'Заявка истекла';
      body  = `Без ответа по задаче «${taskName}»`;
    } else {
      title = c.status || 'Заявка';
      body  = `По задаче «${taskName}»`;
    }

    const dismissedClass = dismissedIds.has(c.id) ? 'notif-item-dismissed' : '';
    return `
      <div class="notif-item ${dismissedClass}" data-id="${c.id}">
        <div class="notif-item-head">
          <span class="notif-item-title">${title}</span>
          <span class="notif-item-date">${formatNotifDate(c.createdAt)}</span>
        </div>
        <div class="notif-item-body">${body}</div>
        ${actions ? `<div class="notif-item-actions">${actions}</div>` : ''}
      </div>`;
  }).join('');

  // Click handlers for action buttons
  notifList.querySelectorAll('.notif-btn').forEach(b => {
    b.addEventListener('click', async ev => {
      ev.stopPropagation();
      const id     = b.dataset.id;
      const action = b.dataset.action;
      b.disabled = true;
      try {
        if (action === 'accept') {
          await confirmationsApi.respond(id, true);
          toast.success('Заявка принята');
        } else if (action === 'reject') {
          await confirmationsApi.respond(id, false, 'Отклонено организатором');
          toast.info('Заявка отклонена');
        } else if (action === 'revoke') {
          await confirmationsApi.revoke(id, currentUserId);
          toast.info('Заявка отозвана');
        }
        await loadConfirmations();
      } catch (e) {
        toast.error(e.message || 'Ошибка операции');
      } finally {
        b.disabled = false;
      }
    });
  });

  refreshBadge();
}

async function loadConfirmations() {
  if (!currentUserId) return;
  try {
    // Параллельно: incoming + outgoing
    const [incoming, outgoing] = await Promise.all([
      confirmationsApi.getByReviewer(currentUserId).catch(() => []),
      confirmationsApi.getByInitiator(currentUserId).catch(() => []),
    ]);
    const inc = Array.isArray(incoming) ? incoming : [];
    const out = Array.isArray(outgoing) ? outgoing : [];

    // Объединяем + дедуп по id
    const seen = new Set();
    confirmations = [...inc, ...out].filter(c => {
      if (!c?.id || seen.has(c.id)) return false;
      seen.add(c.id);
      // оставляем только нужный тип
      const t = c.confirmationType || '';
      return !t || t === CONFIRMATION_TYPES.RESPOND_TO_MINOR_TASK;
    });

    renderNotifications();
  } catch (e) {
    console.error('Failed to load confirmations:', e);
  }
}

// Toggle panel
notifBtn.addEventListener('click', e => {
  e.stopPropagation();
  notifPanel.hidden = !notifPanel.hidden;
});
document.addEventListener('click', e => {
  if (!notifPanel.hidden && !notifPanel.contains(e.target) && !notifBtn.contains(e.target)) {
    notifPanel.hidden = true;
  }
});

// Mark all as dismissed
notifClear.addEventListener('click', () => {
  confirmations.forEach(c => dismissedIds.add(c.id));
  saveDismissed();
  renderNotifications();
});

// ── SignalR realtime hooks ────────────────────────────────────────────────────
function handleIncomingConfirmation(payload) {
  const taskName  = payload?.metaData?.taskName          || 'задаче';
  const applicant = payload?.metaData?.applicantUsername || 'Кто-то';
  const cId       = payload?.id;

  // Оптимистичное обновление локального состояния, чтобы панель тоже подхватила
  if (cId) {
    const idx = confirmations.findIndex(c => c.id === cId);
    if (idx >= 0) confirmations[idx] = { ...confirmations[idx], ...payload };
    else          confirmations.push(payload);
    renderNotifications();
  }

  // Actionable toast с кнопками Принять / Отклонить прямо в уведомлении
  if (cId) {
    toast.action({
      type:     'info',
      title:    'Новая заявка на участие',
      message:  `<strong>${applicant}</strong> хочет присоединиться к «${taskName}»`,
      duration: 0,  // не закрывать автоматически
      actions: [
        {
          label:   'Принять',
          variant: 'success',
          onClick: async dismiss => {
            try {
              await confirmationsApi.respond(cId, true);
              toast.success('Заявка принята');
              await loadConfirmations();
              dismiss();
            } catch (e) {
              toast.error(e.message || 'Не удалось принять заявку');
            }
          },
        },
        {
          label:   'Отклонить',
          variant: 'danger',
          onClick: async dismiss => {
            try {
              await confirmationsApi.respond(cId, false, 'Отклонено организатором');
              toast.info('Заявка отклонена');
              await loadConfirmations();
              dismiss();
            } catch (e) {
              toast.error(e.message || 'Не удалось отклонить заявку');
            }
          },
        },
      ],
    });
  } else {
    // Без id — просто инфо-toast
    toast.info(`Новый отклик${applicant ? ` от ${applicant}` : ''}${taskName ? ` на «${taskName}»` : ''}`);
  }

  // Фоновая синхронизация (на случай если бэк ещё не успел закоммитить на момент SignalR)
  setTimeout(loadConfirmations, 500);
}

function handleConfirmationRespond(payload) {
  const status   = (payload?.status || '').toLowerCase();
  const taskName = payload?.metaData?.taskName || '';
  if (status === 'accepted') {
    toast.success(`Ваша заявка принята${taskName ? ` («${taskName}»)` : ''}`);
  } else if (status === 'rejected') {
    toast.warning(`Ваша заявка отклонена${taskName ? ` («${taskName}»)` : ''}`);
  } else {
    toast.info('Заявка обновлена');
  }
  loadConfirmations();
}

function handleConfirmationRevoked(payload) {
  const taskName  = payload?.metaData?.taskName          || '';
  const applicant = payload?.metaData?.applicantUsername || '';
  toast.info(
    `${applicant ? applicant : 'Пользователь'} отозвал заявку${taskName ? ` на «${taskName}»` : ''}`
  );
  loadConfirmations();
}

onNotification('ShowConfirmationCreated',  handleIncomingConfirmation);
onNotification('ShowConfirmationRespond',  handleConfirmationRespond);
onNotification('ShowConfirmationRevoked',  handleConfirmationRevoked);

// ── Boot ─────────────────────────────────────────────────────────────────────
initMainMap();
loadStatuses();
loadTasks();
loadConfirmations();
startNotifications();

// Если пришли со страницы подтверждений — сразу открываем нужную задачу
const _urlParams = new URLSearchParams(window.location.search);
const _openTaskId = _urlParams.get('openTask');
if (_openTaskId) {
  // Убираем параметр из URL, чтобы не открывалось при перезагрузке
  history.replaceState({}, '', window.location.pathname);
  // openDetail сам загрузит задачу через getById — не ждём loadTasks()
  openDetail(_openTaskId);
}
