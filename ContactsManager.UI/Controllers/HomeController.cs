﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature is not null)
            {
                if (exceptionHandlerPathFeature.Error is not null)
                {
                    ViewBag.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
                }
            }
            return View(); // Views/Shared/Error
        }
    }
}
