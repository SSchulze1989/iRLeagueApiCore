using FluentValidation;
using iRLeagueApiCore.Server.Controllers;
using iRLeagueApiCore.Server.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace iRLeagueApiCore.Server.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultExceptionFilterAttribute : Attribute, IExceptionFilter
    {
        private readonly ILogger<DefaultExceptionFilterAttribute> _logger;

        public DefaultExceptionFilterAttribute(ILogger<DefaultExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException validationException)
            {
                _logger.LogInformation("Bad request - errors: {ValidationErrors}", 
                    validationException.Errors.Select(x => x.ErrorMessage));
                context.Result = validationException.ToActionResult();
                return;
            }
            if (context.Exception is ResourceNotFoundException notFoundException)
            {
                _logger.LogInformation("Resource not found");
                context.Result = new NotFoundResult();
            }
        }
    }
}
