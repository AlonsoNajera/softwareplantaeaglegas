using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Printing;
using System.Xml.Linq;
using static SoftwarePlantas.ControlVolumetrico;

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
            
            // ✅ Omitir Geolocalizacion si es null
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<Geolocalizacion> Geolocalizacion { get; set; }
            
            public List<Producto> Producto { get; set; }
            
            // ✅ Nueva propiedad para BitacoraMensual
            public List<EventoBitacora> BitacoraMensual { get; set; }
        }

        // ✅ Nueva clase para los eventos de la bitácora
        public class EventoBitacora
        {
            public int NumeroRegistro { get; set; }
            public DateTime FechaYHoraEvento { get; set; }
            public int TipoEvento { get; set; }
            public string DescripcionEvento { get; set; }
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
            
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int? ComposOctanajeGasolina { get; set; }
            
            // ✅ Para gasolinas (SP16, SP17)
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string GasolinaConCombustibleNoFosil { get; set; }
            
            // ✅ Para diesel (SP18)
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string DieselConCombustibleNoFosil { get; set; }
            
            public ReporteDeVolumenMensual ReporteDeVolumenMensual { get; set; }

            // ✅ SOLO SP16 y SP17 deben mostrar ComposOctanajeGasolina
            public bool ShouldSerializeComposOctanajeGasolina()
            {
                return ClaveSubProducto == "SP16" || ClaveSubProducto == "SP17";
            }
            
            // ✅ Solo SP16 y SP17 deben mostrar GasolinaConCombustibleNoFosil
            public bool ShouldSerializeGasolinaConCombustibleNoFosil()
            {
                return ClaveSubProducto == "SP16" || ClaveSubProducto == "SP17";
            }
            
            // ✅ Solo SP18 debe mostrar DieselConCombustibleNoFosil
            public bool ShouldSerializeDieselConCombustibleNoFosil()
            {
                return ClaveSubProducto == "SP18";
            }
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
            public string TipoComplemento { get; set; } = "Distribucion";
            public List<Nacional> Nacional { get; set; }
        }

        public class Nacional
        {
            public string RfcClienteOProveedor { get; set; }
            public string NombreClienteOProveedor { get; set; }
        
            public List<CfdiInfo> CFDIs { get; set; }
        }

        public class CfdiInfo
        {
            public string Cfdi { get; set; }
            public string TipoCfdi { get; set; } = "Ingreso";
            
            // ✅ Nullable para omitir cuando no aplica
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public double? PrecioCompra { get; set; }

            // ✅ Nullable para omitir cuando no aplica
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public double? PrecioVenta { get; set; }
            
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

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            var reporte = ObtenerEncabezadoDesdeEmpresa(instanciaConnectionString, fechaFin);
            if (reporte == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No se encontró información en la tabla empresasf');", true);
                return;
            }

            reporte.Producto = ObtenerProductosDesdeBD(fechaInicio, fechaFin, instanciaConnectionString);

            // ✅ Configurar serialización con formato ISO 8601 y zona horaria
            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(reporte, settings);

            string uuid = Guid.NewGuid().ToString().ToUpper();
            string rfcContribuyente = reporte.RfcContribuyente;
            string rfcProveedor = reporte.RfcProveedor;
            string fechaSAT = fechaFin.ToString("yyyy-MM-dd");
            string ClaveInstalacion = reporte.ClaveInstalacion;
            string nombreBase = $"M_{uuid}_{rfcContribuyente}_{rfcProveedor}_{fechaSAT}_{ClaveInstalacion}_DIS_JSON";
            string nombreJson = nombreBase + ".json";
            string nombreZip = nombreBase + ".zip";

            string carpetaTemp = Server.MapPath("~/tempzip/");
            if (!Directory.Exists(carpetaTemp)) Directory.CreateDirectory(carpetaTemp);

            string rutaJson = Path.Combine(carpetaTemp, nombreJson);
            File.WriteAllText(rutaJson, json);

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
                'Distribucion de petrolíferos' AS DescripcionInstalacion,
                0 AS NumeroPozos,
                0 AS NumeroDuctosEntradaSalida,
                0 AS NumeroDuctosTransporteDistribucion,
                0 AS NumeroDispensarios
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
                    }
                    else
                    {
                        return null;
                    }
                }

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
                    ModalidadPermiso = "PER5",
                    NumPermiso = permiso,
                    ClaveInstalacion = claveInstalacion,
                    DescripcionInstalacion = descripcionInstalacion,
                    NumeroPozos = 0,
                    NumeroTanques = numeroTanques,
                    NumeroDuctosEntradaSalida = ductoEntrada,
                    NumeroDuctosTransporteDistribucion = ductoDistribucion,
                    NumeroDispensarios = dispensarios,
                    FechaYHoraReporteMes = fechaFin,
                    Geolocalizacion = null,
                    Producto = new List<Producto>(),
                    // ✅ Agregar BitacoraMensual con el evento fijo
                    BitacoraMensual = new List<EventoBitacora>
                    {
                        new EventoBitacora
                        {
                            NumeroRegistro = 1,
                            FechaYHoraEvento = fechaFin,
                            TipoEvento = 5,
                            DescripcionEvento = "Archivo JSON Mensual (Distribuidora)."
                        }
                    }
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
                        string claveSubProducto = dr["ClaveSubProducto"].ToString();
                        int octanos = dr["Octanos"] != DBNull.Value ? Convert.ToInt32(dr["Octanos"]) : 0;
                        string combustibleNoFosil = dr["CombustibleNoFosil"].ToString();

                        Producto p = new Producto
                        {
                            ClaveProducto = dr["ClaveProducto"].ToString(),
                            ClaveSubProducto = claveSubProducto,

                            // ✅ Solo asignar octanaje para SP16 y SP17 (NO para SP18)
                            ComposOctanajeGasolina = (claveSubProducto == "SP16" || claveSubProducto == "SP17")
                                ? (int?)octanos
                                : null,

                            // ✅ Para SP16 y SP17: usar GasolinaConCombustibleNoFosil
                            GasolinaConCombustibleNoFosil = (claveSubProducto == "SP16" || claveSubProducto == "SP17")
                                ? combustibleNoFosil
                                : null,

                            // ✅ Para SP18: usar DieselConCombustibleNoFosil
                            DieselConCombustibleNoFosil = (claveSubProducto == "SP18")
                                ? combustibleNoFosil
                                : null,

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
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 CAST(SUM(VolumenDisponible) AS DECIMAL(20,2)) AS VolumenDisponible, CAST(FechaMedicionActual AS DATE) AS FechaMedicionActual FROM existenciasvol WHERE MONTH(FechaMedicionActual) = @mes AND YEAR(FechaMedicionActual) = @anio AND ClaveProducto=@cveproducto GROUP BY CAST(FechaMedicionActual AS DATE) ORDER BY FechaMedicionActual DESC", conn);
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
                              Rcap.FechaDocumento AS Fecha, cast(Rcap.PrecioCompra as decimal(15,2)) AS Precio, CAST(Rcap.VolumenPemex AS DECIMAL(15,2)) AS Volumen 
                              FROM Recepciones R 
                              INNER JOIN Recepcionescap Rcap ON R.Folio = Rcap.Folio 
                              INNER JOIN ProveedCombust PC ON Rcap.IdProveedor = PC.IdProveedor 
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
                       
                        if (!grupo.ContainsKey(folio))
                        {
                            grupo[folio] = new Nacional
                            {
                                RfcClienteOProveedor = rfc,
                                NombreClienteOProveedor = nombre,
                                CFDIs = new List<CfdiInfo>()
                            };
                        }

                        string[] uuids = dr["Cfdi"].ToString().Split(',');
                        foreach (string uuid in uuids)
                        {
                            DateTime fechaTransaccion = Convert.ToDateTime(dr["Fecha"]);
                            double volumen = Convert.ToDouble(dr["Volumen"]);
                            double precio = Convert.ToDouble(dr["Precio"]);
                            
                            grupo[folio].CFDIs.Add(new CfdiInfo
                            {
                                Cfdi = uuid.Trim(),
                                PrecioCompra = Math.Round(precio, 2),
                                FechaYHoraTransaccion = fechaTransaccion,
                                VolumenDocumentado = new VolumenDocumentado
                                {
                                    ValorNumerico = Math.Round(volumen, 3),
                                    UnidadDeMedida = "UM03"
                                }
                            });

                            recepciones.TotalDocumentosMes++;
                            recepciones.ImporteTotalRecepcionesMensual += precio * volumen;
                            recepciones.SumaVolumenRecepcionMes.ValorNumerico += volumen;
                        }

                        recepciones.TotalRecepcionesMes++;
                    }

                    recepciones.Complemento.Add(new Complemento { Nacional = grupo.Values.ToList() });
                }
            }
    
            // ✅ Redondear totales también
    recepciones.ImporteTotalRecepcionesMensual = Math.Round(recepciones.ImporteTotalRecepcionesMensual, 2);
    recepciones.SumaVolumenRecepcionMes.ValorNumerico = Math.Round(recepciones.SumaVolumenRecepcionMes.ValorNumerico, 3);
    
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
                SqlCommand cmd = new SqlCommand("SELECT RFC RfcCliente,RazonSocial NombreCliente,'H/21100/COM/201' PermisoCliente,D.UUID AS Cfdi,D.Fecha,CAST(DD.Precio AS DECIMAL(15,2)) Precio,CAST(DD.Cantidad AS DECIMAL(15,2)) AS Volumen FROM Documentos D INNER JOIN DocumentosDetalle DD ON D.UUID=DD.UUID WHERE CAST(d.Fecha AS DATE) BETWEEN @inicio AND @fin and Dd.Codigo=@cveproducto", conn);
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
                                CFDIs = new List<CfdiInfo>()
                            };
                        }
                        
                        DateTime fechaTransaccion = Convert.ToDateTime(dr["Fecha"]);
                        double volumen = Convert.ToDouble(dr["Volumen"]);
                        double precio = Convert.ToDouble(dr["Precio"]);
                        
                        grupo[rfc].CFDIs.Add(new CfdiInfo
                        {
                            Cfdi = dr["Cfdi"].ToString(),
                            // ✅ Solo asignar PrecioVenta (NO PrecioCompra)
                            PrecioCompra = null, // ✅ Explícitamente null para omitir
                            PrecioVenta = Math.Round(precio, 2),
                            FechaYHoraTransaccion = fechaTransaccion,
                            VolumenDocumentado = new VolumenDocumentado
                            {
                                ValorNumerico = Math.Round(volumen, 3),
                                UnidadDeMedida = "UM03"
                            }
                        });

                        entregas.TotalEntregasMes++;
                        entregas.TotalDocumentosMes++;
                        entregas.ImporteTotalEntregasMes += precio * volumen;
                        entregas.SumaVolumenEntregadoMes.ValorNumerico += volumen;
                    }

                    entregas.Complemento.Add(new Complemento { Nacional = grupo.Values.ToList() });
                }
            }
            
            // ✅ Redondear totales también
            entregas.ImporteTotalEntregasMes = Math.Round(entregas.ImporteTotalEntregasMes, 2);
            entregas.SumaVolumenEntregadoMes.ValorNumerico = Math.Round(entregas.SumaVolumenEntregadoMes.ValorNumerico, 3);
            
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



        private List<RecepcionExcel> ObtenerRecepcionesExcel(DateTime inicio, DateTime fin, string cs)
        {
            var list = new List<RecepcionExcel>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                var sql = @"
 SELECT
    Rcap.ClaveProducto,
	Rcap.Producto,
    Rcap.Folio,
    RTRIM(Rcap.RFCProveedor) AS RFCProveedor,
    RTRIM(PC.Nombre) AS NombreProveedor,
    PC.PermisoCRE AS PermisoProveedor,
    RTRIM(Rcap.UUID) AS UUID,
    CAST(Rcap.FechaDocumento AS date) AS FechaDocumento,
    CAST(Rcap.FechaRecepcion AS date) AS FechaRecepcion,
    Rcap.FolioDocumento,
    CAST(Rcap.VolumenPemex AS DECIMAL(15,2)) AS Volumen,
    CAST(Rcap.PrecioCompra AS DECIMAL (15,2)) PrecioCompra
FROM dbo.Recepcionescap Rcap
LEFT JOIN dbo.ProveedCombust PC ON Rcap.IdProveedor = PC.IdProveedor
WHERE CAST(Rcap.FechaDocumento AS date)  BETWEEN @inicio AND @fin
ORDER BY Rcap.ClaveProducto,Rcap.FechaDocumento, Rcap.Folio;";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@inicio", SqlDbType.Date).Value = inicio.Date;
                    cmd.Parameters.Add("@fin", SqlDbType.Date).Value = fin.Date;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new RecepcionExcel
                            {

                                Folio = dr["Folio"]?.ToString(),
                                RFCProveedor = dr["RFCProveedor"]?.ToString(),
                                NombreProveedor = dr["NombreProveedor"]?.ToString(),
                                PermisoProveedor = dr["PermisoProveedor"]?.ToString(),
                                UUID = dr["UUID"]?.ToString(),
                                FechaDocumento = dr["FechaDocumento"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaDocumento"]),
                                FechaRecepcion = dr["FechaRecepcion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaRecepcion"]),
                                FolioDocumento = dr["FolioDocumento"]?.ToString(),
                                Volumen = dr["Volumen"] == DBNull.Value ? 0 : Convert.ToDouble(dr["Volumen"]),
                                PrecioCompra = dr["PrecioCompra"] == DBNull.Value ? 0 : Convert.ToDouble(dr["PrecioCompra"]),
                                ClaveProducto = dr["ClaveProducto"]?.ToString(),
                                Producto = dr["Producto"]?.ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        private List<EntregaExcel> ObtenerEntregasExcel(DateTime inicio, DateTime fin, string cs)
        {
            var list = new List<EntregaExcel>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                var sql = @"
SELECT
    D.UUID,
    RTRIM(D.RFC) AS RFCCliente,
    RTRIM(D.RazonSocial) AS NombreCliente,
    CAST(D.Fecha AS date) AS Fecha,
    DD.Codigo,
    CAST(SUM(DD.Cantidad) AS DECIMAL(15,2)) AS Cantidad,
    CAST(D.Total AS DECIMAL(15,2)) AS Total
FROM dbo.Documentos D
INNER JOIN dbo.DocumentosDetalle DD ON D.UUID = DD.UUID
WHERE CAST(D.Fecha AS date) BETWEEN @inicio AND @fin
GROUP BY  D.UUID,  RTRIM(D.RFC),RTRIM(D.RazonSocial),CAST(D.Fecha AS date),    DD.Codigo,  D.Total
ORDER BY Codigo,CAST(D.Fecha AS date)
;";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@inicio", SqlDbType.Date).Value = inicio.Date;
                    cmd.Parameters.Add("@fin", SqlDbType.Date).Value = fin.Date;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new EntregaExcel
                            {
                                UUID = dr["UUID"]?.ToString(),
                                RFCCliente = dr["RFCCliente"]?.ToString(),
                                NombreCliente = dr["NombreCliente"]?.ToString(),
                                Fecha = dr["Fecha"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["Fecha"]),
                                Codigo = dr["Codigo"]?.ToString(),
                                Cantidad = dr["Cantidad"] == DBNull.Value ? 0 : Convert.ToDouble(dr["Cantidad"]),
                                Total = dr["Total"] == DBNull.Value ? 0 : Convert.ToDouble(dr["Total"])
                            });
                        }
                    }
                }
            }

            return list;
        }


        private byte[] GenerarExcelCompleto(
     List<RecepcionExcel> recepciones,
     List<EntregaExcel> entregas,
     List<ExistenciaExcel> existencias,
     DateTime inicio,
     DateTime fin)
        {
            // EPPlus 4.5.3.3 NO requiere licencia
            using (var package = new ExcelPackage())
            {
                // ===== HOJA 1: RECEPCIONES =====
                var wsRecepciones = package.Workbook.Worksheets.Add("Recepciones");
                int row = 1;

                wsRecepciones.Cells[row, 1].Value = "RECEPCIONES";
                wsRecepciones.Cells[row, 1, row, 10].Merge = true;
                wsRecepciones.Cells[row, 1, row, 10].Style.Font.Bold = true;
                wsRecepciones.Cells[row, 1, row, 10].Style.Font.Size = 14;
                wsRecepciones.Cells[row, 1, row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row++;

                wsRecepciones.Cells[row, 1].Value = "Periodo:";
                wsRecepciones.Cells[row, 2].Value = $"{inicio:yyyy-MM-dd} al {fin:yyyy-MM-dd}";
                row += 2;

                // Encabezados
                string[] headersR = { "Folio", "RFC Proveedor", "Nombre", "Permiso", "UUID",
                             "Fecha Documento", "Fecha Recepción", "Folio Documento",
                             "Volumen", "Precio Compra","ClaveProducto","Producto" };
                for (int i = 0; i < headersR.Length; i++)
                {
                    wsRecepciones.Cells[row, i + 1].Value = headersR[i];
                }

                wsRecepciones.Cells[row, 1, row, 12].Style.Font.Bold = true;
                wsRecepciones.Cells[row, 1, row, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsRecepciones.Cells[row, 1, row, 12].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                wsRecepciones.Cells[row, 1, row, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;

                foreach (var r in recepciones)
                {
                    wsRecepciones.Cells[row, 1].Value = r.Folio ?? "";
                    wsRecepciones.Cells[row, 2].Value = r.RFCProveedor ?? "";
                    wsRecepciones.Cells[row, 3].Value = r.NombreProveedor ?? "";
                    wsRecepciones.Cells[row, 4].Value = r.PermisoProveedor ?? "";
                    wsRecepciones.Cells[row, 5].Value = r.UUID ?? "";
                    wsRecepciones.Cells[row, 6].Value = r.FechaDocumento?.ToString("yyyy-MM-dd") ?? "";
                    wsRecepciones.Cells[row, 7].Value = r.FechaRecepcion?.ToString("yyyy-MM-dd") ?? "";
                    wsRecepciones.Cells[row, 8].Value = r.FolioDocumento ?? "";
                    wsRecepciones.Cells[row, 9].Value = r.Volumen;
                    wsRecepciones.Cells[row, 10].Value = r.PrecioCompra;
                    wsRecepciones.Cells[row, 11].Value = r.ClaveProducto;
                    wsRecepciones.Cells[row, 12].Value = r.Producto;

                    wsRecepciones.Cells[row, 9].Style.Numberformat.Format = "#,##0.000";
                    wsRecepciones.Cells[row, 10].Style.Numberformat.Format = "#,##0.00";
                    row++;
                }

                wsRecepciones.Cells.AutoFitColumns();
                wsRecepciones.View.FreezePanes(5, 1);

                // ===== HOJA 2: ENTREGAS =====
                var wsEntregas = package.Workbook.Worksheets.Add("Entregas");
                row = 1;

                wsEntregas.Cells[row, 1].Value = "ENTREGAS";
                wsEntregas.Cells[row, 1, row, 7].Merge = true;
                wsEntregas.Cells[row, 1, row, 7].Style.Font.Bold = true;
                wsEntregas.Cells[row, 1, row, 7].Style.Font.Size = 14;
                wsEntregas.Cells[row, 1, row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row++;

                wsEntregas.Cells[row, 1].Value = "Periodo:";
                wsEntregas.Cells[row, 2].Value = $"{inicio:yyyy-MM-dd} al {fin:yyyy-MM-dd}";
                row += 2;

                string[] headersE = { "UUID", "RFC Cliente", "Nombre Cliente", "Fecha",
                             "Código Producto", "Cantidad", "Total" };
                for (int i = 0; i < headersE.Length; i++)
                {
                    wsEntregas.Cells[row, i + 1].Value = headersE[i];
                }

                wsEntregas.Cells[row, 1, row, 7].Style.Font.Bold = true;
                wsEntregas.Cells[row, 1, row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsEntregas.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                wsEntregas.Cells[row, 1, row, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;

                double totalCantidad = 0;
                double totalImporte = 0;

                foreach (var e in entregas)
                {
                    wsEntregas.Cells[row, 1].Value = e.UUID ?? "";
                    wsEntregas.Cells[row, 2].Value = e.RFCCliente ?? "";
                    wsEntregas.Cells[row, 3].Value = e.NombreCliente ?? "";
                    wsEntregas.Cells[row, 4].Value = e.Fecha?.ToString("yyyy-MM-dd") ?? "";
                    wsEntregas.Cells[row, 5].Value = e.Codigo ?? "";
                    wsEntregas.Cells[row, 6].Value = e.Cantidad;
                    wsEntregas.Cells[row, 7].Value = e.Total;

                    wsEntregas.Cells[row, 6].Style.Numberformat.Format = "#,##0.000";
                    wsEntregas.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";

                    totalCantidad += e.Cantidad;
                    totalImporte += e.Total;
                    row++;
                }

                wsEntregas.Cells[row, 1].Value = "TOTALES:";
                wsEntregas.Cells[row, 1, row, 5].Merge = true;
                wsEntregas.Cells[row, 1, row, 5].Style.Font.Bold = true;
                wsEntregas.Cells[row, 1, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                wsEntregas.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsEntregas.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                wsEntregas.Cells[row, 6].Value = totalCantidad;
                wsEntregas.Cells[row, 6].Style.Numberformat.Format = "#,##0.000";
                wsEntregas.Cells[row, 6].Style.Font.Bold = true;
                wsEntregas.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsEntregas.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                wsEntregas.Cells[row, 7].Value = totalImporte;
                wsEntregas.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                wsEntregas.Cells[row, 7].Style.Font.Bold = true;
                wsEntregas.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
               

                wsEntregas.Cells.AutoFitColumns();
                wsEntregas.View.FreezePanes(5, 1);

                // ===== HOJA 3: EXISTENCIAS =====
                var wsExistencias = package.Workbook.Worksheets.Add("Existencias");
                row = 1;

                wsExistencias.Cells[row, 1].Value = "EXISTENCIAS";
                wsExistencias.Cells[row, 1, row, 4].Merge = true;
                wsExistencias.Cells[row, 1, row, 4].Style.Font.Bold = true;
                wsExistencias.Cells[row, 1, row, 4].Style.Font.Size = 14;
                wsExistencias.Cells[row, 1, row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                row++;

                wsExistencias.Cells[row, 1].Value = "Periodo:";
                wsExistencias.Cells[row, 2].Value = $"{inicio:yyyy-MM-dd} al {fin:yyyy-MM-dd}";
                row += 2;

                string[] headersEx = { "Fecha Medición", "Clave Producto", "Nombre Producto", "Vol. Disponible" };
                for (int i = 0; i < headersEx.Length; i++)
                {
                    wsExistencias.Cells[row, i + 1].Value = headersEx[i];
                }

                wsExistencias.Cells[row, 1, row, 4].Style.Font.Bold = true;
                wsExistencias.Cells[row, 1, row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsExistencias.Cells[row, 1, row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                wsExistencias.Cells[row, 1, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                row++;

                double totalVolDisponible = 0;

                foreach (var ex in existencias)
                {
                    wsExistencias.Cells[row, 1].Value = ex.FechaMedicion?.ToString("yyyy-MM-dd") ?? "";
                    wsExistencias.Cells[row, 2].Value = ex.ClaveProducto ?? "";
                    wsExistencias.Cells[row, 3].Value = ex.NombreProducto ?? "";
                    wsExistencias.Cells[row, 4].Value = ex.VolumenDisponible;

                    wsExistencias.Cells[row, 4].Style.Numberformat.Format = "#,##0.000";

                    totalVolDisponible += ex.VolumenDisponible;
                    row++;
                }

                wsExistencias.Cells[row, 1].Value = "TOTALES:";
                wsExistencias.Cells[row, 1, row, 3].Merge = true;
                wsExistencias.Cells[row, 1, row, 3].Style.Font.Bold = true;
                wsExistencias.Cells[row, 1, row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                wsExistencias.Cells[row, 1, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsExistencias.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                wsExistencias.Cells[row, 4].Value = totalVolDisponible;
                wsExistencias.Cells[row, 4].Style.Numberformat.Format = "#,##0.000";
                wsExistencias.Cells[row, 4].Style.Font.Bold = true;
                wsExistencias.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                wsExistencias.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                wsExistencias.Cells.AutoFitColumns();
                wsExistencias.View.FreezePanes(5, 1);

                return package.GetAsByteArray();
            }
        }
        protected void btnSoloExcel_Click(object sender, EventArgs e)
        {
            try
            {
                int mes = int.Parse(ddlMes.SelectedValue);
                int anio = int.Parse(ddlAnio.SelectedValue);
                DateTime inicio = new DateTime(anio, mes, 1);
                DateTime fin = inicio.AddMonths(1).AddDays(-1);

                string cs = Session["instanciaSeleccionada"]?.ToString();
                if (string.IsNullOrWhiteSpace(cs))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "err",
                        "Swal.fire('Error','No hay instancia seleccionada.','error');", true);
                    return;
                }

                // 1) Traer recepciones, entregas y existencias
                List<RecepcionExcel> recepciones = ObtenerRecepcionesExcel(inicio, fin, cs);
                List<EntregaExcel> entregas = ObtenerEntregasExcel(inicio, fin, cs);
                List<ExistenciaExcel> existencias = ObtenerExistenciasExcel(inicio, fin, cs);

                // 2) Generar excel con las 3 hojas
                byte[] bytes = GenerarExcelCompleto(recepciones, entregas, existencias, inicio, fin);

                // 3) Descargar
                string fileName = $"ControlVolumetrico_{anio}{mes:00}.xlsx";

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
            catch (ThreadAbortException)
            {
                // Esto es normal después de Response.End(), no hacer nada
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "err",
                    $"Swal.fire('Error','{ex.Message.Replace("'", "\\'").Replace("\r", "").Replace("\n", "")}','error');", true);
            }
        }


        private List<ExistenciaExcel> ObtenerExistenciasExcel(DateTime inicio, DateTime fin, string cs)
        {
            var list = new List<ExistenciaExcel>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                var sql = @"

SELECT
    CAST(ev.FechaMedicionActual AS date) AS FechaMedicion,
    ev.ClaveProducto,
    p.NProdNombre AS NombreProducto,
    sum(ev.VolumenDisponible) VolumenDisponible
FROM dbo.existenciasvol ev
LEFT JOIN dbo.productos p ON ev.ClaveProducto = p.NProdClave
WHERE CAST(ev.FechaMedicionActual AS date) =  @fin
GROUP BY  CAST(ev.FechaMedicionActual AS date),ev.ClaveProducto,p.NProdNombre
ORDER BY CAST(ev.FechaMedicionActual AS date) , ev.ClaveProducto
;";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@inicio", SqlDbType.Date).Value = inicio.Date;
                    cmd.Parameters.Add("@fin", SqlDbType.Date).Value = fin.Date;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new ExistenciaExcel
                            {
                                FechaMedicion = dr["FechaMedicion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaMedicion"]),
                                ClaveProducto = dr["ClaveProducto"]?.ToString(),
                                NombreProducto = dr["NombreProducto"]?.ToString(),
                                VolumenDisponible = dr["VolumenDisponible"] == DBNull.Value ? 0 : Convert.ToDouble(dr["VolumenDisponible"]),
                
                            });
                        }
                    }
                }
            }

            return list;
        }

        public class RecepcionExcel
        {
            public string ClaveProducto { get; set; }
            public string Producto { get; set; }
            public string Folio { get; set; }
            public string RFCProveedor { get; set; }
            public string NombreProveedor { get; set; }
            public string PermisoProveedor { get; set; }
            public string UUID { get; set; }
            public DateTime? FechaDocumento { get; set; }
            public DateTime? FechaRecepcion { get; set; }
            public string FolioDocumento { get; set; }
            public double Volumen { get; set; }
            public double PrecioCompra { get; set; }
        }

        public class EntregaExcel
        {
            public string UUID { get; set; }
            public string RFCCliente { get; set; }
            public string NombreCliente { get; set; }
            public string PermisoCliente { get; set; }
            public DateTime? Fecha { get; set; }
            public string Codigo { get; set; }
            public double Cantidad { get; set; }
            public double Precio { get; set; }
            public double Total { get; set; }
        }

        public class ExistenciaExcel
        {
            public DateTime? FechaMedicion { get; set; }
            public string ClaveProducto { get; set; }
            public string NombreProducto { get; set; }
            public double VolumenInicial { get; set; }
            public double VolumenFinal { get; set; }
            public double VolumenDisponible { get; set; }
            public double Temperatura { get; set; }
            public double Altura { get; set; }
        }
    }
}