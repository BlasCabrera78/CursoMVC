using CapaDatos;
using CapaEntidades;
using CapaEntidades.Paypal;
using CapaNegocio;
using CapaNegocios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

// Alias para evitar conflictos entre capas
using Pay = CapaEntidades.Paypal;

namespace CapaPresentacionTienda.Controllers
{
    public class TiendaController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Carrito()
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

        [HttpPost]
        public JsonResult AgregarCarrito(int idproducto)
        {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;

            bool existe = new CN_Carrito().ExisteCarrito(idcliente, idproducto);

            bool respuesta = false;
            string mensaje = string.Empty;

            if (existe)
            {
                mensaje = "El producto ya existe en el carrito";
            }
            else
            {
                respuesta = new CN_Carrito().OperacionCarrito(idcliente, idproducto, true, out mensaje);
            }

            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CantidadEnCarrito()
        {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            int cantidad = new CN_Carrito().CantidadEnCarrito(idcliente);
            return Json(new { cantidad = cantidad }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarProductosCarrito()
        {

            int idCliente = ((Cliente)Session["Cliente"]).IdCliente;

            List<Carrito> oLista = new List<Carrito>();

            bool conversion;

            oLista = new CN_Carrito().ListarProducto(idCliente).Select(oc => new Carrito()
            {
                oProducto = new Producto()
                {
                    IdProducto = oc.oProducto.IdProducto,
                    Nombre = oc.oProducto.Nombre,
                    oMarca = oc.oProducto.oMarca,
                    Precio = oc.oProducto.Precio,
                    RutaImagen = oc.oProducto.RutaImagen,
                    base64 = CN_Recursos.ConvertirBase64(
                                    Path.Combine(oc.oProducto.RutaImagen, oc.oProducto.NombreImagen),
                                    out conversion),
                    Extencion = Path.GetExtension(oc.oProducto.NombreImagen)
                },
                Cantidad = oc.Cantidad
            }).ToList();

            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult OperacionCarrito(int idproducto, bool sumar)
        {
            try
            {
                int idcliente = ((Cliente)Session["Cliente"]).IdCliente;

                var cd = new CD_Carrito();
                bool ok = cd.OperacionCarrito(idcliente, idproducto, sumar, out string mensaje);

                return Json(new { respuesta = ok, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { respuesta = false, mensaje = "Error interno." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EliminarDeCarrito(int idproducto)
        {
            try
            {
                int idcliente = ((Cliente)Session["Cliente"]).IdCliente;

                var cd = new CD_Carrito();
                bool ok = cd.EliminarCarrito(idcliente, idproducto);

                return Json(new { respuesta = ok }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { respuesta = false, mensaje = "Error interno." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ObtenerDepartamento()
        {

            List<Departamento> oLista = new List<Departamento>();

            oLista = new CN_Ubicacion().ObtenerDepartamento();

            return Json(new { lista = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerProvincia(string IdDepartamento)
        {
            List<Provincia> oLista = new List<Provincia>();

            oLista = new CN_Ubicacion().ObtenerProvincia(IdDepartamento);

            return Json(new { lista = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerDistrito(string IdDepartamento, string IdProvincia)
        {
            List<Distrito> oLista = new List<Distrito>();

            oLista = new CN_Ubicacion().ObtenerDistrito(IdDepartamento, IdProvincia);

            return Json(new { lista = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ProcesarPago(List<Carrito> oListaCarrito, Venta oVenta)
        {
            decimal total = 0m;

            // Crear DataTable para el detalle de la venta
            var detalle_venta = new DataTable { Locale = new CultureInfo("es-PE") };
            detalle_venta.Columns.Add("IdProducto", typeof(string));
            detalle_venta.Columns.Add("Cantidad", typeof(int));
            detalle_venta.Columns.Add("Total", typeof(decimal));

            // Lista de items PayPal
            var oListaItem = new List<Pay.Item>();

            foreach (var oCarrito in oListaCarrito)
            {
                decimal subtotal = Convert.ToDecimal(oCarrito.Cantidad) * oCarrito.oProducto.Precio;
                total += subtotal;

                oListaItem.Add(new Pay.Item
                {
                    name = oCarrito.oProducto.Nombre,
                    quantity = oCarrito.Cantidad.ToString(),
                    unit_amount = new Pay.UnitAmount
                    {
                        currency_code = "USD",
                        value = oCarrito.oProducto.Precio.ToString("G", new CultureInfo("es-PE"))
                    }
                });

                detalle_venta.Rows.Add(new object[]
                {
            oCarrito.oProducto.IdProducto,
            oCarrito.Cantidad,
            subtotal
                });
            }

            // Crear la unidad de compra (PurchaseUnit)
            var purchaseUnit = new Pay.PurchaseUnit
            {
                amount = new Pay.Amount
                {
                    currency_code = "USD",
                    value = total.ToString("G", new CultureInfo("es-PE")),
                    breakdown = new Pay.Breakdown
                    {
                        item_total = new Pay.ItemTotal
                        {
                            currency_code = "USD",
                            value = total.ToString("G", new CultureInfo("es-PE"))
                        }
                    }
                },
                description = "compra de articulo de mi tienda",
                items = oListaItem
            };

            // Crear la orden para PayPal
            var oCheckOutOrder = new Pay.Checkout_Order
            {
                intent = "CAPTURE",
                purchase_units = new List<Pay.PurchaseUnit> { purchaseUnit },
                application_context = new Pay.ApplicationContext
                {
                    brand_name = "MiTienda.com",
                    landing_page = "NO_PREFERENCE",
                    user_action = "PAY_NOW",
                    return_url = "https://localhost:44303/Tienda/PagoEfectuado",
                    cancel_url = "https://localhost:44303/Tienda/Carrito"
                }
            };

            // Datos de la venta
            oVenta.MontoTotal = total;
            oVenta.IdCliente = ((Cliente)Session["Cliente"]).IdCliente;

            // Mantener datos temporales para cuando PayPal regrese
            TempData["Venta"] = oVenta;
            TempData["DetalleVenta"] = detalle_venta;

            // Crear la orden en PayPal
            var opaypal = new CN_Paypal();
            CapaNegocio.Response_Paypal<Pay.Response_Checkout> response_paypal =
                await opaypal.CrearSolicitud(oCheckOutOrder);

            // Retornar la respuesta JSON con los datos de PayPal
            return Json(response_paypal, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<ActionResult> PagoEfectuado()
        {
            string token = Request.QueryString["token"];

            var opaypal = new CN_Paypal();
            var response_paypal = new CapaNegocio.Response_Paypal<Pay.Response_Capture>();

            // Captura del pago en PayPal
            response_paypal = await opaypal.AprobarPago(token);

            ViewData["Status"] = response_paypal.Status;

            if (response_paypal.Status)
            {
                // Recuperar datos de la venta almacenados en TempData
                Venta oVenta = (Venta)TempData["Venta"];
                DataTable detalle_venta = (DataTable)TempData["DetalleVenta"];

                // Guardar el ID de transacción devuelto por PayPal
                oVenta.IdTransaccion = response_paypal.Response.purchase_units[0].payments.captures[0].id;

                string mensaje = string.Empty;
                bool respuesta = new CN_Venta().Registrar(oVenta, detalle_venta, out mensaje);

                ViewData["IdTransaccion"] = oVenta.IdTransaccion;
            }

            return View();
        }

        public ActionResult MisCompras()
        {
            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;

            List<DetalleVenta> oLista = new List<DetalleVenta>();
            bool conversion;

            oLista = new CN_Venta()
                .ListarCompras(idcliente)
                .Select(oc => new DetalleVenta()
                {
                    oProducto = new Producto()
                    {
                        Nombre = oc.oProducto.Nombre,
                        Precio = oc.oProducto.Precio,
                        base64 = CN_Recursos.ConvertirBase64(
                                      Path.Combine(oc.oProducto.RutaImagen, oc.oProducto.NombreImagen),
                                      out conversion),
                        Extencion = Path.GetExtension(oc.oProducto.NombreImagen)
                    },
                    Cantidad = oc.Cantidad,
                    Total = oc.Total,
                    IdTransaccion = oc.IdTransaccion
                })
                .ToList();

            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
            // Si en tu vista devuelves View:
            // return View(oLista);
        }

    }
}