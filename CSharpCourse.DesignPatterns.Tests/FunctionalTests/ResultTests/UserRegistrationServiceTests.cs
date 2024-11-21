using CSharpCourse.DesignPatterns.Functional.Result;
using Moq;

namespace CSharpCourse.DesignPatterns.Tests.FunctionalTests.ResultTests;

public class UserRegistrationServiceTests
{
    [Fact]
    public async Task Success()
    {
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        userRepository
            .Setup(x => x.SaveUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var emailValidator = new Mock<IEmailValidator>();
        emailValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        var service = new UserRegistrationService(
            passwordHasher.Object,
            userRepository.Object,
            emailValidator.Object);

        var result = await service.RegisterUserAsync("email", "password");

        Assert.True(result.IsSuccess);

        var registrationSuccessful = result.Match(
            user => true,
            error => false
        );

        Assert.True(registrationSuccessful);
    }

    [Fact]
    public async Task Error()
    {
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        userRepository
            .Setup(x => x.SaveUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var emailValidator = new Mock<IEmailValidator>();
        emailValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        var service = new UserRegistrationService(
            passwordHasher.Object,
            userRepository.Object,
            emailValidator.Object);

        var result = await service.RegisterUserAsync("email", "password");

        Assert.True(result.IsError);

        var errorCode = result.Match(
            user => string.Empty,
            error => error.Code
        );

        Assert.Equal(ErrorCodes.EmailTaken, errorCode);
    }

    // There is currently a proposal to introduce Option<T> and
    // Result<T> in the C# language.
    // https://github.com/dotnet/csharplang/blob/main/proposals/TypeUnions.md

    // We will use a library called LanguageExt that provides
    // functional extensions for C#.
    // https://github.com/louthy/language-ext

    [Fact]
    public async Task LangExtSuccess()
    {
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        userRepository
            .Setup(x => x.SaveUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        userRepository
            .Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(new User("email", "hashedPassword"));

        var emailValidator = new Mock<IEmailValidator>();
        emailValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        var service = new LangExtUserRegistrationService(
            passwordHasher.Object,
            userRepository.Object,
            emailValidator.Object);

        var user = await service.GetUserAsync("email");

        Assert.True(user.IsSome);

        var userExists = user.Match(
            user => true,
            () => false
        );

        Assert.True(userExists);

        var result = await service.RegisterUserAsync("email", "password");

        Assert.True(result.IsSuccess);

        var registrationSuccessful = result.Match(
            user => true,
            error => false
        );

        Assert.True(registrationSuccessful);
    }

    [Fact]
    public async Task LangExtError()
    {
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        userRepository
            .Setup(x => x.SaveUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        userRepository
            .Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var emailValidator = new Mock<IEmailValidator>();
        emailValidator
            .Setup(x => x.IsValid(It.IsAny<string>()))
            .Returns(true);

        var service = new LangExtUserRegistrationService(
            passwordHasher.Object,
            userRepository.Object,
            emailValidator.Object);

        var user = await service.GetUserAsync("email");

        Assert.True(user.IsNone);

        var userExists = user.Match(
            user => true,
            () => false
        );

        Assert.False(userExists);

        var result = await service.RegisterUserAsync("email", "password");

        Assert.True(result.IsFaulted);

        var errorCode = result.Match(
            user => string.Empty,
            error => error switch
            {
                UserRegistrationException e => e.Code,
                _ => string.Empty
            }
        );

        Assert.Equal(ErrorCodes.EmailTaken, errorCode);
    }
}
