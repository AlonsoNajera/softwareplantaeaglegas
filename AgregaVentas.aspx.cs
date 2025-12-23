using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using ExcelDataReader;
using static SoftwarePlantas.ControlVolumetrico;

namespace SoftwarePlantas
{
    public partial class AgregaVentas : System.Web.UI.Page
    {
        string instancia = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Evitar caché
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (Session["IdUsuario"] == null || Session["Nombre"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            lblWelcome.Text = $"Bienvenido, {Session["Nombre"]}!";

            if (!IsPostBack)
            {
                CargarEstaciones();


                if (Request.QueryString["ok"] == "1")
                {
                    string n = Request.QueryString["n"] ?? "0";
                    string cleanUrl = ResolveUrl("~/AgregaVentas.aspx");

                    // Limpia la URL para que F5 ya no traiga ?ok=1&n=...
                    ScriptManager.RegisterStartupScript(this, GetType(), "swal-ok",
                        $@"window.history.replaceState(null,'','{cleanUrl}');
               Swal.fire('Listo','Se insertaron {n} registros en despachos.','success');", true);
                }

                ViewState["submitToken"] = Guid.NewGuid().ToString();
            }
        }

        private void CargarEstaciones()
        {
            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            string connectionString = WebConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(@"
                SELECT e.Id, e.Nombre
                FROM estaciones e
                INNER JOIN UsersEstaciones ue ON e.Id = ue.idEstacion
                WHERE ue.idUsuario = @IdUsuario", connection))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@IdUsuario", idUsuario);
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
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        protected void ddlEstacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string estacionId = ddlEstacion.SelectedValue;
            if (string.IsNullOrWhiteSpace(estacionId))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    "Swal.fire('Error', 'Selecciona una Estación.', 'error');", true);
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            // Buscar la instancia de la estación
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("SELECT instancia FROM estaciones WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", estacionId);
                connection.Open();
                instancia = command.ExecuteScalar()?.ToString();
            }

            if (string.IsNullOrWhiteSpace(instancia))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    "Swal.fire('Error', 'No se encontró la instancia de la estación.', 'error');", true);
                return;
            }

            // Construir cadena a instancia seleccionada (ajusta credenciales si aplica)
            string instanciaConnectionString =
                $"Server={instancia};Database=Admingas;User Id=sa;Password=$Aes220213Nu4&;";

            if (VerificarConexionBaseDatos(instanciaConnectionString))
            {
                Session["instanciaSeleccionada"] = instanciaConnectionString;
                // Si quieres disparar búsqueda automática:
                // btnBuscar_Click(null, EventArgs.Empty);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert",
                    "Swal.fire('Error', 'No se pudo establecer conexión con la base de datos.', 'error');", true);
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {

            // Anti-refresh / doble envío
            var token = ViewState["submitToken"] as string;
            if (token != null && (string)Session["lastSubmitToken"] == token)
                return;


            // Validaciones
            if (string.IsNullOrEmpty(ddlEstacion.SelectedValue))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                    "Swal.fire('Falta estación', 'Selecciona una estación.', 'warning');", true);
                return;
            }

            if (!fileExcel.HasFile)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                    "Swal.fire('Archivo requerido', 'Selecciona un Excel (.xlsx/.xls).', 'warning');", true);
                return;
            }

            string ext = Path.GetExtension(fileExcel.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                    "Swal.fire('Formato no válido', 'El archivo debe ser .xlsx o .xls.', 'error');", true);
                return;
            }

       
            // Conexión (prioriza la instancia seleccionada)
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            if (string.IsNullOrWhiteSpace(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                    "Swal.fire('Sin conexión', 'No hay instancia seleccionada para conectar.', 'error');", true);
                return;
            }

            try
            {
                DataTable dtDatos = ConstruirDataTableDestino();
                int idEstacion = int.Parse(ddlEstacion.SelectedValue);

                using (Stream stream = fileExcel.PostedFile.InputStream)
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream)) // Lee .xls y .xlsx
                {
                    var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                    });

                    if (ds == null || ds.Tables.Count == 0)
                        throw new Exception("El Excel no contiene hojas con datos.");

                    DataTable hoja = ds.Tables[0];

                    foreach (DataRow r in hoja.Rows)
                    {
                        if (EsVacia(r)) continue;

                        DateTime fecha = Convert.ToDateTime(r["Fecha"]).Date;

                        decimal volumen = ParseDecimal(r["Volumen"]);
                        int posicion = ParseInt(r["Posicion"]);
                        int manguera = ParseInt(r["Manguera"]);
                        string producto = r["Producto"]?.ToString()?.Trim();
                        decimal precio = ParseDecimal(r["Precio"]);
                        int nDisp = ParseInt(r["NDisp"]);
                        int corte = ParseInt(r["Corte"]);

                        int chipDespachador = ParseInt(r["ChipDespachador"]);
                        string nomProducto = r["NomProducto"]?.ToString()?.Trim();

                        dtDatos.Rows.Add(fecha, volumen, posicion, manguera, producto,precio,nDisp,corte,chipDespachador,nomProducto, idEstacion);
                    }
                }

                if (dtDatos.Rows.Count == 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                        "Swal.fire('Sin registros', 'El Excel no tiene filas válidas.', 'info');", true);
                    return;
                }

                // Inserción fila por fila
                using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO Despachos
            (
                Id, TipoReg, Cliente, Solicitud, Posicion, Manguera, ClaveProd, 
                Volumen, Precio, ImporteVenta, FechaHora, FechaHoraG, Producto, TipoVenta, 
                Dispensario, TotalI, TotalV, Transi, NumTicket, PVenta, TPago, Turno, 
                CierreTurno, FolioTotal, Monto_iva, Precio_siva, Chip, Corte, FechaTurno, 
                Vehiculo, Flotilla, EstadoFact, Factura, BaseFactura, ClaveFormaPago, 
                ImporteIEPS, SerieFact, UUID, docAplic, IdBanco, Observaciones
            )
            VALUES
            (
                1,'D', NULL, 0, @Posicion, @Manguera, @Producto, 
                @Volumen, @Precio, @Volumen*@Precio, @Fecha, CAST(@Fecha AS DATE), @NomProducto,'D', @NDisp, 
                0, 0, 0, 0, 0, 'EFECTIVO', 0, 0, 0, 0, 
                0, @ChipDespachador, @Corte, CAST(@Fecha AS DATE), NULL, NULL, 0, 0, 0, '01', 
                0, '', '', 0, '', 'VSystem'
            );", conn))
                {
                    // Parámetros
                    var pFecha = cmd.Parameters.Add("@Fecha", SqlDbType.DateTime2);

                    var pVol = cmd.Parameters.Add("@Volumen", SqlDbType.Decimal);
                    pVol.Precision = 18;
                    pVol.Scale = 3;

                    cmd.Parameters.Add("@Posicion", SqlDbType.Int);
                    cmd.Parameters.Add("@Manguera", SqlDbType.Int);
                    cmd.Parameters.Add("@Producto", SqlDbType.VarChar, 50);
                    cmd.Parameters.Add("@Precio", SqlDbType.Decimal);
                    cmd.Parameters.Add("@NDisp", SqlDbType.Int);
                    cmd.Parameters.Add("@Corte", SqlDbType.Int);
                    cmd.Parameters.Add("@ChipDespachador", SqlDbType.Int);
                    cmd.Parameters.Add("@NomProducto", SqlDbType.VarChar, 50);
                    conn.Open();
                    int insertados = 0;

                    foreach (DataRow r in dtDatos.Rows)
                    {
                        pFecha.Value = ((DateTime)r["Fecha"]).Date; // <-- solo yyyy-MM-dd (00:00:00)

                        cmd.Parameters["@Volumen"].Value = (decimal)r["Volumen"];
                        cmd.Parameters["@Posicion"].Value = (int)r["Posicion"];
                        cmd.Parameters["@Manguera"].Value = (int)r["Manguera"];
                        cmd.Parameters["@Producto"].Value = (string)r["Producto"];
                        cmd.Parameters["@Precio"].Value = (decimal)r["Precio"];
                        cmd.Parameters["@NDisp"].Value = (int)r["NDisp"];
                        cmd.Parameters["@Corte"].Value = (int)r["Corte"];
                        cmd.Parameters["@ChipDespachador"].Value = (int)r["ChipDespachador"];
                        cmd.Parameters["@NomProducto"].Value = (string)r["NomProducto"];

                      

                        insertados += cmd.ExecuteNonQuery();
                    }
                    // Al terminar los inserts:
                    Session["lastSubmitToken"] = token;
                    Response.Redirect($"AgregaVentas.aspx?ok=1&n={insertados}", false);
                    Context.ApplicationInstance.CompleteRequest();

                    //ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                    //    $"Swal.fire('Listo', 'Se insertaron {insertados} registros en despachos.', 'success');", true);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Replace("'", " ");
                ScriptManager.RegisterStartupScript(this, GetType(), "swal",
                    $"Swal.fire('Error', '{msg}', 'error');", true);
            }
        }

        // ==== Helpers ===================================================

        private static DataTable ConstruirDataTableDestino()
        {
            var dt = new DataTable();
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Volumen", typeof(decimal));
            dt.Columns.Add("Posicion", typeof(int));
            dt.Columns.Add("Manguera", typeof(int));
            dt.Columns.Add("Producto", typeof(string)); // Cambia a int si tu columna SQL es numérica

            dt.Columns.Add("Precio", typeof(decimal));
            dt.Columns.Add("Ndisp", typeof(int));
            dt.Columns.Add("Corte", typeof(int));
            dt.Columns.Add("ChipDespachador", typeof(int));
            dt.Columns.Add("NomProducto", typeof(string)); // Cambia a int si tu columna SQL es numérica
            dt.Columns.Add("IdEstacion", typeof(int));
            return dt;

        }

        private static bool EsVacia(DataRow r)
        {
            object f = r.Table.Columns.Contains("Fecha") ? r["Fecha"] : null;
            object v = r.Table.Columns.Contains("Volumen") ? r["Volumen"] : null;

            bool fechaVacia = f == null || f == DBNull.Value || string.IsNullOrWhiteSpace(f.ToString());
            bool volVacio = v == null || v == DBNull.Value || string.IsNullOrWhiteSpace(v.ToString());

            return fechaVacia && volVacio;
        }

    //    private static DateTime ParseFecha(object val)
    //    {
    //        if (val == null || val == DBNull.Value)
    //            throw new Exception("Fecha vacía.");

    //        // Si Excel ya la entrega como DateTime
    //        if (val is DateTime dtNet) return dtNet.Date;

    //        // Si Excel trae número de serie (OADate)
    //        if (val is double d) return DateTime.FromOADate(d).Date;

    //        string s = val.ToString().Trim();

    //        // Formatos preferidos: AÑO/MES/DÍA
    //        string[] formatos = new[]
    //        {
    //    "yyyy/MM/dd",
    //    "yyyy-MM-dd",
    //    "yyyy/M/d",
    //    "yyyy-M-d",
    //    "yyyy/MM/dd HH:mm",
    //    "yyyy/MM/dd HH:mm:ss",
    //    "yyyy-MM-dd HH:mm",
    //    "yyyy-MM-dd HH:mm:ss",

    //    // De respaldo si te llega DÍA/MES/AÑO
    //    "dd/MM/yyyy",
    //    "d/M/yyyy",
    //    "dd/MM/yyyy HH:mm",
    //    "dd/MM/yyyy HH:mm:ss",
    //    "d/M/yyyy HH:mm",
    //    "d/M/yyyy HH:mm:ss"
    //};

    //        if (DateTime.TryParseExact(
    //            s, formatos,
    //            CultureInfo.InvariantCulture,
    //            DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces,
    //            out var parsed))
    //        {
    //            return parsed.Date; // Solo fecha
    //        }

    //        // Último recurso: parseo cultural (es-MX)
    //        if (DateTime.TryParse(s, new CultureInfo("es-MX"),
    //            DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out parsed))
    //            return parsed.Date;

    //        throw new Exception($"Fecha inválida: {s}. Se espera año/mes/día (yyyy/MM/dd).");
    //    }


        private static decimal ParseDecimal(object val)
        {
            if (val == null || val == DBNull.Value) return 0m;
            if (val is double d) return Convert.ToDecimal(d);

            string s = val.ToString().Trim();
            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("es-MX"), out var n)) return n;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out n)) return n;

            throw new Exception($"Volumen inválido: {s}");
        }

        private static int ParseInt(object val)
        {
            if (val == null || val == DBNull.Value) return 0;
            if (val is double d) return Convert.ToInt32(d);

            string s = val.ToString().Trim();
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var n)) return n;

            throw new Exception($"Entero inválido: {s}");
        }
    }
}
