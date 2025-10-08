using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocios
{
    public class CN_Reportes
    {
        private CD_Reporte objCapaDatos = new CD_Reporte();

        public DashBoard VerDashBoard()
        {
            return objCapaDatos.VerDashBoard();
        }
    }
}
