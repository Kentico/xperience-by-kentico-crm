using CMS.OnlineForms;
using CMS.OnlineForms.Types;
using Microsoft.AspNetCore.Mvc;

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
}