<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CancelarCompra.aspx.cs" Inherits="SoftwarePlantas.Compras.CancelarCompra" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Cancelar Compra</title>

    <!-- Bootstrap 5 CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />

    <!-- SweetAlert2 -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- jQuery -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <!-- Bootstrap 5 JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

    <!-- ✅ DEFINIR FUNCIONES GLOBALES AQUÍ -->
   <script type="text/javascript">
       window.abrirModalEditar = function (data) {
           console.log('Abriendo modal con datos:', data);

           try {
               document.getElementById('mFechaDocumento').value = data.fechaDocumento || '';
               document.getElementById('mVolumenCapturado').value = data.volumenCapturado || '';
               document.getElementById('mFolioDocumento').value = data.folioDocumento || '';
               document.getElementById('mUUID').value = data.uuid || '';
               document.getElementById('mClaveVehiculo').value = data.claveVehiculo || '';
               document.getElementById('mTransCRE').value = data.transCRE || '';
               document.getElementById('mPrecioCompra').value = data.precioCompra || '';
               document.getElementById('<%= hfIdProveedor.ClientID %>').value = data.idProveedor || '';

            // Establecer el proveedor seleccionado en el dropdown
            var ddl = document.getElementById('<%= ddlProveedorModal.ClientID %>');
               if (ddl && data.idProveedor) {
                   ddl.value = data.idProveedor;
                   console.log('Proveedor seleccionado:', data.idProveedor);
               }

               var modalElement = document.getElementById('modalEditarRecepcion');
               if (!modalElement) {
                   console.error('No se encontró el elemento del modal');
                   return;
               }

               var modal = new bootstrap.Modal(modalElement);
               modal.show();
               console.log('Modal abierto correctamente');
           } catch (error) {
               console.error('Error al abrir modal:', error);
           }
       };

       window.guardarModal = function () {
           // Guardar valores de los campos en los HiddenFields
           document.getElementById('<%= hfFechaDocumento.ClientID %>').value = document.getElementById('mFechaDocumento').value;
        document.getElementById('<%= hfVolumenCapturado.ClientID %>').value = document.getElementById('mVolumenCapturado').value;
        document.getElementById('<%= hfFolioDocumento.ClientID %>').value = document.getElementById('mFolioDocumento').value;
        document.getElementById('<%= hfUUID.ClientID %>').value = document.getElementById('mUUID').value;
        document.getElementById('<%= hfClaveVehiculo.ClientID %>').value = document.getElementById('mClaveVehiculo').value;
        document.getElementById('<%= hfTransCRE.ClientID %>').value = document.getElementById('mTransCRE').value;
        document.getElementById('<%= hfPrecioCompra.ClientID %>').value = document.getElementById('mPrecioCompra').value;

        // Guardar el IdProveedor seleccionado del dropdown
        var ddl = document.getElementById('<%= ddlProveedorModal.ClientID %>');
        document.getElementById('<%= hfIdProveedor.ClientID %>').value = ddl ? ddl.value : '';

        console.log('IdProveedor a guardar:', ddl ? ddl.value : 'vacío');

        // Cerrar modal
        var modalElement = document.getElementById('modalEditarRecepcion');
        var modal = bootstrap.Modal.getInstance(modalElement);
        if (modal) modal.hide();

        // Disparar postback para guardar en el servidor
        document.getElementById('<%= btnGuardarModal.ClientID %>').click();
       };

       function confirmarEliminar(folio) {
           Swal.fire({
               title: "¿Estás seguro?",
               text: "Esta acción eliminará la Recepcion " + folio + " de forma permanente.",
               icon: "warning",
               showCancelButton: true,
               confirmButtonColor: "#d33",
               cancelButtonColor: "#3085d6",
               confirmButtonText: "Sí, eliminar",
               cancelButtonText: "Cancelar"
           }).then((result) => {
               if (result.isConfirmed) {
                   __doPostBack('btnEliminar', folio);
               }
           });
       }

       function toggleMenu() {
           var sidebar = document.getElementById("sidebarMenu");
           if (sidebar) {
               sidebar.classList.toggle("show");
           }
       }
</script>
</head>


<body>
    <!-- Menú lateral -->
    <uc:MenuLateral ID="MenuLateral" runat="server" />

    <!-- Contenedor principal con margen para el sidebar -->
    <div class="main-content">
        <form id="form1" runat="server">
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

            <!-- Botón para dispositivos móviles -->
            <button type="button" class="menu-toggle" onclick="toggleMenu()">☰</button>

            <!-- Botón de cerrar sesión -->
            <uc:CerrarSesion ID="CerrarSesion" runat="server" />

            <!-- Contenido de la página -->
            <div class="container-fluid px-4 py-3">

                <!-- Título -->
                <div class="row justify-content-center mb-3">
                    <div class="col-12 text-center">
                        <asp:Label ID="lblWelcome" runat="server" CssClass="h4 text-primary"></asp:Label>
                    </div>
                </div>

                <h3 class="text-center mb-4">Cancelar Recepción</h3>

                <!-- Filtros -->
                <div class="row g-3 mb-4">
                    <div class="col-md-3">
                        <label for="txtFechaInicial" class="form-label">Fecha Inicial</label>
                        <asp:TextBox ID="txtFechaInicial" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-md-3">
                        <label for="txtFechaFinal" class="form-label">Fecha Final</label>
                        <asp:TextBox ID="txtFechaFinal" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>

                    <div class="col-md-4">
                        <label for="ddlEstacion" class="form-label">Estación</label>
                        <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged" />
                    </div>

                    <div class="col-md-2 d-flex align-items-end">
                        <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary w-100"
                            Text="Buscar" OnClick="btnBuscar_Click" />
                    </div>
                </div>

                <!-- Resultados -->
                <asp:Panel ID="pnlResultados" runat="server" Visible="false">
                    <div class="table-container">
                        <asp:GridView ID="gvRecepciones" runat="server"
                            AutoGenerateColumns="False"
                            CssClass="table table-bordered table-sm table-hover text-nowrap align-middle"
                            EmptyDataText="Sin registros"
                            DataKeyNames="Id"
                            OnRowCommand="gvRecepciones_RowCommand">

                            <Columns>

                                <asp:TemplateField HeaderText="Id">
                                    <ItemTemplate><%# Eval("Id") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Folio">
                                    <ItemTemplate><%# Eval("Folio") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Tanque">
                                    <ItemTemplate><%# Eval("Tanque") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Producto">
                                    <ItemTemplate><%# Eval("Producto") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Volumen">
                                    <ItemTemplate><%# Eval("VolumenRecepcion") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="FechaDocumento">
                                    <ItemTemplate><%# Eval("FechaDocumento") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Volumen Capturado">
                                    <ItemTemplate><%# Eval("VolumenCapturado") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="FolioDocumento">
                                    <ItemTemplate><%# Eval("FolioDocumento") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="UUID">
                                    <ItemTemplate><%# Eval("UUID") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="ClaveVehiculo">
                                    <ItemTemplate><%# Eval("ClaveVehiculo") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="TransCRE">
                                    <ItemTemplate><%# Eval("TransCRE") %></ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="PrecioCompra">
                                    <ItemTemplate><%# Eval("PrecioCompra") %></ItemTemplate>
                                </asp:TemplateField>

                            <asp:TemplateField HeaderText="Proveedor">
    <ItemTemplate>
        <%# ObtenerNombreProveedorPorId(Eval("IdProveedor")) %>
    </ItemTemplate>
</asp:TemplateField>

                                <asp:TemplateField HeaderText="Acciones">
                                    <ItemStyle Width="180px" />
                                    <ItemTemplate>
                                        <div class="d-flex flex-column gap-1">
                                            <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
                                                Text="Eliminar"
                                                OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Folio")) %>' />

                                            <!-- Abre modal -->
                                          <asp:LinkButton ID="btnModificar" runat="server"
    CssClass="btn btn-warning btn-sm"
    Text="Modificar"
    CommandName="OpenEditModal"
    CommandArgument='<%# Eval("Id") %>' />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                        </asp:GridView>
                    </div>
                </asp:Panel>

                <!-- ===================== -->
                <!-- HiddenFields + botón server -->
                <!-- ===================== -->
                <asp:HiddenField ID="hfId" runat="server" />
                <asp:HiddenField ID="hfFechaDocumento" runat="server" />
                <asp:HiddenField ID="hfVolumenCapturado" runat="server" />
                <asp:HiddenField ID="hfFolioDocumento" runat="server" />
                <asp:HiddenField ID="hfUUID" runat="server" />
                <asp:HiddenField ID="hfClaveVehiculo" runat="server" />
                <asp:HiddenField ID="hfTransCRE" runat="server" />
                <asp:HiddenField ID="hfPrecioCompra" runat="server" />
                <asp:HiddenField ID="hfRFCProveedor" runat="server" />

                <asp:Button ID="btnGuardarModal" runat="server" CssClass="d-none"
                    Text="GuardarModal" OnClick="btnGuardarModal_Click" />

                <!-- ===================== -->
                <!-- Modal Bootstrap -->
                <!-- ===================== -->
                <div class="modal fade" id="modalEditarRecepcion" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">

                            <div class="modal-header">
                                <h5 class="modal-title">Modificar recepción</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button>
                            </div>

                            <div class="modal-body">
                                <div class="row g-3">
                                    <asp:HiddenField ID="hfIdProveedor" runat="server" />
                                    <div class="col-md-4">
                                        <label class="form-label">Fecha documento</label>
                                        <input type="date" id="mFechaDocumento" class="form-control" />
                                    </div>

                                    <div class="col-md-4">
                                        <label class="form-label">Volumen capturado</label>
                                        <input type="text" id="mVolumenCapturado" class="form-control" />
                                    </div>

                                    <div class="col-md-4">
                                        <label class="form-label">Precio compra</label>
                                        <input type="text" id="mPrecioCompra" class="form-control" />
                                    </div>

                                    <div class="col-md-6">
                                        <label class="form-label">Folio documento</label>
                                        <input type="text" id="mFolioDocumento" class="form-control" />
                                    </div>

                                    <div class="col-md-6">
                                        <label class="form-label">UUID</label>
                                        <input type="text" id="mUUID" class="form-control" />
                                    </div>

                                    <div class="col-md-4">
                                        <label class="form-label">Clave vehículo</label>
                                        <input type="text" id="mClaveVehiculo" class="form-control" />
                                    </div>

                                    <div class="col-md-4">
                                        <label class="form-label">TransCRE</label>
                                        <input type="text" id="mTransCRE" class="form-control" />
                                    </div>

                                    <div class="col-md-4">
                                        <label class="form-label">Proveedor</label>
                                        <asp:DropDownList ID="ddlProveedorModal" runat="server" CssClass="form-select"></asp:DropDownList>
                                    </div>

                                </div>
                            </div>

                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                                <button type="button" class="btn btn-success" onclick="guardarModal()">Guardar</button>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </form>
    </div>

   
    <style>
        .main-content {
            margin-left: 250px;
            min-height: 100vh;
            background-color: #f8f9fa;
            transition: margin-left 0.3s ease;
            overflow-x: hidden;
        }

        .table-container {
            width: 100%;
            overflow-x: auto;
            overflow-y: visible;
            -webkit-overflow-scrolling: touch;
            margin-bottom: 20px;
        }

        .table {
            width: 100%;
            min-width: 1400px;
            margin-bottom: 0;
            white-space: nowrap;
        }

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

        .menu-toggle:hover { background-color: #34495e; }

        .table th {
            background-color: #2c3e50;
            color: white;
            padding: 12px 8px;
            font-weight: 600;
            position: sticky;
            top: 0;
            z-index: 10;
        }

        .table td {
            padding: 8px;
            vertical-align: middle;
        }

        @media (max-width: 768px) {
            .main-content { margin-left: 0; }
            .menu-toggle { display: block; }
            .table { min-width: 1200px; }
        }

        .table-container::-webkit-scrollbar { height: 10px; }
        .table-container::-webkit-scrollbar-track { background: #f1f1f1; border-radius: 5px; }
        .table-container::-webkit-scrollbar-thumb { background: #888; border-radius: 5px; }
        .table-container::-webkit-scrollbar-thumb:hover { background: #555; }
    </style>
</body>
</html>
