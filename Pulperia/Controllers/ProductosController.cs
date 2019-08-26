using Pulperia.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    public class ProductosController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        [AllowAnonymous]
        [HttpGet]
        [Route("productos/obtenerPrecio")]
        public decimal ObtenerPrecio(int idProducto, int cantidad)
        {
            decimal ganancia = 1;
            bool esAsociado = db.Compradores.Where(c => c.Email == User.Identity.Name).Single().EsAsociado;

            if (esAsociado)
            {
                ganancia = Convert.ToDecimal(db.Parametros.Where(p => p.Nombre == "PorcentageGananciaAsociado" && p.FechaInicio < DateTime.Now && !p.FechaFin.HasValue).FirstOrDefault().Valor);
            }
            else
            {
                ganancia = Convert.ToDecimal(db.Parametros.Where(p => p.Nombre == "PorcentageGananciaNoAsociado" && p.FechaInicio < DateTime.Now && !p.FechaFin.HasValue).FirstOrDefault().Valor);
            }
            var precio = db.Productos.Where(p => p.Id == idProducto).Single().PrecioCompraIndividual;
            var subTotal = decimal.Round(precio * ganancia, 0, MidpointRounding.AwayFromZero) * cantidad;

            return subTotal;
        }

        // GET: Productos
        public async Task<ActionResult> Index(string rbtnFiltro)
        {
            if (User.IsInRole("Asociado"))
                return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);

            List<Productos> productos = null;

            if (string.IsNullOrEmpty(rbtnFiltro))
            {
                rbtnFiltro = "1";
            }
            switch (rbtnFiltro)
            {
                case "1":
                    productos = await db.Productos.Include(p => p.Categorias).ToListAsync();
                    break;
                case "2":
                    productos = await db.Productos.Where(p => p.CantidadInventario > 0).Include(p => p.Categorias).ToListAsync();
                    break;
                case "3":
                    productos = await db.Productos.Where(p => p.CantidadInventario == 0).Include(p => p.Categorias).ToListAsync();
                    break;
                default:
                    break;
            }

            return View(productos.OrderBy(p => p.Nombre).ThenBy(p => p.IdCategoria));
        }

        // GET: Productos/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = await db.Productos.FindAsync(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            return View(productos);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.IdCategoria = new SelectList(db.Categorias, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Nombre,Descripcion,FechaIngreso,CantidadInventario,PrecioCompraIndividual,IdCategoria")] Productos productos)
        {
            if (ModelState.IsValid)
            {
                productos.FechaCreacion = DateTime.Now;
                db.Productos.Add(productos);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IdCategoria = new SelectList(db.Categorias, "Id", "Nombre", productos.IdCategoria);
            return View(productos);
        }

        // GET: Productos/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = await db.Productos.FindAsync(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdCategoria = new SelectList(db.Categorias, "Id", "Nombre", productos.IdCategoria);
            return View(productos);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Nombre,Descripcion,FechaIngreso,CantidadInventario,PrecioCompraIndividual,IdCategoria")] Productos productos)
        {
            if (ModelState.IsValid)
            {
                productos.FechaCreacion = DateTime.Now; //corregir esto
                productos.FechaActualizacion = DateTime.Now;
                db.Entry(productos).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdCategoria = new SelectList(db.Categorias, "Id", "Nombre", productos.IdCategoria);
            return View(productos);
        }

        // GET: Productos/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Productos productos = await db.Productos.FindAsync(id);
            if (productos == null)
            {
                return HttpNotFound();
            }
            return View(productos);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Productos productos = await db.Productos.FindAsync(id);
            db.Productos.Remove(productos);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
