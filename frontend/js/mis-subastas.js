const API_URL = "https://localhost:7006/api";

async function cargarMisPujas(usuarioId) {
    const contenedor = document.getElementById("listaMisPujas");
    try {
        const res = await fetch(`${API_URL}/usuarios/${usuarioId}/pujas`);
        const pujas = await res.json();

        if (pujas.length === 0) {
            contenedor.innerHTML = "<p class='text-muted'>No participaste en ninguna subasta todavía.</p>";
            return;
        }

        contenedor.innerHTML = `
            <table class="table">
                <thead>
                    <tr>
                        <th>Subasta</th>
                        <th>Mi última oferta</th>
                        <th>Estado</th>
                        <th>¿Voy ganando?</th>
                    </tr>
                </thead>
                <tbody>
                    ${pujas.map(pujas => `
                        <tr>
                            <td>${pujas.tituloSubasta}</td>
                            <td>$${pujas.miUltimaOferta}</td>
                            <td>${pujas.estadoSubasta}</td>
                            <td>${pujas.soyElLider ? "✅ Sí" : "❌ No"}</td>
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        `;
    } catch (error) {
        contenedor.innerHTML = "<p class='text-danger'>Error al cargar tus pujas.</p>";
    }
}

async function cargarMisPublicaciones(vendedorId) {
    const contenedor = document.getElementById("listaMisPublicaciones");
    try {
        const res = await fetch(`${API_URL}/usuarios/${vendedorId}/publicaciones`);
        const publicaciones = await res.json();

        if (publicaciones.length === 0) {
            contenedor.innerHTML = "<p class='text-muted'>No publicaste ninguna subasta todavía.</p>";
            return;
        }

        contenedor.innerHTML = `
            <table class="table">
                <thead>
                    <tr>
                        <th>Título</th>
                        <th>Estado</th>
                        <th>Precio base</th>
                        <th>Cantidad de pujas</th>
                        <th>Recaudado</th>
                    </tr>
                </thead>
                <tbody>
                    ${publicaciones.map(p => {
                        const claseBadge = p.estado === 'FINALIZADA' ? 'badge-finalizada'
                            : p.estado === 'ACTIVA' ? 'badge-activa'
                            : p.estado === 'PROGRAMADA' ? 'badge-programada'
                            : 'badge-desierta';
                        return `
                            <tr>
                                <td>${p.titulo}</td>
                                <td><span class="badge-estado ${claseBadge}">${p.estado}</span></td>
                                <td>$${p.precioBase}</td>
                                <td>${p.cantidadPujas}</td>
                                <td>$${p.montoRecaudado}</td>
                            </tr>
                        `;
                    }).join("")}
                </tbody>
            </table>
        `;
    } catch (error) {
        contenedor.innerHTML = "<p class='text-danger'>Error al cargar tus publicaciones.</p>";
    }
}

function cargarTodo() {
    const usuarioId = document.getElementById("selectUsuario").value;
    cargarMisPujas(usuarioId);
    cargarMisPublicaciones(usuarioId);
}

document.getElementById("selectUsuario").addEventListener("change", cargarTodo);

cargarTodo(); // carga inicial, con el usuario que esté seleccionado por defecto