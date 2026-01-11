using System.Net;

namespace ExpensesScheduler.AuthorizationService.DTO.Responses;

public class AuthenticateResponse : ResponseBase
{
    public string? Token { get; set; }
}