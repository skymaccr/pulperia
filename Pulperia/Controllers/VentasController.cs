using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pulperia.Models;
using Pulperia.Utils;
using System;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    [HandleError]
    public class VentasController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

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
        public ActionResult Create(bool compraRegistrada = false)
        {
            if (User.IsInRole("Asociado"))
                ViewBag.IdComprador = new SelectList(db.Compradores.Where(c => c.Email == User.Identity.Name), "Id", "Nombre");
            else
                ViewBag.IdComprador = new SelectList(db.Compradores, "Id", "Nombre");

            ViewBag.IdProducto = new SelectList(db.Productos.Where(p => p.CantidadInventario > 0).OrderBy(p => p.Nombre).Select(p => new { p.Id, p.Nombre }), "Id", "Nombre");

            if (compraRegistrada)
            {
                ViewBag.CompraRegistrada = true;               
            }

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

                //enviar el correo al usuario indicado la compra que acaba de hacer
                await EnviarCorreoVentaAsync(ventas, User.Identity.Name);

                return RedirectToAction("Create", "Ventas", new { compraRegistrada = true });
            }

            ViewBag.IdComprador = new SelectList(db.Compradores, "Id", "Nombre", ventas.IdComprador);
            ViewBag.IdProducto = new SelectList(db.Productos.Where(p => p.CantidadInventario > 0), "Id", "Nombre", ventas.IdProducto);

            return View(ventas);
        }

        private async Task EnviarCorreoVentaAsync(Ventas ventas, string email)
        {
            StringBuilder emailHTML = new StringBuilder();
            var template = new Templates();
            var emailTemplate = template.GetHTMLTemplate(ConfigurationManager.AppSettings.Get("EmailFactura"));
            emailTemplate = emailTemplate.Replace("[Anno]", DateTime.Now.Year.ToString());
            emailTemplate = emailTemplate.Replace("[Total]", ventas.Precio.ToString());

            var filaVentaTemplate = new StringBuilder();
            filaVentaTemplate.Append("<tr>");
            filaVentaTemplate.Append($"<td valign=\"top\" >{ventas.Cantidad}<br></td>");
            filaVentaTemplate.Append($"<td valign=\"top\" >¢ {ventas.Precio}<br></td>");
            filaVentaTemplate.Append(" <td valign=\top\" ><span style=\"color: rgb(235, 235, ");
            filaVentaTemplate.Append(" 235); font-family: Lato, &quot;Helvetica");
            filaVentaTemplate.Append(" Neue&quot;, Helvetica, Arial, sans-serif;");
            filaVentaTemplate.Append(" font-size: 15px; font-style: normal;");
            filaVentaTemplate.Append(" font-variant-ligatures: normal; font-variant-caps:");
            filaVentaTemplate.Append(" normal; font-weight: 300; letter-spacing: normal;");
            filaVentaTemplate.Append(" orphans: 2; text-align: start; text-indent: 0px;");
            filaVentaTemplate.Append(" text-transform: none; white-space: normal; widows:");
            filaVentaTemplate.Append(" 2; word-spacing: 0px; -webkit-text-stroke-width:");
            filaVentaTemplate.Append(" 0px; background-color: rgb(43, 62, 80);");
            filaVentaTemplate.Append(" text-decoration-style: initial;");
            filaVentaTemplate.Append(" text-decoration-color: initial; display: inline");
            filaVentaTemplate.Append($" !important; float: none;\" >{ventas.FechaCompra.ToString("dd/MM/yyyy HH:mm")} </ span ></ td > ");
            filaVentaTemplate.Append($"<td valign=\"top\" >{ventas.Productos.Nombre.ToString()}<br></td>");
            filaVentaTemplate.Append("</tr>");

            emailTemplate = emailTemplate.Replace("[Ventas]", filaVentaTemplate.ToString());

            var user = UserManager.FindByName(email);
            if (user != null)
            {
                await UserManager.SendEmailAsync(user.Id, $"Su factura de compra", emailTemplate.ToString());
            }
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
