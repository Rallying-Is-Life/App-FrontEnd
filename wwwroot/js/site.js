// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleTheme(){ var t=document.documentElement.getAttribute('data-bs-theme')||'light'; var n=t==='light'?'dark':'light'; document.documentElement.setAttribute('data-bs-theme',n); localStorage.setItem('theme',n); }
(function(){ var s=localStorage.getItem('theme')||'light'; document.documentElement.setAttribute('data-bs-theme',s); })();
