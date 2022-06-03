using Moq;
using elmstoragerefimpl.Controllers;
using elmstoragerefimpl.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using static System.Environment;
using System.Text.Json;

namespace elmlitestoragetests;

[TestClass]
public class TestObjectStorage
{
    [TestMethod]
    public void TestObjectStorageController()
    {
        ObjectStorageController os = GetTestObjectStorageController();

        var id = Guid.NewGuid();

        var badGet = os.Get("test", id);

        Assert.AreEqual(typeof(NotFoundObjectResult), badGet.GetType());
        Assert.AreEqual(404, ((NotFoundObjectResult)badGet).StatusCode);

        string objData = "{\"this\": \"is a test\"}";
        var save = os.Post("test", id, objData);

        Assert.AreEqual(typeof(OkObjectResult), save.GetType());
        Assert.AreEqual(200, ((OkObjectResult)save).StatusCode);
        Assert.AreEqual(id, ((OkObjectResult)save).Value);

        var getData = os.Get("test", id);
        Assert.AreEqual(typeof(OkObjectResult), getData.GetType());
        Assert.AreEqual(200, ((OkObjectResult)getData).StatusCode);
        Assert.AreEqual(objData, ((OkObjectResult)getData).Value);

        // Test overwrite
        string objData2 = "{\"this\": \"is another test\"}";
        var save2 = os.Post("test", id, objData2);
        var getData2 = os.Get("test", id);
        Assert.AreEqual(objData2, ((OkObjectResult)getData2).Value);
        Assert.AreEqual(id, ((OkObjectResult)save2).Value);

        var delData = os.Delete("test", id);

        Assert.AreEqual(typeof(OkResult), delData.GetType());
        Assert.AreEqual(200, ((OkResult)delData).StatusCode);

        var badGet2 = os.Get("test", id);

        Assert.AreEqual(typeof(NotFoundObjectResult), badGet2.GetType());
        Assert.AreEqual(404, ((NotFoundObjectResult)badGet2).StatusCode);

    }

    private static ObjectStorageController GetTestObjectStorageController()
    {
        var mockConfig = new Mock<IConfiguration>();
        var configSection = new Mock<IConfigurationSection>();
        // .Value and .Path
        configSection.Setup(s => s.Value).Returns("~/TestObjectStorage");
        configSection.Setup(s => s.Path).Returns(string.Empty);
        mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configSection.Object);
        //mockConfig.Setup(c => c.GetValue<string>(It.IsAny<string>())).Returns("~/TestObjectStorage");

        var os = new ObjectStorageController(mockConfig.Object);
        return os;
    }

    [TestMethod]
    public void TestGetAll()
    {
        var os = GetTestObjectStorageController();

        var id = Guid.NewGuid();

        string objData = "{\"this\": \"is a test\"}";
        var save = os.Post("test", id, objData);
        Assert.AreEqual(typeof(OkObjectResult), save.GetType());
        Assert.AreEqual(200, ((OkObjectResult)save).StatusCode);

        var id2 = Guid.NewGuid();

        string objData2 = "{\"this\": \"is a test\"}";
        var save2 = os.Post("test", id2, objData2);
        Assert.AreEqual(typeof(OkObjectResult), save2.GetType());
        Assert.AreEqual(200, ((OkObjectResult)save2).StatusCode);

        var objList = os.Get("test", null);
        Assert.AreEqual(typeof(OkObjectResult), objList.GetType());
        Assert.AreEqual(200, ((OkObjectResult)objList).StatusCode);

        var objects = ((IEnumerable<ObjectWithId>)((OkObjectResult)objList).Value).ToList();
        Assert.AreEqual(2, objects.Count);
        Assert.AreEqual(objData, objects.Where(o => o.Id == id).First().Object);
        Assert.AreEqual(objData2, objects.Where(o => o.Id == id2).First().Object);

        // Clean up
        os.Delete("test", id);
        os.Delete("test", id2);
    }

    [TestMethod]
    public void TestList()
    {
        var os = GetTestObjectStorageController();

        var id = Guid.NewGuid();

        string objData = "{\"this\": \"is a test\"}";
        var save = os.Post("test", id, objData);
        Assert.AreEqual(typeof(OkObjectResult), save.GetType());
        Assert.AreEqual(200, ((OkObjectResult)save).StatusCode);

        var id2 = Guid.NewGuid();

        string objData2 = "{\"this\": \"is a test\"}";
        var save2 = os.Post("test", id2, objData2);
        Assert.AreEqual(typeof(OkObjectResult), save2.GetType());
        Assert.AreEqual(200, ((OkObjectResult)save2).StatusCode);

        var objList = os.List("test");
        Assert.AreEqual(typeof(OkObjectResult), objList.GetType());
        Assert.AreEqual(200, ((OkObjectResult)objList).StatusCode);

        var objects = ((IEnumerable<Guid>)((OkObjectResult)objList).Value).ToList();
        Assert.AreEqual(2, objects.Count);
        Assert.IsTrue(objects.Contains(id));
        Assert.IsTrue(objects.Contains(id2));

        // Clean up
        os.Delete("test", id);
        os.Delete("test", id2);
    }

    [TestMethod]
    public void TestAddingWithoutId()
    {
        var os = GetTestObjectStorageController();

        string objData = "{\"this\": \"is a test\"}";
        var save = os.Post("test", null, objData);
        Assert.AreEqual(typeof(OkObjectResult), save.GetType());
        Assert.AreEqual(200, ((OkObjectResult)save).StatusCode);
        Assert.AreEqual(typeof(Guid), ((OkObjectResult)save).Value.GetType());
        var id = ((Guid)((OkObjectResult)save).Value);


        var objList = os.List("test");
        var objects = ((IEnumerable<Guid>)((OkObjectResult)objList).Value).ToList();
        Assert.IsTrue(objects.Contains(id));

        var objAllData = os.Get("test", null);
        var allData = ((IEnumerable<ObjectWithId>)((OkObjectResult)objAllData).Value).ToList();
        Assert.AreEqual(objData, allData.First().Object);

        os.Delete("test", id);
    }

    [TestCleanup]
    public void TestCleanup() 
    {
        foreach (var f in Directory.EnumerateFiles($"{Environment.GetFolderPath(SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}TestObjectStorage"))
        {
            File.Delete(f);
        }
    }
}