const API_URL = "https://localhost:7006/api";
let subastaSeleccionadaId = null; // guardamos acá el id de la subasta que se está por pujar

async function cargarSubastas() {
    try {
        const res = await fetch(`${API_URL}/subastas?estado=ACTIVA`);
        const subastas = await res.json();
        const lista = document.getElementById("lista");

        if (subastas.length === 0) {
            lista.innerHTML = "<p class='text-muted'>No hay subastas activas en este momento.</p>";
            return;
        }

        lista.innerHTML = subastas.map(subasta => `
            <div class="col-md-4 mb-3">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">${subasta.titulo}</h5>
                        <p class="card-text">${subasta.descripcion}</p>
                        <p class="card-text">Precio inicial: $${subasta.precioInicial}</p>
                        <p class="card-text"><strong>Oferta actual: $${subasta.ofertaActual}</strong></p>
                        <button class="btn btn-primary w-100" onclick="abrirModalPuja(${subasta.id})">Pujar</button>
                    </div>
                </div>
            </div>
        `).join("");
    } catch (error) {
        console.error("Error al cargar las subastas:", error);
        document.getElementById("lista").innerHTML = "<p class='text-danger'>Error al cargar las subastas. Por favor, inténtelo de nuevo más tarde.</p>";
    }
}

// Se abre cuando clickeás "Pujar" en cualquier card
function abrirModalPuja(subastaId) {
    subastaSeleccionadaId = subastaId;
    document.getElementById("alertaPuja").classList.add("d-none"); // ocultar error de un intento anterior
    document.getElementById("inputMonto").value = "";

    const modal = new bootstrap.Modal(document.getElementById("modalPujar"));
    modal.show();
}

// Se ejecuta al clickear "Confirmar puja" dentro del modal
async function confirmarPuja() {
    const compradorId = parseInt(document.getElementById("selectComprador").value);
    const monto = parseFloat(document.getElementById("inputMonto").value);
    const alerta = document.getElementById("alertaPuja");

    if (!monto || monto <= 0) {
        alerta.textContent = "Ingresá un monto válido.";
        alerta.classList.remove("d-none");
        return;
    }

    try {
        const res = await fetch(`${API_URL}/subastas/${subastaSeleccionadaId}/pujas`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ compradorId, monto })
        });

        if (!res.ok) {
            const errorBody = await res.json();
            alerta.textContent = errorBody.error || "Ocurrió un error al registrar la puja.";
            alerta.classList.remove("d-none");
            return; // no cerramos el modal, para que puedan corregir
        }

        // Éxito: cerrar el modal y refrescar la lista
        bootstrap.Modal.getInstance(document.getElementById("modalPujar")).hide();
        cargarSubastas();

    } catch (error) {
        alerta.textContent = "No se pudo conectar con el servidor.";
        alerta.classList.remove("d-none");
    }
}

document.getElementById("btnConfirmarPuja").addEventListener("click", confirmarPuja);

cargarSubastas();