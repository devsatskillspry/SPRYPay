using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.Filters
{
    public class OnlyIfSupportAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _cryptoCode;

        public OnlyIfSupportAttribute(string cryptoCode)
        {
            _cryptoCode = cryptoCode;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var options = context.HttpContext.RequestServices.GetService<SPRYPayNetworkProvider>();
            if (options.GetNetwork(_cryptoCode) == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }
    }
}
