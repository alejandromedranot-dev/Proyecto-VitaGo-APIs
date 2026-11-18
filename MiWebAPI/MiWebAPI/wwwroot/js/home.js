// Muestra el nombre del usuario en el navbar si ya inició sesión (auth.js define mostrarUsuario)
document.addEventListener("DOMContentLoaded", () => {
    if (typeof mostrarUsuario === "function") {
        mostrarUsuario("nombreUsuario");
    }
});

function buscarMedicos() {
    const ubicacion = document.querySelectorAll(".vg-field-input")[0].value;
    const especialidad = document.querySelectorAll(".vg-field-input")[1].value;
    const fecha = document.querySelectorAll(".vg-field-input")[2].value;
    // TODO: conectar con el endpoint de búsqueda de la API (GET /api/medicos?...)
    console.log("Buscar médicos:", { ubicacion, especialidad, fecha });
}

function agendarCita(medicoId) {
    window.location.href = `agendar-cita.html?medico=${encodeURIComponent(medicoId)}`;
}