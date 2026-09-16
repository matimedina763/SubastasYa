const API_URL = "https://localhost:7006/api";

document.getElementById("formCrearSubasta").addEventListener("submit", async function (e) {
    e.preventDefault(); // evita que el formulario recargue la página, como hace por defecto

    const alerta = document.getElementById("alertaCrear");
    const exito = document.getElementById("exitoCrear");
    alerta.classList.add("d-none");
    exito.classList.add("d-none");

    const body = {
        vendedorId: parseInt(document.getElementById("vendedorId").value),
        titulo: document.getElementById("titulo").value,
        descripcion: document.getElementById("descripcion").value,
        urlImagen: document.getElementById("urlImagen").value,
        categoriaId: parseInt(document.getElementById("categoriaId").value),
        precioBase: parseFloat(document.getElementById("precioBase").value),
        incrementoMinimo: parseFloat(document.getElementById("incrementoMinimo").value),
        fechaInicio: document.getElementById("fechaInicio").value,
        fechaFin: document.getElementById("fechaFin").value
    };

    try {
        const res = await fetch(`${API_URL}/subastas`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            const errorBody = await res.json();
            alerta.textContent = errorBody.error || "Ocurrió un error al crear la subasta.";
            alerta.classList.remove("d-none");
            return;
        }

        exito.textContent = "¡Subasta creada con éxito!";
        exito.classList.remove("d-none");
        document.getElementById("formCrearSubasta").reset();

    } catch (error) {
        alerta.textContent = "No se pudo conectar con el servidor.";
        alerta.classList.remove("d-none");
    }
});