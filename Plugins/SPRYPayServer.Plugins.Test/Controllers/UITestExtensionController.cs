using System.Collections.Generic;
using System.Threading.Tasks;
using SPRYPayServer.Plugins.Test.Data;
using SPRYPayServer.Plugins.Test.Services;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Plugins.Test
{
    [Route("extensions/test")]
    public class UITestExtensionController : Controller
    {
        private readonly TestPluginService _testPluginService;

        public UITestExtensionController(TestPluginService testPluginService)
        {
            _testPluginService = testPluginService;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            return View(new TestPluginPageViewModel()
            {
                Data = await _testPluginService.Get()
            });
        }


    }

    public class TestPluginPageViewModel
    {
        public List<TestPluginData> Data { get; set; }
    }
}
