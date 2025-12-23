<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EliminarCompras.aspx.cs" Inherits="SoftwarePlantas.Compras.EliminarCompras" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Eliminar Compra</title>
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
                <h3 class="text-center">Eliminar Recepcion</h3>
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
                    <asp:GridView ID="gvRecepciones" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered mt-4" EmptyDataText="Sin registros">
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






</body>
</html>
