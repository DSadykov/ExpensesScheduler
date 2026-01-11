

using ExpensesScheduler.AuthorizationService.DTO.Requests;
using ExpensesScheduler.AuthorizationService.DTO.Responses;

namespace ExpensesScheduler.AuthorizationService.Services;

public interface IAuthorizationService
{
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request);
    Task<AuthenticateResponse> ConfirmEmailAsync(Guid userID);
    Task<AuthenticateResponse> Register(AuthenticateRequest request);
}