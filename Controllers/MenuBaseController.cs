using IPOWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace IPOWeb.Controllers
{
    public class MenuBaseController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var menuJson = HttpContext.Session.GetString("UserMenus");

                if (!string.IsNullOrEmpty(menuJson))
                {
                    var menus = JsonConvert.DeserializeObject<List<MenuModel>>(menuJson);

                    string currentUrl = ".." + HttpContext.Request.Path.Value;

                    var currentMenu = menus?.FirstOrDefault(x =>
                        x.menu_url.Equals(currentUrl, StringComparison.OrdinalIgnoreCase));

                    if (currentMenu != null)
                    {
                        ViewBag.canAdd = currentMenu.add_perm == "Y";
                        ViewBag.canEdit = currentMenu.mod_perm == "Y";
                        ViewBag.canView = currentMenu.view_perm == "Y";
                        ViewBag.canDelete = currentMenu.delete_perm == "Y";
                        ViewBag.canDownload = currentMenu.download_perm == "Y";
                    }
                }
            }
            catch
            {
                // Optional logging
            }

            base.OnActionExecuting(context);
        }
    }
}
