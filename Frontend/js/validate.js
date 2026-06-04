function setError(inputId, errorId, message) {
  const input = document.getElementById(inputId);
  const error = document.getElementById(errorId);
  if (input) input.classList.toggle('error', !!message);
  if (error) error.textContent = message || '';
}

function clearErrors(...pairs) {
  pairs.forEach(([i, e]) => setError(i, e, ''));
}

function val(id) {
  return (document.getElementById(id)?.value || '').trim();
}

export function validateLogin() {
  clearErrors(
    ['login-username', 'login-username-error'],
    ['login-password', 'login-password-error'],
  );
  let ok = true;
  if (!val('login-username')) {
    setError('login-username', 'login-username-error', 'Введите имя пользователя или email');
    ok = false;
  }
  if (!val('login-password')) {
    setError('login-password', 'login-password-error', 'Введите пароль');
    ok = false;
  }
  return ok;
}

export function validateRegister() {
  clearErrors(
    ['reg-fname',     'reg-fname-error'],
    ['reg-sname',     'reg-sname-error'],
    ['reg-lname',     'reg-lname-error'],
    ['reg-phone',     'reg-phone-error'],
    ['reg-email',     'reg-email-error'],
    ['reg-password',  'reg-password-error'],
    ['reg-password2', 'reg-password2-error'],
  );
  let ok = true;

  if (!val('reg-fname') || val('reg-fname').length < 2) {
    setError('reg-fname', 'reg-fname-error', 'Введите имя (минимум 2 символа)');
    ok = false;
  }
  if (!val('reg-sname') || val('reg-sname').length < 2) {
    setError('reg-sname', 'reg-sname-error', 'Введите отчество (минимум 2 символа)');
    ok = false;
  }
  if (!val('reg-lname') || val('reg-lname').length < 2) {
    setError('reg-lname', 'reg-lname-error', 'Введите фамилию (минимум 2 символа)');
    ok = false;
  }

  const phone = val('reg-phone');
  if (!phone) {
    setError('reg-phone', 'reg-phone-error', 'Введите номер телефона');
    ok = false;
  } else if (!/^\+?[\d\s\-()]{7,20}$/.test(phone)) {
    setError('reg-phone', 'reg-phone-error', 'Введите корректный номер телефона');
    ok = false;
  }

  const email = val('reg-email');
  if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    setError('reg-email', 'reg-email-error', 'Введите корректный email');
    ok = false;
  }

  if (!val('reg-password') || val('reg-password').length < 6) {
    setError('reg-password', 'reg-password-error', 'Минимум 6 символов');
    ok = false;
  }
  if (val('reg-password') !== val('reg-password2')) {
    setError('reg-password2', 'reg-password2-error', 'Пароли не совпадают');
    ok = false;
  }
  return ok;
}

export function validateForgot() {
  clearErrors(['forgot-email', 'forgot-email-error']);
  const email = val('forgot-email');
  if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    setError('forgot-email', 'forgot-email-error', 'Введите корректный email');
    return false;
  }
  return true;
}

export { val };
