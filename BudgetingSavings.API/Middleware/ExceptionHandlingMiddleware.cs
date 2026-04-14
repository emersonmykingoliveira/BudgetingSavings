using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BudgetingSavings.BusinessLayer.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ArgumentException ex)
        {
            await CreateProblemDetails(context, HttpStatusCode.BadRequest, ex.Message);
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