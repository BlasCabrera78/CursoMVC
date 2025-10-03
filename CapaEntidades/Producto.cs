using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidades
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public Marca oMarca { get; set; }
        public Categoria oCategoria { get; set; }
        public decimal Precio { get; set; }
        public string PrecioTexto { get; set; }
        public int Stock { get; set; }
        public String RutaImagen { get; set; }
        public String NombreImagen { get; set; }
        public bool Activo { get; set; }
        public string base64 { get; set; }
        public string Extencion { get; set; }

        
    }
}
