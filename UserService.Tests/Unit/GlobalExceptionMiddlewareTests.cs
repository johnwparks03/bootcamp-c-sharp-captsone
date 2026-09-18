using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using UserService.Exceptions;
using UserService.Middleware;

namespace UserService.Tests.Unit;

public class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_EmailAlreadyExists_ReturnsBadRequest()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = _ =>
            throw new EmailAlreadyExistsException();

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    } 
    
    [Fact]
    public async Task InvokeAsync_UnauthorizedToken_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = _ =>
            throw new UnauthorizedTokenException();

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    } 
    
    [Fact]
    public async Task InvokeAsync_LoginFailed_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = _ =>
            throw new LoginFailedException();

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    } 
    
    [Fact]
    public async Task InvokeAsync_SuspendedUser_ReturnsBadRequest()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = _ =>
            throw new SuspendedUserException();

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    } 
    
    [Fact]
    public async Task InvokeAsync_NotFound_ReturnsBadRequest()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = _ =>
            throw new NotFoundException();

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    } 
    
    [Fact]
    public async Task InvokeAsync_Unhandled_ReturnsInternalServerError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = _ =>
            throw new Exception();

        var middleware = new GlobalExceptionMiddleware(
            next,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    } 
}