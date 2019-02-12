using Pulperia.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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


        //// GET: ComprasInventario
        //public async Task<ActionResult> Index()
        //{
        //    if (User.IsInRole("Asociado"))
        //        return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);

        //    var comprasInventario = db.ComprasInventario.Include(c => c.Productos);
        //    return View(await comprasInventario.ToListAsync());
        //}

        //// GET: ComprasInventario/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ComprasInventario comprasInventario = await db.ComprasInventario.FindAsync(id);
        //    if (comprasInventario == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(comprasInventario);
        //}

        //// GET: ComprasInventario/Create
        //public ActionResult Create()
        //{
        //    ViewBag.IdProducto = new SelectList(db.Productos, "Id", "Nombre");
        //    return View();
        //}

        //// POST: ComprasInventario/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,IdProducto,CantidadComprada,FechaCompra,FechaVencimiento,Lote,PrecioIndividual")] ComprasInventario comprasInventario)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.ComprasInventario.Add(comprasInventario);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.IdProducto = new SelectList(db.Productos, "Id", "Nombre", comprasInventario.IdProducto);
        //    return View(comprasInventario);
        //}

        //// GET: ComprasInventario/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ComprasInventario comprasInventario = await db.ComprasInventario.FindAsync(id);
        //    if (comprasInventario == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.IdProducto = new SelectList(db.Productos, "Id", "Nombre", comprasInventario.IdProducto);
        //    return View(comprasInventario);
        //}

        //// POST: ComprasInventario/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,IdProducto,CantidadComprada,FechaCompra,FechaVencimiento,Lote,PrecioIndividual")] ComprasInventario comprasInventario)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(comprasInventario).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.IdProducto = new SelectList(db.Productos, "Id", "Nombre", comprasInventario.IdProducto);
        //    return View(comprasInventario);
        //}

        //// GET: ComprasInventario/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ComprasInventario comprasInventario = await db.ComprasInventario.FindAsync(id);
        //    if (comprasInventario == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(comprasInventario);
        //}

        //// POST: ComprasInventario/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    ComprasInventario comprasInventario = await db.ComprasInventario.FindAsync(id);
        //    db.ComprasInventario.Remove(comprasInventario);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}


        // GET: Home
        public ActionResult Index()
        {
            if (User.IsInRole("Asociado"))
                return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);

            List<ComprasInventario> comprasInventarios = db.ComprasInventario.ToList();

            //Add a Dummy Row.
            comprasInventarios.Insert(0, new ComprasInventario());
            return View(comprasInventarios);
        }

        [HttpPost]
        public string InsertComprasInventario(ComprasInventario comprasInventario)
        {
            db.ComprasInventario.Add(comprasInventario);
            db.SaveChanges();

            
            return Newtonsoft.Json.JsonConvert.SerializeObject(comprasInventario);
        }

        [HttpPost]
        public ActionResult UpdateComprasInventario(ComprasInventario comprasInventario)
        {
            ComprasInventario updatedCI = (from c in db.ComprasInventario
                                           where c.Id == comprasInventario.Id
                                           select c).FirstOrDefault();
            updatedCI.IdProducto = comprasInventario.IdProducto;
            updatedCI.Lote = comprasInventario.Lote;
            updatedCI.PrecioIndividual = comprasInventario.PrecioIndividual;
            updatedCI.FechaCompra = comprasInventario.FechaCompra;
            updatedCI.FechaVencimiento = comprasInventario.FechaVencimiento;
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
