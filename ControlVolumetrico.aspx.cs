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
using Newtonsoft.Json;
using System.IO;
using System.Net.NetworkInformation;
using Microsoft.Ajax.Utilities;
using System.IO.Compression;
using System.Drawing.Printing;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SoftwarePlantas
{
    public partial class ControlVolumetrico : System.Web.UI.Page
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
                CargarAnios();

            }
           
        }

        private void CargarAnios()
        {
            int anioInicio = 2024;
            int anioActual = DateTime.Now.Year;

            ddlAnio.Items.Clear();

            for (int anio = anioInicio; anio <= anioActual; anio++)
            {
                ddlAnio.Items.Add(new System.Web.UI.WebControls.ListItem(anio.ToString(), anio.ToString()));
            }

            // Selecciona el año actual por defecto
            ddlAnio.SelectedValue = anioActual.ToString();
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

            ddlEstacion.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Selecciona una estación", ""));
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
                //// Limpiar el control donde se muestran los datos (por ejemplo, un GridView)
                //gvDespachos.DataSource = null;
                //gvDespachos.DataBind();
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
                    //btnBuscar_Click(null, EventArgs.Empty);
                }
                else
                {
                    // Si la conexión falla, muestra un mensaje de error y detiene la ejecución
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'No se pudo establecer conexión con la base de datos.', 'error');", true);
                }
            }

        }

    



        // 1. Clases para el JSON CMN-1876
        // -----------------------------

        public class CMNReporte
        {
            public string Version { get; set; }
            public string RfcContribuyente { get; set; }
            public string RfcRepresentanteLegal { get; set; }
            public string RfcProveedor { get; set; }
            public List<string> RfcProveedores { get; set; }
            public string Caracter { get; set; }
            public string ModalidadPermiso { get; set; }
            public string NumPermiso { get; set; }
            public string ClaveInstalacion { get; set; }
            public string DescripcionInstalacion { get; set; }
            public int NumeroPozos { get; set; }
            public int NumeroTanques { get; set; }
            public int NumeroDuctosEntradaSalida { get; set; }
            public int NumeroDuctosTransporteDistribucion { get; set; }
            public int NumeroDispensarios { get; set; }
            public DateTime FechaYHoraReporteMes { get; set; }
            public List<Geolocalizacion> Geolocalizacion { get; set; }
            public List<Producto> Producto { get; set; }
        }

        public class Geolocalizacion
        {
            public double GeolocalizacionLatitud { get; set; }
            public double GeolocalizacionLongitud { get; set; }
        }

        public class Producto
        {
            public string ClaveProducto { get; set; }
            public string ClaveSubProducto { get; set; }
            public double ComposOctanajeGasolina { get; set; }
            public string GasolinaConCombustibleNoFosil { get; set; }
            public ReporteDeVolumenMensual ReporteDeVolumenMensual { get; set; }
        }

        public class ReporteDeVolumenMensual
        {
            public ControlDeExistencias ControlDeExistencias { get; set; }
            public Recepciones Recepciones { get; set; }
            public Entregas Entregas { get; set; }
        }

        public class ControlDeExistencias
        {
            public double VolumenExistenciasMes { get; set; }
            public DateTime FechaYHoraEstaMedicionMes { get; set; }
        }

        public class Recepciones
        {
            public int TotalRecepcionesMes { get; set; }
            public SumaVolumen SumaVolumenRecepcionMes { get; set; }
            public int TotalDocumentosMes { get; set; }
            public double ImporteTotalRecepcionesMensual { get; set; }
            public List<Complemento> Complemento { get; set; }
        }

        public class Entregas
        {
            public int TotalEntregasMes { get; set; }
            public SumaVolumen SumaVolumenEntregadoMes { get; set; }
            public int TotalDocumentosMes { get; set; }
            public double ImporteTotalEntregasMes { get; set; }
            public List<Complemento> Complemento { get; set; }
        }

        public class SumaVolumen
        {
            public double ValorNumerico { get; set; }
            public string UnidadDeMedida { get; set; }
        }

        public class Complemento
        {
            public string TipoComplemento { get; set; } = "Comercializacion";
            public List<Nacional> Nacional { get; set; }
        }

        public class Nacional
        {
            public string RfcClienteOProveedor { get; set; }
            public string NombreClienteOProveedor { get; set; }
            public string PermisoClienteOProveedor { get; set; }
            public List<CfdiInfo> CFDIs { get; set; }
        }

        public class CfdiInfo
        {
            public string Cfdi { get; set; }
            public string TipoCfdi { get; set; } = "Ingreso";
            public double PrecioVentaOCompraOContrap { get; set; }
            public DateTime FechaYHoraTransaccion { get; set; }
            public VolumenDocumentado VolumenDocumentado { get; set; }
        }

        public class VolumenDocumentado
        {
            public double ValorNumerico { get; set; }
            public string UnidadDeMedida { get; set; }
        }


        // -----------------------------
        // 2. Código para generar JSON (usar en .aspx.cs)
        // -----------------------------

        protected void btnGenerarJson_Click(object sender, EventArgs e)
        {
            int mes = int.Parse(ddlMes.SelectedValue);
            int anio = int.Parse(ddlAnio.SelectedValue);
            DateTime fechaInicio = new DateTime(anio, mes, 1);
            DateTime fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            // Reemplaza con tu cadena de conexión real según instancia seleccionada
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            // 1. Obtener encabezado desde empresasf
            var reporte = ObtenerEncabezadoDesdeEmpresa(instanciaConnectionString, fechaFin);
            if (reporte == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No se encontró información en la tabla empresasf');", true);
                return;
            }

            // 2. Obtener productos y datos relacionados
            reporte.Producto = ObtenerProductosDesdeBD(fechaInicio, fechaFin, instanciaConnectionString);

            //// 3. Serializar el objeto a JSON
            //string json = JsonConvert.SerializeObject(reporte, Formatting.Indented);

            //// 4. Generar nombre de archivo tipo SAT
            //string uuid = Guid.NewGuid().ToString().ToUpper();
            //string rfcContribuyente = reporte.RfcContribuyente;
            //string rfcProveedor = reporte.RfcProveedor;
            //string fechaSAT = fechaFin.ToString("yyyy-MM-dd");
            //string ClaveInstalacion = reporte.ClaveInstalacion;
            //string fileName = $"M_{uuid}_{rfcContribuyente}_{rfcProveedor}_{fechaSAT}_{ClaveInstalacion}_CMN_JSON.json";

            //// 5. Forzar descarga del archivo JSON
            //Response.Clear();
            //Response.ContentType = "application/json";
            //Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
            //Response.Write(json);
            //Response.End();

            string json = JsonConvert.SerializeObject(reporte, Formatting.Indented);

            string uuid = Guid.NewGuid().ToString().ToUpper();
            string rfcContribuyente = reporte.RfcContribuyente;
            string rfcProveedor = reporte.RfcProveedor;
            string fechaSAT = fechaFin.ToString("yyyy-MM-dd");
            string ClaveInstalacion = reporte.ClaveInstalacion;
            string nombreBase = $"M_{uuid}_{rfcContribuyente}_{rfcProveedor}_{fechaSAT}_{ClaveInstalacion}_CMN_JSON";
            string nombreJson = nombreBase + ".json";
            string nombreZip = nombreBase + ".zip";
            string nombrePdf = nombreBase + ".pdf";
            string carpetaTemp = Server.MapPath("~/tempzip/");
            if (!Directory.Exists(carpetaTemp)) Directory.CreateDirectory(carpetaTemp);

            string rutaJson = Path.Combine(carpetaTemp, nombreJson);
            File.WriteAllText(rutaJson, json);

            //string rutaPdf = Path.Combine(carpetaTemp, nombrePdf);
            //GenerarPDFDesdeJson(json, rutaPdf);

            string rutaZip = Path.Combine(carpetaTemp, nombreZip);
            if (File.Exists(rutaZip)) File.Delete(rutaZip);
            using (var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(rutaJson, nombreJson);
            }


            Response.Clear();
            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", $"attachment; filename={nombreZip}");
            Response.TransmitFile(rutaZip);
            Response.End();

        }


        private CMNReporte ObtenerEncabezadoDesdeEmpresa(string instanciaConnectionString, DateTime fechaFin)
        {
            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 1 
                Rfc AS RfcContribuyente,
                RFCRepLegal AS RfcRepresentanteLegal,
                RFCProvCtlVol AS RfcProveedor,
                PermisoCRE AS Permiso,
                CveInstalacion AS ClaveInstalacion,
                'comercialización de petrolíferos' AS DescripcionInstalacion,
                0 AS NumeroPozos,
                0 AS NumeroDuctosEntradaSalida,
                0 AS NumeroDuctosTransporteDistribucion,
                0 AS NumeroDispensarios,
                0 AS GeoLatitud,
                0 AS GeoLongitud
            FROM empresasf", conn);

                string rfcContribuyente = "";
                string rfcRepresentante = "";
                string rfcProveedor = "";
                string permiso = "";
                string claveInstalacion = "";
                string descripcionInstalacion = "";
                int ductoEntrada = 0;
                int ductoDistribucion = 0;
                int dispensarios = 0;
                double geoLat = 0;
                double geoLong = 0;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        rfcContribuyente = dr["RfcContribuyente"].ToString();
                        rfcRepresentante = dr["RfcRepresentanteLegal"].ToString();
                        rfcProveedor = dr["RfcProveedor"].ToString();
                        permiso = dr["Permiso"].ToString();
                        claveInstalacion = dr["ClaveInstalacion"].ToString();
                        descripcionInstalacion = dr["DescripcionInstalacion"].ToString();
                        ductoEntrada = Convert.ToInt32(dr["NumeroDuctosEntradaSalida"]);
                        ductoDistribucion = Convert.ToInt32(dr["NumeroDuctosTransporteDistribucion"]);
                        dispensarios = Convert.ToInt32(dr["NumeroDispensarios"]);
                        geoLat = Convert.ToDouble(dr["GeoLatitud"]);
                        geoLong = Convert.ToDouble(dr["GeoLongitud"]);
                    }
                    else
                    {
                        return null;
                    }
                } // 🔒 Aquí se cierra el SqlDataReader antes de continuar

                // Ahora sí podemos ejecutar otra consulta
                int numeroTanques = 0;
                using (SqlCommand cmdTanques = new SqlCommand("SELECT COUNT(*) FROM tanques", conn))
                {
                    numeroTanques = (int)cmdTanques.ExecuteScalar();
                }

                return new CMNReporte
                {
                    Version = "1.2",
                    RfcContribuyente = rfcContribuyente,
                    RfcRepresentanteLegal = rfcRepresentante,
                    RfcProveedor = rfcProveedor,
                    RfcProveedores = new List<string> { rfcProveedor },
                    Caracter = "permisionario",
                    ModalidadPermiso = "PER1",
                    NumPermiso = permiso,
                    ClaveInstalacion = claveInstalacion,
                    DescripcionInstalacion = descripcionInstalacion,
                    NumeroPozos = 0,
                    NumeroTanques = numeroTanques,
                    NumeroDuctosEntradaSalida = ductoEntrada,
                    NumeroDuctosTransporteDistribucion = ductoDistribucion,
                    NumeroDispensarios = dispensarios,
                    FechaYHoraReporteMes = fechaFin,
                    Geolocalizacion = new List<Geolocalizacion>
            {
                new Geolocalizacion
                {
                    GeolocalizacionLatitud = geoLat,
                    GeolocalizacionLongitud = geoLong
                }
            },
                    Producto = new List<Producto>()
                };
            }
        }




        private List<Producto> ObtenerProductosDesdeBD(DateTime inicio, DateTime fin, string instanciaConnectionString)
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT 'PR' + CAST(ClaveCRE AS NVARCHAR(2)) AS ClaveProducto, ClaveSubProducto, Octanos, CombustibleNoFosil,NProdClave FROM productos", conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string claveProducto = dr["NProdClave"].ToString();

                        Producto p = new Producto
                        {
                            ClaveProducto = dr["ClaveProducto"].ToString(),
                            ClaveSubProducto = dr["ClaveSubProducto"].ToString(),
                            ComposOctanajeGasolina = Convert.ToDouble(dr["Octanos"]),
                            GasolinaConCombustibleNoFosil = dr["CombustibleNoFosil"].ToString(),
                            ReporteDeVolumenMensual = new ReporteDeVolumenMensual
                            {
                                ControlDeExistencias = ObtenerExistencias(inicio, instanciaConnectionString, claveProducto),
                                Recepciones = ObtenerRecepciones(inicio, fin, instanciaConnectionString, claveProducto),
                                Entregas = ObtenerEntregas(inicio, fin, instanciaConnectionString, claveProducto)
                            }
                        };

                        productos.Add(p);
                    }
                }
            }

            return productos;
        }

        // -----------------------------
        // Método nuevo: ObtenerExistencias desde la tabla existenciasvol
        // -----------------------------

        private ControlDeExistencias ObtenerExistencias(DateTime fecha, string instanciaConnectionString,string claveProducto)
        {
            ControlDeExistencias existencias = new ControlDeExistencias();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 SUM(VolumenDisponible) AS VolumenDisponible, CAST(FechaMedicionActual AS DATE) AS FechaMedicionActual FROM existenciasvol WHERE MONTH(FechaMedicionActual) = @mes AND YEAR(FechaMedicionActual) = @anio AND ClaveProducto=@cveproducto GROUP BY CAST(FechaMedicionActual AS DATE) ORDER BY FechaMedicionActual DESC", conn);
                cmd.Parameters.AddWithValue("@mes", fecha.Month);
                cmd.Parameters.AddWithValue("@anio", fecha.Year);
                cmd.Parameters.AddWithValue("@cveproducto", claveProducto);

                

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        existencias.VolumenExistenciasMes = Convert.ToDouble(dr["VolumenDisponible"]);
                        existencias.FechaYHoraEstaMedicionMes = Convert.ToDateTime(dr["FechaMedicionActual"]);
                    }
                }
            }

            return existencias;
        }

        private Recepciones ObtenerRecepciones(DateTime inicio, DateTime fin, string instanciaConnectionString,string cveproducto)
        {
            Recepciones recepciones = new Recepciones
            {
                Complemento = new List<Complemento>(),
                SumaVolumenRecepcionMes = new SumaVolumen { UnidadDeMedida = "UM03" }
            };

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                                          SELECT max(Rcap.Folio) AS Folio, RTRIM(Rcap.RFCProveedor) RFCProveedor, RTRIM(PC.Nombre) AS NombreProveedor, PC.PermisoCRE AS PermisoProveedor, Rcap.UUID AS Cfdi, 
                                          Rcap.FechaDocumento AS Fecha, Rcap.PrecioCompra AS Precio, Rcap.VolumenPemex AS Volumen 
                                          FROM Recepciones R 
                                          INNER JOIN Recepcionescap Rcap ON R.Folio = Rcap.Folio 
                                          INNER JOIN ProveedCombust PC ON Rcap.RFCProveedor = PC.RFC 
                                          WHERE CAST(Rcap.FechaDocumento AS DATE) BETWEEN @inicio AND @fin and r.ClaveProducto=@cveproducto
										  GROUP BY RTRIM(Rcap.RFCProveedor), RTRIM(PC.Nombre), PC.PermisoCRE, Rcap.UUID,Rcap.FechaDocumento, Rcap.PrecioCompra, Rcap.VolumenPemex", conn);
                cmd.Parameters.AddWithValue("@inicio", inicio);
                cmd.Parameters.AddWithValue("@fin", fin);
                cmd.Parameters.AddWithValue("@cveproducto", cveproducto);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    var grupo = new Dictionary<string, Nacional>();
                    var foliosAgrupados = new Dictionary<string, List<CfdiInfo>>();

                    while (dr.Read())
                    {
                        string folio = dr["Folio"].ToString();
                        string rfc = dr["RFCProveedor"].ToString();
                        string nombre = dr["NombreProveedor"].ToString();
                        string permiso = dr["PermisoProveedor"].ToString();

                        if (!grupo.ContainsKey(folio))
                        {
                            grupo[folio] = new Nacional
                            {
                                RfcClienteOProveedor = rfc,
                                NombreClienteOProveedor = nombre,
                                PermisoClienteOProveedor = permiso,
                                CFDIs = new List<CfdiInfo>()
                            };
                        }

                        string[] uuids = dr["Cfdi"].ToString().Split(',');
                        foreach (string uuid in uuids)
                        {
                            grupo[folio].CFDIs.Add(new CfdiInfo
                            {
                                Cfdi = uuid.Trim(),
                                PrecioVentaOCompraOContrap = Convert.ToDouble(dr["Precio"]),
                                FechaYHoraTransaccion = Convert.ToDateTime(dr["Fecha"]),
                                VolumenDocumentado = new VolumenDocumentado
                                {
                                    ValorNumerico = Convert.ToDouble(dr["Volumen"]),
                                    UnidadDeMedida = "UM03"
                                }
                            });

                            recepciones.TotalDocumentosMes++;
                            recepciones.ImporteTotalRecepcionesMensual += Convert.ToDouble(dr["Precio"]) * Convert.ToDouble(dr["Volumen"]);
                            recepciones.SumaVolumenRecepcionMes.ValorNumerico += Convert.ToDouble(dr["Volumen"]);
                        }

                        recepciones.TotalRecepcionesMes++;
                    }

                    recepciones.Complemento.Add(new Complemento { Nacional = grupo.Values.ToList() });
                }
            }
            return recepciones;
        }
        private Entregas ObtenerEntregas(DateTime inicio, DateTime fin, string instanciaConnectionString,string cveproducto)
        {
            Entregas entregas = new Entregas
            {
                Complemento = new List<Complemento>(),
                SumaVolumenEntregadoMes = new SumaVolumen { UnidadDeMedida = "UM03" }
            };

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT RFC RfcCliente,RazonSocial NombreCliente,'H/21100/COM/201' PermisoCliente,D.UUID AS Cfdi,D.Fecha,DD.Precio,DD.Cantidad AS Volumen FROM Documentos D INNER JOIN DocumentosDetalle DD ON D.UUID=DD.UUID WHERE CAST(d.Fecha AS DATE) BETWEEN @inicio AND @fin and Dd.Codigo=@cveproducto", conn);
                cmd.Parameters.AddWithValue("@inicio", inicio);
                cmd.Parameters.AddWithValue("@fin", fin);
                cmd.Parameters.AddWithValue("@cveproducto", cveproducto);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    var grupo = new Dictionary<string, Nacional>();
                    while (dr.Read())
                    {
                        string rfc = dr["RfcCliente"].ToString();
                        if (!grupo.ContainsKey(rfc))
                        {
                            grupo[rfc] = new Nacional
                            {
                                RfcClienteOProveedor = rfc,
                                NombreClienteOProveedor = dr["NombreCliente"].ToString(),
                                PermisoClienteOProveedor = dr["PermisoCliente"].ToString(),
                                CFDIs = new List<CfdiInfo>()
                            };
                        }
                        grupo[rfc].CFDIs.Add(new CfdiInfo
                        {
                            Cfdi = dr["Cfdi"].ToString(),
                            PrecioVentaOCompraOContrap = Convert.ToDouble(dr["Precio"]),
                            FechaYHoraTransaccion = Convert.ToDateTime(dr["Fecha"]),
                            VolumenDocumentado = new VolumenDocumentado
                            {
                                ValorNumerico = Convert.ToDouble(dr["Volumen"]),
                                UnidadDeMedida = "UM03"
                            }
                        });

                        entregas.TotalEntregasMes++;
                        entregas.TotalDocumentosMes++;
                        entregas.ImporteTotalEntregasMes += Convert.ToDouble(dr["Precio"]) * Convert.ToDouble(dr["Volumen"]);
                        entregas.SumaVolumenEntregadoMes.ValorNumerico += Convert.ToDouble(dr["Volumen"]);
                    }

                    entregas.Complemento.Add(new Complemento { Nacional = grupo.Values.ToList() });
                }
            }
            return entregas;
        }

       

        protected void btnSoloPDF_Click(object sender, EventArgs e)
        {
            try
            {
                int mes = int.Parse(ddlMes.SelectedValue);
                int anio = int.Parse(ddlAnio.SelectedValue);
                DateTime fechaInicio = new DateTime(anio, mes, 1);
                DateTime fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
                string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

                var reporte = ObtenerEncabezadoDesdeEmpresa(instanciaConnectionString, fechaFin);
                if (reporte == null)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No se encontró información de la empresa');", true);
                    return;
                }

                reporte.Producto = ObtenerProductosDesdeBD(fechaInicio, fechaFin, instanciaConnectionString);
                string json = JsonConvert.SerializeObject(reporte, Formatting.Indented);

                string uuid = Guid.NewGuid().ToString().ToUpper();
                string rfcContribuyente = reporte.RfcContribuyente;
                string rfcProveedor = reporte.RfcProveedor;
                string fechaSAT = fechaFin.ToString("yyyy-MM-dd");
                string nombrePdf = $"{uuid}_{fechaSAT}_CMN-1876_CMN_JSON.pdf";

                string rutaPdf = Server.MapPath("~/tempzip/" + nombrePdf);
                GenerarPDFDesdeJson(json, rutaPdf, fechaFin); // ← AQUÍ se llama tu método

                // Descargar el PDF generado
                if (File.Exists(rutaPdf))
                {
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", $"attachment; filename={nombrePdf}");
                    Response.TransmitFile(rutaPdf);
                    Response.Flush();
                    Response.End();
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No se pudo generar el PDF.');", true);
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"alert('Error al generar el PDF: {ex.Message}');", true);
            }
        }




        private void GenerarPDFDesdeJson(string json, string rutaPdf,DateTime fechaFin)
        {
            var reporte = JsonConvert.DeserializeObject<CMNReporte>(json);
        


            using (FileStream fs = new FileStream(rutaPdf, FileMode.Create, FileAccess.Write, FileShare.None))
            using (Document doc = new Document(PageSize.A4, 40f, 40f, 60f, 60f))
            using (PdfWriter writer = PdfWriter.GetInstance(doc, fs))
            {
                doc.Open();

                var boldTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var normalText = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                string mesAnio = fechaFin.ToString("MMMM/yyyy", new System.Globalization.CultureInfo("es-MX"));
                doc.Add(new Paragraph("REPORTE MENSUAL - Fecha reportada: " + mesAnio, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));

                doc.Add(new Paragraph("Fecha de generación: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), normalText));
            
                doc.Add(new Paragraph("RFC Contribuyente: " + reporte.RfcContribuyente, normalText));
                doc.Add(new Paragraph("RFC Proveedor: " + reporte.RfcProveedor, normalText));
                doc.Add(new Paragraph("Clave Instalación: " + reporte.ClaveInstalacion, normalText));
                doc.Add(new Paragraph("Permiso: " + reporte.NumPermiso, normalText));
                doc.Add(new Paragraph(""));

                foreach (var producto in reporte.Producto)
                {
                    doc.Add(new Paragraph($"Producto: {producto.ClaveProducto} - {producto.ClaveSubProducto}", boldTitle));
                    doc.Add(new Paragraph($"Octanaje: {producto.ComposOctanajeGasolina} | No Fosil: {producto.GasolinaConCombustibleNoFosil}", normalText));
                    doc.Add(new Paragraph("Existencias: " + producto.ReporteDeVolumenMensual.ControlDeExistencias.VolumenExistenciasMes +
                                         " al " + producto.ReporteDeVolumenMensual.ControlDeExistencias.FechaYHoraEstaMedicionMes.ToString("yyyy-MM-dd"), normalText));
                    doc.Add(new Paragraph(""));

                    // Tabla de Recepciones
                    var recepciones = producto.ReporteDeVolumenMensual.Recepciones;
                    doc.Add(new Paragraph("Recepciones", boldTitle));
                    PdfPTable tableR = new PdfPTable(5);
                    tableR.WidthPercentage = 100;
                    tableR.SetWidths(new float[] { 25, 20, 20, 15, 20 });

                    tableR.AddCell("RFC Proveedor");
                    tableR.AddCell("Nombre");
                    tableR.AddCell("UUID");
                    tableR.AddCell("Volumen");
                    tableR.AddCell("Fecha");

                    double totalVolumenRecep = 0;
                    int totalDocsRecep = 0;

                    foreach (var comp in recepciones.Complemento)
                    {
                        foreach (var nacional in comp.Nacional)
                        {
                            foreach (var cfdi in nacional.CFDIs)
                            {
                                tableR.AddCell(nacional.RfcClienteOProveedor);
                                tableR.AddCell(nacional.NombreClienteOProveedor);
                                tableR.AddCell(cfdi.Cfdi);
                                tableR.AddCell(cfdi.VolumenDocumentado.ValorNumerico.ToString("N2"));
                                tableR.AddCell(cfdi.FechaYHoraTransaccion.ToString("yyyy-MM-dd"));

                                totalVolumenRecep += cfdi.VolumenDocumentado.ValorNumerico;
                                totalDocsRecep++;
                            }
                        }
                    }

                    PdfPCell cellTotalR = new PdfPCell(new Phrase("Totales:"));
                    cellTotalR.Colspan = 3;
                    cellTotalR.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellTotalR.BackgroundColor = BaseColor.LIGHT_GRAY;
                    tableR.AddCell(cellTotalR);
                    tableR.AddCell(totalVolumenRecep.ToString("N2"));
                    tableR.AddCell(totalDocsRecep.ToString());

                    doc.Add(tableR);
                    doc.Add(new Paragraph(""));

                    // Tabla de Entregas
                    var entregas = producto.ReporteDeVolumenMensual.Entregas;
                    doc.Add(new Paragraph("Entregas", boldTitle));
                    PdfPTable tableE = new PdfPTable(5);
                    tableE.WidthPercentage = 100;
                    tableE.SetWidths(new float[] { 25, 20, 20, 15, 20 });

                    tableE.AddCell("RFC Cliente");
                    tableE.AddCell("Nombre");
                    tableE.AddCell("UUID");
                    tableE.AddCell("Volumen");
                    tableE.AddCell("Fecha");

                    double totalVolumenEnt = 0;
                    int totalDocsEnt = 0;

                    foreach (var comp in entregas.Complemento)
                    {
                        foreach (var nacional in comp.Nacional)
                        {
                            foreach (var cfdi in nacional.CFDIs)
                            {
                                tableE.AddCell(nacional.RfcClienteOProveedor);
                                tableE.AddCell(nacional.NombreClienteOProveedor);
                                tableE.AddCell(cfdi.Cfdi);
                                tableE.AddCell(cfdi.VolumenDocumentado.ValorNumerico.ToString("N2"));
                                tableE.AddCell(cfdi.FechaYHoraTransaccion.ToString("yyyy-MM-dd"));

                                totalVolumenEnt += cfdi.VolumenDocumentado.ValorNumerico;
                                totalDocsEnt++;
                            }
                        }
                    }

                    PdfPCell cellTotalE = new PdfPCell(new Phrase("Totales:"));
                    cellTotalE.Colspan = 3;
                    cellTotalE.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellTotalE.BackgroundColor = BaseColor.LIGHT_GRAY;
                    tableE.AddCell(cellTotalE);
                    tableE.AddCell(totalVolumenEnt.ToString("N2"));
                    tableE.AddCell(totalDocsEnt.ToString());

                    doc.Add(tableE);
                    doc.Add(new Paragraph("\n\n"));
                }

                doc.Close();
            }
        }

    }
}