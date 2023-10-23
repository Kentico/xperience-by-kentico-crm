using DancingGoat.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace DancingGoat.ViewComponents
{
    /// <summary>
    /// Cafe card section view component.
    /// </summary>
    public class CafeCardSectionViewComponent : ViewComponent
    {
        public ViewViewComponentResult Invoke(IEnumerable<CafeViewModel> cafes) => View("~/Components/ViewComponents/CafeCardSection/Default.cshtml", cafes.Take(3));
    }
}
