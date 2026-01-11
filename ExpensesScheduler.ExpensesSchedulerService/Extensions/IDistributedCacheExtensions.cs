using System.Text.Json;

using ExpensesScheduler.ExpensesSchedulerService.DB.Models;

using Microsoft.Extensions.Caching.Distributed;

namespace ExpensesScheduler.ExpensesSchedulerService.Extensions;

public static class IDistributedCacheExtensions
{
    public static async Task<T?> GetValueAsync<T>(this IDistributedCache distributedCache, string key)
    {
        var cachedString = await distributedCache.GetStringAsync(key);

        if (cachedString is not null)
        {
            return JsonSerializer.Deserialize<T>(cachedString);
        }

        return default;
    }

    public static async Task SetValueAsync<T>(this IDistributedCache distributedCache, string key, T value,
        TimeSpan? expirationTime = null)
    {
        await distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromHours(1),
        });
    }

    public static string GetKeyForScheduledExpense(this IDistributedCache distributedCache, Guid userID)
    {
        return $"ScheduledExpense:userid:{userID}";
    }
    public static string GetKeyForExpensesHistory(this IDistributedCache distributedCache, Guid userID)
    {
        return $"ExpensesHistory:userid:{userID}";
    }
}
