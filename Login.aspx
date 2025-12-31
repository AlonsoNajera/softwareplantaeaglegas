<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SoftwarePlantas.WebForm1" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Iniciar Sesión - Sistema</title>
    
    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    
    <!-- SweetAlert2 -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <style>
        :root {
            --primary-color: #2c3e50;
            --secondary-color: #3498db;
            --accent-color: #e74c3c;
            --text-light: #ecf0f1;
            --shadow: rgba(0, 0, 0, 0.2);
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif;
            background: #ffffff;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .login-container {
            max-width: 450px;
            width: 100%;
            animation: fadeInUp 0.6s ease-out;
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .login-card {
            background: white;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
            overflow: hidden;
            border: 1px solid #e0e0e0;
        }

        .login-header {
            background: linear-gradient(135deg, var(--primary-color) 0%, #1a252f 100%);
            color: white;
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }

        .login-header::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320"><path fill="rgba(255,255,255,0.05)" d="M0,96L48,112C96,128,192,160,288,160C384,160,480,128,576,112C672,96,768,96,864,112C960,128,1056,160,1152,165.3C1248,171,1344,149,1392,138.7L1440,128L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path></svg>') no-repeat bottom;
            opacity: 0.5;
        }

        .login-icon {
            font-size: 4rem;
            color: var(--secondary-color);
            margin-bottom: 20px;
            animation: pulse 2s infinite;
            position: relative;
            z-index: 1;
        }

        @keyframes pulse {
            0%, 100% {
                transform: scale(1);
            }
            50% {
                transform: scale(1.05);
            }
        }

        .login-title {
            font-size: 1.8rem;
            font-weight: 700;
            margin-bottom: 10px;
            position: relative;
            z-index: 1;
        }

        .login-subtitle {
            font-size: 0.95rem;
            opacity: 0.9;
            position: relative;
            z-index: 1;
        }

        .login-body {
            padding: 40px 30px;
        }

        .form-group {
            margin-bottom: 25px;
        }

        .form-label {
            font-weight: 600;
            color: var(--primary-color);
            margin-bottom: 8px;
            font-size: 0.9rem;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .form-label i {
            color: var(--secondary-color);
        }

        .form-control {
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            padding: 12px 15px;
            font-size: 0.95rem;
            transition: all 0.3s ease;
        }

        .form-control:focus {
            border-color: var(--secondary-color);
            box-shadow: 0 0 0 0.2rem rgba(52, 152, 219, 0.25);
            outline: none;
        }

        .input-icon {
            position: relative;
        }

        .input-icon i {
            position: absolute;
            right: 15px;
            top: 50%;
            transform: translateY(-50%);
            color: #999;
            pointer-events: none;
        }

        .btn-login {
            width: 100%;
            padding: 14px;
            border-radius: 10px;
            font-weight: 600;
            font-size: 1rem;
            background: linear-gradient(135deg, var(--secondary-color) 0%, #2980b9 100%);
            border: none;
            color: white;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(52, 152, 219, 0.4);
            cursor: pointer;
        }

        .btn-login:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(52, 152, 219, 0.5);
        }

        .btn-login:active {
            transform: translateY(0);
        }

        .logo-container {
            text-align: center;
            padding: 20px 0;
            border-top: 1px solid #f0f0f0;
        }

        .logo-container img {
            max-width: 120px;
            opacity: 0.8;
            transition: opacity 0.3s ease;
        }

        .logo-container img:hover {
            opacity: 1;
        }

        .version-info {
            text-align: center;
            padding: 15px;
            background: #f8f9fa;
            color: #6c757d;
            font-size: 0.85rem;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }

        .version-info i {
            color: var(--secondary-color);
        }

        .alert-message {
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 20px;
            font-size: 0.9rem;
            display: flex;
            align-items: center;
            gap: 10px;
            animation: shake 0.5s;
        }

        .alert-error {
            background: #fee;
            color: var(--accent-color);
            border: 1px solid #fcc;
        }

        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
            75% { transform: translateX(10px); }
        }

        /* Loading spinner */
        .spinner {
            display: none;
            width: 20px;
            height: 20px;
            border: 3px solid rgba(255, 255, 255, 0.3);
            border-top-color: white;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
            margin-left: 10px;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        .btn-login.loading .spinner {
            display: inline-block;
        }

        /* Responsive */
        @media (max-width: 576px) {
            .login-header {
                padding: 30px 20px;
            }

            .login-body {
                padding: 30px 20px;
            }

            .login-title {
                font-size: 1.5rem;
            }

            .login-icon {
                font-size: 3rem;
            }
        }

        /* Password security indicator */
        input[type="password"] {
            -webkit-text-security: disc;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <div class="login-card">
                <!-- Header -->
                <div class="login-header">
                       <!-- Logo -->
                           <div class="logo-container">
                               <img src="/images/logo.png" alt="Logo" />
                           </div>
                  
                    <p class="login-subtitle">Ingresa tus credenciales para continuar</p>
                </div>

                <!-- Body -->
                <div class="login-body">
                    <!-- Alert Message -->
                    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message alert-error">
                        <i class="fas fa-exclamation-circle"></i>
                        <asp:Label ID="lblMessage" runat="server" />
                    </asp:Panel>

                    <!-- Username -->
                    <div class="form-group">
                        <label class="form-label" for="txtUsername">
                            <i class="fas fa-user"></i>
                            Usuario
                        </label>
                        <div class="input-icon">
                            <asp:TextBox ID="txtUsername" runat="server" 
                                CssClass="form-control" 
                                placeholder="Ingrese su usuario"
                                autocomplete="username" />
                        </div>
                    </div>

                    <!-- Password -->
                    <div class="form-group">
                        <label class="form-label" for="txtPassword">
                            <i class="fas fa-lock"></i>
                            Contraseña
                        </label>
                        <div class="input-icon">
                            <asp:TextBox ID="txtPassword" runat="server" 
                                CssClass="form-control" 
                                TextMode="Password" 
                                placeholder="Ingrese su contraseña"
                                autocomplete="current-password" />
                        </div>
                    </div>

                    <!-- Login Button -->
                    <div class="form-group">
                        <asp:Button ID="btnLogin" runat="server" 
                            CssClass="btn-login" 
                            Text="Iniciar Sesión" 
                            OnClick="btnLogin_Click"
                            OnClientClick="return validateAndShowLoading(this);" />
                    </div>
                </div>
                <!-- Version -->
                <div class="version-info">
                    <i class="fas fa-info-circle"></i>
                    <asp:Label ID="lblVersion" runat="server" Text="v1.0.0" />
                </div>
            </div>
        </div>
    </form>

    <script>
        function validateAndShowLoading(btn) {
            const username = document.getElementById('<%= txtUsername.ClientID %>').value.trim();
            const password = document.getElementById('<%= txtPassword.ClientID %>').value.trim();

            if (username === '' || password === '') {
                Swal.fire({
                    icon: 'warning',
                    title: 'Campos vacíos',
                    text: 'Por favor ingrese usuario y contraseña',
                    confirmButtonColor: '#3498db'
                });
                return false;
            }

            // No deshabilitar el botón ni cambiar su contenido para permitir el postback
            return true;
        }

        // Permitir Enter para enviar el formulario
        document.addEventListener('DOMContentLoaded', function () {
            const form = document.getElementById('form1');
            const inputs = form.querySelectorAll('input[type="text"], input[type="password"]');

            inputs.forEach(input => {
                input.addEventListener('keypress', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        const btnLogin = document.getElementById('<%= btnLogin.ClientID %>');
                        // Validar antes de hacer click
                        if (validateAndShowLoading(btnLogin)) {
                            btnLogin.click();
                        }
                    }
                });
            });

            // Auto-focus en el campo de usuario
            const usernameField = document.getElementById('<%= txtUsername.ClientID %>');
            if (usernameField) {
                usernameField.focus();
            }

            // Manejar el evento de submit del formulario
            form.addEventListener('submit', function(e) {
                const btnLogin = document.getElementById('<%= btnLogin.ClientID %>');
                if (btnLogin && !btnLogin.classList.contains('loading')) {
                    btnLogin.classList.add('loading');
                    btnLogin.innerHTML = 'Iniciando sesión...<span class="spinner"></span>';
                }
            });
        });
    </script>
</body>
</html>