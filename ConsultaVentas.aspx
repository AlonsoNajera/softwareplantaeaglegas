<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsultaVentas.aspx.cs" Inherits="SoftwarePlantas.ConsultaVentas" %>
<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>

<!DOCTYPE html>

<html lang="es">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>ConsultaVentas</title>
    
<!-- jQuery (cargar primero) -->
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
            
            <h3 class="text-center mb-4">Consulta Ventas</h3>
            
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

            <!-- GridView -->
            <div class="table-responsive">
                <asp:GridView ID="gvDespachos" runat="server" AutoGenerateColumns="False" DataKeyNames="Transaccion"
                    CssClass="table table-striped table-bordered table-hover small-font-gridview" EmptyDataText="No hay registros disponibles.">
                    <Columns> 
                        <asp:BoundField DataField="Transaccion" HeaderText="Transaccion" ReadOnly="True"/>
                        <asp:BoundField DataField="Posicion" HeaderText="Posicion" ReadOnly="True" />
                        <asp:BoundField DataField="NumTicket" HeaderText="NumTicket" ReadOnly="True" Visible="False" />
                        <asp:BoundField DataField="Producto" HeaderText="Producto" ReadOnly="True" />
                        <asp:BoundField DataField="FechaHora" HeaderText="FechaHora" ReadOnly="True" DataFormatString="{0:dd/MM/yyyy hh:mm:ss tt}" HtmlEncode="False" />
                        
                        <asp:TemplateField HeaderText="Precio">
                            <ItemStyle CssClass="precio-column" />
                            <ItemTemplate>
                                <asp:Label ID="lblPrecioVenta" runat="server" Text='<%# String.Format("{0:N2}",Eval("Precio")) %>'/>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Volumen">
                            <ItemTemplate>
                                <asp:Label ID="lblVolumenVenta" runat="server" Text='<%# String.Format("{0:N2}", Eval("Volumen")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Importe">
                            <ItemTemplate>
                                <asp:Label ID="lblImporteVenta" runat="server" Text='<%# String.Format("{0:N2}", Eval("ImporteVenta")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="uuid" HeaderText="UUID" ReadOnly="True" ItemStyle-CssClass="uuid-column"/>
                        <asp:BoundField DataField="Factura" HeaderText="Factura" ReadOnly="True" ItemStyle-CssClass="txt-column"/>
                        <asp:BoundField DataField="VentaEliminada" HeaderText="VentaEliminada" ReadOnly="True" Visible="False" />

                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <div class="d-flex gap-1 flex-wrap">
                                    <asp:Button ID="btnAccion" runat="server" 
                                        CssClass='<%# string.IsNullOrEmpty(Eval("uuid").ToString()) ? "btn btn-primary btn-sm" : "btn btn-success btn-sm" %>'
                                        Text='<%# string.IsNullOrEmpty(Eval("uuid").ToString()) ? "Subir XML" : "Facturado" %>'
                                        OnClientClick='<%# string.IsNullOrEmpty(Eval("uuid").ToString()) ? string.Format("abrirModal(\"{0}\"); return false;", Eval("Transaccion")) : "return false;" %>'
                                        Enabled='<%# Eval("VentaEliminada") != DBNull.Value && !Convert.ToBoolean(Eval("VentaEliminada")) %>' />
                                    
                                    <asp:Button ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
                                        Text="Eliminar"
                                        OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Transaccion")) %>'
                                        Enabled='<%# Eval("VentaEliminada") != DBNull.Value && !Convert.ToBoolean(Eval("VentaEliminada")) %>' />
                                    
                                    <asp:Button ID="btnEliminarFactura" runat="server" CssClass="btn btn-dark btn-sm"
                                        Text="Desenlazar"
                                        OnClientClick='<%# string.Format("confirmarEliminarFactura(\"{0}\"); return false;", Eval("Transaccion")) %>'
                                        Enabled='<%# Eval("VentaEliminada") != DBNull.Value && !Convert.ToBoolean(Eval("VentaEliminada")) %>' />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- Modal para subir XML -->
        <div class="modal fade" id="modalSubirXML" tabindex="-1" aria-labelledby="modalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalLabel">Subir XML</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <asp:HiddenField ID="hfTransaccion" runat="server" />
                        <p><strong>Transacción: </strong> <span id="lblTransaccionModal"></span></p>
                        <asp:FileUpload ID="fileUploadXML" runat="server" CssClass="form-control" />
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnSubirXML" runat="server" CssClass="btn btn-primary" Text="Subir" OnClick="btnSubirXML_Click" />
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
    </form>
</div>

<script>
    function confirmarEliminar(transaccion) {
        Swal.fire({
            title: "¿Estás seguro?",
            text: "Esta acción eliminará la venta de manera permanente.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                // Si el usuario confirma, hacer postback y llamar al servidor
                __doPostBack('btnEliminar', transaccion);
            }
        });
    }

    function confirmarEliminarFactura(transaccion) {
        Swal.fire({
            title: "¿Estás seguro?",
            text: "Esta acción eliminará la Factura de manera permanente.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                // Si el usuario confirma, hacer postback y llamar al servidor
                __doPostBack('btnEliminarFactura', transaccion);
            }
        });
    }

    function mostrarAlerta(titulo, texto, icono) {
        Swal.fire({
            title: titulo,
            text: texto,
            icon: icono,
            confirmButtonText: 'Aceptar',
            allowOutsideClick: false
        });
    }

    function abrirModal(transaccion) {
        document.getElementById('<%= hfTransaccion.ClientID %>').value = transaccion;
        document.getElementById('lblTransaccionModal').innerText = transaccion;

        // Cerrar cualquier modal abierto antes de abrir otro
        var modals = document.querySelectorAll('.modal.show');
        modals.forEach(function (modal) {
            var modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                modalInstance.hide();
            }
        });

        setTimeout(function () {
            var myModal = new bootstrap.Modal(document.getElementById('modalSubirXML'), {
                backdrop: true,  // Permite cerrar haciendo clic fuera
                keyboard: true   // Permite cerrar con la tecla ESC
            });
            myModal.show();
        }, 300);
    }


    // Función para alternar la visibilidad del menú en pantallas pequeñas
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
    .small-font-gridview {
        font-size: 0.85em;
    }

    .small-font-gridview th {
        background-color: lightgray;
        color: white;
        padding: 12px 8px;
        font-weight: 600;
    }

    .small-font-gridview td {
        padding: 8px;
        vertical-align: middle;
    }

    .uuid-column {
        max-width: 200px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .txt-column {
        max-width: 150px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .precio-column {
        text-align: right;
    }

    /* Modal */
    .modal {
        z-index: 1055 !important;
    }

    .modal-backdrop {
        z-index: 1050 !important;
    }

    /* Responsive */
    @media (max-width: 768px) {
        .main-content {
            margin-left: 0;
        }

        .menu-toggle {
            display: block;
        }

        .small-font-gridview {
            font-size: 0.75em;
        }
    }
</style>
</body>
</html>
