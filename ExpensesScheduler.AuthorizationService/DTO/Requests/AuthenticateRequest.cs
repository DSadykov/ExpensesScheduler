namespace ExpensesScheduler.AuthorizationService.DTO.Requests;

public class AuthenticateRequest
{
    public required string Email { get;  set; }
    public required string Password { get; set; }
}