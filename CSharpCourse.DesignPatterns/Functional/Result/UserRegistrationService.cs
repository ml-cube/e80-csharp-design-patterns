using LanguageExt;
using LanguageExt.Common;

namespace CSharpCourse.DesignPatterns.Functional.Result;

internal class User
{
    public string Email { get; }
    public string HashedPassword { get; }

    public User(string email, string hashedPassword)
    {
        Email = email;
        HashedPassword = hashedPassword;
    }
}

internal interface IPasswordHasher
{
    string HashPassword(string password);
}

internal interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task SaveUserAsync(User user);
    Task<User?> GetUserAsync(string email);
}

internal interface IEmailValidator
{
    bool IsValid(string email);
}

public class UserRegistrationException : Exception
{
    public string Code { get; }

    public UserRegistrationException(string code, string message) : base(message)
    {
        Code = code;
    }
}

internal class UserRegistrationService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly IEmailValidator _emailValidator;

    // Bridge pattern
    public UserRegistrationService(
        IPasswordHasher passwordHasher,
        IUserRepository userRepository,
        IEmailValidator emailValidator)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _emailValidator = emailValidator;
    }

    // The consumer of this method will have to handle the error cases
    // by catching the exception. We can document the possible exceptions
    // but we cannot force the consumer to handle them.
    public async Task<User> RegisterUserAsync(
        string email, string password)
    {
        // Validate email
        if (!_emailValidator.IsValid(email))
        {
            throw new UserRegistrationException(
                "INVALID_EMAIL",
                "The provided email address is invalid.");
        }

        // Check if email is already taken
        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new UserRegistrationException(
                "EMAIL_TAKEN",
                "This email address is already registered.");
        }

        // Validate password
        if (password.Length < 8)
        {
            throw new UserRegistrationException(
                "WEAK_PASSWORD",
                "Password must be at least 8 characters long.");
        }

        // Hash password
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Create user
        var user = new User(email, hashedPassword);
        await _userRepository.SaveUserAsync(user);

        return user;
    }
}
