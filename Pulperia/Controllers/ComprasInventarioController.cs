using Pulperia.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    public class ComprasInventarioController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AjusteInventario()
        {
            //ejecutar el cierre mensual
            db.SP_AjusteInventario();

            //redireccionar a una pagina donde se vea el cierre
            return RedirectToAction("Index", "HistoricoComprasInventarios", new { ajusteEjecutado = true });
        }

        [HttpPost]
        public ActionResult ObtenerPrecioProducto(int idProducto)
        {
            var precio =  db.Productos.Where(p => p.Id == idProducto).Select(p => p.PrecioCompraIndividual).FirstOrDefault();
            return Json(new { success = true, precioProducto = precio }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            if (User.IsInRole("Asociado"))
                return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);

            List<ComprasInventario> comprasInventarios = db.ComprasInventario.Include(c => c.Productos).ToList();

            //Add a Dummy Row.
            comprasInventarios.Insert(0, new ComprasInventario());

            ViewBag.Productos = new SelectList(db.Productos.OrderBy(o => o.Nombre).ToList(), "Id", "Nombre");
            return View(comprasInventarios);
        }

        [HttpPost]
        public string InsertComprasInventario(ComprasInventario comprasInventario)
        {
            db.ComprasInventario.Add(comprasInventario);
            db.SaveChanges();
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(comprasInventario);
            return result;
        }

        [HttpPost]
        public ActionResult UpdateComprasInventario(ComprasInventario comprasInventario)
        {
            ComprasInventario updatedCI = (from c in db.ComprasInventario
                                           where c.Id == comprasInventario.Id
                                           select c).FirstOrDefault();

            updatedCI.IdProducto = updatedCI.IdProducto;
            if(!string.IsNullOrEmpty(comprasInventario.Lote))
                updatedCI.Lote = comprasInventario.Lote;
            if(comprasInventario.PrecioIndividual.HasValue)
                updatedCI.PrecioIndividual = comprasInventario.PrecioIndividual;
            if(comprasInventario.FechaCompra.HasValue)
                updatedCI.FechaCompra = comprasInventario.FechaCompra;
            if(comprasInventario.FechaVencimiento.HasValue)
                updatedCI.FechaVencimiento = comprasInventario.FechaVencimiento;
            if(comprasInventario.CantidadComprada.HasValue)
                updatedCI.CantidadComprada = comprasInventario.CantidadComprada;

            db.SaveChanges();

            return Json(new { success = true, responseText = "Se ha actualizado la compra" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteComprasInventario(int ciID)
        {
            ComprasInventario customer = (from c in db.ComprasInventario
                                          where c.Id == ciID
                                          select c).FirstOrDefault();
            db.ComprasInventario.Remove(customer);
            db.SaveChanges();

            return Json(new { success = true, responseText = "Se ha borrado la compra" }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
