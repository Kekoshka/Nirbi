/* ── Burger / Sidebar toggle (mobile) ──────────────────────────────────── */
(function () {
  'use strict';

  const sidebar  = document.querySelector('.sidebar');
  const overlay  = document.querySelector('.sidebar-overlay');
  const burger   = document.querySelector('.btn-burger');

  if (!sidebar || !burger) return;

  function openSidebar() {
    sidebar.classList.add('is-open');
    if (overlay) overlay.classList.add('is-visible');
    burger.setAttribute('aria-expanded', 'true');
    // swap icon to ✕
    burger.querySelector('.icon-open')  && (burger.querySelector('.icon-open').style.display  = 'none');
    burger.querySelector('.icon-close') && (burger.querySelector('.icon-close').style.display = '');
  }

  function closeSidebar() {
    sidebar.classList.remove('is-open');
    if (overlay) overlay.classList.remove('is-visible');
    burger.setAttribute('aria-expanded', 'false');
    burger.querySelector('.icon-open')  && (burger.querySelector('.icon-open').style.display  = '');
    burger.querySelector('.icon-close') && (burger.querySelector('.icon-close').style.display = 'none');
  }

  burger.addEventListener('click', () => {
    sidebar.classList.contains('is-open') ? closeSidebar() : openSidebar();
  });

  if (overlay) overlay.addEventListener('click', closeSidebar);

  // Close on Escape
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeSidebar();
  });

  // Close when a nav link is clicked (SPA-style navigation on mobile)
  sidebar.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', () => {
      if (window.innerWidth <= 640) closeSidebar();
    });
  });
})();
