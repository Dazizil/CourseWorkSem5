using Microsoft.AspNetCore.Diagnostics;
using SocialNetwork.Application.Common.Exceptions;

namespace SocialNetwork.Api.Infrastructure;

/// <summary>Преобразует исключения прикладного слоя в RFC 7807 ProblemDetails.</summary>
public class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not AppException appException)
            return false;

        httpContext.Response.StatusCode = appException switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Title = appException.Message, Status = httpContext.Response.StatusCode },
        });
    }
}
