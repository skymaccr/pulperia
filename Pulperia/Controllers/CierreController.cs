using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Pulperia.Models;
using Pulperia.Utils;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Pulperia.Controllers
{
    public class CierreController : Controller
    {
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

        private PulperiaEntities db = new PulperiaEntities();

        // GET: Cierre
        public ActionResult Index()
        {
            if (User.IsInRole("Asociado"))
                return new HttpStatusCodeResult(HttpStatusCode.MethodNotAllowed);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Run()
        {
            await EnviarCorreosAsync();

            //ejecutar el cierre mensual
            db.SP_CierreMensual();

            //redireccionar a una pagina donde se vea el cierre
            return RedirectToAction("Index", "HistoricoVentas", new { cierreEjecutado = true });
        }

        private async Task EnviarCorreosAsync()
        {
            try
            {
                var i = 1;
                var meses = from m in CultureInfo.CurrentCulture.DateTimeFormat.MonthNames
                            select new
                            {
                                mes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(m),
                                valor = i++
                            };

                var mesActual = meses.Where(m => m.valor == DateTime.Now.Month).Single().mes;

                StringBuilder emailHTML = new StringBuilder();
                var template = new Templates();
                var emailTemplate = template.GetHTMLTemplate(ConfigurationManager.AppSettings.Get("EmailCobro"));
                emailTemplate = emailTemplate.Replace("[Mes]", mesActual);
                emailTemplate = emailTemplate.Replace("[Anno]", DateTime.Now.Year.ToString());

                var filaVentaTemplate = new StringBuilder();
                filaVentaTemplate.Append("<tr>");
                filaVentaTemplate.Append("<td valign=\"top\" >[Cantidad]<br></td>");
                filaVentaTemplate.Append("<td valign=\"top\" >¢ [Precio]<br></td>");
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
                filaVentaTemplate.Append(" !important; float: none;\" >[FechaCompra] </ span ></ td > ");
                filaVentaTemplate.Append("<td valign=\"top\" >[Producto]<br></td>");
                filaVentaTemplate.Append("</tr>");

                var compradoresVentas = db.Compradores.Include("Ventas").Where(c => c.Activo);
                foreach (var comprador in compradoresVentas)
                {
                    if (comprador.Ventas.Any())
                    {
                        //calcular el detalle de las ventas
                        var ventas = comprador.Ventas;
                        //calcular el monto total a ser rebajado
                        var total = ventas.Sum(v => v.Precio);

                        //enviar el correo al comprador
                        var user = UserManager.FindByName(comprador.Email);

                        if (user != null)
                        {
                            var myTemplate = emailTemplate;
                            myTemplate = myTemplate.Replace("[NombreCliente]", $"{ comprador.Nombre} {comprador.Apellidos}");
                            myTemplate = myTemplate.Replace("[Total]", total.ToString());

                            StringBuilder filaVentas = new StringBuilder();
                            foreach (var venta in comprador.Ventas)
                            {
                                var fila = filaVentaTemplate.ToString();
                                fila = fila.Replace("[Cantidad]", venta.Cantidad.ToString());
                                fila = fila.Replace("[Precio]", venta.Precio.ToString());
                                fila = fila.Replace("[FechaCompra]", venta.FechaCompra.ToString("dd/MM/yyyy HH:mm"));
                                fila = fila.Replace("[Producto]", venta.Productos.Nombre.ToString());
                                filaVentas.Append(fila);
                            }

                            myTemplate = myTemplate.Replace("[Ventas]", filaVentas.ToString());

                            await UserManager.SendEmailAsync(user.Id, $"Cobro del mes de {mesActual}", myTemplate.ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}