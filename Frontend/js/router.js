const routes = {};
let currentScreen = null;

export function route(hash, handler) {
  routes[hash] = handler;
}

function navigate(hash) {
  const handler = routes[hash] || routes['#login'];
  if (handler) handler();
}

export function initRouter() {
  window.addEventListener('hashchange', () => navigate(window.location.hash));
  document.addEventListener('click', e => {
    const a = e.target.closest('a[href^="#"]');
    if (!a) return;
    e.preventDefault();
    window.location.hash = a.getAttribute('href');
  });
  navigate(window.location.hash || '#login');
}

export function showScreen(id) {
  if (currentScreen === id) return;
  document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
  const el = document.getElementById(`screen-${id}`);
  if (el) el.classList.add('active');
  currentScreen = id;
}

// main-layout удалён — функция больше не нужна, оставляем для совместимости
export function showAuthLayout(_show) {
  // index.html содержит только auth-layout, навигация между страницами через href
}
