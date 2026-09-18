const API_URL = "https://localhost:7006/api";
let subastaSeleccionadaId = null; // guardamos acá el id de la subasta que se está por pujar

async function cargarSubastas() {
    await cargarSubastasActivas();
    await cargarSubastasCerradas();
}

async function cargarSubastasActivas() {
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
                        <p class="oferta-actual">Oferta actual: $${subasta.ofertaActual}</p>                        <p class="card-text">${subasta.liderNombre ? `Líder: <strong>${subasta.liderNombre}</strong>` : "Sin ofertas todavía"}</p>
                        <p class="card-text temporizador" data-fecha-fin="${subasta.fechaFin}">--:--</p>
                        <button class="btn btn-primary w-100" onclick="abrirModalPuja(${subasta.id})">Pujar</button>
                    </div>
                </div>
            </div>
        `).join("");

        iniciarTemporizadores();
    } catch (error) {
        console.error("Error al cargar las subastas activas:", error);
        document.getElementById("lista").innerHTML = "<p class='text-danger'>Error al cargar las subastas.</p>";
    }
}

async function cargarSubastasCerradas() {
    try {
        const [resFinalizadas, resDesiertas] = await Promise.all([
            fetch(`${API_URL}/subastas?estado=FINALIZADA`),
            fetch(`${API_URL}/subastas?estado=DESIERTA`)
        ]);
        const finalizadas = await resFinalizadas.json();
        const desiertas = await resDesiertas.json();
        const cerradas = [...finalizadas, ...desiertas];

        const listaCerradas = document.getElementById("listaCerradas");

        if (cerradas.length === 0) {
            listaCerradas.innerHTML = "<p class='text-muted'>Todavía no hay subastas cerradas.</p>";
            return;
        }

        listaCerradas.innerHTML = cerradas.map(subasta => {
            const claseBadge = subasta.estado === 'FINALIZADA' ? 'badge-finalizada' : 'badge-desierta';
            const textoBadge = subasta.estado === 'FINALIZADA' ? 'Finalizada' : 'Desierta';

            return `
                <div class="col-md-4 mb-3">
                    <div class="card card-cerrada">
                        <div class="card-body">
                            <h5 class="card-title text-muted">${subasta.titulo}</h5>
                            <p class="card-text">${subasta.descripcion}</p>
                            <p class="card-text">Oferta final: $${subasta.ofertaActual}</p>
                            <p class="card-text">${subasta.liderNombre ? `Ganador: <strong>${subasta.liderNombre}</strong>` : "Sin ganador"}</p>
                            <span class="badge-estado ${claseBadge}">${textoBadge}</span>
                        </div>
                    </div>
                </div>
            `;
        }).join("");
    } catch (error) {
        console.error("Error al cargar las subastas cerradas:", error);
    }
}

// Se abre cuando clickeás "Pujar" en cualquier card
function abrirModalPuja(subastaId) {
    subastaSeleccionadaId = subastaId;
    document.getElementById("inputMonto").value = "";

    const modal = new bootstrap.Modal(document.getElementById("modalPujar"));
    modal.show();

    consultarEstadoPuja(); // consulta con el usuario que esté seleccionado por defecto
}

document.getElementById("selectComprador").addEventListener("change", consultarEstadoPuja);

// Se ejecuta al clickear "Confirmar puja" dentro del modal
async function confirmarPuja() {
    const compradorId = parseInt(document.getElementById("selectComprador").value);
    const monto = parseFloat(document.getElementById("inputMonto").value);
    const btnConfirmar = document.getElementById("btnConfirmarPuja");

    if (!monto || monto <= 0) {
        mostrarToast("Ingresá un monto válido.", "error");
        return;
    }

    // Spinner: deshabilitar el botón y mostrar estado de carga
    const textoOriginal = btnConfirmar.innerHTML;
    btnConfirmar.disabled = true;
    btnConfirmar.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Procesando...`;

    try {
        const res = await fetch(`${API_URL}/subastas/${subastaSeleccionadaId}/pujas`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ compradorId, monto })
        });

        if (!res.ok) {
            const errorBody = await res.json();
            mostrarToast(errorBody.error || "Ocurrió un error al registrar la puja.", "error");
            return;
        }

        bootstrap.Modal.getInstance(document.getElementById("modalPujar")).hide();
        mostrarToast("¡Puja registrada con éxito!", "exito");
        cargarSubastas();

    } catch (error) {
        mostrarToast("No se pudo conectar con el servidor.", "error");
    } finally {
        // Se ejecuta siempre, haya éxito o error -> el botón vuelve a su estado normal
        btnConfirmar.disabled = false;
        btnConfirmar.innerHTML = textoOriginal;
    }
}

function iniciarTemporizadores() {
    setInterval(() => {
        document.querySelectorAll(".temporizador").forEach(el => {
            const fechaFin = new Date(el.dataset.fechaFin);
            const ahora = new Date();
            const diferenciaMs = fechaFin - ahora;

            if (diferenciaMs <= 0) {
                el.textContent = "Cerrada";
                el.classList.remove("temporizador-critico");
                return;
            }

            el.textContent = `⏱ ${formatearTiempo(diferenciaMs)}`;

            if (diferenciaMs <= 60000) {
                el.classList.add("temporizador-critico");
            } else {
                el.classList.remove("temporizador-critico");
            }
        });
    }, 1000);
}

function formatearTiempo(diferenciaMs) {
    const totalSegundos = Math.floor(diferenciaMs / 1000);
    const dias = Math.floor(totalSegundos / 86400);
    const horas = Math.floor((totalSegundos % 86400) / 3600);
    const minutos = Math.floor((totalSegundos % 3600) / 60);
    const segundos = totalSegundos % 60;

    let partes = [];
    if (dias > 0) partes.push(`${dias}d`);
    if (dias > 0 || horas > 0) partes.push(`${horas}h`);
    if (dias > 0 || horas > 0 || minutos > 0) partes.push(`${minutos}m`);
    partes.push(`${segundos.toString().padStart(2, "0")}s`); // los segundos siempre se muestran

    return partes.join(" ");
}

async function consultarEstadoPuja() {
    const compradorId = document.getElementById("selectComprador").value;
    const infoDiv = document.getElementById("estadoPujaInfo");

    try {
        const res = await fetch(`${API_URL}/subastas/${subastaSeleccionadaId}/estado-puja?compradorId=${compradorId}`);
        const estado = await res.json();

        infoDiv.classList.remove("d-none");
        if (estado.liderando) {
            infoDiv.className = "alert alert-success mb-3";
            infoDiv.textContent = `✅ Vas liderando con $${estado.ofertaActual}. Próxima oferta mínima: $${estado.proximaOferta}`;
        } else if (estado.superado) {
            infoDiv.className = "alert alert-warning mb-3";
            infoDiv.textContent = `⚠️ Fuiste superado. Oferta actual: $${estado.ofertaActual}. Próxima oferta mínima: $${estado.proximaOferta}`;
        } else {
            infoDiv.className = "alert alert-info mb-3";
            infoDiv.textContent = `Oferta actual: $${estado.ofertaActual}. Próxima oferta mínima: $${estado.proximaOferta}`;
        }
    } catch (error) {
        infoDiv.classList.add("d-none");
    }
}

document.getElementById("btnConfirmarPuja").addEventListener("click", confirmarPuja);

cargarSubastas();