// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
async function updateUnreadBadge() {
    try {
        const res   = await fetch('/Messaging/UnreadCount');
        if (!res.ok) return;
        const data  = await res.json();
        const badge = document.getElementById('msg-badge');
        if (!badge) return;
        badge.textContent    = data.count;
        badge.style.display  = data.count > 0 ? 'inline' : 'none';
    } catch (e) {}
}

// Run immediately on every page load, then every 30 seconds
document.addEventListener('DOMContentLoaded', () => {
    updateUnreadBadge();
    setInterval(updateUnreadBadge, 30000);
});