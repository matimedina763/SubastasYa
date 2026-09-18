const API_URL = "https://localhost:7006/api";

document.getElementById("formCrearSubasta").addEventListener("submit", async function (e) {
    e.preventDefault();

    const btnSubmit = document.querySelector("#formCrearSubasta button[type='submit']");
    const textoOriginal = btnSubmit.innerHTML;

    const precioBase = parseFloat(document.getElementById("precioBase").value);
    const incrementoMinimo = parseFloat(document.getElementById("incrementoMinimo").value);
    const fechaInicio = new Date(document.getElementById("fechaInicio").value);
    const fechaFin = new Date(document.getElementById("fechaFin").value);

    // Validaciones en pantalla, ANTES de tocar el backend (evita peticiones innecesarias)
    if (precioBase <= 0) {
        mostrarToast("El precio base debe ser un valor positivo.", "error");
        return;
    }
    if (incrementoMinimo <= 0) {
        mostrarToast("El incremento mínimo debe ser un valor positivo.", "error");
        return;
    }
    if (fechaFin <= fechaInicio) {
        mostrarToast("La fecha de fin debe ser posterior a la de inicio.", "error");
        return;
    }

    const body = {
        vendedorId: parseInt(document.getElementById("vendedorId").value),
        titulo: document.getElementById("titulo").value,
        descripcion: document.getElementById("descripcion").value,
        urlImagen: document.getElementById("urlImagen").value,
        categoriaId: parseInt(document.getElementById("categoriaId").value),
        precioBase: precioBase,
        incrementoMinimo: incrementoMinimo,
        fechaInicio: fechaInicio.toISOString(),
        fechaFin: fechaFin.toISOString()
    };

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Publicando...`;

    try {
        const res = await fetch(`${API_URL}/subastas`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            const errorBody = await res.json();
            mostrarToast(errorBody.error || "Ocurrió un error al crear la subasta.", "error");
            return;
        }

        mostrarToast("¡Subasta creada con éxito!", "exito");
        document.getElementById("formCrearSubasta").reset();

    } catch (error) {
        mostrarToast("No se pudo conectar con el servidor.", "error");
    } finally {
        btnSubmit.disabled = false;
        btnSubmit.innerHTML = textoOriginal;
    }
});