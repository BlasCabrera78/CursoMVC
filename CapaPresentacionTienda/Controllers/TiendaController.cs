using CapaEntidades;
using CapaNegocios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CapaPresentacionTienda.Controllers
{
    public class TiendaController : Controller
    {
        // GET: Teinda
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DetalleProducto(int idproducto = 0)
        {
            bool conversion;
            var oProducto = new CN_Producto().Listar()
                              .FirstOrDefault(p => p.IdProducto == idproducto);

            if (oProducto != null)
            {
                oProducto.base64 = CN_Recursos.ConvertirBase64(
                    Path.Combine(oProducto.RutaImagen, oProducto.NombreImagen),
                    out conversion
                );
                oProducto.Extencion = Path.GetExtension(oProducto.NombreImagen); // ".png", ".jpg", etc.
            }

            return View(oProducto); // IMPORTANTE: pasar el modelo a la vista
        }

        [HttpGet]
        public JsonResult ListarCategorias()
        {
            List<Categoria> lista = new List<Categoria>();
            lista = new CN_Categoria().Listar();
            return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarMarcaporCategoria(int idcategoria)
        {
            List<Marca> lista = new List<Marca>();
            lista = new CN_Marca().ListarMarcaPorCategoria(idcategoria);
            return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarProducto(int idcategoria, int idmarca)
        {
            List<Producto> lista = new List<Producto>();

            bool conversion;

            lista = new CN_Producto().Listar().Select(p => new Producto()
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                oMarca = p.oMarca,
                oCategoria = p.oCategoria,
                Precio = p.Precio,
                Stock = p.Stock,
                RutaImagen = p.RutaImagen,
                base64 = CN_Recursos.ConvertirBase64(Path.Combine(p.RutaImagen, p.NombreImagen), out conversion),
                Extencion = Path.GetExtension(p.NombreImagen),
                Activo = p.Activo
            })
            .Where(p =>
                p.oCategoria.IdCategoria == (idcategoria == 0 ? p.oCategoria.IdCategoria : idcategoria) &&
                p.oMarca.IdMarca == (idmarca == 0 ? p.oMarca.IdMarca : idmarca) &&
                p.Stock > 0 && p.Activo == true
            )
            .ToList();

            var jsonresult = Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            jsonresult.MaxJsonLength = int.MaxValue;

            return jsonresult;
        }
    }
}