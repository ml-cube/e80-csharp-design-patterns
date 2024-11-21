using CSharpCourse.DesignPatterns.Assignments;

namespace CSharpCourse.DesignPatterns.Tests.AssignmentTests;

public class StatelessRuleVisitorTests
{
    // We could also have used a builder to create the rules
    private readonly AndRule _passwordRule = new()
    {
        Rules =
        [
            new MinLengthRule { MinLength = 8 },
            new OrRule
            {
                Rules =
                [
                    new ContainsAtLeastOneCharacterRule
                    {
                        Characters = "0123456789"
                    },
                    new ContainsAtLeastOneCharacterRule
                    {
                        Characters = "!?%&*-_."
                    }
                ]
            }
        ]
    };

    private const string PasswordRequirementsMessage = """
                                                       All the following conditions must be true:
                                                         - The value must have at least 8 characters
                                                         - One of the following conditions must be true:
                                                           - The value must contain at least one of these characters: 0123456789
                                                           - The value must contain at least one of these characters: !?%&*-_.

                                                       """;
    
    [Fact]
    public void RuleVerifier()
    {
        var ruleVerifier = new RuleVerifier();
        Assert.False(ruleVerifier.Visit(_passwordRule, "admin"));
        Assert.True(ruleVerifier.Visit(_passwordRule, "admin123"));
    }

    [Fact]
    public void RuleRequirementsBuilder()
    {
        var passwordRequirements = new RuleRequirementsBuilder();
        var context = new RuleRequirementsContext();
        _ = passwordRequirements.Visit(_passwordRule, context);

        Assert.Equal(PasswordRequirementsMessage, context.ToString());
    }
}
