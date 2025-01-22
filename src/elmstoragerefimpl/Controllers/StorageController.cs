using elmstoragerefimpl.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace elmstoragerefimpl.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public StorageController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        [Route("{ObjectType}/List")]
        public async Task<IActionResult> List(string objectType)
        {
            try
            {
                var ids = await _storageService.ListObjectsAsync(objectType);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        [Route("{ObjectType}")]
        [Route("{ObjectType}/{id}")]
        public async Task<IActionResult> Get(string objectType, Guid? id)
        {
            try
            {
                if (id.HasValue)
                {
                    var obj = await _storageService.GetObjectAsync(objectType, id.Value);
                    return obj != null ? Ok(obj) : NotFound($"Object {objectType} with id {id} not found");
                }
                else
                {
                    var objects = await _storageService.GetAllObjectsAsync(objectType);
                    return Ok(objects);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost]
        [Route("{ObjectType}")]
        [Route("{ObjectType}/{id}")]
        public async Task<IActionResult> Post(string objectType, Guid? id, [FromBody] string data)
        {
            try
            {
                var objId = await _storageService.SaveObjectAsync(objectType, id, data);
                return Ok(objId);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpDelete]
        [Route("{ObjectType}/{id}")]
        public async Task<IActionResult> Delete(string objectType, Guid id)
        {
            try
            {
                var result = await _storageService.DeleteObjectAsync(objectType, id);
                return result ? Ok() : NotFound($"Object {objectType} with id {id} not found");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
