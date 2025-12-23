<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CerrarSesion.ascx.cs" Inherits="SoftwarePlantas.CerrarSesion" %>

<!-- Font Awesome para iconos -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

<div class="session-bar">
    <div class="session-timer">
        <i class="fas fa-clock"></i>
        <span id="sessionCountdown">1800</span>s
    </div>
    <asp:LinkButton ID="btnCerrarSesion" runat="server" CssClass="btn-logout" OnClick="btnCerrarSesion_Click" ToolTip="Cerrar Sesión">
        <i class="fas fa-sign-out-alt"></i> Cerrar Sesión
    </asp:LinkButton>
</div>

<style>
    /* Barra de sesión minimalista */
    .session-bar {
        position: fixed;
        top: 15px;
        right: 20px;
        display: flex;
        align-items: center;
        gap: 15px;
        z-index: 9999;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
    }

    /* Contador de sesión */
    .session-timer {
        display: flex;
        align-items: center;
        gap: 8px;
        background: rgba(255, 255, 255, 0.95);
        backdrop-filter: blur(10px);
        padding: 8px 15px;
        border-radius: 20px;
        font-size: 14px;
        font-weight: 600;
        color: #2c3e50;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        border: 1px solid rgba(52, 152, 219, 0.2);
        transition: all 0.3s ease;
    }

    .session-timer i {
        color: #3498db;
        font-size: 14px;
    }

    #sessionCountdown {
        color: #3498db;
        min-width: 45px;
        text-align: center;
        font-variant-numeric: tabular-nums;
    }

    /* Botón de cerrar sesión */
    .btn-logout {
        display: flex;
        align-items: center;
        gap: 8px;
        background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%);
        color: white !important;
        padding: 10px 18px;
        border: none;
        border-radius: 20px;
        font-size: 13px;
        font-weight: 600;
        text-decoration: none;
        cursor: pointer;
        transition: all 0.3s ease;
        box-shadow: 0 2px 8px rgba(231, 76, 60, 0.3);
    }

    .btn-logout:hover {
        background: linear-gradient(135deg, #c0392b 0%, #a93226 100%);
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(231, 76, 60, 0.4);
        color: white !important;
        text-decoration: none;
    }

    .btn-logout i {
        font-size: 14px;
    }

    /* Estados de advertencia */
    .session-warning .session-timer {
        background: rgba(255, 243, 205, 0.95);
        border-color: rgba(243, 156, 18, 0.3);
    }

    .session-warning #sessionCountdown {
        color: #f39c12;
        animation: pulse-warning 1s ease-in-out infinite;
    }

    .session-warning .session-timer i {
        color: #f39c12;
    }

    .session-danger .session-timer {
        background: rgba(255, 235, 235, 0.95);
        border-color: rgba(231, 76, 60, 0.3);
    }

    .session-danger #sessionCountdown {
        color: #e74c3c;
        animation: pulse-danger 0.5s ease-in-out infinite;
    }

    .session-danger .session-timer i {
        color: #e74c3c;
    }

    @keyframes pulse-warning {
        0%, 100% { opacity: 1; }
        50% { opacity: 0.6; }
    }

    @keyframes pulse-danger {
        0%, 100% { opacity: 1; transform: scale(1); }
        50% { opacity: 0.7; transform: scale(1.05); }
    }

    /* Responsive */
    @media (max-width: 768px) {
        .session-bar {
            top: 10px;
            right: 10px;
            gap: 10px;
        }

        .session-timer {
            padding: 6px 12px;
            font-size: 12px;
        }

        .btn-logout {
            padding: 8px 14px;
            font-size: 12px;
        }

        .btn-logout span {
            display: none;
        }

        .btn-logout i {
            margin: 0;
        }
    }

    /* Para pantallas muy pequeñas, solo mostrar iconos */
    @media (max-width: 480px) {
        .session-timer {
            padding: 6px 10px;
        }

        #sessionCountdown {
            min-width: 35px;
            font-size: 12px;
        }
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
        inactivityTimer = setTimeout(logoutUser, inactivityTime * 1000);
    }

    function logoutUser() {
        document.getElementById(logoutButtonID).click();
    }

    function startCountdown() {
        countdownTimer = setInterval(() => {
            remainingTime--;
            updateCountdownDisplay();
            updateSessionBarStyle();
            
            if (remainingTime <= 0) {
                clearInterval(countdownTimer);
            }
        }, 1000);
    }

    function updateCountdownDisplay() {
        const countdown = document.getElementById("sessionCountdown");
        if (countdown) {
            countdown.textContent = remainingTime;
        }
    }

    function updateSessionBarStyle() {
        const sessionBar = document.querySelector('.session-bar');
        if (!sessionBar) return;

        if (remainingTime <= 30) {
            sessionBar.classList.add('session-danger');
            sessionBar.classList.remove('session-warning');
        } else if (remainingTime <= 60) {
            sessionBar.classList.add('session-warning');
            sessionBar.classList.remove('session-danger');
        } else {
            sessionBar.classList.remove('session-warning', 'session-danger');
        }
    }

    // Detecta actividad en la ventana y reinicia el temporizador
    window.addEventListener('load', resetTimer);
    window.addEventListener('mousemove', resetTimer);
    window.addEventListener('mousedown', resetTimer);
    window.addEventListener('touchstart', resetTimer);
    window.addEventListener('click', resetTimer);
    window.addEventListener('keypress', resetTimer);
    window.addEventListener('scroll', resetTimer);
</script>
