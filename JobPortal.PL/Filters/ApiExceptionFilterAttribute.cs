using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using JobPortal.BLL.Infrastructure;

namespace JobPortal.PL.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is ValidationException valEx)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, valEx.Message + " (Field: " + valEx.Property + ")");
            }
            else if (context.Exception is AccessDeniedException accEx)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, accEx.Message);
            }
            else if (context.Exception is NotFoundException nfEx)
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.NotFound, nfEx.Message);
            }
            else
            {
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Внутрішня помилка сервера");
            }
        }
    }
}