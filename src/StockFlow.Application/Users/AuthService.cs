using Microsoft.AspNetCore.Identity;
using StockFlow.Application.Common;
using StockFlow.Application.Users.Dtos;
using StockFlow.Domain.Users;

namespace StockFlow.Application.Users;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new Error("AUTH_INVALID_CREDENTIALS", "Invalid email or password.", ErrorType.Unauthorized);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result<LoginResponse>.Failure(invalidCredentialsError);
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Result<LoginResponse>.Failure(invalidCredentialsError);
        }

        if (!user.IsActive)
        {
            var inactiveError = new Error("AUTH_USER_INACTIVE", "This account has been deactivated.", ErrorType.Forbidden);
            return Result<LoginResponse>.Failure(inactiveError);
        }

        var (token, expiresAtUtc) = _jwtTokenGenerator.Generate(user);
        var userResponse = new UserResponse(user.Id, user.Email, user.Role.ToString(), user.IsActive);

        return Result<LoginResponse>.Success(new LoginResponse(token, expiresAtUtc, userResponse));
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            var notFoundError = new Error("USER_NOT_FOUND", $"User {id} was not found.", ErrorType.NotFound);
            return Result<UserResponse>.Failure(notFoundError);
        }

        return Result<UserResponse>.Success(new UserResponse(user.Id, user.Email, user.Role.ToString(), user.IsActive));
    }

    public async Task<Result<UserResponse>> SetStatusAsync(int actorId, int targetId, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (actorId == targetId && !request.IsActive)
        {
            var selfDeactivateError = new Error("USER_CANNOT_DEACTIVATE_SELF", "You cannot deactivate your own account.", ErrorType.Validation);
            return Result<UserResponse>.Failure(selfDeactivateError);
        }

        var user = await _userRepository.GetByIdAsync(targetId, cancellationToken);
        if (user is null)
        {
            var notFoundError = new Error("USER_NOT_FOUND", $"User {targetId} was not found.", ErrorType.NotFound);
            return Result<UserResponse>.Failure(notFoundError);
        }

        user.IsActive = request.IsActive;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserResponse>.Success(new UserResponse(user.Id, user.Email, user.Role.ToString(), user.IsActive));
    }

    public async Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Role>(request.Role, ignoreCase: true, out var role))
        {
            var invalidRoleError = new Error("USER_INVALID_ROLE", $"'{request.Role}' is not a valid role.", ErrorType.Validation);
            return Result<UserResponse>.Failure(invalidRoleError);
        }

        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            var duplicateEmailError = new Error("USER_EMAIL_DUPLICATE", $"A user with email '{request.Email}' already exists.", ErrorType.Conflict);
            return Result<UserResponse>.Failure(duplicateEmailError);
        }

        var user = new User
        {
            Email = request.Email,
            Role = role,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var created = await _userRepository.AddAsync(user, cancellationToken);

        return Result<UserResponse>.Success(new UserResponse(created.Id, created.Email, created.Role.ToString(), created.IsActive));
    }
}
