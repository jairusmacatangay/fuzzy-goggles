using Microsoft.AspNetCore.Mvc;

namespace VitasysEHRWeb.Areas
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewData["ErrorCode"] = Convert.ToString(statusCode);
            ViewData["ErrorMessage"] = statusCode switch
            {
                // bad request
                400 => "Oops! The page isn't working right now.",
                // unauthorized
                401 => "Oops! You are not authorized to access the page.",
                // forbidden
                403 => "Oops! You don't have permission to access this resource.",
                // not found
                404 => "Oops! The page you requested wasn't found.",
                // internal server error
                500 => "Oops! It's us, not you. There was an internal server error occurred.",
                // bad gateway
                502 => "Oops! Bad gateway encountered.",
                // service is unavailable
                503 => "Oops! Service is temporarily unavailable.",
                // gateway timeout
                504 => "Oops! This page is taking way too long to load.",
                // default
                _ => "Oops! An error occurred while processing your request.",
            };
            return View("Error");
        }
    }
}
