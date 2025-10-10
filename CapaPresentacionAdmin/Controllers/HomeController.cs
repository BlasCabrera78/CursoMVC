﻿using CapaEntidades;
using CapaNegocios;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CapaPresentacionAdmin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Usuarios()
        {
            return View();
        }

        [HttpGet]
        public JsonResult ListarUsuarios()
        {
            List<Usuario> oLista = new List<Usuario>();

            oLista = new CN_Usuarios().Listar();

            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarUsuarios(Usuario obj)
        {
            object resultado;
            string mensaje = string.Empty;

            if (obj.IdUsuario == 0)
            {
                resultado = new CN_Usuarios().Registrar(obj, out mensaje);
            }
            else
            {
                resultado = new CN_Usuarios().Editar(obj, out mensaje);
            }

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EliminarUsuarios(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new CN_Usuarios().Eliminar(id, out mensaje);

            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult ListaReporte(string fechainicio, string fechafin, string idtransaccion)
        {
            List<Reporte> oLista = new List<Reporte>();

            oLista = new CN_Reportes().Ventas(fechainicio, fechafin, idtransaccion);

            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult VistaDashBoard()
        {
            DashBoard objeto = new CN_Reportes().VerDashBoard();

            return Json(new { resultado = objeto }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public FileResult ExportarVenta(string fechainicio, string fechafin, string idtransaccion)
        {
            List<Reporte> oLista = new List<Reporte>();
            oLista = new CN_Reportes().Ventas(fechainicio, fechafin, idtransaccion);

            DataTable dt = new DataTable();

            dt.Locale = new System.Globalization.CultureInfo("es-PE");
            dt.Columns.Add("Fecha Venta", typeof(string));
            dt.Columns.Add("Cliente", typeof(string));
            dt.Columns.Add("Producto", typeof(string));
            dt.Columns.Add("Precio", typeof(decimal));
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("Total", typeof(decimal));
            dt.Columns.Add("IdTransaccion", typeof(string));

            foreach (Reporte rp in oLista)
            {
                dt.Rows.Add(new object[]
                {
                    rp.FechaVenta,
                    rp.Cliente,
                    rp.Producto,
                    rp.Precio,
                    rp.Cantidad,
                    rp.Total,
                    rp.IdTransaccion
                });
            }

            dt.TableName = "Datos";

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ReporteVenta" + DateTime.Now.ToString() + ".xlsx"
                    );
                }
            }
        }

    } 
}