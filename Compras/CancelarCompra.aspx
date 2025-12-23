<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CancelarCompra.aspx.cs" Inherits="SoftwarePlantas.Compras.CancelarCompra" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Cancelar Compra</title>
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
    <%-- --%><link href="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap.min.css" rel="stylesheet">

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />



</head>
<%--<body runat="server" class="header-welcome">

    <div class="container mt-5 small" style="margin-left: 150px;">
        <form runat="server">

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
                <h3 class="text-center">Cancelar Recepcion</h3>
                <div class="row mt-4">
                    <div class="col-md-2">
                        <label for="txtFechaInicial" class="form-label">Fecha Inicial</label>
                        <asp:TextBox ID="txtFechaInicial" runat="server" CssClass="form-control" TextMode="Date" placeholder="Seleccione una fecha"></asp:TextBox>
                    </div>
                    <div class="col-md-2">
                        <label for="txtFechaFinal" class="form-label">Fecha Final</label>
                        <asp:TextBox ID="txtFechaFinal" runat="server" CssClass="form-control" TextMode="Date" placeholder="Seleccione una fecha"></asp:TextBox>
                    </div>
                    <div class="col-md-3">
                        <label for="ddlEstacion" class="form-label">Estación</label>
                        <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>


                    <div class="text-center mt-4">
                        <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary" Text="Buscar" OnClick="btnBuscar_Click" />
                    </div>


                </div>

       <asp:Panel ID="pnlResultados" runat="server" Visible="false">
    <div class="table-responsive">
        <asp:GridView ID="gvRecepciones" runat="server" AutoGenerateColumns="False" 
                      CssClass="table table-bordered mt-4" EmptyDataText="Sin registros">
            <Columns>
                <asp:BoundField DataField="Folio" HeaderText="Folio" />
                <asp:BoundField DataField="Tanque" HeaderText="Tanque" />
                <asp:BoundField DataField="Producto" HeaderText="Producto" />
                <asp:BoundField DataField="VolumenRecepcion" HeaderText="Volumen" />
                <asp:BoundField DataField="FechaDocumento" HeaderText="FechaDocumento" />
                <asp:BoundField DataField="VolumenCapturado" HeaderText="Volumen Capturado" />
                <asp:BoundField DataField="FolioDocumento" HeaderText="FolioDocumento" />
                <asp:BoundField DataField="UUID" HeaderText="UUID" />
                <asp:BoundField DataField="ClaveVehiculo" HeaderText="ClaveVehiculo" />
                <asp:BoundField DataField="TransCRE" HeaderText="TransCRE" />
                <asp:BoundField DataField="PrecioCompra" HeaderText="PrecioCompra" />
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <asp:Button ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
                                    Text="Eliminar"
                                    OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Folio")) %>' />
                        <asp:Button ID="btnModificar" runat="server" CssClass="btn btn-danger btn-sm"
                                    Text="Modificar"
                                    OnClientClick='<%# string.Format("confirmarModificar(\"{0}\"); return false;", Eval("Folio")) %>' />
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
            text: "Esta acción eliminará la Recepcion permanente.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        }).then((result) => {
            if (result.isConfirmed) {
                // Si el usuario confirma, Folio postback y llamar al servidor
                __doPostBack('btnEliminar', Folio);
            }
        });
    }


</script>






</body>--%>
<body runat="server" class="header-welcome" style="margin-left: 150px;">


    <div class="container-fluid mt-5 px-4" >
        <form runat="server">

            <uc:CerrarSesion ID="CerrarSesion" runat="server" />

            <!-- Botón menú lateral -->
            <button type="button" class="menu-toggle" onclick="toggleMenu()">☰</button>
            <uc:MenuLateral ID="MenuLateral" runat="server" />

            <!-- Título -->
            <div class="row justify-content-center">
                <div class="col-md-6 text-center">
                    <asp:Label ID="lblWelcome" runat="server" CssClass="h4 text-primary"></asp:Label>
                </div>
            </div>
            <h3 class="text-center">Cancelar Recepcion</h3>

            <!-- Filtros -->
            <div class="row mt-4">
                <div class="col-md-2">
                    <label for="txtFechaInicial" class="form-label">Fecha Inicial</label>
                    <asp:TextBox ID="txtFechaInicial" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-2">
                    <label for="txtFechaFinal" class="form-label">Fecha Final</label>
                    <asp:TextBox ID="txtFechaFinal" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-3">
                    <label for="ddlEstacion" class="form-label">Estación</label>
                    <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged" />
                </div>
                <div class="col-md-2 align-self-end">
                    <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary w-100" Text="Buscar" OnClick="btnBuscar_Click" />
                </div>
            </div>

            <!-- Resultados -->
            <asp:Panel ID="pnlResultados" runat="server" Visible="false" CssClass="mt-4">
                <div class="table-responsive">
                    <asp:GridView ID="gvRecepciones" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-bordered table-sm table-hover text-nowrap align-middle"
                        EmptyDataText="Sin registros"
                        OnRowEditing="gvRecepciones_RowEditing"
                        OnRowCancelingEdit="gvRecepciones_RowCancelingEdit"
                        OnRowUpdating="gvRecepciones_RowUpdating"
                        DataKeyNames="Folio">
                        <Columns>
                            <asp:TemplateField HeaderText="Folio">
                                <ItemTemplate>
                                    <%# Eval("Folio") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Tanque">
                                <ItemTemplate>
                                    <%# Eval("Tanque") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Producto">
                                <ItemTemplate>
                                    <%# Eval("Producto") %>
                                </ItemTemplate>
                            </asp:TemplateField>


                            <asp:TemplateField HeaderText="Volumen">
                                <ItemTemplate>
                                    <%# Eval("VolumenRecepcion") %>
                                </ItemTemplate>
                            </asp:TemplateField>




                            <asp:TemplateField HeaderText="FechaDocumento">
                                <ItemTemplate>
                                    <%# Eval("FechaDocumento") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control"
                                        Text='<%# Convert.ToDateTime(Eval("FechaDocumento")).ToString("yyyy-MM-dd") %>'
                                        TextMode="Date" />

                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Volumen Capturado">
                                <ItemTemplate>
                                    <%# Eval("VolumenCapturado") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtVolumenCapturado" runat="server" CssClass="form-control" Text='<%# Bind("VolumenCapturado") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="FolioDocumento">
                                <ItemTemplate>
                                    <%# Eval("FolioDocumento") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtFolioDocumento" runat="server" CssClass="form-control" Text='<%# Bind("FolioDocumento") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="UUID">
                                <ItemTemplate>
                                    <%# Eval("UUID") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtUUID" runat="server" CssClass="form-control" Text='<%# Bind("UUID") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="ClaveVehiculo">
                                <ItemTemplate>
                                    <%# Eval("ClaveVehiculo") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtClaveVehiculo" runat="server" CssClass="form-control" Text='<%# Bind("ClaveVehiculo") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="TransCRE">
                                <ItemTemplate>
                                    <%# Eval("TransCRE") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtTransCRE" runat="server" CssClass="form-control" Text='<%# Bind("TransCRE") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="PrecioCompra">
                                <ItemTemplate>
                                    <%# Eval("PrecioCompra") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtPrecioCompra" runat="server" CssClass="form-control" Text='<%# Bind("PrecioCompra") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>



                            <%--  <asp:TemplateField HeaderText="Acciones">
                                <ItemStyle Width="150px" />
                                <ItemTemplate>
                                    <div class="d-flex gap-1 justify-content-center">
                                        <asp:Button ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
                                            Text="Eliminar"
                                            OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Folio")) %>' />
                                        <asp:Button ID="btnModificar" runat="server" CssClass="btn btn-warning btn-sm"
                                            Text="Modificar"
                                            OnClientClick='<%# string.Format("confirmarModificar(\"{0}\"); return false;", Eval("Folio")) %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>--%>

                            <asp:TemplateField HeaderText="Acciones">
    <ItemStyle Width="180px" />
    <ItemTemplate>
        <div class="d-flex flex-column gap-1">
            <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
                Text="Eliminar"
                OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Folio")) %>' />
            <asp:LinkButton ID="btnModificar" runat="server" CssClass="btn btn-warning btn-sm"
                Text="Modificar" CommandName="Edit" />
        </div>
    </ItemTemplate>
    <EditItemTemplate>
        <div class="d-flex flex-column gap-1">
            <asp:LinkButton ID="btnGuardar" runat="server" CssClass="btn btn-success btn-sm"
                Text="Guardar" CommandName="Update" />
            <asp:LinkButton ID="btnCancelar" runat="server" CssClass="btn btn-secondary btn-sm"
                Text="Cancelar" CommandName="Cancel" />
        </div>
    </EditItemTemplate>
</asp:TemplateField>


                        </Columns>

                    </asp:GridView>
                </div>
            </asp:Panel>

        </form>
    </div>
    <script>


        function confirmarEliminar(Folio) {
            Swal.fire({
                title: "¿Estás seguro?",
                text: "Esta acción eliminará la Recepcion permanente.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#3085d6",
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar"
            }).then((result) => {
                if (result.isConfirmed) {
                    // Si el usuario confirma, Folio postback y llamar al servidor
                    __doPostBack('btnEliminar', Folio);
                }
            });
        }




    </script>



</body>


</html>
