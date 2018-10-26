using System.Web;
using System.Web.Mvc;

namespace Pulperia.App_Start
{
    public class CustomViewForHttpStatusResultFilter : IResultFilter, IExceptionFilter
    {
        string viewName;
        int statusCode;

        public CustomViewForHttpStatusResultFilter(HttpStatusCodeResult prototype, string viewName)
            : this(prototype.StatusCode, viewName)
        {
        }

        public CustomViewForHttpStatusResultFilter(int statusCode, string viewName)
        {
            this.viewName = viewName;
            this.statusCode = statusCode;
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            HttpStatusCodeResult httpStatusCodeResult = filterContext.Result as HttpStatusCodeResult;

            if (httpStatusCodeResult != null && httpStatusCodeResult.StatusCode == statusCode)
            {
                ExecuteCustomViewResult(filterContext.Controller.ControllerContext);

            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnException(ExceptionContext filterContext)
        {
            HttpException httpException = filterContext.Exception as HttpException;

            if (httpException != null && httpException.GetHttpCode() == statusCode)
            {
                ExecuteCustomViewResult(filterContext.Controller.ControllerContext);
                // This causes ELMAH not to log exceptions, so commented out
                //filterContext.ExceptionHandled = true;
            }
        }

        void ExecuteCustomViewResult(ControllerContext controllerContext)
        {
            ViewResult viewResult = new ViewResult();
            viewResult.ViewName = viewName;
            viewResult.ViewData = controllerContext.Controller.ViewData;
            viewResult.TempData = controllerContext.Controller.TempData;
            viewResult.ExecuteResult(controllerContext);
            controllerContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}