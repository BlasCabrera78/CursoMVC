using CapaEntidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Cliente
    {
        public List<Cliente> Listar()
        {
            List<Cliente> lista = new List<Cliente>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    string query = "select IdCliente,Nombre,Apellidos,Correo,Clave,Reestablecer from CLIENTE";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(
                                new Cliente()
                                {
                                    IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                    Nombre = dr["Nombre"].ToString(),
                                    Apellidos = dr["Apellidos"].ToString(),
                                    Correo = dr["Correo"].ToString(),
                                    Clave = dr["Clave"].ToString(),
                                    Reestablecer = Convert.ToBoolean(dr["Reestablecer"])
                                });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Cliente>();
            }

            return lista;
        }

        public int Registrar(Cliente obj, out string Mensaje)
        {
            int idautogenerado = 0;

            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarCliente", oconexion);
                    cmd.Parameters.AddWithValue("Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("Apellidos", obj.Apellidos);
                    cmd.Parameters.AddWithValue("Correo", obj.Correo);
                    cmd.Parameters.AddWithValue("Clave", obj.Clave);
                    cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    idautogenerado = Convert.ToInt32(cmd.Parameters["Resultado"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                idautogenerado = 0;
                Mensaje += ex.Message;
            }

            return idautogenerado;
        }
        public bool CambiarClave(int idCliente, string nuevaclave, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("update cliente set clave  = @nuevaclave, reestablecer = 0 where idcliente = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", idCliente);
                    cmd.Parameters.AddWithValue("@nuevaclave", nuevaclave);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje += ex.Message;
            }

            return resultado;

        }

        public bool ReestablecerClave(int idCliente, string clave, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("update CLIENTE set clave  = @clave, reestablecer = 1 where IdCliente = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", idCliente);
                    cmd.Parameters.AddWithValue("@clave", clave);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje += ex.Message;
            }

            return resultado;

        }

        //public bool Editar(Usuario obj, out string Mensaje)
        //{
        //    bool resultado = false;
        //    Mensaje = String.Empty;

        //    try
        //    {
        //        using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
        //        {
        //            SqlCommand cmd = new SqlCommand("sp_EditarUsuario", oconexion);
        //            cmd.Parameters.AddWithValue("IdUsuario", obj.IdUsuario);
        //            cmd.Parameters.AddWithValue("Nombre", obj.Nombre);
        //            cmd.Parameters.AddWithValue("Apellidos", obj.Apellidos);
        //            cmd.Parameters.AddWithValue("Correo", obj.Correo);
        //            cmd.Parameters.AddWithValue("Activo", obj.Activo);
        //            cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
        //            cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            oconexion.Open();
        //            cmd.ExecuteNonQuery();

        //            resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
        //            Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resultado = false;
        //        Mensaje += ex.Message;
        //    }

        //    return resultado;
        //}

        //    public bool Eliminar(int id, out string Mensaje)
        //    {
        //        bool resultado = false;
        //        Mensaje = string.Empty;

        //        try
        //        {
        //            using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
        //            {
        //                SqlCommand cmd = new SqlCommand("delete top (1) from usuario where IdUsuario = @id", oconexion);
        //                cmd.Parameters.AddWithValue("@id", id);
        //                cmd.CommandType = CommandType.Text;
        //                oconexion.Open();
        //                resultado = cmd.ExecuteNonQuery() > 0 ? true : false;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            resultado = false;
        //            Mensaje += ex.Message;
        //        }

        //        return resultado;

        //    }

        //}
    }
}
