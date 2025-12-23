<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AgregarCompras.aspx.cs" Inherits="SoftwarePlantas.Compras.AgregarCompras" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Agregar Recepcion</title>
    
    <!-- jQuery -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    
    <!-- Bootstrap 5 JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    
    <!-- SweetAlert2 CSS -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css">
    
    <!-- SweetAlert2 JS -->
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</head>
<body>

<!-- Menú lateral -->
<uc:MenuLateral ID="MenuLateral" runat="server" />

<!-- Contenedor principal con margen para el sidebar -->
<div class="main-content">
    <form id="form1" runat="server">
        <!-- Botón para dispositivos móviles -->
        <button type="button" class="menu-toggle" onclick="toggleMenu()">☰</button>

        <!-- Botón de cerrar sesión -->
        <uc:CerrarSesion ID="CerrarSesion" runat="server" />

        <!-- Contenido de la página -->
        <div class="container-fluid px-4 py-3">
            <div class="row justify-content-center mb-3">
                <div class="col-12 text-center">
                    <asp:Label ID="lblWelcome" runat="server" CssClass="h4 text-primary"></asp:Label>
                </div>
            </div>
            
            <h3 class="text-center mb-4">Agregar Recepción</h3>
            
            <div class="row justify-content-center">
                <div class="col-md-6">
                    <div class="card shadow-sm">
                        <div class="card-body">
                            <div class="mb-3">
                                <label for="txtFecha" class="form-label">Fecha</label>
                                <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                            
                            <div class="mb-3">
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
                       
    
                            <div class="d-grid">
                                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</div>

<script>
    function toggleMenu() {
        var sidebar = document.getElementById("sidebarMenu");
        sidebar.classList.toggle("show");
    }
</script>

<style>
    /* Contenedor principal con margen para el sidebar */
    .main-content {
        margin-left: 250px;
        min-height: 100vh;
        background-color: #f8f9fa;
        transition: margin-left 0.3s ease;
    }

    /* Botón toggle para móviles */
    .menu-toggle {
        display: none;
        position: fixed;
        top: 15px;
        left: 15px;
        z-index: 1100;
        background-color: #2c3e50;
        color: white;
        border: none;
        padding: 10px 15px;
        font-size: 18px;
        cursor: pointer;
        border-radius: 5px;
        box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    }

    .menu-toggle:hover {
        background-color: #34495e;
    }

    /* Card styling */
    .card {
        border: none;
        border-radius: 10px;
    }

    /* Responsive */
    @media (max-width: 768px) {
        .main-content {
            margin-left: 0;
        }

        .menu-toggle {
            display: block;
        }
    }
</style>
</body>
</html>
