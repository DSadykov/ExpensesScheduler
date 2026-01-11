using System.Net;

using Azure.Core;

using ExpensesScheduler.AuthorizationService.DTO.Requests;
using ExpensesScheduler.AuthorizationService.DTO.Responses;
using ExpensesScheduler.AuthorizationService.Services;

using Microsoft.AspNetCore.Mvc;

namespace ExpensesScheduler.AuthorizationService.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthorizationController(IAuthorizationService authorizationService) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<AuthenticateResponse>> Authenticate(AuthenticateRequest request)
    {
        var authenticateResult = await authorizationService.AuthenticateAsync(request);
        return ActionResultByCode(authenticateResult);
    }

    [HttpPost]
    public async Task<ActionResult<AuthenticateResponse>> Register(AuthenticateRequest request)
    {
        var registerResult = await authorizationService.Register(request);
        return ActionResultByCode(registerResult);
    }

    [HttpGet]
    public async Task<ActionResult<AuthenticateResponse>> ConfirmEmail([FromQuery] Guid userID)
    {
        var result = await authorizationService.ConfirmEmailAsync(userID);
        return ActionResultByCode(result);
    }
    private ActionResult<T> ActionResultByCode<T>(T result)
        where T : ResponseBase => result.StatusCode switch
        {
            HttpStatusCode.OK => (ActionResult<T>)Ok(result),
            HttpStatusCode.BadRequest => (ActionResult<T>)BadRequest(result),
            HttpStatusCode.Unauthorized => (ActionResult<T>)Unauthorized(result),
            _ => throw new NotImplementedException(),
        };
}
