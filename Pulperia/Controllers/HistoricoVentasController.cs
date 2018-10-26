using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Pulperia.Models;
using System.Globalization;

namespace Pulperia.Controllers
{
    public class HistoricoVentasController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        // GET: HistoricoVentas
        public async Task<ActionResult> Index(string mesId, string annoId)
        {
            if (User.IsInRole("Asociado"))
                return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);

            int mes = DateTime.Now.Month;
            int anno = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(mesId))
                mes = Convert.ToInt32(mesId);

            if (!string.IsNullOrEmpty(annoId))
                anno = Convert.ToInt32(annoId);

            ViewBag.MesSeleccionado = mes;
            ViewBag.AnnoSeleccionado = anno;

            int i = 1;
            var meses = from m in CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                        select new
                        {
                            mes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(m),
                            valor = i++
                        };

            ViewBag.Annos = new Dictionary<int, int> { { 2018, 2018 }, { 2019, 2019 }, { 2020, 2020 }, { 2021, 2021 }, { 2022, 2022 } };
            ViewBag.Meses = meses.Take(12);

            List<HistoricoVentas> historicoVentas;

            if (this.Request.QueryString["cierreEjecutado"] != null)
            {
                mes = DateTime.Now.Month;
                anno = DateTime.Now.Year;
                ViewBag.MesSeleccionado = mes;
                ViewBag.AnnoSeleccionado = anno;

                historicoVentas = await db.HistoricoVentas.Where(h => h.Mes == mes && h.Anno == anno).OrderBy(o => o.IdComprador).ToListAsync();
            }
            else
            {
                historicoVentas = await db.HistoricoVentas.Where(h => h.Anno == anno && h.Mes == mes).OrderBy(o => o.IdComprador).ToListAsync();
            }

            if (historicoVentas.Any())
            {
                ViewBag.TotalCompras = historicoVentas.Sum(x => x.Precio);
                ViewBag.TotalVendido = historicoVentas.Sum(x => x.PrecioOriginalProducto * x.Cantidad);
                ViewBag.TotalPeriodo = historicoVentas.Sum(x => x.Precio) - historicoVentas.Sum(x => x.PrecioOriginalProducto * x.Cantidad);

                List<HistoricoPersona> listaPersona = new List<HistoricoPersona>();

                foreach (var item in historicoVentas.Select(p => p.IdComprador).Distinct())
                {
                    var historico = historicoVentas.Where(x => x.IdComprador == item);
                    listaPersona.Add(new HistoricoPersona()
                    {
                        Monto = historico.Sum(x => x.Precio),
                        Nombre = historico.Select(x => x.NombreComprador).First()
                    });            
                }

                ViewBag.PorPersona = listaPersona;
            }
            else
            {
                ViewBag.TotalCompras = 0M;
                ViewBag.TotalVendido = 0M;
                ViewBag.TotalPeriodo = 0M;
                ViewBag.PorPersona = new List<HistoricoPersona>();
            }

            return View(historicoVentas);
        }

        // GET: HistoricoVentas/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HistoricoVentas historicoVentas = await db.HistoricoVentas.FindAsync(id);
            if (historicoVentas == null)
            {
                return HttpNotFound();
            }
            return View(historicoVentas);
        }

        // POST: HistoricoVentas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,IdComprador,IdProducto,Cantidad,Precio,FechaCompra,NombreComprador,NombreProducto,PrecioOriginalProducto,Mes,Anno")] HistoricoVentas historicoVentas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(historicoVentas).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(historicoVentas);
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

    public class FiltroHistorico
    {
        public int Mes { get; set; }
        public int Anno { get; set; }
    }

    public class HistoricoPersona
    {
        public string Nombre { get; set; }
        public decimal Monto { get; set; }
    }
}
