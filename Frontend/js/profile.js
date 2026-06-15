import { toast }       from './toast.js';
import { tokenStore }  from './tokenStore.js';
import { authApi }     from './api.js';
import { profileApi, PROFILE_FIELDS }  from './profileApi.js';
import { startNotifications } from './notifications.js';

// ── Guard ────────────────────────────────────────────────────────────────────
if (!tokenStore.hasSession()) {
  window.location.href = 'index.html#login';
}

const currentUserId = tokenStore.getUserId();
if (!currentUserId) {
  window.location.href = 'index.html#login';
}

// ── State ────────────────────────────────────────────────────────────────────
let profile = null;       // текущие данные профиля (UserProfile)
let activeTab = 'info';   // 'info' | 'education' | 'security'

// ── DOM ──────────────────────────────────────────────────────────────────────
const avatarInitials  = document.getElementById('avatar-initials');
const profileFullName = document.getElementById('profile-full-name');
const profileEmail    = document.getElementById('profile-email');
const profilePhone    = document.getElementById('profile-phone-display');

const tabBtns = document.querySelectorAll('.profile-tab-btn');
const tabPanes = document.querySelectorAll('.profile-tab-pane');

const formInfo      = document.getElementById('form-info');
const formEducation = document.getElementById('form-education');
const formSecurity  = document.getElementById('form-security');

// ── Helpers ───────────────────────────────────────────────────────────────────
function val(id) {
  return (document.getElementById(id)?.value ?? '').trim();
}

function setFieldValue(id, value) {
  const el = document.getElementById(id);
  if (el) el.value = value ?? '';
}

function setError(inputId, errorId, msg) {
  const el = document.getElementById(inputId);
  const er = document.getElementById(errorId);
  if (el) el.classList.toggle('error', !!msg);
  if (er) er.textContent = msg || '';
}

function clearErrors(...pairs) {
  pairs.forEach(([i, e]) => setError(i, e, ''));
}

function setLoading(btnId, loading) {
  const btn = document.getElementById(btnId);
  if (!btn) return;
  btn.disabled = loading;
  const lbl = btn.querySelector('.btn-label');
  const sp  = btn.querySelector('.btn-spinner');
  if (lbl) lbl.hidden = loading;
  if (sp)  sp.hidden  = !loading;
}

function initials(p) {
  if (!p) return '?';
  const parts = [p.firstName, p.secondName, p.lastName]
    .filter(Boolean)
    .map(s => s[0].toUpperCase());
  return [parts[0], parts[parts.length > 1 ? parts.length - 1 : 1]].filter(Boolean).join('') || '?';
}

function fullName(p) {
  if (!p) return '—';
  return [p.lastName, p.firstName, p.secondName].filter(Boolean).join(' ') || '—';
}

// ── Load & render profile ─────────────────────────────────────────────────────
async function loadProfile() {
  try {
    showSkeleton(true);
    // Запрашиваем полный набор полей (включая мессенджеры)
    profile = await profileApi.getById(currentUserId, PROFILE_FIELDS);
    renderProfile();
  } catch (e) {
    toast.error('Не удалось загрузить профиль');
    console.error('[profile] load error:', e);
  } finally {
    showSkeleton(false);
  }
}

function showSkeleton(on) {
  document.querySelectorAll('.profile-skeleton').forEach(el => {
    el.classList.toggle('is-loading', on);
  });
}

function renderProfile() {
  if (!profile) return;

  // Header
  avatarInitials.textContent  = initials(profile);
  profileFullName.textContent = fullName(profile);
  profileEmail.textContent    = profile.email   || '—';
  profilePhone.textContent    = profile.phone   || '—';

  // Info tab fields
  setFieldValue('pf-firstname',  profile.firstName);
  setFieldValue('pf-secondname', profile.secondName);
  setFieldValue('pf-lastname',   profile.lastName);
  setFieldValue('pf-phone',      profile.phone);
  setFieldValue('pf-email',      profile.email);
  setFieldValue('pf-birthdate',  profile.birthDate);
  setFieldValue('pf-city',       profile.city);
  setFieldValue('pf-about',      profile.about);

  // Messengers
  setFieldValue('pf-tg',  profile.tg);
  setFieldValue('pf-vk',  profile.vk);
  setFieldValue('pf-max', profile.max);

  // Education tab fields
  setFieldValue('pf-edu-place',       profile.educationPlace);
  setFieldValue('pf-edu-field',       profile.educationField);
  setFieldValue('pf-edu-start',       profile.educationStartYear);
  setFieldValue('pf-edu-end',         profile.educationEndYear);
}

// ── Tab switching ─────────────────────────────────────────────────────────────
tabBtns.forEach(btn => {
  btn.addEventListener('click', () => {
    tabBtns.forEach(b => b.classList.remove('active'));
    tabPanes.forEach(p => p.hidden = true);
    btn.classList.add('active');
    activeTab = btn.dataset.tab;
    const pane = document.getElementById(`tab-${activeTab}`);
    if (pane) pane.hidden = false;
  });
});

// ── Form: Personal info ───────────────────────────────────────────────────────
formInfo.addEventListener('submit', async e => {
  e.preventDefault();
  clearErrors(
    ['pf-firstname',  'pf-firstname-error'],
    ['pf-secondname', 'pf-secondname-error'],
    ['pf-lastname',   'pf-lastname-error'],
    ['pf-phone',      'pf-phone-error'],
    ['pf-email',      'pf-email-error'],
    ['pf-current-pwd-info', 'pf-current-pwd-info-error'],
  );

  let ok = true;
  if (!val('pf-firstname') || val('pf-firstname').length < 2) {
    setError('pf-firstname', 'pf-firstname-error', 'Введите имя (минимум 2 символа)');
    ok = false;
  }
  if (!val('pf-secondname') || val('pf-secondname').length < 2) {
    setError('pf-secondname', 'pf-secondname-error', 'Введите отчество (минимум 2 символа)');
    ok = false;
  }
  if (!val('pf-lastname') || val('pf-lastname').length < 2) {
    setError('pf-lastname', 'pf-lastname-error', 'Введите фамилию (минимум 2 символа)');
    ok = false;
  }
  const phone = val('pf-phone');
  if (phone && !/^\+?[\d\s\-()]{7,20}$/.test(phone)) {
    setError('pf-phone', 'pf-phone-error', 'Введите корректный номер телефона');
    ok = false;
  }
  const email = val('pf-email');
  if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    setError('pf-email', 'pf-email-error', 'Введите корректный email');
    ok = false;
  }
  const currentPwd = val('pf-current-pwd-info');
  if (!currentPwd) {
    setError('pf-current-pwd-info', 'pf-current-pwd-info-error', 'Введите текущий пароль для подтверждения изменений');
    ok = false;
  }
  if (!ok) return;

  setLoading('btn-save-info', true);
  try {
    await profileApi.update(currentUserId, {
      Id:              currentUserId,
      FirstName:       val('pf-firstname')  || null,
      SecondName:      val('pf-secondname') || null,
      LastName:        val('pf-lastname')   || null,
      Phone:           val('pf-phone')      || null,
      Email:           val('pf-email')      || null,
      BirthDate:       val('pf-birthdate')  || null,
      City:            val('pf-city')       || null,
      About:           val('pf-about')      || null,
      // Мессенджеры — имена строго как в UpdateUserRequest/UserProfile (vk/tg/max),
      // чтобы байндинг не зависел от политики именования JSON на бэке.
      vk:              val('pf-vk')  || null,
      tg:              val('pf-tg')  || null,
      max:             val('pf-max') || null,
      CurrentPassword: currentPwd,
    });
    toast.success('Профиль обновлён');
    document.getElementById('pf-current-pwd-info').value = '';
    await loadProfile();
  } catch (err) {
    toast.error(err.message || 'Не удалось сохранить профиль');
  } finally {
    setLoading('btn-save-info', false);
  }
});

// ── Form: Education ───────────────────────────────────────────────────────────
formEducation.addEventListener('submit', async e => {
  e.preventDefault();
  clearErrors(
    ['pf-edu-start', 'pf-edu-start-error'],
    ['pf-edu-end',   'pf-edu-end-error'],
    ['pf-current-pwd-edu', 'pf-current-pwd-edu-error'],
  );

  let ok = true;
  const startYear = val('pf-edu-start');
  const endYear   = val('pf-edu-end');
  const yearRe    = /^\d{4}$/;

  if (startYear && !yearRe.test(startYear)) {
    setError('pf-edu-start', 'pf-edu-start-error', 'Введите год в формате ГГГГ');
    ok = false;
  }
  if (endYear && !yearRe.test(endYear)) {
    setError('pf-edu-end', 'pf-edu-end-error', 'Введите год в формате ГГГГ');
    ok = false;
  }
  if (startYear && endYear && Number(endYear) < Number(startYear)) {
    setError('pf-edu-end', 'pf-edu-end-error', 'Год окончания не может быть раньше начала');
    ok = false;
  }
  const currentPwd = val('pf-current-pwd-edu');
  if (!currentPwd) {
    setError('pf-current-pwd-edu', 'pf-current-pwd-edu-error', 'Введите текущий пароль для подтверждения изменений');
    ok = false;
  }
  if (!ok) return;

  setLoading('btn-save-edu', true);
  try {
    await profileApi.update(currentUserId, {
      Id:                  currentUserId,
      EducationPlace:      val('pf-edu-place') || null,
      EducationField:      val('pf-edu-field') || null,
      EducationStartYear:  startYear || null,
      EducationEndYear:    endYear   || null,
      CurrentPassword:     currentPwd,
    });
    toast.success('Образование обновлено');
    document.getElementById('pf-current-pwd-edu').value = '';
    await loadProfile();
  } catch (err) {
    toast.error(err.message || 'Не удалось сохранить данные об образовании');
  } finally {
    setLoading('btn-save-edu', false);
  }
});

// ── Form: Security (change password) ─────────────────────────────────────────
formSecurity.addEventListener('submit', async e => {
  e.preventDefault();
  clearErrors(
    ['pf-pwd-current',  'pf-pwd-current-error'],
    ['pf-pwd-new',      'pf-pwd-new-error'],
    ['pf-pwd-confirm',  'pf-pwd-confirm-error'],
  );

  let ok = true;
  if (!val('pf-pwd-current')) {
    setError('pf-pwd-current', 'pf-pwd-current-error', 'Введите текущий пароль');
    ok = false;
  }
  if (!val('pf-pwd-new') || val('pf-pwd-new').length < 6) {
    setError('pf-pwd-new', 'pf-pwd-new-error', 'Новый пароль: минимум 6 символов');
    ok = false;
  }
  if (val('pf-pwd-new') !== val('pf-pwd-confirm')) {
    setError('pf-pwd-confirm', 'pf-pwd-confirm-error', 'Пароли не совпадают');
    ok = false;
  }
  if (!ok) return;

  setLoading('btn-change-pwd', true);
  try {
    await profileApi.update(currentUserId, {
      Id:              currentUserId,
      CurrentPassword: val('pf-pwd-current'),
      NewPassword:     val('pf-pwd-new'),
    });
    toast.success('Пароль изменён');
    formSecurity.reset();
  } catch (err) {
    toast.error(err.message || 'Не удалось изменить пароль');
  } finally {
    setLoading('btn-change-pwd', false);
  }
});

// ── Password toggles ──────────────────────────────────────────────────────────
function setupPwdToggle(btnId, inputId) {
  const btn   = document.getElementById(btnId);
  const input = document.getElementById(inputId);
  if (!btn || !input) return;
  btn.addEventListener('click', () => {
    input.type = input.type === 'password' ? 'text' : 'password';
  });
}
setupPwdToggle('toggle-current-pwd-info', 'pf-current-pwd-info');
setupPwdToggle('toggle-current-pwd-edu',  'pf-current-pwd-edu');
setupPwdToggle('toggle-pwd-current',      'pf-pwd-current');
setupPwdToggle('toggle-pwd-new',          'pf-pwd-new');
setupPwdToggle('toggle-pwd-confirm',      'pf-pwd-confirm');

// ── Logout ────────────────────────────────────────────────────────────────────
document.getElementById('btn-logout')?.addEventListener('click', async () => {
  const refresh = tokenStore.getRefresh();
  tokenStore.clear();
  if (refresh) await authApi.logout(refresh).catch(() => {});
  window.location.href = 'index.html#login';
});

// ── Boot ──────────────────────────────────────────────────────────────────────
loadProfile();
startNotifications();
