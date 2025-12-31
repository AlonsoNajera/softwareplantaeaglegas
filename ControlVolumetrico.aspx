<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ControlVolumetrico.aspx.cs" Inherits="SoftwarePlantas.ControlVolumetrico" %>
<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Control Volumetrico SAT</title>
    
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
    <form runat="server">
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
            
            <h3 class="text-center mb-4">GENERAR JSON SAT</h3>
            
            <!-- Filtros -->
            <div class="row g-3 mb-4 justify-content-center">
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

                <div class="col-md-4">
                    <label for="ddlEstacion" class="form-label">Estación</label>
                    <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>
            </div>

            <!-- Botones de acción -->
            <div class="row justify-content-center">
                <div class="col-md-6 text-center">
                    <div class="d-grid gap-2 d-md-flex justify-content-md-center">
                        <asp:Button ID="btnGenerarJson" runat="server" CssClass="btn btn-primary btn-lg" Text="Generar JSON" OnClick="btnGenerarJson_Click" />
                        <asp:Button ID="btnSoloPDF" runat="server" Text="Generar Solo PDF" CssClass="btn btn-warning btn-lg" OnClick="btnSoloPDF_Click" />
                        <asp:Button ID="btnSoloExcel" runat="server"
    Text="Descargar Recepciones Excel"
    CssClass="btn btn-success btn-lg"
    OnClick="btnSoloExcel_Click"
    OnClientClick="mostrarCargandoExcel(); return true;" />


                    </div>
                </div>
            </div>
        </div>
    </form>
</div>
    <script>
        function mostrarCargandoExcel() {
            Swal.fire({
                title: 'Generando Excel...',
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => Swal.showLoading()
            });

            // Cerrar el SweetAlert después de 3 segundos (ajusta según tu caso)
            setTimeout(function () {
                Swal.close();
            }, 3000);
        }
</script>
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
