const API_URL = "https://localhost:7006/api";

document.getElementById("formCrearSubasta").addEventListener("submit", async function (e) {
    e.preventDefault();

    const btnSubmit = document.querySelector("#formCrearSubasta button[type='submit']");
    const textoOriginal = btnSubmit.innerHTML;

    const body = {
        vendedorId: parseInt(document.getElementById("vendedorId").value),
        titulo: document.getElementById("titulo").value,
        descripcion: document.getElementById("descripcion").value,
        urlImagen: document.getElementById("urlImagen").value,
        categoriaId: parseInt(document.getElementById("categoriaId").value),
        precioBase: parseFloat(document.getElementById("precioBase").value),
        incrementoMinimo: parseFloat(document.getElementById("incrementoMinimo").value),
        fechaInicio: new Date(document.getElementById("fechaInicio").value).toISOString(),
        fechaFin: new Date(document.getElementById("fechaFin").value).toISOString()
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