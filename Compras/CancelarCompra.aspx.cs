using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftwarePlantas.Compras
{
    public partial class CancelarCompra : System.Web.UI.Page
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
                Response.Redirect("~/Login.aspx");
                return;
            }

            lblWelcome.Text = $"Bienvenido, {Session["Nombre"].ToString()}!";

            if (!IsPostBack)
            {
                CargarEstaciones();

                txtFechaInicial.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtFechaFinal.Text = DateTime.Now.ToString("yyyy-MM-dd");

                // Cargar catálogo para el modal (si ya hay instancia seleccionada, se llenará)
                // Si aún no hay instancia, se vuelve a cargar cuando se seleccione estación.
                CargarProveedoresModalSafe();
            }

            // Eliminar por __doPostBack
            if (Request["__EVENTTARGET"] == "btnEliminar" && Request["__EVENTARGUMENT"] != null)
            {
                string folio = Request["__EVENTARGUMENT"];
                EliminarRecepcion(folio);
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

            string connectionString = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            // Consulta para obtener el valor de 'instancia' usando el ID de la estación seleccionada
            string query = "SELECT instancia FROM estaciones WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", estacionId);
                connection.Open();
                instancia = command.ExecuteScalar()?.ToString();
            }

            if (string.IsNullOrEmpty(instancia))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    "alert", "Swal.fire('Error', 'Selecciona una Estacion.', 'error');", true);
                return;
            }

            // Construye la cadena de conexión de la instancia seleccionada
            string instanciaConnectionString = $"Server={instancia};Database=Admingas;User Id=sa;Password=$Aes220213Nu4&;";

            if (VerificarConexionBaseDatos(instanciaConnectionString))
            {
                Session["instanciaSeleccionada"] = instanciaConnectionString;

                // Cargar proveedores para el modal ahora que ya hay instancia
                CargarProveedoresModalSafe();

                // Recargar datos
                btnBuscar_Click(null, EventArgs.Empty);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(),
                    "alert", "Swal.fire('Error', 'No se pudo establecer conexión con la base de datos.', 'error');", true);
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            string fechaInicio = txtFechaInicial.Text;
            string fechaFin = txtFechaFinal.Text;

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Instancia no seleccionada.', 'error');", true);
                return;
            }

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = @"
                    SELECT 
                        R.Folio,
                        R.Tanque,
                        R.Producto,
                        R.VolumenRecepcion,
                        RCAP.FechaDocumento,
                        RCAP.VolumenPemex AS VolumenCapturado,
                        RCAP.FolioDocumento,
                        RCAP.UUID,
                        RCAP.ClaveVehiculo,
                        RCAP.TransCRE,
                        RCAP.PrecioCompra,
                        TRIM(RCAP.RFCProveedor) AS RFCProveedor,
                        RCAP.Id,
                        RCAP.IdProveedor
                    FROM Recepciones R 
                    INNER JOIN Recepcionescap RCAP ON R.Folio = RCAP.Folio
                    WHERE CAST(RCAP.FechaDocumento AS DATE) BETWEEN @Inicio AND @Fin";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Inicio", fechaInicio);
                cmd.Parameters.AddWithValue("@Fin", fechaFin);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            gvRecepciones.DataSource = dt;
            gvRecepciones.DataBind();
            pnlResultados.Visible = true;
        }

        private void EliminarRecepcion(string folio)
        {
            if (string.IsNullOrEmpty(folio))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'No se recibió el Folio.', 'error');", true);
                return;
            }

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Instancia no seleccionada.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // Actualizar Recepciones usando Folio
                    string updateRecepcion = @"
                        UPDATE Recepciones
                        SET Estado = 0,
                            UUID='',
                            TransCRE='',
                            ClaveVehiculo='',
                            VolumenPemex=0,
                            FolioDocumento=0,
                            FechaDocumento=NULL,
                            TipoDocumento='',
                            TeminalAlmacenamiento=''
                        WHERE Folio = @Folio";

                    using (SqlCommand cmd1 = new SqlCommand(updateRecepcion, conn, trans))
                    {
                        cmd1.Parameters.AddWithValue("@Folio", folio);
                        cmd1.ExecuteNonQuery();
                    }

                    // Eliminar de RecepcionesCap usando Folio
                    string deleteRelacionada = "DELETE FROM Recepcionescap WHERE Folio = @Folio";
                    using (SqlCommand cmd2 = new SqlCommand(deleteRelacionada, conn, trans))
                    {
                        cmd2.Parameters.AddWithValue("@Folio", folio);
                        cmd2.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "error",
                        $"Swal.fire('Error', 'Error al cancelar: {EscapeJs(ex.Message)}', 'error');", true);
                    return;
                }
            }

            ScriptManager.RegisterStartupScript(this, GetType(), "exito",
                $"Swal.fire('Eliminado', 'Folio {EscapeJs(folio)} cancelado correctamente.', 'success');", true);

            btnBuscar_Click(null, null);
        }

        // ============================
        // PROVEEDORES (MODAL)
        // ============================

        private void CargarProveedoresModalSafe()
        {
            // Si todavía no hay instancia seleccionada, no intentes cargar proveedores.
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            if (string.IsNullOrEmpty(instanciaConnectionString)) return;

            DataTable proveedores = ObtenerProveedores();

            ddlProveedorModal.DataSource = proveedores;
            ddlProveedorModal.DataTextField = "Nombre";
            ddlProveedorModal.DataValueField = "IdProveedor"; // Cambiado de RFC a IdProveedor
            ddlProveedorModal.DataBind();

            ddlProveedorModal.Items.Insert(0, new ListItem("Seleccione...", ""));
        }

        private DataTable ObtenerProveedores()
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = "SELECT TRIM(RFC) AS RFC, Nombre,IdProveedor FROM ProveedCombust ORDER BY Nombre";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        // ============================
        // ABRIR MODAL DESDE GRIDVIEW
        // ============================

        protected void gvRecepciones_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenEditModal")
            {
                try
                {
                    string id = e.CommandArgument.ToString();
                    string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

                    DataTable dt = new DataTable();
                    using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
                    {
                        string query = @"
                            SELECT 
                                RCAP.Id,
                                RCAP.FechaDocumento,
                                RCAP.VolumenPemex AS VolumenCapturado,
                                RCAP.FolioDocumento,
                                RCAP.UUID,
                                RCAP.ClaveVehiculo,
                                RCAP.TransCRE,
                                RCAP.PrecioCompra,
                                RTRIM(RCAP.RFCProveedor) AS RFCProveedor,
                                RCAP.IdProveedor
                            FROM Recepcionescap RCAP
                            WHERE RCAP.Id = @Id";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", id);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        
                        // Guardar el Id
                        hfId.Value = id;
                        
                        // Cargar proveedores
                        ddlProveedorModal.DataSource = ObtenerProveedores();
                        ddlProveedorModal.DataTextField = "Nombre";
                        ddlProveedorModal.DataValueField = "IdProveedor"; // Cambiado
                        ddlProveedorModal.DataBind();
                        
                        // Formatear fecha
                        string fechaDoc = row["FechaDocumento"] != DBNull.Value 
                            ? Convert.ToDateTime(row["FechaDocumento"]).ToString("yyyy-MM-dd") 
                            : "";
                        
                        string idProveedor = row["IdProveedor"] != DBNull.Value 
                            ? row["IdProveedor"].ToString() 
                            : "";
                        
                        // Escapar valores para JavaScript
                        string script = string.Format(@"
                            setTimeout(function() {{
                                abrirModalEditar({{
                                    fechaDocumento: '{0}',
                                    volumenCapturado: '{1}',
                                    folioDocumento: '{2}',
                                    uuid: '{3}',
                                    claveVehiculo: '{4}',
                                    transCRE: '{5}',
                                    precioCompra: '{6}',
                                    rfcProveedor: '{7}',
                                    idProveedor: '{8}'
                                }});
                            }}, 100);
                        ", 
                            fechaDoc,
                            row["VolumenCapturado"].ToString().Replace("'", "\\'"),
                            row["FolioDocumento"].ToString().Replace("'", "\\'"),
                            row["UUID"].ToString().Replace("'", "\\'"),
                            row["ClaveVehiculo"].ToString().Replace("'", "\\'"),
                            row["TransCRE"].ToString().Replace("'", "\\'"),
                            row["PrecioCompra"].ToString().Replace("'", "\\'"),
                            row["RFCProveedor"].ToString().Replace("'", "\\'"),
                            idProveedor
                        );
                        
                        ScriptManager.RegisterStartupScript(this, GetType(), "openModal_" + id, script, true);
                    }
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "error",
                        $"Swal.fire('Error', 'Error al abrir el modal: {ex.Message.Replace("'", "\\'")}', 'error');", true);
                }
            }
        }

        private DataRow ObtenerRecepcionPorId(int id)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = @"
                    SELECT
                        RCAP.Id,
                        RCAP.FechaDocumento,
                        RCAP.VolumenPemex AS VolumenCapturado,
                        RCAP.FolioDocumento,
                        RCAP.UUID,
                        RCAP.ClaveVehiculo,
                        RCAP.TransCRE,
                        RCAP.PrecioCompra,
                        TRIM(RCAP.RFCProveedor) AS RFCProveedor
                    FROM Recepcionescap RCAP
                    WHERE RCAP.Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0) return null;
                    return dt.Rows[0];
                }
            }
        }

        // ============================
        // GUARDAR DESDE MODAL
        // ============================

        protected void btnGuardarModal_Click(object sender, EventArgs e)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Instancia no seleccionada.', 'error');", true);
                return;
            }

            if (!int.TryParse(hfId.Value, out int id))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Id inválido.', 'error');", true);
                return;
            }

            // Fecha
            DateTime? fechaDocumento = null;
            if (!string.IsNullOrWhiteSpace(hfFechaDocumento.Value))
            {
                if (DateTime.TryParseExact(hfFechaDocumento.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime f))
                {
                    fechaDocumento = f;
                }
            }

            // Resto de campos
            string volumenCapturado = hfVolumenCapturado.Value;
            string folioDoc = hfFolioDocumento.Value;
            string uuid = hfUUID.Value;
            string claveVehiculo = hfClaveVehiculo.Value;
            string transCRE = hfTransCRE.Value;
            string precioCompra = hfPrecioCompra.Value;
            
            // Obtener IdProveedor y RFC del proveedor seleccionado
            int? idProveedor = null;
            string rfcProveedor = "";
            
            if (!string.IsNullOrWhiteSpace(hfIdProveedor.Value) && int.TryParse(hfIdProveedor.Value, out int idProv))
            {
                idProveedor = idProv;
                
                // Consultar el RFC correspondiente al IdProveedor seleccionado
                using (SqlConnection connRFC = new SqlConnection(instanciaConnectionString))
                {
                    string queryRFC = "SELECT TRIM(RFC) FROM ProveedCombust WHERE IdProveedor = @IdProveedor";
                    SqlCommand cmdRFC = new SqlCommand(queryRFC, connRFC);
                    cmdRFC.Parameters.AddWithValue("@IdProveedor", idProveedor.Value);
                    
                    connRFC.Open();
                    object resultRFC = cmdRFC.ExecuteScalar();
                    if (resultRFC != null)
                    {
                        rfcProveedor = resultRFC.ToString().Trim().ToUpperInvariant();
                    }
                }
            }

            // Actualizar en la base de datos
            using (SqlConnection con = new SqlConnection(instanciaConnectionString))
            {
                con.Open();

                string query = @"
                    UPDATE RecepcionesCap
                    SET FechaDocumento = @Fecha,
                        VolumenPemex = @VolumenCapturado,
                        FolioDocumento = @FolioDocumento,
                        UUID = @UUID,
                        ClaveVehiculo = @ClaveVehiculo,
                        TransCRE = @TransCRE,
                        PrecioCompra = @PrecioCompra,
                        RFCProveedor = @RFCProveedor,
                        IdProveedor = @IdProveedor
                    WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Fecha puede ser null
                    if (fechaDocumento.HasValue)
                        cmd.Parameters.AddWithValue("@Fecha", fechaDocumento.Value);
                    else
                        cmd.Parameters.AddWithValue("@Fecha", DBNull.Value);

                    cmd.Parameters.AddWithValue("@VolumenCapturado", volumenCapturado);
                    cmd.Parameters.AddWithValue("@FolioDocumento", folioDoc);
                    cmd.Parameters.AddWithValue("@UUID", uuid);
                    cmd.Parameters.AddWithValue("@ClaveVehiculo", claveVehiculo);
                    cmd.Parameters.AddWithValue("@TransCRE", transCRE);
                    cmd.Parameters.AddWithValue("@PrecioCompra", precioCompra);
                    
                    // RFC e IdProveedor obtenidos dinámicamente
                    cmd.Parameters.AddWithValue("@RFCProveedor", string.IsNullOrEmpty(rfcProveedor) ? (object)DBNull.Value : rfcProveedor);
                    
                    if (idProveedor.HasValue)
                        cmd.Parameters.AddWithValue("@IdProveedor", idProveedor.Value);
                    else
                        cmd.Parameters.AddWithValue("@IdProveedor", DBNull.Value);
        
                    cmd.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    
                    if (rowsAffected == 0)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "warning",
                            "Swal.fire('Advertencia', 'No se actualizó ningún registro.', 'warning');", true);
                        return;
                    }
                }
            }

            // Refrescar grid
            btnBuscar_Click(null, null);

            ScriptManager.RegisterStartupScript(this, GetType(), "ok",
                "Swal.fire('Guardado', 'Se actualizaron los datos correctamente.', 'success');", true);
        }

        // ============================
        // UTILIDAD
        // ============================

        private string EscapeJs(string s)
        {
            return (s ?? "")
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", "");
        }

        protected string ObtenerNombreProveedor(string rfc)
        {
            if (string.IsNullOrEmpty(rfc))
                return "";

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            string nombre = "";

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = "SELECT Nombre FROM ProveedCombust WHERE TRIM(RFC) = @RFC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RFC", (rfc ?? "").Trim());

                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                    nombre = result.ToString();
            }

            return nombre;
        }

        protected string ObtenerNombreProveedorPorId(object idProveedorObj)
        {
            if (idProveedorObj == null || idProveedorObj == DBNull.Value)
                return "";

            if (!int.TryParse(idProveedorObj.ToString(), out int idProveedor))
                return "";

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            if (string.IsNullOrEmpty(instanciaConnectionString))
                return "";

            string nombre = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
                {
                    string query = "SELECT Nombre FROM ProveedCombust WHERE IdProveedor = @IdProveedor";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@IdProveedor", idProveedor);

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        nombre = result.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener nombre del proveedor: {ex.Message}");
            }

            return nombre;
        }
    }
}
