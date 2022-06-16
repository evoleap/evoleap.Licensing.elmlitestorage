using elmstoragerefimpl.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace elmstoragerefimpl.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ObjectStorageController : ApiController
{

    private string StorageRoot { get; set; }

    public ObjectStorageController(IConfiguration config)
    {
        var storageRootPath = config.GetValue<string>("StorageRoot");
        // C# doesn't handle the home folder Unix mnemonic
        if (storageRootPath.StartsWith("~/"))
        {
            storageRootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                storageRootPath.Substring(1);
        }
        if (!System.IO.Directory.Exists(storageRootPath))
        {
            System.IO.Directory.CreateDirectory(storageRootPath);
        }
        StorageRoot = storageRootPath;
    }

    [HttpGet]
    [Route("{ObjectType}/List")]
    public IActionResult List(string ObjectType)
    {
        try
        {
            return Ok(EnumerateObjects(ObjectType).Select(o => o.Item2));
        }
        catch (System.Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet]
    [Route("{ObjectType}")]
    [Route("{ObjectType}/{id}")]
    public IActionResult Get(string ObjectType, Guid? id)
    {
        if (id.HasValue)
        {
            string fileName = GetFileName(ObjectType, id.Value);

            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    return Ok(GetFileData(fileName));
                }
                else
                {
                    return NotFound($"Object {ObjectType} with id {id} not found");
                }
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        else
        {
            try
            {
                // Enumerate all objects of type ObjectType by listing Object.*.json                
                var ret = EnumerateObjects(ObjectType)
                    .Select(f => new ObjectWithId() { Id = f.Item2, Object = GetFileData(f.Item1) });
                return Ok(ret);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }

    private IEnumerable<(string, Guid)> EnumerateObjects(string ObjectType) =>
        System.IO.Directory.EnumerateFiles(StorageRoot, $"{ObjectType}.*.json")
            .Select(f =>
            {
                // Get just the filename
                var sep = f.LastIndexOf(Path.DirectorySeparatorChar);
                if (sep > 0)
                {
                    return (FullPath: f, FileName: f.Substring(sep + 1));
                }
                else
                {
                    return (FullPath: f, FileName: f);
                }
            })
            .Select(f => (FullPath: f.FullPath, id: f.FileName.Substring(ObjectType.Length + 1, f.FileName.Substring(ObjectType.Length + 1).IndexOf("."))))
            .Where(f => Guid.TryParse(f.id, out var _))
            .Select(f => (f.FullPath, Guid.Parse(f.id)));

    private string GetFileData(string fileName)
    {
        using var sr = System.IO.File.OpenText(fileName);
        return sr.ReadToEnd();
    }

    [HttpPost]
    [Route("{ObjectType}")]
    [Route("{ObjectType}/{id}")]
    public IActionResult Post(string ObjectType, Guid? id, [FromBody] string data)
    {
        Guid objId = id ?? Guid.NewGuid();
        string fileName = GetFileName(ObjectType, objId);
        try
        {
            using var outFile = System.IO.File.OpenWrite(fileName);
            using var sr = new StreamWriter(outFile);
            sr.Write(data);
            sr.Flush();
            return Ok(objId);
        }
        catch (System.Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpDelete]
    [Route("{ObjectType}/{id}")]
    public IActionResult Delete(string ObjectType, Guid id)
    {
        string fileName = GetFileName(ObjectType, id);
        try
        {
            System.IO.File.Delete(fileName);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    private string GetFileName(string ObjectType, Guid id) =>
        $"{StorageRoot}/{ObjectType}.{id}.json";
}