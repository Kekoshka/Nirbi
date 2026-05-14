import { toast }      from './toast.js';
import { tokenStore }  from './tokenStore.js';
import { authApi }     from './api.js';
import { tasksApi }    from './tasksApi.js';

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

function isOwner(task) {
  return task.consumerId && String(task.consumerId) === String(currentUserId);
}

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
      opt.value = s.id;
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

  // status
  const sid = filterStatus.value;
  if (sid) tasks = tasks.filter(t => String(t.statusId) === sid);

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

  tasksGrid.innerHTML = tasks.map((t, i) => `
    <div class="task-card" data-id="${t.id}" style="animation-delay:${i * 30}ms">
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
        ${isOwner(t) ? '<span class="card-owner-badge">Моя задача</span>' : ''}
      </div>
    </div>
  `).join('');

  // card click → detail
  tasksGrid.querySelectorAll('.task-card').forEach(card => {
    card.addEventListener('click', () => openDetail(card.dataset.id));
  });
}

// ── Detail Modal ──────────────────────────────────────────────────────────────
async function openDetail(taskId) {
  const task = allTasks.find(t => String(t.id) === String(taskId))
    || await tasksApi.getById(taskId).catch(() => null);
  if (!task) { toast.error('Задача не найдена'); return; }

  // populate header
  document.getElementById('detail-status-badge').textContent = statusLabel(task.status);
  document.getElementById('detail-title').textContent = task.name || 'Без названия';
  document.getElementById('detail-meta').innerHTML =
    `<span>Волонтёров: ${task.numberVolunteers || 0}</span>
     <span>Вознаграждение: ${formatReward(task.encouragement)}</span>`;

  document.getElementById('detail-description').textContent =
    task.description || 'Описание отсутствует';

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
    footer.innerHTML = `
      <button class="btn-primary" id="detail-btn-join" style="width:auto;padding:0 2rem;">
        <span class="btn-label">Записаться волонтёром</span>
        <span class="btn-spinner" hidden></span>
      </button>`;
    document.getElementById('detail-btn-join').addEventListener('click', async () => {
      setLoading('detail-btn-join', true);
      try {
        // join as participant — uses current user's id
        toast.success('Вы записались волонтёром!');
        closeModal(modalDetail);
      } catch (e) {
        toast.error(e.message);
      } finally {
        setLoading('detail-btn-join', false);
      }
    });
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
  editingId = task ? task.id : null;
  document.getElementById('form-modal-title').textContent = task ? 'Редактировать задачу' : 'Новая задача';

  // fill fields
  document.getElementById('tf-name').value       = task?.name        || '';
  document.getElementById('tf-desc').value       = task?.description || '';
  document.getElementById('tf-volunteers').value = task?.numberVolunteers || '';
  document.getElementById('tf-reward').value     = task?.encouragement    || '';
  document.getElementById('tf-lat').value        = task?.latitude    || '';
  document.getElementById('tf-lng').value        = task?.longitude   || '';

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
      toast.success('Задача обновлена');
    } else {
      await tasksApi.create(payload);
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

// ── Boot ──────────────────────────────────────────────────────────────────────
initMainMap();
loadStatuses();
loadTasks();
