using System.Net;

namespace ExpensesScheduler.AuthorizationService.DTO.Responses;

public class ResponseBase
{
    public string? ErrorMessage { get; set; }
    public required HttpStatusCode StatusCode { get; set; }
}