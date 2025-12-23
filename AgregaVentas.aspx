<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AgregaVentas.aspx.cs" Inherits="SoftwarePlantas.AgregaVentas" %>

<%@ Register Src="~/MenuLateral.ascx" TagPrefix="uc" TagName="MenuLateral" %>
<%@ Register Src="~/CerrarSesion.ascx" TagPrefix="uc" TagName="CerrarSesion" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Agregar Ventas</title>
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

<body runat="server" class="header-welcome" style="margin-left: 150px;">


    <div class="container-fluid mt-5 px-4">
        <form id="form1" runat="server">

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
            <h3 class="text-center">Agregar Ventas</h3>

            <!-- Filtros -->
            <div class="row g-3 align-items-end">
                <div class="col-md-5">
                    <label for="ddlEstacion" class="form-label">Estación</label>
                    <asp:DropDownList ID="ddlEstacion" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstacion_SelectedIndexChanged" />
                </div>

                <div class="col-md-5">
                    <label for="fileExcel" class="form-label">Archivo Excel (.xlsx/.xls)</label>
                    <asp:FileUpload ID="fileExcel" runat="server" CssClass="form-control" />
                </div>

                <div class="col-md-3">
                  <asp:Button ID="btnBuscar" runat="server"
    CssClass="btn btn-primary w-100"
    Text="Agregar Ventas"
    UseSubmitBehavior="false"
    OnClientClick="return enviarVentas();"
    OnClick="btnBuscar_Click" />   <!-- 👈 esto faltaba -->



                </div>
            </div>




        </form>
    </div>
 <script>
     var _enviando = false;

     function enviarVentas() {
         if (_enviando) return false;

         var btn = document.getElementById('<%= btnBuscar.ClientID %>');
      var ddl = document.getElementById('<%= ddlEstacion.ClientID %>');
      var fu = document.getElementById('<%= fileExcel.ClientID %>');
    var form = document.getElementById('<%= form1.ClientID %>') || document.forms[0];

    // Validaciones mínimas
    if (!ddl || !ddl.value) { Swal.fire('Falta estación','Selecciona una estación.','warning'); return false; }
    if (!fu || !fu.value)   { Swal.fire('Archivo requerido','Selecciona un Excel.','warning'); return false; }
    if (!/\.xlsx?$/i.test(fu.value)) { Swal.fire('Formato no válido','Debe ser .xlsx o .xls','error'); return false; }

    // UI
    if (btn) { btn.disabled = true; if (btn.value !== undefined) btn.value = 'Procesando...'; else btn.innerText = 'Procesando...'; }
    _enviando = true;

    // Preferir __doPostBack si está disponible
    if (typeof __doPostBack === 'function') {
      __doPostBack('<%= btnBuscar.UniqueID %>', '');
    } else {
      // Fijar __EVENTTARGET para que el servidor ejecute btnBuscar_Click
      var et = document.getElementById('__EVENTTARGET');
      var ea = document.getElementById('__EVENTARGUMENT');
      if (!et) { et = document.createElement('input'); et.type='hidden'; et.id='__EVENTTARGET'; et.name='__EVENTTARGET'; form.appendChild(et); }
      if (!ea) { ea = document.createElement('input'); ea.type='hidden'; ea.id='__EVENTARGUMENT'; ea.name='__EVENTARGUMENT'; form.appendChild(ea); }
      et.value = '<%= btnBuscar.UniqueID %>';
             ea.value = '';
             form.submit();
         }
         return false; // evitamos el submit por defecto (ya enviamos nosotros)
     }
 </script>





</body>


</html>
