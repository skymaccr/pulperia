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
using System.Runtime.Serialization;
using System.Globalization;

namespace Pulperia.Controllers
{
    public class CompradoresController : Controller
    {
        private PulperiaEntities db = new PulperiaEntities();

        // GET: Compradores
        public async Task<ActionResult> Index()
        {            
            return View(await db.Compradores.OrderBy(o => o.Nombre).ToListAsync());
        }

        // GET: Compradores/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Compradores compradores = await db.Compradores.FindAsync(id);
            if (compradores == null)
            {
                return HttpNotFound();
            }
            return View(compradores);
        }

        // GET: Compradores/Create
        public ActionResult Create()
        {
           
            if (this.Request.QueryString["created"] != null &&
                this.Request.QueryString["email"] != null)
            {
                Compradores model = new Compradores()
                {
                    Email = Request.QueryString["email"],
                };
                
                return View(model);
            }
            else
                return View();
        }

        // POST: Compradores/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Nombre,Apellidos,EsAsociado,Activo,Email")] Compradores compradores)
        {
            if (ModelState.IsValid)
            {
                db.Compradores.Add(compradores);
                await db.SaveChangesAsync();

                if (this.Request.QueryString["created"] != null &&
                this.Request.QueryString["email"] != null)
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Index");
            }

            return View(compradores);
        }

        // GET: Compradores/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Compradores compradores = await db.Compradores.FindAsync(id);
            if (compradores == null)
            {
                return HttpNotFound();
            }
            return View(compradores);
        }

        // POST: Compradores/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Nombre,Apellidos,EsAsociado,Activo,Email")] Compradores compradores)
        {
            if (ModelState.IsValid)
            {
                db.Entry(compradores).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(compradores);
        }

        // GET: Compradores/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Compradores compradores = await db.Compradores.FindAsync(id);
            if (compradores == null)
            {
                return HttpNotFound();
            }
            return View(compradores);
        }

        // POST: Compradores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Compradores compradores = await db.Compradores.FindAsync(id);
            db.Compradores.Remove(compradores);
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
