using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocios
{
    public class CN_Cliente
    {
        private CD_Cliente objCapaDatos = new CD_Cliente();

        public List<Cliente> Listar()
        {
            return objCapaDatos.Listar();
        }

        public int Registrar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (string.IsNullOrEmpty(obj.Nombre) || string.IsNullOrWhiteSpace(obj.Nombre))
            {
                Mensaje = "El nombre de usuario no puede ser vacio";
            }
            else if (string.IsNullOrEmpty(obj.Apellidos) || string.IsNullOrWhiteSpace(obj.Apellidos))
            {
                Mensaje = "El Apellido de usuario no puede ser vacio";
            }
            else if (string.IsNullOrEmpty(obj.Correo) || string.IsNullOrWhiteSpace(obj.Correo))
            {
                Mensaje = "El Correo de usuario no puede ser vacio";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {
                string clave = CN_Recursos.GenerarClave();
                string asunto = "Creacion de Cuenta";
                string mensajeCorreo = "<h3>Su cuenta fue creada correctamente!</h3></br><p>Su clave para acceder es: !clave!</p>";
                mensajeCorreo = mensajeCorreo.Replace("!clave!", clave);

                bool respuesta = CN_Recursos.EnviarCorrero(obj.Correo, asunto, mensajeCorreo);

                if (respuesta)
                {
                    obj.Clave = CN_Recursos.ConvertirSha256(clave);

                    return objCapaDatos.Registrar(obj, out Mensaje);
                }
                else
                {
                    Mensaje = "No se puede enviar el correo";
                    return 0;
                }

            }
            else
            {
                return 0;
            }

        }

        public bool CambiarClave(int idCliente, string nuevaclave, out string Mensaje)
        {
            return objCapaDatos.CambiarClave(idCliente, nuevaclave, out Mensaje);
        }

        public bool ReestablecerClave(int idCliente, string correo, out string Mensaje)
        {
            Mensaje = string.Empty;
            string nuevaclave = CN_Recursos.GenerarClave();
            bool resultado = objCapaDatos.ReestablecerClave(idCliente, CN_Recursos.ConvertirSha256(nuevaclave), out Mensaje);

            if (resultado)
            {
                string asunto = "Clave Reestablecida";
                string mensajeCorreo = "<h3>Su cuenta fue creestablecida correctamente!</h3></br><p>Su clave para acceder es: !clave!</p>";
                mensajeCorreo = mensajeCorreo.Replace("!clave!", nuevaclave);

                bool respuesta = CN_Recursos.EnviarCorrero(correo, asunto, mensajeCorreo);
                if (!respuesta)
                {
                    Console.WriteLine("Fallo el envío del correo");
                }
                else
                {
                    Console.WriteLine("Correo enviado correctamente");
                }

                if (respuesta)
                {

                    return true;
                }
                else
                {
                    Mensaje = "No se pudo enviar";
                    return false;
                }
            }
            else
            {
                Mensaje = "No se pudo reestablecerr";
                return false;
            }

        }
    }
}
