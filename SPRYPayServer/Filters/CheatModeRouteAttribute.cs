using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Services;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.DependencyInjection;

namespace SPRYPayServer.Filters
{
    public class CheatModeRouteAttribute : Attribute, IActionConstraint
    {
        public int Order => 100;

        public bool Accept(ActionConstraintContext context)
        {
            return context.RouteContext.HttpContext.RequestServices.GetRequiredService<SPRYPayServerEnvironment>().CheatMode;
        }
    }
}
