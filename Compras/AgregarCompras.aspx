<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AgregarCompras.aspx.cs" Inherits="SoftwarePlantas.Compras.AgregarCompras" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Agregar Recepcion</title>
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
        <form id="form1" runat="server">

            <uc:CerrarSesion ID="CerrarSesion" runat="server" />

            <!-- Resto del contenido de la página -->
            <div class="container mt-5">
                <!-- Ajusta el padding-top para evitar que el contenido se oculte detrás del botón -->

                <!-- Llamada al control de usuario para el menú lateral -->
                <button type="button" class="menu-toggle" onclick="toggleMenu()">☰</button>

                <uc:MenuLateral ID="MenuLateral" runat="server" />


                <div class="row justify-content-center">
                    <div class="col-md-6 text-center">
                        <asp:Label ID="lblWelcome" runat="server" CssClass="h4 text-primary"></asp:Label>
                    </div>
                </div>
                <h3 class="text-center">Agregar Recepcion</h3>
                <div class="mb-3">
                    <label for="txtFecha" class="form-label">Fecha</label>
                    <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                        <div class="md-3">
                        <label for="ddlEstacion" class="form-label">Estación</label>
                        <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>

                <div class="mb-3">
                    <label for="ddlTanque" class="form-label">Tanque</label>
                    <asp:DropDownList ID="ddlTanque" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>

                <div class="mb-3">
                    <label for="txtLitros" class="form-label">Cantidad en litros</label>
                    <asp:TextBox ID="txtLitros" runat="server" CssClass="form-control" />
                </div>
           

                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />


            </div>




    </form>




    </div>







</body>
</html>
