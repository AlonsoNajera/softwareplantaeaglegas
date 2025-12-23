<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SoftwarePlantas.WebForm1" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <title>Login</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container mt-5">

            <div class="row justify-content-center">
                <div class="col-md-4">
                    <h2 class="text-center">Iniciar Sesión</h2>
                     <!-- Logo debajo del botón de inicio de sesión -->
                    <div class="text-center mt-4">
                        <img src="/images/logo.png" alt="Logo de la empresa" class="img-fluid" style="max-width: 150px;" />
                    </div>

                    <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
                    <div class="form-group">
                        <label for="username">Usuario</label>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Ingrese su usuario" />
                    </div>
                    <div class="form-group mt-2">
                        <label for="password">Contraseña</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Ingrese su contraseña" autocomplete="off" />
                    </div>
                    <div class="form-group mt-3 text-center">
                        <asp:Button ID="btnLogin" runat="server" CssClass="btn btn-primary" Text="Iniciar Sesión" OnClick="btnLogin_Click" />
                    </div>
                </div>
            </div>
        </div>
            
      <!-- Mostrar la versión del sistema en la parte inferior -->
        <div class="row justify-content-center mt-3">
            <div class="col-md-4 text-center">
                <asp:Label ID="lblVersion" runat="server" CssClass="text-muted"></asp:Label>
            </div>
        </div>

    </form>
    <style>
        input[type="password"] {
    -webkit-text-security: disc; /* Forcing display of bullets on some browsers */
}

    </style>
</body>
</html>
