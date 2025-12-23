
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MenuLateral.ascx.cs" Inherits="SoftwarePlantas.MenuLateral" %>

<div class="sidebar" id="sidebarMenu">
    <h3 class="text-center">Menú</h3>
    <asp:HyperLink ID="lnkInicio" runat="server" NavigateUrl="~/Default.aspx" CssClass="d-block px-3 py-2" Text="Home"></asp:HyperLink>
    <asp:HyperLink ID="lnkConsulta" runat="server" NavigateUrl="~/ConsultaVentas.aspx" CssClass="d-block px-3 py-2" Text="Ventas"></asp:HyperLink>
    <asp:HyperLink ID="lnkVolumetrico" runat="server" NavigateUrl="~/ControlVolumetrico.aspx" CssClass="d-block px-3 py-2" Text="CV SAT"></asp:HyperLink>
    
    <div class="menu-compras px-3 py-2">
        <span class="d-block font-weight-bold">Compras &#9654;</span>

        <div class="submenu">
            <asp:HyperLink ID="lnkOrdenesCompra" runat="server" NavigateUrl="~/Compras/AgregarCompras.aspx" CssClass="d-block py-1" Text="Agregar Recepción"></asp:HyperLink>
            <asp:HyperLink ID="lnkProveedores" runat="server" NavigateUrl="~/Compras/EliminarCompras.aspx" CssClass="d-block py-1" Text="Eliminar Recepción"></asp:HyperLink>
            <asp:HyperLink ID="lnkRecepciones" runat="server" NavigateUrl="~/Compras/CancelarCompra.aspx" CssClass="d-block py-1" Text="Cancelar Recepción"></asp:HyperLink>
        </div>
    </div>
    <%-- <asp:HyperLink ID="lnkAgregarVentas" runat="server" NavigateUrl="~/AgregaVentas.aspx" CssClass="d-block px-3 py-2" Text="AgregaVentas"></asp:HyperLink>--%>


</div>

<style>
 .sidebar {
    min-width: 160px;
    position: fixed;
    top: 0;
    left: 0;
    height: 100%;
    background-color: lightgray;
    padding-top: 100px;
    color: black;
    z-index: 1;
    transform: translateX(0);
    transition: transform 0.3s ease;
}

.sidebar h3 {
    margin-left: 20px;
}

.sidebar a {
    color: black;
    text-decoration: none;
    padding: 10px 20px;
    display: block;
}

.sidebar a:hover {
    background-color: #495057;
}

.menu-compras {
    position: relative;
}

.menu-compras > span {
    cursor: pointer;
}

.menu-compras:hover .submenu {
    display: block;
}

.submenu {
    display: none;
    position: absolute;
    top: 0;
    left: 100%; /* se abre a la derecha */
    background-color: #e0e0e0;
    min-width: 180px;
    box-shadow: 2px 2px 5px rgba(0,0,0,0.2);
    padding: 5px 0;
    z-index: 2;
}

.submenu a {
    padding-left: 20px;
    font-size: 14px;
    white-space: nowrap;
}

.menu-compras:hover {
    background-color: #bdbdbd;
}

@media (max-width: 768px) {
    .sidebar {
        transform: translateX(-250px);
    }

    .sidebar.show {
        transform: translateX(0);
    }
}


</style>
