using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Xml;
using System.IO; // Importante para manejar archivos y directorios
using Microsoft.Ajax.Utilities;
using System.Net.NetworkInformation;
using System.Web.Services.Description;
using WebGrease.Css.Ast;
using System.Xml.Linq;
using System.Collections;
using System.Data.SqlTypes;
using System.Text;
using System.Xml.XPath;

namespace SoftwarePlantas
{
    public partial class ConsultaVentas : System.Web.UI.Page
    {
        string instancia = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            // Evitar el almacenamiento en caché
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            if (Session["IdUsuario"] == null || Session["Nombre"] == null)
            {
                // Si la sesión ha expirado o el usuario no está autenticado, redirigir a Login.aspx
                Response.Redirect("Login.aspx");
            }
            else
            {
                // Establecer el mensaje de bienvenida con el nombre del usuario
                lblWelcome.Text = $"Bienvenido, {Session["Nombre"].ToString()}!";
            }
            if (!IsPostBack)
            {
                CargarEstaciones();
                CargarDespachos();
                txtFechaInicial.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtFechaFinal.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if (Request["__EVENTTARGET"] == "btnEliminar" && Request["__EVENTARGUMENT"] != null)
            {
                string transaccion = Request["__EVENTARGUMENT"];
                EliminarVenta(transaccion);
            }

            if (Request["__EVENTTARGET"] == "btnEliminarFactura" && Request["__EVENTARGUMENT"] != null)
            {
                string transaccion = Request["__EVENTARGUMENT"];
                EliminarFactura(transaccion);
            }

       
        }

        private void CargarDespachos()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Transaccion");
            dt.Columns.Add("NumTicket");
            dt.Columns.Add("Producto");
            dt.Columns.Add("FechaHora");
            dt.Columns.Add("Precio");
            dt.Columns.Add("Volumen");
            dt.Columns.Add("ImporteVenta");
            dt.Columns.Add("uuid");
            dt.Columns.Add("factura");
            dt.Columns.Add("Posicion");
            dt.Columns.Add("VentaEliminada");
            // Simula que la tabla está vacía para que el GridView aparezca sin registros.
            dt.Rows.Add(dt.NewRow());

            gvDespachos.DataSource = dt;
            gvDespachos.DataBind();

            // Ocultar los botones de edición cuando la tabla está vacía.
            if (gvDespachos.Rows.Count == 1)
            {
                gvDespachos.Rows[0].Visible = false;
            }
        }

        private void CargarEstaciones()
        {
            int IdUsuario = Convert.ToInt32(Session["IdUsuario"]);

            string connectionString = WebConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT e.Id, e.Nombre 
            FROM estaciones e
            INNER JOIN UsersEstaciones ue ON e.Id = ue.idEstacion
            WHERE ue.idUsuario = @IdUsuario";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@IdUsuario", IdUsuario);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                ddlEstacion.DataSource = dt;
                ddlEstacion.DataTextField = "Nombre";
                ddlEstacion.DataValueField = "Id";
                ddlEstacion.DataBind();
            }

            ddlEstacion.Items.Insert(0, new ListItem("Selecciona una estación", ""));
        }

        private bool VerificarConexionBaseDatos(string connectionString)
        {
            try
            {
                using (SqlConnection testConnection = new SqlConnection(connectionString))
                {
                    testConnection.Open();
                    return true; // Conexión exitosa
                }
            }
            catch
            {
                return false; // Fallo de conexión
            }
        }

        protected void ddlEstacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string estacionId = ddlEstacion.SelectedValue;
           

            string connectionString = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            // Consulta para obtener el valor de 'instancia' usando el ID de la estación seleccionada
            string query = "SELECT instancia FROM estaciones WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", estacionId);
                    connection.Open();
                    instancia = command.ExecuteScalar()?.ToString();
                    connection.Close();
                }
            }


            if (instancia is null)
            {
                // Si la conexión falla, muestra un mensaje de error y detiene la ejecución
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'Selecciona una Estacion.', 'error');", true);
                // Limpiar el control donde se muestran los datos (por ejemplo, un GridView)
                gvDespachos.DataSource = null;
                gvDespachos.DataBind();
                return;

            }

            else
            {
                // Construye la cadena de conexión de la instancia seleccionada
                string instanciaConnectionString = $"Server={instancia};Database=Admingas;User Id=sa;Password=$Aes220213Nu4&;";

                // Llama al método para verificar la conexión
                if (VerificarConexionBaseDatos(instanciaConnectionString))
                {
                    // Si la conexión es exitosa, guarda la instancia en la sesión
                    Session["instanciaSeleccionada"] = instanciaConnectionString;
                    // Recargar los datos en el GridView
                    btnBuscar_Click(null, EventArgs.Empty);
                }
                else
                {
                    // Si la conexión falla, muestra un mensaje de error y detiene la ejecución
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'No se pudo establecer conexión con la base de datos.', 'error');", true);
                }
            }

        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {

                // Obtiene la cadena de conexión de la sesión
                string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

                // Consulta los despachos y llena la tabla
                DateTime fechaInicial, fechaFinal;
            string seleccion = ddlEstacion.SelectedValue; // Obtiene el valor seleccionado

            if (!string.IsNullOrWhiteSpace(seleccion))
            {


                // Llama al método para verificar la conexión
                if (VerificarConexionBaseDatos(instanciaConnectionString))
                {

                    if (!DateTime.TryParse(txtFechaInicial.Text, out fechaInicial) || !DateTime.TryParse(txtFechaFinal.Text, out fechaFinal))
                    {
                        // Mensaje de error en caso de que las fechas no sean válidas
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'Por favor ingrese un rango de fechas válido.', 'error');", true);
                        return;
                    }

                    using (SqlConnection connection = new SqlConnection(instanciaConnectionString))
                    {
                        //string query = "SELECT ID, Tanque, ClaveProducto, VolumenUtil, VolumenFondaje, VolumenDisponible, VolumenVenta, VolumenRecibido, CAST(FechaMedicionActual AS DATE) FechaMedicionActual FROM existenciasvol WHERE CAST(FechaMedicionActual AS DATE) BETWEEN @FechaInicial AND @FechaFinal AND ClaveProducto=@Producto order by CAST(FechaMedicionActual AS DATE)";
                        string query = @"
                                   SELECT Transaccion,NumTicket,Producto,FechaHora,Volumen,Precio,ImporteVenta,uuid,ClaveProd,CASE WHEN SerieFact='' AND VentaEliminada=0 THEN 'SIN RELACION' WHEN SerieFact='' AND VentaEliminada=1 THEN 'VENTA ELIMINADA' ELSE SerieFact+'-'+ CAST(Factura AS nvarchar(200)) END AS Factura,Posicion,VentaEliminada FROM DESPACHOS
                                   WHERE CAST(FechaHoraG AS DATE) BETWEEN @FechaInicial AND @FechaFinal AND TipoReg!='A'
                                   ORDER BY  Transaccion
                                ";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@FechaInicial", fechaInicial);
                        command.Parameters.AddWithValue("@FechaFinal", fechaFinal);


                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        gvDespachos.DataSource = dt;
                        gvDespachos.DataBind();
                    }

                }
                else
                {
                    // Si la conexión falla, muestra un mensaje de error y detiene la ejecución
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'No se pudo establecer conexión con la base de datos.', 'error');", true);
                }

            }
            else
            { 
                // Mostrar mensaje de error al usuario
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                    "Swal.fire('Error', 'Selecciona una Estación.', 'error');", true);

            return; // Detener la ejecución
        }


    }

        //private string ObtenerValorXML(XmlDocument xmlDoc, string xpath, XmlNamespaceManager ns, string defaultValue = "")
        //{
        //    XmlNode node = xmlDoc.SelectSingleNode(xpath, ns);
        //    return node != null ? node.Value : defaultValue;
        //}



        private string ObtenerValorXML(XDocument xmlDoc, string xpath, XmlNamespaceManager ns, string defaultValue = "")
        {
            try
            {
                if (xmlDoc == null || string.IsNullOrEmpty(xpath))
                {
                    return defaultValue; // 🔍 Si el XML está vacío o el XPath no es válido, devolver el valor predeterminado
                }

                // 🔍 Si la consulta es para un atributo (contiene '@')
                if (xpath.Contains("@"))
                {
                    int atIndex = xpath.LastIndexOf('@');
                    string parentPath = xpath.Substring(0, atIndex - 1); // Obtener el nodo padre
                    string attributeName = xpath.Substring(atIndex + 1); // Obtener el nombre del atributo

                    XElement parentElement = xmlDoc.XPathSelectElement(parentPath, ns);
                    XAttribute attribute = parentElement?.Attribute(attributeName);

                    return attribute != null ? attribute.Value.Trim() : defaultValue; // 🔹 Devolver el valor del atributo si existe
                }
                else
                {
                    // 🔍 Si no es un atributo, intentar obtener un nodo (XElement)
                    XElement element = xmlDoc.XPathSelectElement(xpath, ns);
                    if (element != null)
                    {
                        return element.Value.Trim(); // 🔹 Si es un nodo, devolver su contenido
                    }
                }

                return defaultValue; // 🔍 Si no se encontró nada, devolver el valor por defecto
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerValorXML: {ex.Message}"); // 🔴 Log para depuración
                return defaultValue; // 🔍 En caso de error, devolver el valor predeterminado
            }
        }









        //protected void btnSubirXML_Click(object sender, EventArgs e)
        //{
        //    if (fileUploadXML.HasFile)
        //    {
        //        string transaccion = hfTransaccion.Value; // Obtener la Transacción seleccionada

        //        if (string.IsNullOrEmpty(transaccion))
        //        {
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta", "Swal.fire('Error', 'No se ha seleccionado ninguna transacción.', 'error');", true);
        //            return;
        //        }


        //        try
        //        {
        //            // Leer el archivo directamente sin guardarlo en disco
        //            using (Stream stream = fileUploadXML.PostedFile.InputStream)
        //            {
        //                XmlDocument xmlDoc = new XmlDocument();
        //                xmlDoc.Load(stream); // Cargar XML desde el flujo




        //                XmlNamespaceManager ns = new XmlNamespaceManager(xmlDoc.NameTable);
        //                ns.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
        //                ns.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

        //// Extraer el RFC y el nombre del cliente desde el nodo Emisor
        //XmlNode rfcNode = xmlDoc.SelectSingleNode("//cfdi:Receptor/@Rfc", ns);
        //XmlNode nombreClienteNode = xmlDoc.SelectSingleNode("//cfdi:Receptor/@Nombre", ns);
        //XmlNode regimenFiscalNode = xmlDoc.SelectSingleNode("//cfdi:Receptor/@RegimenFiscalReceptor", ns); // Régimen fiscal del cliente
        //XmlNode codigoPostalNode = xmlDoc.SelectSingleNode("//cfdi:Receptor/@DomicilioFiscalReceptor", ns); // Código postal del cliente


        ////

        //XmlNode totalNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@Total", ns); // Total de la factura
        //XmlNode subtotalNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@SubTotal", ns); // Subtotal de la factura
        //XmlNode metodoPagoNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@MetodoPago", ns); // Método de pago
        //XmlNode fechaNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@Fecha", ns); // Fecha de la factura
        //XmlNode FormaPagoNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@FormaPago", ns); // Fecha de la factura
        //                                                                                      //
        //                                                                                      // Extraer el total de los impuestos trasladados desde el nodo Impuestos
        //XmlNode totalImpuestosNode = xmlDoc.SelectSingleNode("//cfdi:Impuestos/@TotalImpuestosTrasladados", ns);


        //// Extraer la serie y folio de la factura desde el nodo Comprobante
        //XmlNode serieNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@Serie", ns);
        //XmlNode folioNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante/@Folio", ns);


        //// Extraer datos del XML
        //XmlNode uuidNode = xmlDoc.SelectSingleNode("//tfd:TimbreFiscalDigital/@UUID", ns);
        //XmlNode cantidadNode = xmlDoc.SelectSingleNode("//cfdi:Concepto/@Cantidad", ns);
        //XmlNode importeNode = xmlDoc.SelectSingleNode("//cfdi:Concepto/@Importe", ns);
        //XmlNode valorUnitarioNode = xmlDoc.SelectSingleNode("//cfdi:Concepto/@ValorUnitario", ns);
        //XmlNode clavesatNode = xmlDoc.SelectSingleNode("//cfdi:Concepto/@ClaveProdServ", ns);
        //XmlNode DescripcionNode = xmlDoc.SelectSingleNode("//cfdi:Concepto/@Descripcion", ns);


        //string uuid = uuidNode != null ? uuidNode.Value : "";
        //string cantidad = cantidadNode != null ? cantidadNode.Value : "0";
        //string importe = importeNode != null ? importeNode.Value : "0.00";
        //string valorUnitario = valorUnitarioNode != null ? valorUnitarioNode.Value : "0.00";
        //string rfcCliente = rfcNode != null ? rfcNode.Value : "SIN RFC";
        //string nomCliente = nombreClienteNode != null ? nombreClienteNode.Value : "DESCONOCIDO";
        //string serieFact = serieNode != null ? serieNode.Value : "";
        //string folioFact = folioNode != null ? folioNode.Value : "";
        //string RCliente = regimenFiscalNode != null ? regimenFiscalNode.Value : "";
        //string Cp = codigoPostalNode != null ? codigoPostalNode.Value : "";

        //string calvesat = clavesatNode != null ? clavesatNode.Value : "";
        //string descripcion = DescripcionNode != null ? DescripcionNode.Value : "SIN DESCRIPCIÓN";

        //string TotalFact = totalNode != null ? totalNode.Value : "0.00";
        //string Subtotal = subtotalNode != null ? subtotalNode.Value : "0.00";
        //string MetodoPago = metodoPagoNode != null ? metodoPagoNode.Value : "PPD";

        //DateTime fechaFact = DateTime.TryParse(fechaNode?.Value, out DateTime fechaTemp) ? fechaTemp : DateTime.MinValue;
        //string Impuestos = totalImpuestosNode != null ? totalImpuestosNode.Value : "0.00";
        //string FormaPago = FormaPagoNode != null ? FormaPagoNode.Value : "99";



        //                // Validar si ya esta la Factura
        //                bool facturaCorrecta = ValidarFactura(uuid);


        //                if (!facturaCorrecta)
        //                {

        //                    // Validar y agregar cliente si no existe
        //                    if (!string.IsNullOrEmpty(rfcCliente) && !string.IsNullOrEmpty(nomCliente))
        //                    {
        //                        bool clienteExiste = ValidarClienteEnBD(rfcCliente);

        //                        if (!clienteExiste)
        //                        {
        //                            AgregarCliente(rfcCliente, nomCliente, RCliente, Cp);
        //                        }
        //                    }

        //                    // Validar o insertar cliente y obtener el Id
        //                    int idCliente = ObtenerIdCliente(rfcCliente);

        //                    string ObtenerProducto = ObtenerIdProducto(transaccion).ToString();

        //                    decimal iepsVenta = ObtenerIEPS(ObtenerProducto);


        //                    if (uuidNode != null && cantidadNode != null && importeNode != null && valorUnitarioNode != null)
        //                    {


        //                        // Actualizar la base de datos
        //                        ActualizarDespacho(transaccion, uuid, cantidad, TotalFact, valorUnitario, serieFact, folioFact, iepsVenta);
        //                        AgregarFactura(idCliente, rfcCliente, serieFact, uuid, folioFact, nomCliente, Cp, TotalFact, Subtotal, MetodoPago, fechaFact, Impuestos, FormaPago, iepsVenta, cantidad);
        //                        AgregarDetalleFactura(ObtenerProducto, transaccion, valorUnitario, serieFact, uuid, folioFact, TotalFact, Subtotal, fechaFact, Impuestos, iepsVenta, cantidad, calvesat, descripcion);

        //                        // Mostrar SweetAlert desde el servidor
        //                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                                "Swal.fire({title: 'Éxito', text: 'El XML se procesó correctamente.', icon: 'success', confirmButtonText: 'Aceptar'});", true);
        //                        // Recarga los datos del GridView para mostrar la vista sin edición
        //                        btnBuscar_Click(sender, EventArgs.Empty); // Asegúrate de que este método carga los datos en el GridView

        //                    }
        //                    else
        //                    {
        //                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta", "Swal.fire('Error', 'No se encontraron datos válidos en el XML.', 'error');", true);
        //                    }
        //                }
        //            }
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta", "Swal.fire('Error', 'Ya existe este UUID Relacionado.', 'error');", true);
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    else
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //        "Swal.fire({title: 'Aviso', text: 'Esta factura ya está relacionada en el sistema.', icon: 'warning', confirmButtonText: 'Aceptar'});", true);
        //    }
        //}

        //protected void btnSubirXML_Click(object sender, EventArgs e)
        //{
        //    if (fileUploadXML.HasFile)
        //    {
        //        if (!int.TryParse(hfTransaccion.Value, out int transaccion))
        //        {
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                "Swal.fire('Error', 'No se ha seleccionado una transacción válida.', 'error');", true);
        //            return;
        //        }

        //        try
        //        {
        //            using (Stream stream = fileUploadXML.PostedFile.InputStream)
        //            {
        //                XmlDocument xmlDoc = new XmlDocument();
        //                xmlDoc.Load(stream); // Cargar XML desde el flujo

        //                // Convertir XML a cadena
        //                string xmlString = xmlDoc.OuterXml;

        //                //// Eliminar la declaración de codificación si existe
        //                //if (xmlString.Contains("<?xml"))
        //                //{
        //                //    int start = xmlString.IndexOf("<?xml");
        //                //    int end = xmlString.IndexOf("?>", start);

        //                //    if (end > start)
        //                //    {
        //                //        xmlString = xmlString.Substring(end + 2).Trim();
        //                //    }
        //                //}

        //                // Guardar XML en la base de datos
        //                GuardarXMLenBD(transaccion, xmlString);



        //                XDocument xmlProcesado = XDocument.Parse(ObtenerXMLDesdeBD(transaccion));


        //                // 📌 Validar si el XML está vacío o nulo
        //                if (xmlProcesado.Root == null)
        //                {
        //                    ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                        "Swal.fire('Error', 'No se encontró XML para esta transacción.', 'error');", true);
        //                    return;
        //                }

        //                // 📌 Convertir XML a string con formato bonito
        //                // string xmlStringLeido = xmlProcesado.ToString();
        //                // 🚨 No uses HttpUtility.JavaScriptStringEncode() aquí, ya que distorsiona el XML.
        //                string xmlStringLeido = xmlProcesado.ToString();

        //                // Solo usar JavaScriptStringEncode al pasarlo a JavaScript
        //                string xmlParaJavaScript = HttpUtility.JavaScriptStringEncode(xmlStringLeido);


        //                // 📌 Guardar en sesión para usar en el evento del botón "Continuar"
        //                Session["xmlStringProcesado"] = xmlStringLeido;
        //                Session["transaccionProcesada"] = transaccion;



        //                btnContinuar_Click(null, EventArgs.Empty);

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                "Swal.fire('Error', 'Ocurrió un error al procesar el XML.', 'error');", true);
        //        }
        //    }
        //    else
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //            "Swal.fire({title: 'Aviso', text: 'Debes seleccionar un archivo XML.', icon: 'warning', confirmButtonText: 'Aceptar'});", true);
        //    }
        //}


        //protected void btnSubirXML_Click(object sender, EventArgs e)
        //{
        //    if (fileUploadXML.HasFile)
        //    {
        //        if (!int.TryParse(hfTransaccion.Value, out int transaccion))
        //        {
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                "Swal.fire('Error', 'No se ha seleccionado una transacción válida.', 'error');", true);
        //            return;
        //        }

        //        try
        //        {
        //            // Leer XML desde el archivo
        //            string xmlString;
        //            using (StreamReader reader = new StreamReader(fileUploadXML.PostedFile.InputStream, Encoding.UTF8))
        //            {
        //                xmlString = reader.ReadToEnd();
        //            }

        //            // ✅ Asegurar que el XML no tenga caracteres inesperados
        //            xmlString = xmlString.Replace("\0", "").Trim();

        //            // ✅ Eliminar declaración de XML si existe
        //            if (xmlString.StartsWith("<?xml"))
        //            {
        //                int start = xmlString.IndexOf("<?xml");
        //                int end = xmlString.IndexOf("?>", start);

        //                if (end > start)
        //                {
        //                    xmlString = xmlString.Substring(end + 2).Trim();
        //                }
        //            }

        //            // ✅ Guardar XML en la base de datos
        //            GuardarXMLenBD(transaccion, xmlString);

        //            // ✅ Obtener XML guardado
        //            XDocument xmlProcesado = XDocument.Parse(ObtenerXMLDesdeBD(transaccion));

        //            // 📌 Verificar que el XML no esté vacío
        //            if (xmlProcesado.Root == null)
        //            {
        //                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                    "Swal.fire('Error', 'No se encontró XML para esta transacción.', 'error');", true);
        //                return;
        //            }

        //            // ✅ Convertir XML a string para enviarlo a JavaScript
        //            string xmlStringLeido = xmlProcesado.ToString();
        //            //string xmlParaJavaScript = HttpUtility.JavaScriptStringEncode(xmlStringLeido);

        //            // 📌 Guardar en sesión
        //            // Convertir a UTF-8 antes de almacenar en sesión
        //            byte[] utf8Bytes = Encoding.UTF8.GetBytes(xmlStringLeido);
        //            Session["xmlStringProcesado"] = utf8Bytes;

        //            Session["transaccionProcesada"] = transaccion;

        //            // 📌 Continuar con el siguiente paso
        //            btnContinuar_Click(null, EventArgs.Empty);
        //        }
        //        catch (Exception ex)
        //        {
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                $"Swal.fire('Error', 'Ocurrió un error al procesar el XML: {ex.Message}', 'error');", true);
        //        }
        //    }
        //    else
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //            "Swal.fire({title: 'Aviso', text: 'Debes seleccionar un archivo XML.', icon: 'warning', confirmButtonText: 'Aceptar'});", true);
        //    }
        //}


        //protected void btnContinuar_Click(object sender, EventArgs e)
        //{
        //    if (Session["xmlStringProcesado"] == null || Session["transaccionProcesada"] == null)
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //            "Swal.fire('Error', 'No se encontró XML para procesar.', 'error');", true);
        //        return;
        //    }

        //    string xmlStringLeido = Session["xmlStringProcesado"].ToString();
        //    int transaccion = (int)Session["transaccionProcesada"];

        //    try
        //    {
        //        //// 🔍 Eliminar declaración de codificación si existe
        //        //if (xmlStringLeido.StartsWith("<?xml"))
        //        //{
        //        //    int start = xmlStringLeido.IndexOf("<?xml");
        //        //    int end = xmlStringLeido.IndexOf("?>", start);

        //        //    if (end > start)
        //        //    {
        //        //        xmlStringLeido = xmlStringLeido.Substring(end + 2).Trim();
        //        //    }
        //        //}

        //        //// 🔍 Validar que el XML sigue siendo correcto antes de procesarlo
        //        //if (string.IsNullOrWhiteSpace(xmlStringLeido) || !xmlStringLeido.TrimStart().StartsWith("<"))
        //        //{
        //        //    ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //        //        "Swal.fire('Error', 'El XML recuperado no tiene una estructura válida.', 'error');", true);
        //        //    return;
        //        //}

        //        byte[] utf8BytesRecuperados = (byte[])Session["xmlStringProcesado"];
        //        string xmlStringLeidoByte = Encoding.UTF8.GetString(utf8BytesRecuperados);

        //        XmlDocument xmlDocProcesado = new XmlDocument();
        //        xmlDocProcesado.LoadXml(xmlStringLeidoByte);





        //        XmlNamespaceManager ns = new XmlNamespaceManager(xmlDocProcesado.NameTable);
        //        ns.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
        //        ns.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

        //        // 📌 Extraer valores del XML
        //        string uuid = ObtenerValorXML(xmlDocProcesado, "//tfd:TimbreFiscalDigital/@UUID", ns);
        //        // 📌 Obtener la cantidad como string desde el XML
        //        string cantidadString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Concepto/@Cantidad", ns, "0");
        //        decimal cantidad = decimal.TryParse(cantidadString, out decimal cantidadDecimal) ? cantidadDecimal : 0;




        //        // 📌 Obtener el importe como string desde el XML
        //        string importeString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Concepto/@Importe", ns, "0.00");
        //        decimal importe = decimal.TryParse(importeString, out decimal importeDecimal) ? importeDecimal : 0.00m;



        //        string serieFact = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@Serie", ns, "");
        //        string folioFact = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@Folio", ns, "0");

        //        // 📌 Validar si ya existe la factura
        //        if (ValidarFactura(uuid))
        //        {
        //            ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //                "Swal.fire('Error', 'Ya existe este UUID Relacionado.', 'error');", true);
        //            return;
        //        }

        //        // 📌 Continuar con la validación e inserción de datos en la BD
        //        string rfcCliente = ObtenerValorXML(xmlDocProcesado, "//cfdi:Receptor/@Rfc", ns, "SIN RFC");
        //        string nomCliente = ObtenerValorXML(xmlDocProcesado, "//cfdi:Receptor/@Nombre", ns, "DESCONOCIDO");
        //        string RCliente = ObtenerValorXML(xmlDocProcesado, "//cfdi:Receptor/@RegimenFiscalReceptor", ns, "");
        //        string Cp = ObtenerValorXML(xmlDocProcesado, "//cfdi:Receptor/@DomicilioFiscalReceptor", ns, "");



        //        // 📌 Obtener el total como string desde el XML
        //        string totalFactString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@Total", ns, "0.00");

        //        // 📌 Intentar convertirlo a decimal, si falla, asignar 0.00 como valor por defecto
        //        decimal TotalFact = decimal.TryParse(totalFactString, out decimal totalFactDecimal) ? totalFactDecimal : 0.00m;


        //        // 📌 Obtener el subtotal como string desde el XML
        //        string subtotalString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@SubTotal", ns, "0.00");

        //        // 📌 Intentar convertirlo a decimal, si falla, asignar 0.00 como valor por defecto
        //        decimal Subtotal = decimal.TryParse(subtotalString, out decimal subtotalDecimal) ? subtotalDecimal : 0.00m;


        //        string MetodoPago = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@MetodoPago", ns, "PPD");

        //        // 📌 Obtener el valor del XML como string
        //        string fechaString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@Fecha", ns, "");

        //        // 📌 Intentar convertirlo a DateTime, si falla, usar DateTime.MinValue
        //        DateTime fechaFact = DateTime.TryParse(fechaString, out DateTime fechaTemp) ? fechaTemp : DateTime.MinValue;

        //        // 📌 Obtener el valor de impuestos como string desde el XML
        //        string impuestosString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Impuestos/@TotalImpuestosTrasladados", ns, "0.00");

        //        // 📌 Intentar convertirlo a decimal, si falla, asignar 0.00 como valor por defecto
        //        decimal Impuestos = decimal.TryParse(impuestosString, out decimal impuestosDecimal) ? impuestosDecimal : 0.00m;

        //        string FormaPago = ObtenerValorXML(xmlDocProcesado, "//cfdi:Comprobante/@FormaPago", ns, "99");

        //        // 📌 Obtener el valor unitario como string desde el XML
        //        string valorUnitarioString = ObtenerValorXML(xmlDocProcesado, "//cfdi:Concepto/@ValorUnitario", ns, "0.0");

        //        // 📌 Intentar convertirlo a decimal, si falla, asignar 0.0 como valor por defecto
        //        decimal valorUnitario = decimal.TryParse(valorUnitarioString, out decimal valorUnitarioDecimal) ? valorUnitarioDecimal : 0.0m;




        //        string calvesat = ObtenerValorXML(xmlDocProcesado, "//cfdi:Concepto/@ClaveProdServ", ns, "");
        //        string descripcion = ObtenerValorXML(xmlDocProcesado, "//cfdi:Concepto/@Descripcion", ns, "SIN DESCRIPCIÓN");


        //        // 📌 Validar y agregar cliente si no existe
        //        if (!string.IsNullOrEmpty(rfcCliente) && !string.IsNullOrEmpty(nomCliente))
        //        {
        //            if (!ValidarClienteEnBD(rfcCliente))
        //            {
        //                AgregarCliente(rfcCliente, nomCliente, RCliente, Cp);
        //            }
        //        }

        //        // 📌 Obtener ID del cliente
        //        int idCliente = ObtenerIdCliente(rfcCliente);

        //        string ObtenerProducto = ObtenerIdProducto(transaccion).ToString() ?? "0";
        //        decimal iepsVenta = ObtenerIEPS(ObtenerProducto);


        //        ActualizarDespacho(transaccion, uuid, cantidad, TotalFact, valorUnitario, serieFact, folioFact, iepsVenta);
        //        AgregarFactura(idCliente, rfcCliente, serieFact, uuid, folioFact, nomCliente, Cp, TotalFact, Subtotal, MetodoPago, fechaFact, Impuestos, FormaPago, iepsVenta, cantidad);
        //        AgregarDetalleFactura(ObtenerProducto, transaccion, valorUnitario, serieFact, uuid, folioFact, TotalFact, Subtotal,fechaFact, Impuestos, iepsVenta, cantidad, calvesat, descripcion);

        //        //// 📌 Mensaje de éxito
        //        //ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //        //    "Swal.fire({title: 'Éxito', text: 'El XML se procesó correctamente.', icon: 'success', confirmButtonText: 'Aceptar'});", true);

        //        // 📌 Limpiar la sesión
        //        Session.Remove("xmlStringProcesado");
        //        Session.Remove("transaccionProcesada");

        //        // 📌 Recargar los datos
        //        btnBuscar_Click(sender, EventArgs.Empty);
        //    }
        //    catch (XmlException xmlEx)
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //            $"Swal.fire('Error', 'Error de XML: {xmlEx.Message}', 'error');", true);
        //    }
        //    catch (Exception ex)
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //            $"Swal.fire('Error', 'Ocurrió un error inesperado: {ex.Message}', 'error');", true);
        //    }
        //}

        protected void btnSubirXML_Click(object sender, EventArgs e)
        {
            if (fileUploadXML.HasFile)
            {
                if (!int.TryParse(hfTransaccion.Value, out int transaccion))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                        "Swal.fire('Error', 'No se ha seleccionado una transacción válida.', 'error');", true);
                    return;
                }

                try
                {
                    // 📌 Leer el archivo XML desde el FileUpload con codificación UTF-8
                    string xmlString;
                    using (StreamReader reader = new StreamReader(fileUploadXML.PostedFile.InputStream, Encoding.UTF8))
                    {
                        xmlString = reader.ReadToEnd();
                    }

                    // 📌 Validar si el XML es correcto antes de guardarlo
                    try
                    {
                        XDocument.Parse(xmlString); // Si hay un error en el XML, lanzará una excepción
                    }
                    catch (XmlException ex)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            $"Swal.fire('Error', 'El XML no es válido: {ex.Message}', 'error');", true);
                        return;
                    }

                    // ✅ El XML está bien formado y listo para guardarse
                    GuardarXMLenBD(transaccion, xmlString);


                    // ✅ Obtener XML guardado
                    XDocument xmlProcesado = XDocument.Parse(xmlString);

                    // 📌 Verificar que el XML no esté vacío
                    if (xmlProcesado.Root == null)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Error', 'No se encontró XML para esta transacción.', 'error');", true);
                        return;
                    }

                    // ✅ Convertir XML a string para sesión
                    string xmlStringLeido = xmlProcesado.ToString();

                    // 📌 Guardar en sesión (UTF-8 seguro)
                    byte[] utf8Bytes = Encoding.UTF8.GetBytes(xmlStringLeido);
                    Session["xmlStringProcesado"] = utf8Bytes;
                    Session["transaccionProcesada"] = transaccion;

                    // 📌 Procesar XML y extraer valores
                    XmlNamespaceManager ns = new XmlNamespaceManager(new NameTable());
                    ns.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");
                    ns.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

                    string uuid = ObtenerValorXML(xmlProcesado, "//tfd:TimbreFiscalDigital/@UUID", ns);
                    string cantidadString = ObtenerValorXML(xmlProcesado, "//cfdi:Concepto/@Cantidad", ns, "0");
                    decimal cantidad = decimal.TryParse(cantidadString, out decimal cantidadDecimal) ? cantidadDecimal : 0;

                    string importeString = ObtenerValorXML(xmlProcesado, "//cfdi:Concepto/@Importe", ns, "0.00");
                    decimal importe = decimal.TryParse(importeString, out decimal importeDecimal) ? importeDecimal : 0.00m;

                    string serieFact = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@Serie", ns, "");
                    string folioFact = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@Folio", ns, "0");

                    if (ValidarFactura(uuid))
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Error', 'Ya existe este UUID Relacionado.', 'error');", true);
                        return;
                    }

                    string rfcCliente = ObtenerValorXML(xmlProcesado, "//cfdi:Receptor/@Rfc", ns, "SIN RFC");
                    string nomCliente = ObtenerValorXML(xmlProcesado, "//cfdi:Receptor/@Nombre", ns, "DESCONOCIDO");
                    string regimenCliente = ObtenerValorXML(xmlProcesado, "//cfdi:Receptor/@RegimenFiscalReceptor", ns, "");
                    string cpCliente = ObtenerValorXML(xmlProcesado, "//cfdi:Receptor/@DomicilioFiscalReceptor", ns, "");

                    string totalFactString = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@Total", ns, "0.00");
                    decimal totalFact = decimal.TryParse(totalFactString, out decimal totalFactDecimal) ? totalFactDecimal : 0.00m;

                    string subtotalString = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@SubTotal", ns, "0.00");
                    decimal subtotal = decimal.TryParse(subtotalString, out decimal subtotalDecimal) ? subtotalDecimal : 0.00m;

                    string metodoPago = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@MetodoPago", ns, "PPD");

                    string fechaString = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@Fecha", ns, "");
                    DateTime fechaFact = DateTime.TryParse(fechaString, out DateTime fechaTemp) ? fechaTemp : DateTime.MinValue;

                    string impuestosString = ObtenerValorXML(xmlProcesado, "//cfdi:Impuestos/@TotalImpuestosTrasladados", ns, "0.00");
                    decimal impuestos = decimal.TryParse(impuestosString, out decimal impuestosDecimal) ? impuestosDecimal : 0.00m;

                    string formaPago = ObtenerValorXML(xmlProcesado, "//cfdi:Comprobante/@FormaPago", ns, "99");

                    string valorUnitarioString = ObtenerValorXML(xmlProcesado, "//cfdi:Concepto/@ValorUnitario", ns, "0.0");
                    decimal valorUnitario = decimal.TryParse(valorUnitarioString, out decimal valorUnitarioDecimal) ? valorUnitarioDecimal : 0.0m;

                    string claveSAT = ObtenerValorXML(xmlProcesado, "//cfdi:Concepto/@ClaveProdServ", ns, "");
                    string descripcion = ObtenerValorXML(xmlProcesado, "//cfdi:Concepto/@Descripcion", ns, "SIN DESCRIPCIÓN");

                    // 📌 Validar y agregar cliente si no existe
                    if (!string.IsNullOrEmpty(rfcCliente) && !string.IsNullOrEmpty(nomCliente) && !ValidarClienteEnBD(rfcCliente))
                    {
                        AgregarCliente(rfcCliente, nomCliente, regimenCliente, cpCliente);
                    }

                    int idCliente = ObtenerIdCliente(rfcCliente);
                    string idProducto = ObtenerIdProducto(transaccion).ToString() ?? "0";
                    decimal iepsVenta = ObtenerIEPS(idProducto);





                    //ActualizarUUID(transaccion, uuid);

                    // 📌 Insertar datos en la base de datos
                    ActualizarDespacho(transaccion, uuid, cantidad, totalFact, valorUnitario, serieFact, folioFact, iepsVenta);
                    AgregarFactura(idCliente, rfcCliente, serieFact, uuid, folioFact, nomCliente, cpCliente, totalFact, subtotal, metodoPago, fechaFact, impuestos, formaPago, iepsVenta, cantidad);
                    AgregarDetalleFactura(idProducto, transaccion, valorUnitario, serieFact, uuid, folioFact, totalFact, subtotal, fechaFact, impuestos, iepsVenta, cantidad, claveSAT, descripcion);

                    // 📌 Mensaje de éxito
                    ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                        "Swal.fire({title: 'Exito', text: 'El XML se proceso correctamente.', icon: 'success', confirmButtonText: 'Aceptar'});", true);

                    // 📌 Limpiar la sesión
                    Session.Remove("xmlStringProcesado");
                    Session.Remove("transaccionProcesada");

                    // 📌 Recargar los datos
                    btnBuscar_Click(sender, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                        $"Swal.fire('Error', 'Ocurrio un error al procesar el XML: {ex.Message}', 'error');", true);
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    "Swal.fire({title: 'Aviso', text: 'Debes seleccionar un archivo XML.', icon: 'warning', confirmButtonText: 'Aceptar'});", true);
            }
        }


        //private string ObtenerXMLDesdeBD(int transaccion)
        //{
        //    string xmlContent = "";

        //    try
        //    {
        //        string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

        //        using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
        //        {
        //            string query = "SELECT XMLContent FROM XMLFacturas WHERE Transaccion = @Transaccion";

        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@Transaccion", transaccion);

        //                conn.Open();
        //                object result = cmd.ExecuteScalar();

        //                if (result != null)
        //                {
        //                    xmlContent = result.ToString(); // Se recupera el XML directamente
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
        //            $"Swal.fire('Error', 'Error al recuperar el XML de la BD: {ex.Message}', 'error');", true);
        //    }

        //    return xmlContent;
        //}


        //private string ObtenerXMLDesdeBD(int transaccion)
        //            {
        //                string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
        //                string xmlString = "";

        //                using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
        //                {
        //                    string query = "SELECT XMLContent FROM XMLFacturas WHERE Transaccion = @Transaccion";

        //                    using (SqlCommand cmd = new SqlCommand(query, conn))
        //                    {
        //                        cmd.Parameters.AddWithValue("@Transaccion", transaccion);

        //                        conn.Open();
        //                        object result = cmd.ExecuteScalar();
        //                        if (result != null)
        //                        {
        //                            xmlString = result.ToString();
        //                        }
        //                    }
        //                }

        //                // 🔍 Verificar si el XML contiene caracteres escapados
        //                if (xmlString.Contains("\\u003c") || xmlString.Contains("\\u003e"))
        //                {
        //                    xmlString = HttpUtility.HtmlDecode(xmlString);
        //                }

        //                // 🔍 Validar si el XML recuperado es válido antes de retornarlo
        //                if (string.IsNullOrWhiteSpace(xmlString) || !xmlString.TrimStart().StartsWith("<"))
        //                {
        //                    throw new Exception("El XML recuperado no es válido.");
        //                }

        //                return xmlString;
        //            }







        //private void GuardarXMLenBD(int transaccion, string xmlContent)
        //{
        //    // 🔍 Si el XML contiene la declaración de codificación, la eliminamos
        //    if (xmlContent.StartsWith("<?xml"))
        //    {
        //        int start = xmlContent.IndexOf("<?xml");
        //        int end = xmlContent.IndexOf("?>", start);

        //        if (end > start)
        //        {
        //            xmlContent = xmlContent.Substring(end + 2).Trim();
        //        }
        //    }

        //    // 🔍 Decodificar el XML antes de guardarlo (por si viene con encoding raro)
        //    xmlContent = HttpUtility.HtmlDecode(xmlContent);

        //    string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

        //    using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
        //    {
        //        string query = "INSERT INTO XMLFacturas (Transaccion, XMLContent) VALUES (@Transaccion, @XMLContent)";

        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@Transaccion", transaccion);
        //            // ⚠️ Importante: Definir el tipo de dato como NVARCHAR(MAX)
        //            cmd.Parameters.Add("@XMLContent", SqlDbType.NVarChar, -1).Value = xmlContent;


        //            conn.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        private void GuardarXMLenBD(int transaccion, string xmlContent)
        {
            try
            {
                // 🔍 Validar que el XML es correcto antes de guardarlo
                XDocument.Parse(xmlContent); // Si el XML no es válido, lanzará una excepción

                string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

                using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
                {
                    string query = "INSERT INTO XMLFacturas (Transaccion, XMLContent) VALUES (@Transaccion, @XMLContent)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Transaccion", transaccion);

                        // ⚠️ Guardar el XML en un campo de tipo XML en SQL Server
                        cmd.Parameters.Add("@XMLContent", SqlDbType.NVarChar, -1).Value = xmlContent;


                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (XmlException ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    $"Swal.fire('Error', 'El XML no es valido: {ex.Message}', 'error');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    $"Swal.fire('Error', 'Error al guardar el XML en la BD: {ex.Message}', 'error');", true);
            }
        }




        private decimal ObtenerIEPS(string claveProd)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();

                // Consulta para obtener el IEPS del producto
                string query = "SELECT NProdIEPS FROM Productos WHERE NProdClave = @ClaveProd";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClaveProd", claveProd);

                    object resultado = cmd.ExecuteScalar();

                    if (resultado != null && decimal.TryParse(resultado.ToString(), out decimal ieps))
                    {
                        return ieps;
                    }
                }
            }

            return 0m; // Retorna 0 si no se encontró el producto o el IEPS
        }


        private void ActualizarDespacho(int transaccion, string uuid, decimal cantidad, decimal TotalFact, decimal valorUnitario,string serieFact, string foliofact,decimal iepsVenta)
        {
            // Obtiene la cadena de conexión de la sesión
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            decimal PrecioDecimal = valorUnitario;
            decimal PrecioNeto = (valorUnitario + iepsVenta) * 1.16m;
            bool originalValue = false; // Variable para almacenar el valor de "Original"

            string query="";
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {

                conn.Open();
                // Primero, verificar si el campo "Original" es 0 antes de actualizar
                string checkQuery = "SELECT Original FROM Despachos WHERE Transaccion = @Transaccion";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Transaccion", transaccion);
                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        originalValue = Convert.ToBoolean(result);
                    }
                }
                if (originalValue) // Si "Original" es 1 (true), mostrar advertencia y salir
                {
                    query = @"UPDATE Despachos 
                         SET  
                        UUID = @UUID, Volumen = @Cantidad, ImporteVenta = @Importe, Precio = @ValorUnitario, 
                                EstadoFact=3, SerieFact=@serieFact,Factura=@foliofact
                         WHERE Transaccion = @Transaccion";
                }

                else
                {
                    query = @"UPDATE Despachos 
                         SET  VolumenAntes = Volumen, 
                              ImporteAntes = ImporteVenta, 
                              PrecioAntes = Precio, 
                              Original = 1,
                        UUID = @UUID, Volumen = @Cantidad, ImporteVenta = @Importe, Precio = @ValorUnitario, 
                                EstadoFact=3, SerieFact=@serieFact,Factura=@foliofact
                         WHERE Transaccion = @Transaccion";

                }
   

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UUID", uuid);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@Importe", TotalFact);
                    cmd.Parameters.AddWithValue("@ValorUnitario", PrecioNeto);
                    cmd.Parameters.AddWithValue("@Transaccion", transaccion);
                    cmd.Parameters.AddWithValue("@serieFact", serieFact);
                    cmd.Parameters.AddWithValue("@foliofact", foliofact);
                    //conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ActualizarUUID(int transaccion, string uuid)
        {
            // Obtiene la cadena de conexión de la sesión
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
           
            string query = "";
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {

                conn.Open();
            
                    query = @"UPDATE XMLFacturas 
                         SET  UUID = @UUID
                         WHERE Transaccion = @Transaccion";

             


                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UUID", uuid);
                     cmd.Parameters.AddWithValue("@Transaccion", transaccion);
                  
                    //conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Método para validar si el cliente ya existe en la base de datos
        private bool ValidarClienteEnBD(string rfcCliente)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = "SELECT COUNT(*) FROM Clientes WHERE RFC = @RFC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RFC", rfcCliente);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();

                    return count > 0; // Retorna true si el cliente ya existe
                }
            }
        }



        // Método para validar si el cliente ya existe en la base de datos
        private bool ValidarFactura(string uuid)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = "SELECT COUNT(*) FROM Documentos WHERE UUID = @uuid";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uuid", uuid);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();

                    return count > 0; // Retorna true si el cliente ya existe
                }
            }
        }

        // Método para agregar un nuevo cliente a la base de datos
        private void AgregarCliente(string rfcCliente, string nomCliente,string RCliente,string Cp)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = "INSERT INTO Clientes ( RFC, RazonSocial, RegimenFiscal, CP, Email, Domicilio, Estacion, Telefonos, CuentaContable, Activo, Ilimitado, LimiteCred, SaldoInicial, Cargos, Abonos, Saldo, Banco, Cuenta, RfcCtaOrd, idGrupo) VALUES (@RFC, @Nombre,@regimen,@CP,'','',0,'','',1,0,0,0,0,0,0,'','','',0)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RFC", rfcCliente);
                    cmd.Parameters.AddWithValue("@Nombre", nomCliente);
                    cmd.Parameters.AddWithValue("@regimen", RCliente);
                    cmd.Parameters.AddWithValue("@CP", Cp);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Método para agregar Documento

        private void AgregarFactura(int idCliente, string rfcCliente, string serieFact, string uuid,string folioFact, string nomCliente, string Cp, decimal TotalFact, decimal Subtotal, string MetodoPago, DateTime fechaFact, decimal Impuestos, string FormaPago,decimal iepsVenta, decimal cantidad)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            decimal cantidadDecimal = cantidad;
            decimal iepsFactura = cantidadDecimal * iepsVenta;
            decimal impuestosDecimal = Impuestos; 


            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = @"INSERT INTO Documentos 
                        (IdCliente, Serie, FolioInt, Fecha, RFC, RazonSocial, CP, Domicilio, Email, Estatus, FechaCancel, UsuarioCancela, 
                        Descuento, MotivoDescuento, SubTotal, IVA, IEPS, Total, Impuestos, Observaciones, MetodoPago, ClaveFormaPago, 
                        ArchivoPDF, UUID, Credito, Pagado, NotaCred, PParcial, Usuario, Anticipo, IdFlotilla, LoteVales) 
                        VALUES 
                        (@IdCliente, @Serie, @FolioInt, @FechaFactura, @RFC, @RazonSocial, @CP, @Domicilio, @Email, @Estatus, @FechaCancel, 
                        @UsuarioCancela, @Descuento, @MotivoDescuento, @Subtotal, @IVA, @IEPS, @Total, @Impuestos, @Observaciones, 
                        @MetPago, @FormaPago, @ArchivoPDF, @UUID, @Credito, @Pagado, @NotaCred, @PParcial, @Usuario, @Anticipo, 
                        @IdFlotilla, @LoteVales)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    cmd.Parameters.AddWithValue("@Serie", serieFact);
                    cmd.Parameters.AddWithValue("@FolioInt", folioFact);
                    cmd.Parameters.AddWithValue("@FechaFactura", fechaFact != DateTime.MinValue ? fechaFact : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RFC", rfcCliente);
                    cmd.Parameters.AddWithValue("@RazonSocial", nomCliente);
                    cmd.Parameters.AddWithValue("@CP", Cp);
                    cmd.Parameters.AddWithValue("@Domicilio", "-");
                    cmd.Parameters.AddWithValue("@Email", "-");
                    cmd.Parameters.AddWithValue("@Estatus", 1);
                    cmd.Parameters.AddWithValue("@FechaCancel", DBNull.Value);
                    cmd.Parameters.AddWithValue("@UsuarioCancela", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descuento", 0);
                    cmd.Parameters.AddWithValue("@MotivoDescuento", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subtotal", Subtotal);
                    cmd.Parameters.AddWithValue("@IVA", impuestosDecimal - iepsFactura);
                    cmd.Parameters.AddWithValue("@IEPS", iepsFactura);
                    cmd.Parameters.AddWithValue("@Total", TotalFact);
                    cmd.Parameters.AddWithValue("@Impuestos", Impuestos);
                    cmd.Parameters.AddWithValue("@Observaciones", "-");
                    cmd.Parameters.AddWithValue("@MetPago", MetodoPago);
                    cmd.Parameters.AddWithValue("@FormaPago", FormaPago);
                    cmd.Parameters.AddWithValue("@ArchivoPDF", "-");
                    cmd.Parameters.AddWithValue("@UUID", uuid);
                    cmd.Parameters.AddWithValue("@Credito", 1);
                    cmd.Parameters.AddWithValue("@Pagado", 0);
                    cmd.Parameters.AddWithValue("@NotaCred", 0);
                    cmd.Parameters.AddWithValue("@PParcial", 0);
                    cmd.Parameters.AddWithValue("@Usuario", "SYSTEM");
                    cmd.Parameters.AddWithValue("@Anticipo", 0);
                    cmd.Parameters.AddWithValue("@IdFlotilla", 0);
                    cmd.Parameters.AddWithValue("@LoteVales", 0);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

            }
        }
        private void AgregarDetalleFactura(string ObtenerProducto,int transaccion, decimal valorUnitario, string serieFact, string uuid, string folioFact, decimal TotalFact, decimal Subtotal, DateTime fechaFact, decimal Impuestos, decimal iepsVenta, decimal cantidad, string clavesat, string descripcion)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            decimal cantidadDecimal = cantidad;
            decimal iepsFactura = cantidadDecimal * iepsVenta;
            decimal impuestosDecimal = Impuestos;
            decimal PrecioDecimal = valorUnitario;
            decimal PrecioNeto = (valorUnitario + iepsVenta) * 1.16m;


            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = @"INSERT INTO DocumentosDetalle 
                        (Serie, FolioInt, Codigo, Cantidad, Descripcion, UMedida, Precio, PrecioB, 
                        SubTotal, IVA, IEPS, Total, Estatus, Fecha, Transaccion, NumTicket, ClaveSAT, 
                        CveUnidadSAT, PrecioBaseI, Combustible, UUID) 
                        VALUES 
                        (@Serie, @FolioInt, @Codigo, @Cantidad, @Descripcion, @UMedida, @Precio, @PrecioB, 
                        @SubTotal, @IVA, @IEPS, @Total, @Estatus, @Fecha, @Transaccion, @NumTicket, @ClaveSAT, 
                        @CveUnidadSAT, @PrecioBaseI, @Combustible, @UUID)";


                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                   
                    cmd.Parameters.AddWithValue("@Serie", serieFact);
                    cmd.Parameters.AddWithValue("@FolioInt", folioFact);
                    cmd.Parameters.AddWithValue("@Codigo", ObtenerProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@UMedida", "LTR");
                    cmd.Parameters.AddWithValue("@Precio", PrecioNeto);
                    cmd.Parameters.AddWithValue("@PrecioB", valorUnitario);
                    cmd.Parameters.AddWithValue("@Subtotal", Subtotal);
                    cmd.Parameters.AddWithValue("@IVA", impuestosDecimal - iepsFactura);
                    cmd.Parameters.AddWithValue("@IEPS", iepsFactura);
                    cmd.Parameters.AddWithValue("@Total", TotalFact);
                    cmd.Parameters.AddWithValue("@Estatus", 1);
                    cmd.Parameters.AddWithValue("@Fecha", fechaFact != DateTime.MinValue ? fechaFact : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Transaccion", transaccion);
                    cmd.Parameters.AddWithValue("@NumTicket", 0);
                    cmd.Parameters.AddWithValue("@ClaveSAT", clavesat);
                    cmd.Parameters.AddWithValue("@CveUnidadSAT", "LTR");
                    cmd.Parameters.AddWithValue("@PrecioBaseI", 0);
                    cmd.Parameters.AddWithValue("@Combustible", 1);
                    cmd.Parameters.AddWithValue("@UUID", uuid);
               

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

            }
        }

        private int ObtenerIdCliente(string rfcCliente)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();

                // Verificar si el cliente ya existe
                string queryExiste = "SELECT IdCliente FROM Clientes WHERE RFC = @RFC";
                using (SqlCommand cmdExiste = new SqlCommand(queryExiste, conn))
                {
                    cmdExiste.Parameters.AddWithValue("@RFC", rfcCliente);

                    object resultado = cmdExiste.ExecuteScalar();

                    if (resultado != null)
                    {
                        // Si el cliente ya existe, devolver su Id
                        return Convert.ToInt32(resultado);
                    }

                    // Agregar un valor de retorno en caso de error
                    return -1; // Puede ser -1 o lanzar una excepción si prefieres
                }

            
            }
        }
        private int ObtenerIdProducto(int transaccion)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();

                // Consulta para obtener ClaveProd de la transacción
                string queryExiste = "SELECT ClaveProd FROM Despachos WHERE transaccion = @transaccion";
                using (SqlCommand cmdExiste = new SqlCommand(queryExiste, conn))
                {
                    cmdExiste.Parameters.AddWithValue("@transaccion", transaccion);

                    object resultado = cmdExiste.ExecuteScalar();

                    if (resultado != null && int.TryParse(resultado.ToString(), out int claveProd))
                    {
                        // Retornar la clave del producto si es un número válido
                        return claveProd;
                    }
                }
            }

            // Retornar -1 si no se encontró el producto o hubo un error
            return -1;
        }

        private void EliminarVenta(string transaccion)
        {
            // Validar que la transacción no sea nula o vacía
            if (string.IsNullOrEmpty(transaccion))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    "Swal.fire('Error', 'La transacción no es válida.', 'error');", true);
                return;
            }

            // Obtener la cadena de conexión de la sesión
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            // Validar que la cadena de conexión no sea nula
            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    "Swal.fire('Error', 'No se ha seleccionado una instancia de base de datos.', 'error');", true);
                return;
            }

            // Conexión a la base de datos
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();

                // Verificar si la transacción tiene UUID asignado
                string queryVerificar = "SELECT COUNT(*) FROM Despachos WHERE Transaccion = @Transaccion AND (UUID IS NULL OR UUID = '')";
                using (SqlCommand cmdVerificar = new SqlCommand(queryVerificar, conn))
                {
                    cmdVerificar.Parameters.AddWithValue("@Transaccion", transaccion);
                    int count = (int)cmdVerificar.ExecuteScalar();

                    if (count == 0)
                    {
                        // Si la transacción tiene UUID, mostrar mensaje y no permitir la eliminación
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Error', 'No se puede eliminar esta venta porque ya esta facturada.', 'error');", true);
                        return;
                    }
                }

                // Si no tiene UUID, proceder con la actualización
                string query = "UPDATE Despachos SET Original=1,VentaEliminada=1,VolumenAntes=Volumen,ImporteAntes=ImporteVenta,PrecioAntes=Precio,Volumen=0, ImporteVenta=0, TipoVenta='F', Tpago='Jarreo' WHERE Transaccion = @Transaccion";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Transaccion", transaccion);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Mostrar mensaje de éxito
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Eliminado', 'La venta ha sido eliminada correctamente.', 'success');", true);
                    }
                    else
                    {
                        // Mostrar mensaje si la transacción no existe en la base de datos
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Error', 'No se encontro la transaccion en la base de datos.', 'error');", true);
                    }
                }
            }

            // Recargar los datos en el GridView
            btnBuscar_Click(null, EventArgs.Empty);
        }

        private void EliminarFactura(string transaccion)
        {
            // Validar que la transacción no sea nula o vacía
            if (string.IsNullOrEmpty(transaccion))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    "Swal.fire('Error', 'La transacción no es válida.', 'error');", true);
                return;
            }

            // Obtener la cadena de conexión de la sesión
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            // Validar que la cadena de conexión no sea nula
            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                    "Swal.fire('Error', 'No se ha seleccionado una instancia de base de datos.', 'error');", true);
                return;
            }

            // Conexión a la base de datos
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();

                // Verificar si la transacción tiene UUID asignado
                string queryVerificar = "SELECT COUNT(*) FROM Despachos WHERE Transaccion = @Transaccion AND (UUID!='')";
                using (SqlCommand cmdVerificar = new SqlCommand(queryVerificar, conn))
                {
                    cmdVerificar.Parameters.AddWithValue("@Transaccion", transaccion);
                    int count = (int)cmdVerificar.ExecuteScalar();

                    if (count == 0)
                    {
                        // Si la transacción tiene UUID, mostrar mensaje y no permitir la eliminación
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Error', 'No se puede realizar la operacion porque no tiene UUID.', 'error');", true);
                        return;
                    }
                }


                string uuidFactura = string.Empty; // Inicializar la variable para almacenar el UUID
                string queryUUID = "SELECT top(1) UUID FROM Despachos WHERE Transaccion = @Transaccion";

                using (SqlConnection conn2 = new SqlConnection(instanciaConnectionString)) // Asegúrate de usar tu cadena de conexión
                {
                  

                    using (SqlCommand cmd = new SqlCommand(queryUUID, conn))
                    {
                        cmd.Parameters.AddWithValue("@Transaccion", transaccion); // Parámetro para la consulta

                        object resultado = cmd.ExecuteScalar(); // Ejecutar la consulta y obtener el resultado

                        if (resultado != null && resultado != DBNull.Value)
                        {
                            uuidFactura = resultado.ToString(); // Convertir el resultado a string
                        }
                    }
                }

              

                // Proceder con la eliminación si el UUID existe
                string eliminarDocumento = "DELETE FROM Documentos WHERE UUID = @UUID";
                using (SqlCommand cmdEliminar = new SqlCommand(eliminarDocumento, conn))
                {
                    cmdEliminar.Parameters.AddWithValue("@UUID", uuidFactura);
                    cmdEliminar.ExecuteNonQuery();

                   
                }

                // Proceder con la eliminación si el UUID existe
                string eliminarDetelleDocumento = "DELETE FROM DocumentosDetalle WHERE UUID = @UUID";
                using (SqlCommand cmdEliminar = new SqlCommand(eliminarDetelleDocumento, conn))
                {
                    cmdEliminar.Parameters.AddWithValue("@UUID", uuidFactura);
                    cmdEliminar.ExecuteNonQuery();

                }



                // Proceder con la eliminación si el UUID existe
                string eliminarXML = "DELETE FROM XMLFacturas WHERE transaccion = @transaccion";
                using (SqlCommand cmdEliminarXML = new SqlCommand(eliminarXML, conn))
                {
                    cmdEliminarXML.Parameters.AddWithValue("@transaccion", transaccion);
                    cmdEliminarXML.ExecuteNonQuery();

                }


                string query = "UPDATE Despachos SET Volumen=VolumenAntes,ImporteVenta=ImporteAntes,Precio=PrecioAntes,UUID='', EstadoFact=0, SerieFact='', Factura=0 WHERE Transaccion = @Transaccion";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Transaccion", transaccion);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Mostrar mensaje de éxito
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Factura Eliminada', 'La Factura Eliminada correctamente.', 'success');", true);
                    }
                    else
                    {
                        // Mostrar mensaje si la transacción no existe en la base de datos
                        ScriptManager.RegisterStartupScript(this, GetType(), "alerta",
                            "Swal.fire('Error', 'No se encontró la transaccion en la base de datos.', 'error');", true);
                    }
                }
            }

            // Recargar los datos en el GridView
            btnBuscar_Click(null, EventArgs.Empty);
        }

          

    }
}