// 1. Aquí guardamos todo el diseño del menú ya en español y con sus enlaces
const menuHTML = `
    <nav class="vg-navbar-pistache">
        <!-- Logo (Redirige siempre al Inicio) -->
        <a href="index.html" style="display:flex; align-items:center; gap:12px; text-decoration:none;">
            <div style="background:#ecfdf5; width:40px; height:40px; border-radius:10px; display:flex; align-items:center; justify-content:center;">
                <img src="/assets/img/vitago-icon.png" alt="vitaGo" style="width:24px;">
            </div>
            <div style="line-height:1;">
                <span style="font-weight:800; font-size:18px; color:#0f172a; display:block;">vitaGo</span>
                <span style="font-size:11px; font-weight:500; color:#64748b;">Citas médicas, simplificadas.</span>
            </div>
        </a>

        <!-- Links centrales en Español con sus rutas -->
        <div class="vg-nav-pistache-links d-none d-lg-flex">
            <a href="index.html">Inicio</a>
            <a href="index.html#medicos">Doctores</a>
            <a href="index.html#especialidades">Especialidades</a>
        </div>

        <!-- Botones de Iniciar sesión y Registrarse -->
        <div style="display:flex; gap:10px; align-items:center;">
            <a href="login.html" class="btn" style="background:#fff; color:#0f172a; border:1px solid #e2e8f0; border-radius:20px; font-weight:600; padding:6px 20px;">
                Iniciar sesión
            </a>
            <!-- AQUÍ ESTÁ LA CORRECCIÓN: Ahora apunta a registro-medico.html -->
            <a href="registro-medico.html" class="btn" style="background:#10b981; color:#fff; border-radius:20px; font-weight:600; padding:6px 20px; border: 1px solid #10b981;">
                Registrarse
            </a>
        </div>
    </nav>
`;

// 2. Esta función inyecta el menú cuando la página carga
document.addEventListener("DOMContentLoaded", () => {
    const contenedorMenu = document.getElementById("menu-dinamico");
    if (contenedorMenu) {
        contenedorMenu.innerHTML = menuHTML;
    }
});