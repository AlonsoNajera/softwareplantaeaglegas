using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;

namespace SoftwarePlantas
{



    public class DatabaseConnection
    {
        private string connectionString;
        // Obtén la cadena de conexión desde Web.config
        // Obtén la cadena de conexión desde Web.config


        public DatabaseConnection(string connectionName)
        {
            // Obtén la cadena de conexión desde Web.config
            connectionString = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
        }

        public void ExecuteQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true; // La conexión fue exitosa
                }
            }
            catch (Exception)
            {
                return false; // Hubo un error durante la conexión
            }
        }


        public string ExecuteQuery(string query, SqlParameter sqlParameter1, SqlParameter sqlParameter2)
        {
            string resultado = "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(connectionString))
                {
                    // Abre la conexión
                    conexion.Open();

                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        using (SqlDataReader reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Aquí puedes manejar los resultados obtenidos de la base de datos
                                string nombreUsuario = reader["Nombre"].ToString();
                                resultado += nombreUsuario + "<br />";
                            }
                        }
                    }
                }

                // Devuelve la información obtenida de la base de datos
                return resultado;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return "Error: " + ex.Message;
            }
        }
    }
}