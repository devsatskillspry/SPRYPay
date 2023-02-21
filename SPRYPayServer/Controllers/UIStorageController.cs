using System.Threading.Tasks;
using SPRYPayServer.Abstractions.Extensions;
using SPRYPayServer.Storage.Services;
using Microsoft.AspNetCore.Mvc;

namespace SPRYPayServer.Storage
{
    [Route("Storage")]
    public class UIStorageController : Controller
    {
        private readonly FileService _FileService;

        public UIStorageController(FileService fileService)
        {
            _FileService = fileService;
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(string fileId)
        {
            var url = await _FileService.GetFileUrl(Request.GetAbsoluteRootUri(), fileId);
            if (url is null)
                return NotFound();
            return new RedirectResult(url);
        }
    }
}
