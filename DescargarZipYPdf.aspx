<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DescargarZipYPdf.aspx.cs" Inherits="SoftwarePlantas.DescargarZipYPdf" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Descargar ZIP y PDF</title>
    <script type="text/javascript">
        function descargarZipYabrirPDF(pdfUrl) {
            setTimeout(function () {
                window.open(pdfUrl, '_blank');
            }, 2000);
        }
    </script>
</head>
<body onload="descargarZipYabrirPDF('<%= Session["rutaPdfWeb"] %>')">
    <form id="form1" runat="server">
        <asp:Literal ID="litScript" runat="server"></asp:Literal>
    </form>
</body>
</html>
