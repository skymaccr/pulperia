using Pulperia.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    [HandleError]
    public class VentasController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        // GET: Ventas
        public async Task<ActionResult> Index()
        {
            var currentMonth = DateTime.Now.Month;
            IQueryable<Ventas> ventas;
            if (User.IsInRole("Asociado"))
            {
                //obtener las ventas del asociado
                var idComprador = db.Compradores.Where(c => c.Email == User.Identity.Name).Select(c => c.Id).SingleOrDefault();
                ventas = db.Ventas.Include(v => v.Compradores).Include(v => v.Productos).Where(v => v.IdComprador == idComprador);
            }
            else
            {
                ventas = db.Ventas.Include(v => v.Compradores).Include(v => v.Productos);
            }

            if (ventas.Any())
                ViewBag.TotalCompras = ventas.Sum(x => x.Precio);
            else
                ViewBag.TotalCompras = 0M;

            return View(await ventas.ToListAsync());
        }

        // GET: Ventas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ventas ventas = await db.Ventas.FindAsync(id);
            if (ventas == null)
            {
                return HttpNotFound();
            }
            return View(ventas);
        }

        // GET: Ventas/Create
        public ActionResult Create()
        {
            if (User.IsInRole("Asociado"))
                ViewBag.IdComprador = new SelectList(db.Compradores.Where(c => c.Email == User.Identity.Name), "Id", "Nombre");
            else
                ViewBag.IdComprador = new SelectList(db.Compradores, "Id", "Nombre");

            ViewBag.IdProducto = new SelectList(db.Productos.Where(p => p.CantidadInventario > 0).OrderBy(p => p.Nombre).Select(p => new { p.Id, p.Nombre }), "Id", "Nombre");

            return View();
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,IdComprador,IdProducto,Cantidad,Precio,FechaCompra")] Ventas ventas)
        {
            if (ModelState.IsValid)
            {
                //recalcular el precio por si alguien lo modifica manualmente
                ventas.Precio = CalcularPrecio(ventas.IdProducto, ventas.Cantidad);
                ventas.FechaCompra = DateTime.Now;
                db.Ventas.Add(ventas);
                //rebajar el inventario
                var producto = db.Productos.Where(p => p.Id == ventas.IdProducto).Single();
                producto.CantidadInventario -= ventas.Cantidad;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IdComprador = new SelectList(db.Compradores, "Id", "Nombre", ventas.IdComprador);
            ViewBag.IdProducto = new SelectList(db.Productos.Where(p => p.CantidadInventario > 0), "Id", "Nombre", ventas.IdProducto);

            return View(ventas);
        }

        private decimal CalcularPrecio(int idProducto, int cantidad)
        {
            var ganancia = Convert.ToDecimal(db.Parametros.Where(p => p.Nombre == "PorcentageGanancia" && p.FechaInicio < DateTime.Now && !p.FechaFin.HasValue).FirstOrDefault().Valor);
            var precio = db.Productos.Where(p => p.Id == idProducto).Single().PrecioCompraIndividual;
            var subTotal = decimal.Round(precio * ganancia, 0, MidpointRounding.AwayFromZero) * cantidad;

            return subTotal;
        }

        // GET: Ventas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ventas ventas = await db.Ventas.FindAsync(id);

            //no se puede editar una venta de dias pasados si es un asociado
            if (ventas == null ||
            (User.IsInRole("Asociado") && 
             DateTime.Now.Subtract(ventas.FechaCompra).Minutes > 5))
            {
                throw new ApplicationException("No tiene permiso de borrar");
            }

            Session["CantidadPrevia"] = ventas.Cantidad;

            if (User.IsInRole("Asociado"))
                ViewBag.IdComprador = new SelectList(db.Compradores.Where(c => c.Email == User.Identity.Name), "Id", "Nombre", ventas.IdComprador);
            else
                ViewBag.IdComprador = new SelectList(db.Compradores, "Id", "Nombre", ventas.IdComprador);

            ViewBag.IdProducto = new SelectList(db.Productos.Where(p => p.Id == ventas.IdProducto), "Id", "Nombre", ventas.IdProducto);

            if (ViewBag.IdProducto == null)
                return HttpNotFound();

            return View(ventas);
        }

        // POST: Ventas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]        
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,IdComprador,IdProducto,Cantidad,Precio,FechaCompra")] Ventas ventas)
        {
            if (ModelState.IsValid)
            {
                //buscar la compra a ver de cuanto era la cantidad
                var cantidadVentaPrevia = Convert.ToInt32(Session["CantidadPrevia"]);

                ventas.FechaCompra = DateTime.Now;
                //recalcular el precio
                ventas.Precio = CalcularPrecio(ventas.IdProducto, ventas.Cantidad);

                db.Entry(ventas).State = EntityState.Modified;
                await db.SaveChangesAsync();

                var producto = db.Productos.Where(p => p.Id == ventas.IdProducto).Single();
                //verificar si se suma o resta la cantidad
                if (cantidadVentaPrevia > ventas.Cantidad)
                {
                    //reducir esta cantidad
                    producto.CantidadInventario += cantidadVentaPrevia - ventas.Cantidad;
                }
                else if (cantidadVentaPrevia < ventas.Cantidad)
                {
                    //sumar esta cantidad
                    producto.CantidadInventario -= ventas.Cantidad - cantidadVentaPrevia;
                }

                db.Entry(producto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                Session["CantidadPrevia"] = null;
                return RedirectToAction("Index");
            }

            ViewBag.IdComprador = new SelectList(db.Compradores, "Id", "Nombre", ventas.IdComprador);
            ViewBag.IdProducto = new SelectList(db.Productos.Where(p => p.CantidadInventario > 0), "Id", "Nombre", ventas.IdProducto);

            return View(ventas);
        }

        // GET: Ventas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ventas ventas = await db.Ventas.FindAsync(id);

            //no se pueden borrar ventas del pasado si es un asociado
            if (ventas == null ||
            (User.IsInRole("Asociado") && 
             DateTime.Now.Subtract(ventas.FechaCompra).Minutes > 5))
            {
                throw new ApplicationException("No tiene permiso de borrar");
            }
            return View(ventas);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]       
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Ventas ventas = await db.Ventas.FindAsync(id);

            //devolver el inventario
            var producto = db.Productos.Where(p => p.Id == ventas.IdProducto).Single();
            producto.CantidadInventario += ventas.Cantidad;

            db.Ventas.Remove(ventas);
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
