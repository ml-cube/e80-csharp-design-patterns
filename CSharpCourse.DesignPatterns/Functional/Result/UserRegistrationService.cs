using LanguageExt;
using LanguageExt.Common;

namespace CSharpCourse.DesignPatterns.Functional.Result;

// The result pattern is a way to implement type discrimination
// (or type union) in C#.

// It is a struct so it can be allocated on the stack
internal readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    // Construct from a value
    public Result(TValue value)
    {
        _value = value;
        _error = default;
        IsSuccess = true;
    }

    // Construct from an error
    public Result(TError error)
    {
        _value = default;
        _error = error;
        IsSuccess = false;
    }

    // Implicit conversion from a value
    public static implicit operator Result<TValue, TError>(TValue value) => new(value);

    // Implicit conversion from an error
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    // Functional programming helpers
    // We DO NOT give direct access to the value or error, so we
    // force the user to handle both paths
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<TError, TResult> onError) =>
        IsSuccess ? onSuccess(_value!) : onError(_error!);
}

// We don't use a struct since a reference type is easier to pass along
// if we don't want to immediately handle the error
internal record UserRegistrationError
{
    public string Message { get; }
    public string Code { get; }

    public UserRegistrationError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}

// It is good practice to have a separate, centralized place
// for error codes
internal static class ErrorCodes
{
    public const string InvalidEmail = "INVALID_EMAIL";
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string WeakPassword = "WEAK_PASSWORD";
}

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

    public async Task<Result<User, UserRegistrationError>> RegisterUserAsync(
        string email, string password)
    {
        // Validate email
        if (!_emailValidator.IsValid(email))
        {
            // Implicit conversion from UserRegistrationError to
            // Result<User, UserRegistrationError>
            return new UserRegistrationError(
                ErrorCodes.InvalidEmail,
                "The provided email address is invalid.");
        }

        // Check if email is already taken
        if (await _userRepository.EmailExistsAsync(email))
        {
            return new UserRegistrationError(
                ErrorCodes.EmailTaken,
                "This email address is already registered.");
        }

        // Validate password
        if (password.Length < 8)
        {
            return new UserRegistrationError(
                ErrorCodes.WeakPassword,
                "Password must be at least 8 characters long.");
        }

        // Hash password
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Create user
        var user = new User(email, hashedPassword);
        await _userRepository.SaveUserAsync(user);

        // Implicit conversion from User to
        // Result<User, UserRegistrationError>
        return user;
    }
}

public class UserRegistrationException : Exception
{
    public string Code { get; }

    public UserRegistrationException(string code, string message) : base(message)
    {
        Code = code;
    }
}

internal class LangExtUserRegistrationService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly IEmailValidator _emailValidator;

    // Bridge pattern
    public LangExtUserRegistrationService(
        IPasswordHasher passwordHasher,
        IUserRepository userRepository,
        IEmailValidator emailValidator)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _emailValidator = emailValidator;
    }

    // There is an implicit conversion from User? to Option<User>
    public async Task<Option<User>> GetUserAsync(string email)
        => await _userRepository.GetUserAsync(email);

    public async Task<Result<User>> RegisterUserAsync(
        string email, string password)
    {
        // Validate email
        if (!_emailValidator.IsValid(email))
        {
            // We can create a failure result directly from the exception.
            // IMPORTANT: We're not "throwing" the exception, we're just
            // creating a failure result from it.
            return new Result<User>(new UserRegistrationException(
                ErrorCodes.InvalidEmail,
                "The provided email address is invalid."));
        }

        // Check if email is already taken
        if (await _userRepository.EmailExistsAsync(email))
        {
            return new Result<User>(new UserRegistrationException(
                ErrorCodes.EmailTaken,
                "This email address is already registered."));
        }

        // Validate password
        if (password.Length < 8)
        {
            return new Result<User>(new UserRegistrationException(
                ErrorCodes.WeakPassword,
                "Password must be at least 8 characters long."));
        }

        // Hash password
        string hashedPassword = _passwordHasher.HashPassword(password);

        // Create user
        var user = new User(email, hashedPassword);
        await _userRepository.SaveUserAsync(user);

        return user;
    }
}
