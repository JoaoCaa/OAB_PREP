/**
 * OAB Prep — Toast notifications
 */

export function showToast(message, type = 'success', duration = 3500) {
  let root = document.getElementById('toast-root');
  if (!root) {
    root = document.createElement('div');
    root.id = 'toast-root';
    document.body.appendChild(root);
  }
  const el = document.createElement('div');
  el.className = 'toast-item';
  el.innerHTML = `<div class="toast-dot toast-${type}"></div>${message}`;
  root.appendChild(el);
  setTimeout(() => el.remove(), duration);
}
