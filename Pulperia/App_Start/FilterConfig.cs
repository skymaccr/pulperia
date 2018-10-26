using Pulperia.App_Start;
using System.Web.Mvc;

namespace Pulperia
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());

            filters.Add(new CustomViewForHttpStatusResultFilter(new HttpNotFoundResult(), "Error404"));
            filters.Add(new CustomViewForHttpStatusResultFilter(404, "Error404"));
        }
    }
}
