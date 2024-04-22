using CMS.OnlineForms;
using CMS.OnlineForms.Types;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DancingGoat.Controllers;

[Route("[controller]/[action]")]
public class TestController : Controller
{
    [HttpGet("{id:int}")]
    public IActionResult FormLead(int id)
    {
        var item = BizFormItemProvider.GetItem(id, DancingGoatContactUsItem.CLASS_NAME);
        item.Update();
        return Ok();
    }

    public IActionResult TestDate()
    {
        string jsonString = "{\"CreatedDate\":\"2024-01-28T16:43:35.000+0000\"}";
        var myObject = JsonConvert.DeserializeObject<MyObject>(jsonString);
        var myObject2 = JsonSerializer.Deserialize<MyObject>(jsonString);

        Console.WriteLine($"CreatedDate: {myObject.CreatedDate}");
        return Ok();
    }

    public class MyObject
    {
        [JsonProperty("CreatedDate")]
        public DateTimeOffset CreatedDate { get; set; }
    }
}