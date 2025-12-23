<%@ Page Title="Home Page" Language="C#"  AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SoftwarePlantas._Default" %>
<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>


<!DOCTYPE html>

<html lang="es">

<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SoftwarePlanta</title>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body runat="server"  class="header-welcome">

<div class="container mt-5 small" style="margin-left: 150px; ">
    <form runat="server">

         <uc:CerrarSesion ID="CerrarSesion" runat="server" />

        <div class="container mt-5" > <!-- Ajusta el padding-top para evitar que el contenido se oculte detrás del botón -->
          
           <!-- Llamada al control de usuario para el menú lateral -->
      <button type="button" class="menu-toggle" onclick="toggleMenu()">☰</button>

        <uc:MenuLateral ID="MenuLateral" runat="server" />
        

            <div class="row justify-content-center">
                <div class="col-md-6 text-center">
                    <asp:Label ID="lblWelcome" runat="server" CssClass="h4 text-primary"></asp:Label>
                </div>
            </div>
               <h3 class="text-center">Bienvenido</h3>
           </div>
    </form>
</div>
        <script>
            // Función para alternar la visibilidad del menú en pantallas pequeñas
            function toggleMenu() {
                var sidebar = document.getElementById("sidebarMenu");
                sidebar.classList.toggle("show");
            }


        </script>
        <style>
        /* Estilos del botón de menú para pantallas pequeñas */
        .menu-toggle {
            display: none;
        }

        @media (max-width: 768px) {
            .menu-toggle {
                display: block;
                position: fixed;
                top: 15px;
                left: 15px;
                z-index: 2;
                background-color: #343a40;
                color: white;
                border: none;
                padding: 10px;
                font-size: 18px;
                cursor: pointer;
            }

            .content {
                margin-left: 0; /* Sin margen cuando el menú está oculto */
            }
        }

        /* Estilo para el overlay de carga */
#loadingOverlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(255, 255, 255, 0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    display: none;
}
 .header-welcome {
    margin-top: 100px; /* Ajusta el valor según lo necesites */
    text-align: center; /* Centra el contenido horizontalmente */
    font-family: Arial, sans-serif; /* Fuente opcional */
}
  
    </style>


</body>

</html>

