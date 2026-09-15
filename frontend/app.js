const API_URL = "http://localhost:5085/api";

async function cargarSubastas() {
    try {
        const res = await fetch(`${API_URL}/subastas`);
        const subastas = await res.json();
        console.log("Lo que me devuelve el Back: ", subastas[0]);
        const lista = document.getElementById("lista");
        lista.innerHTML = subastas.map(subasta => `
            <div class="col-md-4 mb-3">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">${subasta.titulo}</h5>  
                        <p class="card-text">${subasta.descripcion}</p>
                        <p class="card-text">Precio inicial: $${subasta.precioInicial}</p>
                        <button class="btn btn-primary w-100">Pujar</button>
                    </div>
                </div>
            </div>
        `).join("");
    } catch (error) {
        console.error("Error al cargar las subastas:", error);
        document.getElementById("lista").innerHTML = "<p class='text-danger'>Error al cargar las subastas. Por favor, inténtelo de nuevo más tarde.</p>";
    }
}

cargarSubastas();