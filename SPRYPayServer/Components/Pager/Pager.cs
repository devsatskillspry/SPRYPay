using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPRYPayServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Components
{
    public class Pager : ViewComponent
    {
        public Pager()
        {
        }
        public IViewComponentResult Invoke(BasePagingViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}
