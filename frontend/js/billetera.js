const API_URL = "https://localhost:7006/api/Billeteras";

async function cargarBilletera(usuarioId) {
    try {
        const respuesta = await fetch(`${API_URL}/${usuarioId}`);

        if (!respuesta.ok) {
            throw new Error("Billetera no encontrada.");
        }
        const saldo = await respuesta.json();

        document.getElementById("saldoTotal").textContent = `$${saldo.saldoTotal.toFixed(2)}`;
        document.getElementById("saldoRetenido").textContent = `$${saldo.saldoRetenido.toFixed(2)}`;
        document.getElementById("saldoDisponible").textContent = `$${saldo.saldoDisponible.toFixed(2)}`;
    } catch (error) {
        console.error("Error al cargar la billetera: ", error);
        document.getElementById("saldoTotal").textContent = "Error";
        document.getElementById("saldoRetenido").textContent = "Error";
        document.getElementById("saldoDisponible").textContent = "Error";
    }
}

async function cargarMovimientos(usuarioId) {
    const tabla = document.getElementById("tablaMovimientos");
    try {
        const respuesta = await fetch(`${API_URL}/${usuarioId}/movimientos`);
        const movimientos = await respuesta.json();

        if (movimientos.length === 0) {
            tabla.innerHTML = `<tr><td colspan="4" class="text-center">No hay movimientos por el momento.</td></tr>`;
            return;
        }

        tabla.innerHTML = movimientos.map(m => `
            <tr>
                <td>${m.tipo}</td>
                <td>$${m.monto}</td>
                <td>${new Date(m.fecha).toLocaleString()}</td>
                <td>${m.subastaId ?? "-"}</td>
            </tr>
        `).join("");
    } catch (error) {
        tabla.innerHTML = `<tr><td colspan="4" class="text-center text-danger">Error al cargar los movimientos.</td></tr>`;
    }
}

async function depositar(event) {
    event.preventDefault();

    const usuarioId = document.getElementById("selectUsuario").value;
    const monto = parseFloat(document.getElementById("montoDeposito").value);
    const btnSubmit = document.querySelector("#formDeposito button[type='submit']");
    const textoOriginal = btnSubmit.innerHTML;

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Depositando...`;

    try {
        const respuesta = await fetch(`${API_URL}/${usuarioId}/depositos`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ monto })
        });

        if (!respuesta.ok) {
            const errorBody = await respuesta.json();
            mostrarToast(errorBody.error || "Error al depositar.", "error");
            return;
        }

        mostrarToast("¡Depósito realizado con éxito!", "exito");
        document.getElementById("formDeposito").reset();

        cargarBilletera(usuarioId);
        cargarMovimientos(usuarioId);

    } catch (error) {
        mostrarToast("No se pudo conectar con el servidor.", "error");
    } finally {
        btnSubmit.disabled = false;
        btnSubmit.innerHTML = textoOriginal;
    }
}

function cargarTodo() {
    const usuarioId = document.getElementById("selectUsuario").value;
    cargarBilletera(usuarioId);
    cargarMovimientos(usuarioId);
}

document.getElementById("selectUsuario").addEventListener("change", cargarTodo);
document.getElementById("formDeposito").addEventListener("submit", depositar);

cargarTodo(); // carga inicial