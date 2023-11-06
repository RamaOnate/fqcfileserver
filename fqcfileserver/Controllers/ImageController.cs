using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace fqcfileserver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("upload")]
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
            // this path will be relative and will be of the form images/{machineSerialNumber}/{fileName}
            var relativeFilePath = filePath.Replace(Directory.GetCurrentDirectory(), "");
            relativeFilePath = relativeFilePath.Replace("\\", "/");
            return Ok(new { filePath = relativeFilePath, fileName });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> replacePicture(IFormFile file, string pathOfFileToDelete, string machineSerialNumber)
        {
            var fileExtension = Path.GetExtension(file.FileName);

            // get the file name without the extension
            var newFileName = Path.GetFileNameWithoutExtension(file.FileName);

            // get the file path
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), pathOfFileToDelete);
            oldFilePath = oldFilePath.Replace("/images/", "");
            oldFilePath = oldFilePath.Substring(oldFilePath.LastIndexOf("/") + 1);

            // obtain full absolute path of the file to delete
            oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber, oldFilePath);

            // check if the file exists before deleting it
            if (!System.IO.File.Exists(oldFilePath))
            {
                return NotFound();
            }

            System.IO.File.Delete(oldFilePath);

            var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber, file.FileName);

            // save the file
            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeFilePath = newFilePath.Replace(Directory.GetCurrentDirectory(), "");
            relativeFilePath = relativeFilePath.Replace("\\", "/");
            return Ok(new { filePath = relativeFilePath, file.FileName });
            
        }

        [HttpPost("delete")]
        public IActionResult Delete(string pathOfFileToDelete, string machineSerialNumber)
        {
            // obtain full absolute path of the file to delete
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), pathOfFileToDelete);
            oldFilePath = oldFilePath.Replace("/images/", "");
            oldFilePath = oldFilePath.Substring(oldFilePath.LastIndexOf("/") + 1);

            // obtain full absolute path of the file to delete
            oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber, oldFilePath);
            // check if the file exists before deleting it
            if (!System.IO.File.Exists(oldFilePath))
            {
                return NotFound();
            }

            System.IO.File.Delete(oldFilePath);

            return Ok();
        }

        [HttpGet("get")]
        public IActionResult GetMultiple(string machineSerialNumber)
        {
            // get all the files in the machineSerialNumber folder
            var files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber));

            // return the file paths to the client
            // these paths will be relative and will be of the form images/{machineSerialNumber}/{fileName}
            var relativeFilePaths = new DataTable();

            relativeFilePaths.Columns.Add("filePath", typeof(string));

            foreach (var file in files)
            {
                // but only add full absolute paths to the list, not just the stored relatives
                var fullAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), "images", machineSerialNumber);
                fullAbsolutePath = fullAbsolutePath.Replace("\\", "/");
                var relativeFilePath = file.Replace(fullAbsolutePath, "");
                relativeFilePath = relativeFilePath.Replace("\\", "/");
                relativeFilePaths.Rows.Add(relativeFilePath);
            }

            // convert relativeFilePaths to a json string
            var json = JsonConvert.SerializeObject(relativeFilePaths);
            return Ok(json);

        }

    }
}
