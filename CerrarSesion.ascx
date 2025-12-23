<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CerrarSesion.ascx.cs" Inherits="SoftwarePlantas.CerrarSesion" %>

<div class="top-bar" class="header-top">
    <div>
        Tiempo restante de sesión: <span id="sessionCountdown">60</span> segundos
    </div>
    <asp:LinkButton ID="btnCerrarSesion" runat="server" CssClass="btn btn-danger" OnClick="btnCerrarSesion_Click" ToolTip="Cerrar Sesión">
        <i class="fas fa-sign-out-alt"></i> Cerrar Sesión
    </asp:LinkButton>
</div>

<style>
    /* Estilo para la barra superior con el botón de cerrar sesión */
.top-bar {
    position: fixed;
    top: 0;
    left: 0; /* Asegura que la barra comience desde el lado izquierdo */
    width: 100%; /* Ocupa todo el ancho de la página */
    background-color: black; /* Fondo negro */
    color: white; /* Texto blanco */
    padding: 10px; /* Espaciado interno */
    z-index: 10; /* Asegura que esté por encima de otros elementos */
    text-align: right; /* Alinea los elementos a la derecha */
    font-family: Arial, sans-serif; /* Fuente opcional */
}


</style>

<script>
    const inactivityTime = <%= System.Configuration.ConfigurationManager.AppSettings["InactivityTimeInSeconds"] %>; // Tiempo en segundos desde Web.config

    let remainingTime = inactivityTime;
    let inactivityTimer;
    let countdownTimer;

    // Captura el ClientID de btnCerrarSesion desde el servidor
    const logoutButtonID = '<%= btnCerrarSesion.ClientID %>';

    function resetTimer() {
        clearTimeout(inactivityTimer);
        clearInterval(countdownTimer);
        remainingTime = inactivityTime;
        updateCountdownDisplay();
        startCountdown();
        inactivityTimer = setTimeout(logoutUser, inactivityTime * 1000); // Convierte a milisegundos
    }

    function logoutUser() {
        // Simular clic en el botón de cierre de sesión
        document.getElementById(logoutButtonID).click();
    }

    function startCountdown() {
        countdownTimer = setInterval(() => {
            remainingTime--;
            updateCountdownDisplay();
            if (remainingTime <= 0) {
                clearInterval(countdownTimer);
            }
        }, 1000); // Actualiza cada segundo
    }

    function updateCountdownDisplay() {
        document.getElementById("sessionCountdown").textContent = remainingTime;
    }

    // Detecta actividad en la ventana y reinicia el temporizador
    window.onload = resetTimer;
    window.onmousemove = resetTimer;
    window.onmousedown = resetTimer;
    window.ontouchstart = resetTimer;
    window.onclick = resetTimer;
    window.onkeypress = resetTimer;
    window.onscroll = resetTimer;
</script>
