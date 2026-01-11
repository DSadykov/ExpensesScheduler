using System.Net;
using System.Text.Json;

using ExpensesScheduler.ExpensesSchedulerService.DB;
using ExpensesScheduler.ExpensesSchedulerService.DB.Models;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;
using ExpensesScheduler.ExpensesSchedulerService.Extensions;
using ExpensesScheduler.ExpensesSchedulerService.Services.Models;

namespace ExpensesScheduler.ExpensesSchedulerService.Services;

public class SheduledExpensesService(ScheduledExpensesDbContext dbContext,
    ILogger<SheduledExpensesService> logger) : ISheduledExpensesService
{
    public async Task<UpdateScheduledExpenseResponse> UpdateScheduledExpenseAsync(UpdateScheduledExpenseRequest request, Guid userID)
    {
        logger.LogInformation("UpdateScheduledExpenseAsync called for UserId: {UserId}, ExpenseId: {ExpenseId}", userID, request.ExpenseID);

        try
        {
            var entity = dbContext
            .ScheduledExpenses
            .FirstOrDefault(x => x.Id == request.ExpenseID && x.UserID == userID);

            if (entity == null)
            {
                logger.LogWarning("Requested expense not found. UserId: {UserId}, ExpenseId: {ExpenseId}", userID, request.ExpenseID);
                return new() { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Requested expense not found" };
            }

            entity.OneTimeOnly = request.OneTimeOnly ?? entity.OneTimeOnly;
            entity.Description = request.Description ?? entity.Description;

            if (request.HappensInDays is not null)
            {
                entity.HappensInDays = request.HappensInDays;
                entity.EveryMonth = false;
            }
            else if (request.ScheduleType is not null)
            {
                if (request.ScheduleType == ScheduleTypes.PerMonth)
                {
                    entity.EveryMonth = true;
                    entity.HappensInDays = null;
                }
                else
                {
                    entity.EveryMonth = false;
                    entity.HappensInDays = request.ScheduleType switch
                    {
                        ScheduleTypes.PerDay => 1,
                        ScheduleTypes.PerWeek => 7,
                    };
                }
            }

            await dbContext.SaveChangesAsync();

            var updatedEntity = await dbContext.ScheduledExpenses.FindAsync(entity.Id);

            logger.LogInformation("Scheduled expense updated. UserId: {UserId}, ExpenseId: {ExpenseId}", userID, entity.Id);

            return new()
            {
                StatusCode = HttpStatusCode.OK,
                UpdatedExpense = updatedEntity
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating scheduled expense for UserId: {UserId}, Request: {Request}", userID, JsonSerializer.Serialize(request));
            return new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = $"Eror while updating entity" +
                $" {JsonSerializer.Serialize(request)}"
            };
        }

    }
    public async Task<AddScheduledExpenseResponse> AddScheduledExpenseAsync(AddScheduledExpenseRequest request, Guid userID)
    {
        logger.LogInformation("AddScheduledExpenseAsync called for UserId: {UserId}, Amount: {Amount}", userID, request.Amount);

        try
        {
            ScheduledExpenseModel entity = new()
            {
                UserID = userID,
                Amount = request.Amount,
                CreatedDate = DateTime.UtcNow.Date,
                Description = request.Description,
                HappensInDays = request.HappensInDays ??
                request.ScheduleType switch
                {
                    ScheduleTypes.PerDay => 1,
                    ScheduleTypes.PerWeek => 7,
                    _ => throw new ArgumentException($"Unsupported ScheduleType - {request.ScheduleType}")
                },
                EveryMonth = request.ScheduleType == ScheduleTypes.PerMonth,
                OneTimeOnly = request.OneTimeOnly,
                Id = Guid.NewGuid(),
            };

            await dbContext.ScheduledExpenses.AddAsync(entity);

            await dbContext.SaveChangesAsync();

            var createdEntity = await dbContext.ScheduledExpenses.FindAsync(entity.Id);

            logger.LogInformation("Scheduled expense created. UserId: {UserId}, ExpenseId: {ExpenseId}", userID, createdEntity.Id);

            return new()
            {
                CreatedExpense = createdEntity,
                StatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding scheduled expense for UserId: {UserId}, Request: {Request}", userID, JsonSerializer.Serialize(request));
            // add logger
            return new()
            {
                ErrorMessage = $"Error while adding expense with body" +
                $" {JsonSerializer.Serialize(request)}",

                StatusCode = HttpStatusCode.BadRequest
            };
        }

    }

    public async Task<ScheduledExpensesResponse> GetScheduledExpensesByUserIDAsync(Guid userID)
    {
        logger.LogInformation("GetScheduledExpensesByUserIDAsync called for UserId: {UserId}", userID);

        Dictionary<Guid, ScheduledExpenseModel> scheduledExpenseModels;

        scheduledExpenseModels = dbContext.GetScheduledExpensesByUserID(userID) ?? new Dictionary<Guid, ScheduledExpenseModel>();

        logger.LogInformation("Loaded {Count} scheduled expenses from DB for user {UserId}", scheduledExpenseModels.Count, userID);

        return new ScheduledExpensesResponse()
        {
            StatusCode = HttpStatusCode.OK,
            ScheduledExpenses = [.. scheduledExpenseModels.Values],
            UserID = userID
        };
    }

    public async Task<BaseResponse> DeleteScheduledExpenseAsync(DeleteScheduledExpenseRequest request, Guid userID)
    {
        logger.LogInformation("DeleteScheduledExpenseAsync called for UserId: {UserId}, ExpenseId: {ExpenseId}", userID, request.ExpenseID);

        try
        {
            var entity = dbContext
            .ScheduledExpenses
            .FirstOrDefault(x => x.Id == request.ExpenseID && x.UserID == userID);
            if (entity == null)
            {
                logger.LogWarning("Requested expense to delete not found. UserId: {UserId}, ExpenseId: {ExpenseId}", userID, request.ExpenseID);
                return new() { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = "Requested expense not found" };
            }
            dbContext.ScheduledExpenses.Remove(entity);

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Scheduled expense deleted. UserId: {UserId}, ExpenseId: {ExpenseId}", userID, request.ExpenseID);

            return new()
            {
                StatusCode = HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while deleting scheduled expense for UserId: {UserId}, Request: {Request}", userID, JsonSerializer.Serialize(request));
            return new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = $"Eror while deleting entity" +
                $" {JsonSerializer.Serialize(request)}"
            };
        }
    }

    public async Task<GetExpensesHistoryResponse> GetExpensesHistoryAsync(Guid userID, DateTime start, DateTime end)
    {
        logger.LogInformation("GetExpensesHistoryAsync called for UserId: {UserId}, Start: {Start}, End: {End}", userID, start, end);

        try
        {
            var resultExpenses = dbContext.GetExpensesHistoryModels(userID, start, end) ?? new List<ExpensesHistoryModel>();

            return new() { StatusCode = HttpStatusCode.OK, ExpensesHistory = resultExpenses, UserID = userID };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving expenses history for user {UserId}. Range: {Start:o} - {End:o}", userID, start, end);
            return new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = $"Error while retrieving expenses history. Range: {start:o} - {end:o}"
            };
        }
    }
}
