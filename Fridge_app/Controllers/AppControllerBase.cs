using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fridge_app.Controllers
{
    /// <summary>
    /// Base controller that provides shared helpers for all application controllers.
    /// </summary>
    public abstract class AppControllerBase : Controller
    {
        /// <summary>
        /// Returns the currently authenticated user's ID parsed from claims.
        /// </summary>
        protected int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
