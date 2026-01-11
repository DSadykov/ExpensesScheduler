
namespace ExpensesScheduler.AuthorizationService.Services;

public interface IJwtService
{
    string GenerateJwtToken(Guid userId);
}