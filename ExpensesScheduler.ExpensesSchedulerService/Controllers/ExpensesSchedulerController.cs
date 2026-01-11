using System.Net;

using ExpensesScheduler.Authorization.Attributes;
using ExpensesScheduler.Authorization.Extensions;
using ExpensesScheduler.ExpensesSchedulerService.DB;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;
using ExpensesScheduler.ExpensesSchedulerService.Services;

using Microsoft.AspNetCore.Mvc;

namespace ExpensesScheduler.ExpensesSchedulerService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ExpensesSchedulerController
    (ScheduledExpensesDbContext dbContext,
    ISheduledExpensesService expensesService,
    INotificationMessageSender notificationMessageSender) : ControllerBase
{
    [HttpGet("TestSendNotifications")]
    public async Task<ActionResult> TestSendNotifications()
    {
        var scheduledExpenses = dbContext.GetScheduledExpenses(DateTime.UtcNow);
        var sentExpenses = await notificationMessageSender.SendNotificationsAsync(scheduledExpenses);
        await dbContext.UpdateSentScheduledExpenses(sentExpenses);
        return Ok();
    }

    [HttpGet("GetScheduledExpenses")]
    public async Task<ActionResult<ScheduledExpensesResponse>> Get()
    {
        var userID = HttpContext.GetUserID();

        ScheduledExpensesResponse result =
            await expensesService.GetScheduledExpensesByUserIDAsync(userID);

        return ActionResultByCode(result);
    }
    [HttpPost("AddScheduledExpense")]
    public async Task<ActionResult<AddScheduledExpenseResponse>> AddScheduledExpense(AddScheduledExpenseRequest request)
    {
        var result = await expensesService.AddScheduledExpenseAsync(request, HttpContext.GetUserID());
        return ActionResultByCode(result);
    }

    [HttpPut("UpdateScheduledExpense")]
    public async Task<ActionResult<UpdateScheduledExpenseResponse>> UpdateScheduledExpense(UpdateScheduledExpenseRequest request)
    {
        var result = await expensesService.UpdateScheduledExpenseAsync(request, HttpContext.GetUserID());
        return ActionResultByCode(result);
    }

    [HttpDelete("DeleteScheduledExpense")]
    public async Task<ActionResult<BaseResponse>> DeleteScheduledExpense([FromBody] DeleteScheduledExpenseRequest request)
    {
        var result = await expensesService.DeleteScheduledExpenseAsync(request, HttpContext.GetUserID());
        return ActionResultByCode(result);
    }

    [HttpGet("GetExpensesHistory")]
    public async Task<ActionResult<GetExpensesHistoryResponse>> GetExpensesHistory([FromQuery]DateTime start, [FromQuery] DateTime end)
    {
        if (start > end)
        {
            return ActionResultByCode(new GetExpensesHistoryResponse() 
            { StatusCode = HttpStatusCode.BadRequest, 
                ErrorMessage = "Start date must be less than or equal to end date"});
        }
        var result = await expensesService.GetExpensesHistoryAsync(HttpContext.GetUserID(),start, end);
        return ActionResultByCode(result);
    }

    private ActionResult<T> ActionResultByCode<T>(T result)
        where T : BaseResponse => result.StatusCode switch
        {
            HttpStatusCode.OK => (ActionResult<T>)Ok(result),
            HttpStatusCode.BadRequest => (ActionResult<T>)BadRequest(result),
            _ => throw new NotImplementedException(),
        };
}
