
using System.Net;

using ExpensesScheduler.AuthorizationService.DB;
using ExpensesScheduler.AuthorizationService.DTO.Requests;
using ExpensesScheduler.AuthorizationService.DTO.Responses;
using ExpensesScheduler.Messaging.DTO;
using ExpensesScheduler.Messaging.Producer;

namespace ExpensesScheduler.AuthorizationService.Services;

public class AuthorizationService(UsersDbContext usersDbContext,
                                  IJwtService jwtService,
                                  IPasswordHasher passwordHasher,
                                  IMessageProducer<NewUserCreatedMessage> producer) : IAuthorizationService
{
    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request)
    {
        var user = await usersDbContext
            .GetUserAsync(request.Email);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.Password))
        {
            return new()
            {
                ErrorMessage = "Wrong email or password!",
                StatusCode = HttpStatusCode.Unauthorized
            };
        }

        if (!user.IsEmailConfirmed)
        {
            return new()
            {
                ErrorMessage = "Email is not confirmed!",
                StatusCode = HttpStatusCode.Unauthorized
            };
        }

        var token = jwtService.GenerateJwtToken(user.ID);

        return new() { Token = token, StatusCode = HttpStatusCode.OK };
    }

    public async Task<AuthenticateResponse> ConfirmEmailAsync(Guid userID)
    {
        var user = usersDbContext.UserModels.FirstOrDefault(x => x.ID == userID);

        if (user is null)
        {
            return new()
            {
                ErrorMessage = "User not found",
                StatusCode = HttpStatusCode.BadRequest
            };
        }

        user.IsEmailConfirmed = true;

        await usersDbContext.SaveChangesAsync();

        return new() { StatusCode = HttpStatusCode.OK };
    }

    public async Task<AuthenticateResponse> Register(AuthenticateRequest request)
    {
        if (usersDbContext.CheckUserExists(request.Email))
        {
            return new()
            {
                ErrorMessage = "Email is already taken!",
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        var createdUser = await usersDbContext
            .AddUserAsync(passwordHasher.HashPassword(request.Password), request.Email);

        await producer.ProduceAsync(new() 
        { Email = createdUser.Email, UserID = createdUser.ID }, default);

        return new() 
        {
            StatusCode = HttpStatusCode.OK
        };
    }
}
