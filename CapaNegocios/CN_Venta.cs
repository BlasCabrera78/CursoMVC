using CapaDatos;
using CapaEntidades;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace CapaNegocios
{
    public class CN_Venta
    {
        private CD_Venta objCapaDato = new CD_Venta();

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            return objCapaDato.Registrar(obj, DetalleVenta, out Mensaje);
        }

        public List<DetalleVenta> ListarCompras(int idcliente)
        {
            return ojsnDatos.ListarCompras(idcliente);
        }
    }
}
