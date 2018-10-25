using Pulperia.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    public class HistoricoComprasInventariosController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        // GET: HistoricoComprasInventarios
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

            var consulta = await db.HistoricoComprasInventario.Where(h => h.Anno == anno && h.Mes == mes).ToListAsync();

            ViewBag.TotalComprado = consulta.Sum(x => x.PrecioInvividual * x.CantidadComprada);

            return View(consulta);
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
