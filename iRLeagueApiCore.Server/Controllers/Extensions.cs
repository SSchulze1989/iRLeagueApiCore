﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace iRLeagueApiCore.Server.Controllers
{
    public static class Extensions
    {
        public static ActionResult ToActionResult(this ValidationException ex)
        {
            var response = new
            {
                Status = "Bad request",
                Errors = ex.Errors.Select(error => new
                {
                    Property = error.PropertyName,
                    Error = error.ErrorMessage,
                    Value = error.AttemptedValue
                })
            };
            return new BadRequestObjectResult(response);
        }
    }
}
