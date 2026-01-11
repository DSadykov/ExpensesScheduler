using System.Net;

namespace ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;

public class BaseResponse
{
    public string? ErrorMessage { get; set; }
    public required HttpStatusCode StatusCode {  get; set; }
}
