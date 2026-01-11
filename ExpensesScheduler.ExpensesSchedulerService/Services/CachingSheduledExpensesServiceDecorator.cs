using System.Net;

using ExpensesScheduler.ExpensesSchedulerService.DB.Models;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Requests;
using ExpensesScheduler.ExpensesSchedulerService.DTO.Responses;
using ExpensesScheduler.ExpensesSchedulerService.Extensions;
using ExpensesScheduler.ExpensesSchedulerService.Services.Models;

using Microsoft.Extensions.Caching.Distributed;

namespace ExpensesScheduler.ExpensesSchedulerService.Services;

public class CachingSheduledExpensesServiceDecorator
    (ISheduledExpensesService inner,
    IDistributedCache cache,
    ILogger<CachingSheduledExpensesServiceDecorator> logger) : ISheduledExpensesService
{

    public async Task<UpdateScheduledExpenseResponse> UpdateScheduledExpenseAsync(UpdateScheduledExpenseRequest request, Guid userID)
    {
        var response = await inner.UpdateScheduledExpenseAsync(request, userID);

        if (response.StatusCode == HttpStatusCode.OK && response.UpdatedExpense is not null)
        {
            try
            {
                var key = cache.GetKeyForScheduledExpense(userID);
                var cachedExpenses = await cache.GetValueAsync<Dictionary<Guid, ScheduledExpenseModel>>(key);
                if (cachedExpenses is not null)
                {
                    cachedExpenses[request.ExpenseID] = response.UpdatedExpense;
                    await cache.SetValueAsync(key, cachedExpenses);
                    logger.LogDebug("Cache updated for user {UserId} after updating expense {ExpenseId}", userID, request.ExpenseID);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update cache after UpdateScheduledExpenseAsync for user {UserId}", userID);
            }
        }

        return response;
    }

    public async Task<AddScheduledExpenseResponse> AddScheduledExpenseAsync(AddScheduledExpenseRequest request, Guid userID)
    {
        var response = await inner.AddScheduledExpenseAsync(request, userID);

        if (response.StatusCode == HttpStatusCode.OK && response.CreatedExpense is not null)
        {
            try
            {
                var key = cache.GetKeyForScheduledExpense(userID);
                var cachedExpenses = await cache.GetValueAsync<Dictionary<Guid, ScheduledExpenseModel>>(key);
                if (cachedExpenses is not null)
                {
                    cachedExpenses[response.CreatedExpense.Id] = response.CreatedExpense;
                    await cache.SetValueAsync(key, cachedExpenses);
                    logger.LogDebug("Cache updated for user {UserId} after adding expense {ExpenseId}", userID, response.CreatedExpense.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update cache after AddScheduledExpenseAsync for user {UserId}", userID);
            }
        }

        return response;
    }

    public async Task<ScheduledExpensesResponse> GetScheduledExpensesByUserIDAsync(Guid userID)
    {
        try
        {
            var key = cache.GetKeyForScheduledExpense(userID);
            var cachedExpenses = await cache.GetValueAsync<Dictionary<Guid, ScheduledExpenseModel>>(key);
            if (cachedExpenses is not null)
            {
                logger.LogDebug("Cache hit for scheduled expenses of user {UserId}. ItemCount: {Count}", userID, cachedExpenses.Count);
                return new ScheduledExpensesResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    ScheduledExpenses = [.. cachedExpenses.Values],
                    UserID = userID
                };
            }

            logger.LogDebug("Cache miss for scheduled expenses of user {UserId}. Delegating to inner service.", userID);
            var response = await inner.GetScheduledExpensesByUserIDAsync(userID);
            if (response.StatusCode == HttpStatusCode.OK && response.ScheduledExpenses is not null)
            {
                try
                {
                    var dict = response.ScheduledExpenses.ToDictionary(x => x.Id, x => x);
                    await cache.SetValueAsync(key, dict);
                    logger.LogDebug("Cache set for user {UserId} after DB load", userID);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to set cache for scheduled expenses for user {UserId}", userID);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in caching layer while getting scheduled expenses for user {UserId}", userID);
            // Fallback to inner service
            return await inner.GetScheduledExpensesByUserIDAsync(userID);
        }
    }

    public async Task<BaseResponse> DeleteScheduledExpenseAsync(DeleteScheduledExpenseRequest request, Guid userID)
    {
        var response = await inner.DeleteScheduledExpenseAsync(request, userID);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            try
            {
                var key = cache.GetKeyForScheduledExpense(userID);
                var cachedExpenses = await cache.GetValueAsync<Dictionary<Guid, ScheduledExpenseModel>>(key);
                if (cachedExpenses is not null)
                {
                    cachedExpenses.Remove(request.ExpenseID);
                    await cache.SetValueAsync(key, cachedExpenses);
                    logger.LogDebug("Cache updated for user {UserId} after deleting expense {ExpenseId}", userID, request.ExpenseID);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update cache after DeleteScheduledExpenseAsync for user {UserId}", userID);
            }
        }

        return response;
    }

    public async Task<GetExpensesHistoryResponse> GetExpensesHistoryAsync(Guid userID, DateTime start, DateTime end)
    {
        var key = cache.GetKeyForExpensesHistory(userID);
        var cachedValue = await cache.GetValueAsync<ExpensesHistoryCacheModel>(key);
        var resultExpenses = new List<ExpensesHistoryModel>();

        try
        {
            if (cachedValue is not null)
            {
                cachedValue.ExpensesHistoryModels ??= [];

                logger.LogDebug("Cache hit for expenses history for user {UserId}. CachedRange: {CachedStart} - {CachedEnd}, CachedCount: {Count}",
                    userID, cachedValue.Start, cachedValue.End, cachedValue.ExpensesHistoryModels?.Count ?? 0);

                if (cachedValue.Start.Date > start.Date)
                {
                    logger.LogInformation("Expanding cached history earlier for user {UserId}: loading {Start} - {OldStart}", userID, start, cachedValue.Start);
                    var earlierResp = await inner.GetExpensesHistoryAsync(userID, start, cachedValue.Start);
                    var earlier = earlierResp.ExpensesHistory ?? new List<ExpensesHistoryModel>();
                    if (earlier is not null && earlier.Count > 0)
                    {
                        cachedValue.ExpensesHistoryModels.AddRange(earlier);
                        logger.LogDebug("Added {Count} earlier history items to cache for user {UserId}", earlier.Count, userID);
                    }
                    cachedValue.Start = start;
                }

                if (cachedValue.End.Date < end.Date)
                {
                    logger.LogInformation("Expanding cached history later for user {UserId}: loading {OldEnd} - {End}", userID, cachedValue.End, end);
                    var laterResp = await inner.GetExpensesHistoryAsync(userID, cachedValue.End, end);
                    var later = laterResp.ExpensesHistory ?? new List<ExpensesHistoryModel>();
                    if (later is not null && later.Count > 0)
                    {
                        cachedValue.ExpensesHistoryModels.AddRange(later);
                        logger.LogDebug("Added {Count} later history items to cache for user {UserId}", later.Count, userID);
                    }
                    cachedValue.End = end;
                }

                if (cachedValue.ExpensesHistoryModels.Count > 1)
                {
                    cachedValue.ExpensesHistoryModels = [.. cachedValue.ExpensesHistoryModels
                        .GroupBy(x => x.Id)
                        .Select(g => g.First())
                        .OrderBy(x => x.DateTime)];
                    logger.LogDebug("De-duplicated and sorted cached history for user {UserId}. NewCount: {Count}", userID, cachedValue.ExpensesHistoryModels.Count);
                }

                resultExpenses = [.. cachedValue.ExpensesHistoryModels.Where(x => x.DateTime >= start && x.DateTime <= end)];

                await cache.SetValueAsync(key, cachedValue);
                logger.LogDebug("Persisted expanded history cache for user {UserId}. Range: {Start} - {End}", userID, cachedValue.Start, cachedValue.End);
            }
            else
            {
                logger.LogInformation("Cache miss for expenses history for user {UserId}. Loading {Start} - {End} from inner service.", userID, start, end);
                var fetchedResp = await inner.GetExpensesHistoryAsync(userID, start, end);
                var fetched = fetchedResp.ExpensesHistory ?? new List<ExpensesHistoryModel>();
                resultExpenses = fetched is null ? [] : [.. fetched];

                await cache.SetValueAsync(key, new ExpensesHistoryCacheModel()
                {
                    ExpensesHistoryModels = resultExpenses,
                    End = end,
                    Start = start,
                });

                logger.LogInformation("Fetched {Count} history items from inner service and cached for user {UserId}. Range: {Start} - {End}", resultExpenses.Count, userID, start, end);
            }

            return new() { StatusCode = HttpStatusCode.OK, ExpensesHistory = resultExpenses, UserID = userID };
        }
        catch (Exception ex)
        {
            var correlationId = Guid.NewGuid();
            logger.LogError(ex, "Error in caching layer while retrieving expenses history for user {UserId} (correlationId: {CorrelationId}). Range: {Start:o} - {End:o}", userID, correlationId, start, end);
            // Fallback to inner service
            var fallback = await inner.GetExpensesHistoryAsync(userID, start, end);
            return new() { StatusCode = fallback.StatusCode, ExpensesHistory = fallback.ExpensesHistory, UserID = userID, ErrorMessage = fallback.ErrorMessage };
        }
    }
}
