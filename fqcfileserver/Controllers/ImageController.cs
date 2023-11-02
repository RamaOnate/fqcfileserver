using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace fqcfileserver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("upload")]
        //[EnableCors("MyPolicy")]
        public async Task<IActionResult> Upload(IFormFile file, string machineSerialNumber)
        {
            // images will be saved under a folder that has the machineSerialNumber
            // if machineSerialNumber folder does not exist, create it
            // this folder will be located inside the "images" folder
            // the file name will be the same as the original file name
            // if the file name already exists, add a number to the end of the file name
            // return the file path to the client as well as the file name
            // the client will use the file path to display the image

            // get the file extension
            var fileExtension = Path.GetExtension(file.FileName);

            // get the file name without the extension
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);

            // get the file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber, file.FileName);

            // check if the file exists
            if (System.IO.File.Exists(filePath))
            {
                // if the file exists, add a number to the end of the file name
                var fileNumber = 1;
                while (System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber, $"{fileName}({fileNumber}){fileExtension}");
                    fileNumber++;
                }
            }

            // create the directory if it does not exist
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber));
            }

            // save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // return the file path to the client
            return Ok(new { filePath, fileName });
        }

    }
}
