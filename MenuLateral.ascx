<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MenuLateral.ascx.cs" Inherits="SoftwarePlantas.MenuLateral" %>

<!-- Font Awesome para iconos -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">

<div class="sidebar" id="sidebarMenu">
    <div class="sidebar-header">
        <i class="fas fa-chart-line"></i>
        <h3>Sistema</h3>
    </div>

    <nav class="sidebar-nav">
        <asp:HyperLink ID="lnkInicio" runat="server" NavigateUrl="~/Default.aspx" CssClass="nav-item">
            <i class="fas fa-home"></i>
            <span>Home</span>
        </asp:HyperLink>

        <asp:HyperLink ID="lnkConsulta" runat="server" NavigateUrl="~/ConsultaVentas.aspx" CssClass="nav-item">
            <i class="fas fa-shopping-cart"></i>
            <span>Ventas</span>
        </asp:HyperLink>

        <asp:HyperLink ID="lnkVolumetrico" runat="server" NavigateUrl="~/ControlVolumetrico.aspx" CssClass="nav-item">
            <i class="fas fa-file-invoice"></i>
            <span>CV SAT</span>
        </asp:HyperLink>

        <!-- Menú Compras con submenú desplegable (acordeón) -->
        <div class="nav-item menu-compras">
            <div class="nav-item-header" onclick="toggleSubmenu(event)">
                <i class="fas fa-box"></i>
                <span>Compras</span>
                <i class="fas fa-chevron-down chevron-icon"></i>
            </div>

            <div class="submenu">
                <asp:HyperLink ID="lnkOrdenesCompra" runat="server" NavigateUrl="~/Compras/AgregarCompras.aspx" CssClass="submenu-item">
                    <i class="fas fa-plus-circle"></i>
                    <span>Agregar Recepción</span>
                </asp:HyperLink>
                <asp:HyperLink ID="lnkProveedores" runat="server" NavigateUrl="~/Compras/EliminarCompras.aspx" CssClass="submenu-item">
                    <i class="fas fa-trash-alt"></i>
                    <span>Eliminar Recepción</span>
                </asp:HyperLink>
                <asp:HyperLink ID="lnkRecepciones" runat="server" NavigateUrl="~/Compras/CancelarCompra.aspx" CssClass="submenu-item">
                    <i class="fas fa-ban"></i>
                    <span>Cancelar Recepción</span>
                </asp:HyperLink>
            </div>
        </div>
    </nav>

    <div class="sidebar-footer">
        <small><i class="fas fa-info-circle"></i> v1.0.0</small>
    </div>
</div>

<style>
/* Variables de color */
:root {
    --sidebar-bg: #2c3e50;
    --sidebar-hover: #34495e;
    --sidebar-active: #3498db;
    --text-primary: #ecf0f1;
    --text-secondary: #bdc3c7;
    --submenu-bg: #1a252f;
    --shadow: rgba(0, 0, 0, 0.3);
}

/* Contenedor principal del sidebar */
.sidebar {
    min-width: 250px;
    width: 250px;
    position: fixed;
    top: 0;
    left: 0;
    height: 100vh;
    background: linear-gradient(180deg, var(--sidebar-bg) 0%, #1a252f 100%);
    color: var(--text-primary);
    z-index: 1000;
    transform: translateX(0);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 2px 0 10px var(--shadow);
    display: flex;
    flex-direction: column;
    overflow-y: auto;
    overflow-x: visible; /* Permite que el contenido sea visible horizontalmente */
}

/* Scrollbar personalizado */
.sidebar::-webkit-scrollbar {
    width: 6px;
}

.sidebar::-webkit-scrollbar-track {
    background: transparent;
}

.sidebar::-webkit-scrollbar-thumb {
    background: var(--sidebar-hover);
    border-radius: 3px;
}

.sidebar::-webkit-scrollbar-thumb:hover {
    background: var(--sidebar-active);
}

/* Header del sidebar */
.sidebar-header {
    padding: 30px 20px;
    text-align: center;
    background: rgba(0, 0, 0, 0.2);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.sidebar-header i {
    font-size: 2.5rem;
    color: var(--sidebar-active);
    margin-bottom: 10px;
    display: block;
}

.sidebar-header h3 {
    margin: 0;
    font-size: 1.4rem;
    font-weight: 600;
    color: var(--text-primary);
    letter-spacing: 1px;
}

/* Navegación */
.sidebar-nav {
    flex: 1;
    padding: 20px 0;
}

/* Items de navegación */
.nav-item {
    display: flex;
    align-items: center;
    padding: 15px 20px;
    color: var(--text-secondary);
    text-decoration: none;
    transition: all 0.3s ease;
    cursor: pointer;
    border-left: 3px solid transparent;
    position: relative;
}

.nav-item:hover {
    background-color: var(--sidebar-hover);
    color: var(--text-primary);
    border-left-color: var(--sidebar-active);
    padding-left: 25px;
}

.nav-item.active {
    background-color: var(--sidebar-hover);
    color: var(--sidebar-active);
    border-left-color: var(--sidebar-active);
}

.nav-item i {
    font-size: 1.2rem;
    width: 30px;
    margin-right: 15px;
    text-align: center;
}

.nav-item span {
    font-size: 0.95rem;
    font-weight: 500;
    flex: 1;
}

/* Menú Compras con submenú desplegable (acordeón) */
.menu-compras {
    position: relative;
    cursor: default;
    display: block;
}

.nav-item-header {
    display: flex;
    align-items: center;
    width: 100%;
    cursor: pointer;
    user-select: none;
}

.chevron-icon {
    margin-left: auto;
    font-size: 0.9rem;
    transition: transform 0.3s ease;
}

.menu-compras.active .chevron-icon {
    transform: rotate(180deg);
}

/* Submenú desplegable (acordeón) */
.submenu {
    max-height: 0;
    overflow: hidden;
    background-color: var(--submenu-bg);
    transition: max-height 0.4s ease, padding 0.4s ease, opacity 0.3s ease;
    padding: 0;
    opacity: 0;
    display: block;
    position: relative;
    z-index: 1;
}

.menu-compras.active .submenu {
    max-height: 500px; /* Aumentado para asegurar que se vea todo */
    padding: 10px 0;
    opacity: 1;
}

.submenu-item {
    display: flex;
    align-items: center;
    padding: 12px 20px 12px 45px;
    color: var(--text-secondary);
    text-decoration: none;
    transition: all 0.2s ease;
    border-left: 3px solid transparent;
    position: relative;
    z-index: 2;
}

.submenu-item:hover {
    background-color: var(--sidebar-hover);
    color: var(--text-primary);
    border-left-color: var(--sidebar-active);
    padding-left: 50px;
    text-decoration: none;
}

.submenu-item.active {
    background-color: var(--sidebar-hover);
    color: var(--sidebar-active);
    border-left-color: var(--sidebar-active);
}

.submenu-item i {
    font-size: 1rem;
    width: 25px;
    margin-right: 12px;
    color: inherit;
}

.submenu-item span {
    font-size: 0.9rem;
    white-space: nowrap;
    color: inherit;
}

/* Footer del sidebar */
.sidebar-footer {
    padding: 15px 20px;
    text-align: center;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(0, 0, 0, 0.2);
}

.sidebar-footer small {
    color: var(--text-secondary);
    font-size: 0.8rem;
}

.sidebar-footer i {
    margin-right: 5px;
}

/* Responsive */
@media (max-width: 768px) {
    .sidebar {
        transform: translateX(-250px);
    }

    .sidebar.show {
        transform: translateX(0);
    }
}

/* Efecto de brillo al hover */
.nav-item::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.1), transparent);
    transform: translateX(-100%);
    transition: transform 0.6s;
}

.nav-item:hover::before {
    transform: translateX(100%);
}

/* Animación de entrada del menú */
@keyframes fadeInLeft {
    from {
        opacity: 0;
        transform: translateX(-20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

.nav-item {
    animation: fadeInLeft 0.5s ease backwards;
}

.nav-item:nth-child(1) { animation-delay: 0.1s; }
.nav-item:nth-child(2) { animation-delay: 0.2s; }
.nav-item:nth-child(3) { animation-delay: 0.3s; }
.nav-item:nth-child(4) { animation-delay: 0.4s; }
</style>

<script>
    // Toggle del submenú de Compras
    function toggleSubmenu(event) {
        event.preventDefault();
        event.stopPropagation();
        
        const menuCompras = document.querySelector('.menu-compras');
        menuCompras.classList.toggle('active');
    }

    // Marcar el item activo según la URL actual
    document.addEventListener('DOMContentLoaded', function() {
        const currentPath = window.location.pathname;
        const navItems = document.querySelectorAll('.nav-item, .submenu-item');
        
        navItems.forEach(item => {
            const href = item.getAttribute('href');
            if (href && currentPath.toLowerCase().includes(href.toLowerCase())) {
                item.classList.add('active');
                
                // Si es un item del submenú, abrir el menú de Compras
                if (item.classList.contains('submenu-item')) {
                    document.querySelector('.menu-compras').classList.add('active');
                }
            }
        });
    });
</script>
