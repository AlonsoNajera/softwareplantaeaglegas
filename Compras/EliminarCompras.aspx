<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EliminarCompras.aspx.cs" Inherits="SoftwarePlantas.Compras.EliminarCompras" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Eliminar Compra</title>
    
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
            
            <h3 class="text-center mb-4">Eliminar Recepción</h3>
            
            <!-- Filtros -->
            <div class="row g-3 mb-4">
                <div class="col-md-3">
                    <label for="txtFechaInicial" class="form-label">Fecha Inicial</label>
                    <asp:TextBox ID="txtFechaInicial" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="col-md-3">
                    <label for="txtFechaFinal" class="form-label">Fecha Final</label>
                    <asp:TextBox ID="txtFechaFinal" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="col-md-4">
                    <label for="ddlEstacion" class="form-label">Estación</label>
                    <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>
                <div class="col-md-2 d-flex align-items-end">
                    <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary w-100" Text="Buscar" OnClick="btnBuscar_Click" />
                </div>
            </div>

            <!-- Resultados -->
            <asp:Panel ID="pnlResultados" runat="server" Visible="false">
                <div class="table-responsive">
                    <asp:GridView ID="gvRecepciones" runat="server" 
                        AutoGenerateColumns="False" 
                        CssClass="table table-bordered table-hover table-sm" 
                        EmptyDataText="Sin registros">
                        <HeaderStyle CssClass="table-header" />
                        <Columns>
                            <asp:BoundField DataField="Folio" HeaderText="Folio" />
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                            <asp:BoundField DataField="Tanque" HeaderText="Tanque" />
                            <asp:BoundField DataField="Producto" HeaderText="Producto" />
                            <asp:BoundField DataField="VolumenRecepcion" HeaderText="Volumen" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:Button ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
                                        Text="Eliminar"
                                        OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Folio")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>
        </div>
    </form>
</div>

<script>
    function confirmarEliminar(Folio) {
        Swal.fire({
            title: "¿Estás seguro?",
            text: "Esta acción eliminará la Recepción de forma permanente.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                __doPostBack('btnEliminar', Folio);
            }
        });
    }

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

    /* Estilos del GridView */
    .table-header {
        background-color: #2c3e50;
        color: white;
        padding: 12px 8px;
        font-weight: 600;
    }

    .table th {
        background-color: #2c3e50;
        color: white;
    }

    .table td {
        vertical-align: middle;
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
