using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Components.ThemeSwitch
{
    public class ThemeSwitch : ViewComponent
    {
        public IViewComponentResult Invoke(string cssClass = null)
        {
            var vm = new ThemeSwitchViewModel
            {
                CssClass = cssClass,
            };
            return View(vm);
        }
    }
}
