using CapaEntidades;
using CapaNegocios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CapaPresentacionAdmin.Controllers
{
    public class MantenedorController : Controller
    {
        // GET: Mantenedor
        public ActionResult Categoria()
        {
            return View();
        }

        public ActionResult Marca()
        {
            return View();
        }

        public ActionResult Producto()
        {
            return View();
        }

        //Categoria
        #region Categoria

        [HttpGet]
        public JsonResult ListarCategoria()
        {
            List<Categoria> oLista = new List<Categoria>();

            oLista = new CN_Categoria().Listar();

            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarCategoria(Categoria obj)
        {
            object resultado;
            string mensaje = string.Empty;

            if (obj.IdCategoria == 0)
            {
                resultado = new CN_Categoria().Registrar(obj, out mensaje);
            }
            else
            {
                resultado = new CN_Categoria().Editar(obj, out mensaje);
            }

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarCategoria(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new CN_Categoria().Eliminar(id, out mensaje);

            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Marca
        #region MARCA

        [HttpGet]
        public JsonResult ListarMarca()
        {
            List<Marca> oLista = new List<Marca>();

            oLista = new CN_Marca().Listar();

            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarMarca(Marca obj)
        {
            object resultado;
            string mensaje = string.Empty;

            if (obj.IdMarca == 0)
            {
                resultado = new CN_Marca().Registrar(obj, out mensaje);
            }
            else
            {
                resultado = new CN_Marca().Editar(obj, out mensaje);
            }

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarMarca(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new CN_Marca().Eliminar(id, out mensaje);

            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Producto 
        #region Producto 

        [HttpGet]
        public JsonResult ListarProducto()
        {
            try
            {
                var lista = new CN_Producto().Listar();
                return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log ex
                return Json(new { data = new List<Producto>() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GuardarProducto(string obj, HttpPostedFileBase archivoImagen)
        {
            // Respuesta uniforme
            bool ok = false;
            int id = 0;
            string mensaje = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(obj))
                    return Json(new { ok = false, mensaje = "Objeto vacío" }, JsonRequestBehavior.AllowGet);

                var oProducto = JsonConvert.DeserializeObject<Producto>(obj);
                if (oProducto == null)
                    return Json(new { ok = false, mensaje = "Producto inválido" }, JsonRequestBehavior.AllowGet);

                // Normaliza precio: acepta "12,4" y "12.4"
                if (!string.IsNullOrWhiteSpace(oProducto.PrecioTexto))
                {
                    var txt = oProducto.PrecioTexto.Replace(',', '.');
                    if (decimal.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio))
                    {
                        oProducto.Precio = precio;
                    }
                    else
                    {
                        return Json(new { ok = false, mensaje = "Precio inválido" }, JsonRequestBehavior.AllowGet);
                    }
                }

                var cn = new CN_Producto();

                if (oProducto.IdProducto == 0)
                {
                    int idGenerado = cn.Registrar(oProducto, out mensaje);
                    if (idGenerado != 0)
                    {
                        oProducto.IdProducto = idGenerado;
                        id = idGenerado;
                        ok = true;
                    }
                    else
                    {
                        ok = false;
                        // mensaje ya viene del SP
                    }
                }
                else
                {
                    ok = cn.Editar(oProducto, out mensaje);
                    id = oProducto.IdProducto;
                }

                // Si el producto se guardó/actualizó, procesa imagen si llegó archivo
                if (ok && archivoImagen != null && archivoImagen.ContentLength > 0)
                {
                    string contentType = archivoImagen.ContentType?.ToLowerInvariant();
                    if (contentType != "image/png" && contentType != "image/jpeg")
                    {
                        return Json(new { ok = false, mensaje = "Formato de imagen no permitido (PNG o JPG)" }, JsonRequestBehavior.AllowGet);
                    }

                    // Carpeta física desde Web.config: <add key="ServidorFotos" value="C:\inetpub\wwwroot\MiTienda\Uploads\Productos" />
                    string rutaGuardar = ConfigurationManager.AppSettings["ServidorFotos"];
                    if (string.IsNullOrWhiteSpace(rutaGuardar))
                        return Json(new { ok = false, mensaje = "Ruta de imágenes no configurada (ServidorFotos)" }, JsonRequestBehavior.AllowGet);

                    if (!Directory.Exists(rutaGuardar))
                    {
                        try { Directory.CreateDirectory(rutaGuardar); }
                        catch (Exception exDir)
                        {
                            // Log exDir
                            return Json(new { ok = false, mensaje = "No se pudo crear carpeta de imágenes" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    string ext = Path.GetExtension(archivoImagen.FileName)?.ToLowerInvariant();
                    // Normaliza extensión por MIME
                    if (contentType == "image/jpeg" && (ext != ".jpg" && ext != ".jpeg")) ext = ".jpg";
                    if (contentType == "image/png" && ext != ".png") ext = ".png";

                    string nombreImagen = $"prod_{oProducto.IdProducto}{ext}";
                    string rutaFisica = Path.Combine(rutaGuardar, nombreImagen);

                    try
                    {
                        // Sobrescribe si existía
                        archivoImagen.SaveAs(rutaFisica);

                        // Actualiza en BD ruta/nombre
                        oProducto.RutaImagen = rutaGuardar;
                        oProducto.NombreImagen = nombreImagen;

                        bool imgOk = cn.GuardarDatosImagen(oProducto, out var msgImg);
                        if (!imgOk)
                        {
                            return Json(new { ok = false, mensaje = string.IsNullOrWhiteSpace(msgImg) ? "No se pudo actualizar imagen" : msgImg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception exSave)
                    {
                        // Log exSave
                        return Json(new { ok = false, mensaje = "No se pudo guardar la imagen en el servidor" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { ok = ok, id = id, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log ex
                return Json(new { ok = false, mensaje = "Error interno al guardar" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ImagenProducto(int id)
        {
            try
            {
                var producto = new CN_Producto().Listar().FirstOrDefault(p => p.IdProducto == id);

                if (producto == null)
                    return Json(new { conversion = false, mensaje = "Producto no encontrado" }, JsonRequestBehavior.AllowGet);

                var nombre = producto.NombreImagen ?? string.Empty;
                var rutaBase = producto.RutaImagen ?? string.Empty;

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(rutaBase))
                    return Json(new { conversion = false, mensaje = "Producto sin imagen" }, JsonRequestBehavior.AllowGet);

                var ruta = Path.Combine(rutaBase, nombre);
                if (!System.IO.File.Exists(ruta))
                    return Json(new { conversion = false, mensaje = "Archivo de imagen no encontrado" }, JsonRequestBehavior.AllowGet);

                bool conversion;
                string base64 = CN_Recursos.ConvertirBase64(ruta, out conversion);

                string ext = (Path.GetExtension(nombre) ?? string.Empty).ToLowerInvariant();
                string mime = ext == ".png" ? "png" : (ext == ".jpg" || ext == ".jpeg" ? "jpeg" : "png");

                return Json(new { conversion = conversion, textobase64 = base64, extencion = mime }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log ex
                return Json(new { conversion = false, mensaje = "Error interno al obtener imagen" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EliminarProducto(int id)
        {
            try
            {
                bool respuesta = new CN_Producto().Eliminar(id, out string mensaje);
                return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log ex
                return Json(new { respuesta = false, mensaje = "Error interno al eliminar" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

#endregion