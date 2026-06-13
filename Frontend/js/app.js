import { toast }      from './toast.js';
import { tokenStore } from './tokenStore.js';
import { authApi }    from './api.js';
import { route, initRouter, showScreen, showAuthLayout } from './router.js';
import { validateLogin, validateRegister, validateForgot, val } from './validate.js';

// ── Password toggle helper ───────────────────────────────────────────────────
function setupPasswordToggle(toggleId, inputId) {
  const btn   = document.getElementById(toggleId);
  const input = document.getElementById(inputId);
  if (!btn || !input) return;
  btn.addEventListener('click', () => {
    input.type = input.type === 'password' ? 'text' : 'password';
  });
}

// ── Button loading state ─────────────────────────────────────────────────────
function setLoading(btnId, loading) {
  const btn     = document.getElementById(btnId);
  const label   = btn?.querySelector('.btn-label');
  const spinner = btn?.querySelector('.btn-spinner');
  if (!btn) return;
  btn.disabled   = loading;
  if (label)   label.hidden   = loading;
  if (spinner) spinner.hidden = !loading;
}

// ── Session helpers ──────────────────────────────────────────────────────────
function saveSession(data, displayName) {
  // Бэкенд возвращает: { userId, accessToken, refreshToken, tokenType, expiresIn }
  tokenStore.save(data.accessToken, data.refreshToken, data.userId, displayName);
}

// ── Forms ────────────────────────────────────────────────────────────────────

// Login
document.getElementById('form-login').addEventListener('submit', async e => {
  e.preventDefault();
  if (!validateLogin()) return;
  setLoading('btn-login', true);
  try {
    const username = val('login-username');
    const data = await authApi.login(username, val('login-password'));
    // При логине имя пользователя не возвращается бэкендом отдельно,
    // сохраняем введённый логин как отображаемое имя
    saveSession(data, username);
    window.location.href = 'tasks.html';
  } catch (err) {
    toast.error(err.message);
    setLoading('btn-login', false);
  }
});

// Register — новый контракт: FName, SName, LName, phone, email, password
document.getElementById('form-register').addEventListener('submit', async e => {
  e.preventDefault();
  if (!validateRegister()) return;
  setLoading('btn-register', true);
  try {
    const data = await authApi.register({
      fName:    val('reg-fname'),
      sName:    val('reg-sname'),
      lName:    val('reg-lname'),
      phone:    val('reg-phone'),
      email:    val('reg-email'),
      password: val('reg-password'),
    });
    // Бэкенд после регистрации сразу логинит и возвращает токены
    const displayName = val('reg-email');
    saveSession(data, displayName);
    window.location.href = 'tasks.html';
  } catch (err) {
    toast.error(err.message);
    setLoading('btn-register', false);
  }
});

// Forgot password
document.getElementById('form-forgot').addEventListener('submit', async e => {
  e.preventDefault();
  if (!validateForgot()) return;
  setLoading('btn-forgot', true);
  try {
    await authApi.forgotPassword(val('forgot-email'));
    document.getElementById('form-forgot').hidden    = true;
    document.getElementById('forgot-success').hidden = false;
    document.getElementById('forgot-switch').hidden  = true;
  } catch (err) {
    toast.error(err.message);
  } finally {
    setLoading('btn-forgot', false);
  }
});

// Back button on forgot screen
document.getElementById('btn-back-forgot').addEventListener('click', () => {
  window.location.hash = '#login';
});

// ── Password toggles ─────────────────────────────────────────────────────────
setupPasswordToggle('login-pwd-toggle',  'login-password');
setupPasswordToggle('reg-pwd-toggle',    'reg-password');
setupPasswordToggle('reg-pwd2-toggle',   'reg-password2');

// ── Routes ───────────────────────────────────────────────────────────────────
route('#login',    () => showScreen('login'));
route('#register', () => showScreen('register'));
route('#forgot',   () => {
  document.getElementById('form-forgot').hidden    = false;
  document.getElementById('forgot-success').hidden = true;
  document.getElementById('forgot-switch').hidden  = false;
  showScreen('forgot');
});

// ── Boot ──────────────────────────────────────────────────────────────────────
function boot() {
  // Already has a session — skip auth, go to tasks
  if (tokenStore.hasSession()) {
    window.location.href = 'tasks.html';
    return;
  }
  showAuthLayout(true);
  initRouter();
}

boot();
