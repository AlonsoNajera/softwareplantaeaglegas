<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ControlVolumetrico.aspx.cs" Inherits="SoftwarePlantas.ControlVolumetrico" %>
<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Control Volumetrico SAT</title>
<!-- Bootstrap CSS -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />

<!-- Bootstrap JS (Debe estar antes de </body>) -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    <!-- SweetAlert2 CSS -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">

<!-- SweetAlert2 JS -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- jQuery -->
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

<!-- Bootstrap JS -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.bundle.min.js"></script>

<!-- Bootstrap CSS (Si aún no está incluido) -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap.min.css" rel="stylesheet">


</head>


<body runat="server" class="header-welcome">

<div class="container mt-5 small" style="margin-left: 150px;">
    <form runat="server">

         <uc:CerrarSesion ID="CerrarSesion" runat="server" />

          <!-- Resto del contenido de la página -->
        <div class="container mt-5" > <!-- Ajusta el padding-top para evitar que el contenido se oculte detrás del botón -->
          
           <!-- Llamada al control de usuario para el menú lateral -->
      <button type="button" class="menu-toggle" onclick="toggleMenu()">☰</button>

        <uc:MenuLateral ID="MenuLateral" runat="server" />
        

            <div class="row justify-content-center">
                <div class="col-md-6 text-center">
                    <asp:Label ID="lblWelcome" runat="server" CssClass="h4 text-primary"></asp:Label>
                </div>
            </div>
               <h3 class="text-center">GENERAR JSON SAT</h3>
                        <div class="row mt-4">
         

                    <div class="col-md-2">
                        <label for="ddlMes" class="form-label">Mes</label>
                        <asp:DropDownList ID="ddlMes" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Enero" Value="1" />
                            <asp:ListItem Text="Febrero" Value="2" />
                            <asp:ListItem Text="Marzo" Value="3" />
                            <asp:ListItem Text="Abril" Value="4" />
                            <asp:ListItem Text="Mayo" Value="5" />
                            <asp:ListItem Text="Junio" Value="6" />
                            <asp:ListItem Text="Julio" Value="7" />
                            <asp:ListItem Text="Agosto" Value="8" />
                            <asp:ListItem Text="Septiembre" Value="9" />
                            <asp:ListItem Text="Octubre" Value="10" />
                            <asp:ListItem Text="Noviembre" Value="11" />
                            <asp:ListItem Text="Diciembre" Value="12" />
                        </asp:DropDownList>
                    </div>

                     <div class="col-md-2">
                        <label for="ddlAnio" class="form-label">Año</label>
                        <asp:DropDownList ID="ddlAnio" runat="server" CssClass="form-select" />
                    </div>

            <div class="col-md-3">
                <label for="ddlEstacion" class="form-label">Estación</label>
                <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
<%--            <div class="col-md-3">
                <label for="ddlProducto" class="form-label">Producto</label>
                <asp:UpdatePanel ID="UpdatePanelProducto" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProducto_SelectedIndexChanged">
                        </asp:DropDownList>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>--%>

            <div class="text-center mt-4">
                <asp:Button ID="btnGenerarJson" runat="server" CssClass="btn btn-primary" Text="Generar JSON" OnClick="btnGenerarJson_Click" />
                <asp:Button ID="btnSoloPDF" runat="server" Text="Generar Solo PDF" CssClass="btn btn-warning" OnClick="btnSoloPDF_Click" />
            </div>

      
        </div>
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

        .uuid-column {
    max-width: 100px; /* Ajusta el tamaño según necesites */
        min-width: 200px;  /* Aumenta el ancho mínimo */
    white-space: nowrap; /* Evita que el texto se divida en varias líneas */
    overflow: hidden;
    text-overflow: ellipsis; /* Muestra "..." si el texto es muy largo */
}
          .txt-column {
    max-width: 10px; /* Ajusta el tamaño según necesites */
     min-width: 150px;  /* Aumenta el ancho mínimo */
    white-space: nowrap; /* Evita que el texto se divida en varias líneas */
    overflow: hidden;
    text-overflow: ellipsis; /* Muestra "..." si el texto es muy largo */
}

          .precio-column {
    max-width: 10px; /* Ajusta el tamaño según necesites */
  
    text-align: left; /* Alinea el texto a la derecha (opcional) */
    white-space: nowrap; /* Evita que el texto se divida en varias líneas */
    overflow: hidden;
    text-overflow: ellipsis; /* Muestra "..." si el texto es muy largo */
}

        .modal {
    z-index: 1050 !important; /* Asegura que el modal esté en la parte superior */
}

.modal-backdrop {
    z-index: 1049 !important; /* La capa de fondo debe estar justo debajo del modal */
}

body.modal-open {
    overflow: hidden; /* Evita que el fondo se desplace cuando el modal está abierto */
}

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
                font-size: 12px;
                cursor: pointer;
            }

            .content {
                margin-left: 0; /* Sin margen cuando el menú está oculto */
            }
        }

            .small-font-gridview {
        font-size: 0.65em; /* Ajusta el valor según el tamaño que desees */
    }

    .small-font-gridview th,
    .small-font-gridview td {
        padding: 5px; /* Opcional: ajusta el espacio entre el texto y los bordes */
    }

            /* Asegura que los campos de entrada en el modo de edición mantengan el mismo tamaño de letra */
        table input[type="text"] {
            font-size: 10px; /* Ajusta al tamaño que necesites */
            width: 100%; /* Esto hará que el campo ocupe todo el espacio de la celda */
            box-sizing: border-box; /* Asegura que el padding no exceda el ancho de la celda */
        }
        .header-welcome {
    margin-top: 100px; /* Ajusta el valor según lo necesites */
    text-align: center; /* Centra el contenido horizontalmente */
    font-family: Arial, sans-serif; /* Fuente opcional */
}
        .btn-black {
    background-color: black; /* Fondo negro */
    color: white; /* Texto en blanco */
    border: none; /* Elimina bordes adicionales */
    padding: 5px 10px; /* Ajusta el tamaño del botón */
    font-size: 14px; /* Tamaño del texto */
    border-radius: 4px; /* Bordes redondeados opcionales */
    cursor: pointer;
}

.btn-black:hover {
    background-color: #333; /* Cambia el color al pasar el mouse */
     color: white;
}
  
    </style>
</body>
</html>
