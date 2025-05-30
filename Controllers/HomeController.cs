using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RPG_dotnet.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Produces("text/html")]
        public ContentResult GetWelcomeMessage()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var swaggerUrl = $"{baseUrl}/swagger";

            var html = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Welcome</title>
                </head>
                <body style='font-family:Segoe UI,sans-serif; line-height:1.5;'>
                    <h2>👋 Welcome to Fate Phantasm API</h2>
                    <p>Please click <a href='{swaggerUrl}' target='_blank'>here</a> to explore the API documentation.</p>
                </body>
                </html>";

            return Content(html, "text/html");
        }
    }
}