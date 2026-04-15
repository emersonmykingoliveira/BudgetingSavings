using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BudgetingSavings.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            StringBuilder sb = new StringBuilder();
            ex.Errors.ToList().ForEach(error => sb.AppendLine(error.ErrorMessage));

            await CreateProblemDetails(context, HttpStatusCode.BadRequest, sb.ToString());
        }
        catch (Exception)
        {
            await CreateProblemDetails(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private async Task CreateProblemDetails(HttpContext context, HttpStatusCode httpStatusCode, string errorMessage)
    {
        context.Response.StatusCode = (int)httpStatusCode;
        context.Response.ContentType = "application/json";

        var problem = new ProblemDetails
        {
            Status = (int)httpStatusCode,
            Detail = errorMessage
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}