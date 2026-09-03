using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;

namespace BlazorChart240928.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly string dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");

        [HttpGet("list")]
        public IActionResult GetFileList ()
        {
            // data 폴더 내의 모든 .json 파일 목록을 반환
            var files = Directory.GetFiles(dataFolder, "*.json")
                                 .Select(Path.GetFileName)  // 파일 이름만 반환
                                 .ToList();
            return Ok(files);
        }
    }
}
