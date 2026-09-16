const API_URL = "https://localhost:7006/api/Billeteras";

/* Se guarda la dirección base de Billeteras */
async function cargarBilletera(usuarioId)
{
    try {
        const respuesta = await fetch(`${API_URL}/${usuarioId}`);   /* Define una función que recibe el ID del usuario. */

        if(!respuesta.ok) {
            throw new Error("Billetera no encontrada.");
        }
        const saldo = await respuesta.json();                       /* Consulta */

        document.getElementById("saldoTotal").textContent =         /* Los document.getElementById convierte la respuesta del backend en un objeto JavaScript */
            `$${saldo.saldoTotal.toFixed(4)}`;

        document.getElementById("saldoRetenido").textContent =
            `$${saldo.saldoRetenido.toFixed(4)}`;

        document.getElementById("saldoDisponible").textContent =
            `$${saldo.saldoDisponible.toFixed(4)}`;
    } catch (error) {
        console.error("Error al cargar la billetera: ", error);
    }
}

cargarBilletera(4);
