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
               <h3 class="text-center">Consulta Ventas</h3>
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
                <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-primary" Text="Buscar" OnClick="btnBuscar_Click" />
            </div>

            <div class="mt-5">


  <asp:GridView ID="gvDespachos" runat="server" AutoGenerateColumns="False" DataKeyNames="Transaccion"
            CssClass="table table-striped table-bordered small-font-gridview" EmptyDataText="No hay registros disponibles.">
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
                        <!-- Botón Subir XML (Solo se muestra si UUID está vacío) -->
                     <asp:Button ID="btnAccion" runat="server" CssClass='<%# string.IsNullOrEmpty(Eval("uuid").ToString()) ? "btn btn-primary btn-sm" : "btn btn-success btn-sm" %>'
            Text='<%# string.IsNullOrEmpty(Eval("uuid").ToString()) ? "Subir XML" : "Facturado" %>'
            OnClientClick='<%# string.IsNullOrEmpty(Eval("uuid").ToString()) ? string.Format("abrirModal(\"{0}\"); return false;", Eval("Transaccion")) : "return false;" %>'
          Enabled='<%# Eval("VentaEliminada") != DBNull.Value && !Convert.ToBoolean(Eval("VentaEliminada")) %>'  />
                          

        
                        <!-- Espacio entre botones -->
                        &nbsp;

                        <!-- Botón Eliminar (Siempre visible) -->
                 <asp:Button ID="btnEliminar" runat="server" CssClass="btn btn-danger btn-sm"
            Text="Eliminar"
            OnClientClick='<%# string.Format("confirmarEliminar(\"{0}\"); return false;", Eval("Transaccion")) %>'
              Enabled='<%# Eval("VentaEliminada") != DBNull.Value && !Convert.ToBoolean(Eval("VentaEliminada")) %>' />
                          <!-- Botón Eliminar (Siempre visible) -->
                   <!-- Botón Desenlazar (Siempre visible) -->
        <asp:Button ID="btnEliminarFactura" runat="server" CssClass="btn btn-black btn-sm"
            Text="Desenlazar"
            OnClientClick='<%# string.Format("confirmarEliminarFactura(\"{0}\"); return false;", Eval("Transaccion")) %>'
           Enabled='<%# Eval("VentaEliminada") != DBNull.Value && !Convert.ToBoolean(Eval("VentaEliminada")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                    
        </Columns>
    </asp:GridView>
</div>
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
                <asp:HiddenField ID="hfTransaccion" runat="server" /> <!-- Almacena la Transacción -->
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

<!-- Modal para mostrar el XML -->
<div id="xmlModal" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="xmlModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="xmlModalLabel">Vista Previa del XML</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <pre id="xmlContent" style="white-space: pre-wrap; word-wrap: break-word;"></pre>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
               <%-- <asp:Button ID="btnContinuar" runat="server" CssClass="btn btn-primary" OnClick="btnContinuar_Click" Text="Continuar" />--%>
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
