// Contenedor de toasts: se crea una sola vez, en cualquier página que lo necesite
function crearContenedorToasts() {
    if (document.getElementById("toast-container")) return;

    const contenedor = document.createElement("div");
    contenedor.id = "toast-container";
    contenedor.className = "toast-container position-fixed top-0 end-0 p-3";
    contenedor.style.zIndex = "1080";
    document.body.appendChild(contenedor);
}

/**
 * Muestra un toast de éxito o error.
 * @param {string} mensaje - el texto a mostrar
 * @param {"exito"|"error"} tipo - define el color
 */
function mostrarToast(mensaje, tipo = "exito") {
    crearContenedorToasts();

    const colorClase = tipo === "exito" ? "text-bg-success" : "text-bg-danger";
    const icono = tipo === "exito" ? "✅" : "⚠️";

    const toastEl = document.createElement("div");
    toastEl.className = `toast align-items-center ${colorClase} border-0`;
    toastEl.setAttribute("role", "alert");
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${icono} ${mensaje}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    document.getElementById("toast-container").appendChild(toastEl);

    const toastBootstrap = new bootstrap.Toast(toastEl, { delay: 4000 }); // se oculta solo a los 4 seg
    toastBootstrap.show();

    // Limpieza: sacar el elemento del DOM una vez que termina de ocultarse
    toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
}