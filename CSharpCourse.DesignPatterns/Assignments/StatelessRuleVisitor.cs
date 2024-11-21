using System.Text;

namespace CSharpCourse.DesignPatterns.Assignments;

internal interface IRule
{
    TResult Accept<TInput, TResult>(IRuleVisitor<TInput, TResult> visitor, TInput input);
}

internal class AndRule : IRule
{
    public IEnumerable<IRule> Rules { get; set; } = [];
    public TResult Accept<TInput, TResult>(IRuleVisitor<TInput, TResult> visitor, TInput input) => visitor.Visit(this, input);
}

internal class OrRule : IRule
{
    public IEnumerable<IRule> Rules { get; set; } = [];
    public TResult Accept<TInput, TResult>(IRuleVisitor<TInput, TResult> visitor, TInput input) => visitor.Visit(this, input);
}

internal class MinLengthRule : IRule
{
    public int MinLength { get; set; }
    public TResult Accept<TInput, TResult>(IRuleVisitor<TInput, TResult> visitor, TInput input) => visitor.Visit(this, input);
}

internal class ContainsCharacterRule : IRule
{
    public char Character { get; set; }
    public TResult Accept<TInput, TResult>(IRuleVisitor<TInput, TResult> visitor, TInput input) => visitor.Visit(this, input);
}

internal class ContainsAtLeastOneCharacterRule : IRule
{
    public string Characters { get; set; } = string.Empty;
    public TResult Accept<TInput, TResult>(IRuleVisitor<TInput, TResult> visitor, TInput input) => visitor.Visit(this, input);
}

// This is already defined in the file and CANNOT be used to pass
// an input parameter to the visitor, since its generic type parameter
// is covariant and should only be used as a return type.
internal interface IRuleVisitor<out TResult>
{
    TResult Visit(AndRule rule);
    TResult Visit(OrRule rule);
    TResult Visit(MinLengthRule rule);
    TResult Visit(ContainsCharacterRule rule);
    TResult Visit(ContainsAtLeastOneCharacterRule rule);
}

internal interface IRuleVisitor<in TInput, out TResult>
{
    TResult Visit(AndRule rule, TInput input);
    TResult Visit(OrRule rule, TInput input);
    TResult Visit(MinLengthRule rule, TInput input);
    TResult Visit(ContainsCharacterRule rule, TInput input);
    TResult Visit(ContainsAtLeastOneCharacterRule rule, TInput input);
}

internal class RuleVerifier : IRuleVisitor<string, bool>
{
    public bool Visit(AndRule rule, string input) => rule.Rules.All(r => r.Accept(this, input));
    public bool Visit(OrRule rule, string input) => rule.Rules.Any(r => r.Accept(this, input));
    public bool Visit(MinLengthRule rule, string input) => input.Length >= rule.MinLength;
    public bool Visit(ContainsCharacterRule rule, string input) => input.Contains(rule.Character);
    public bool Visit(ContainsAtLeastOneCharacterRule rule, string input) => rule.Characters.Any(input.Contains);
}

// We use a record because it's immutable
internal record RuleRequirementsContext
{
    public StringBuilder StringBuilder { get; } = new();
    public int Level { get; set; }

    public override string ToString() => StringBuilder.ToString();
}

// We need a return type, even if it's meaningless in this case.
// We use the flyweight pattern to avoid creating multiple instances of the same object.
internal record NoReturn
{
    private NoReturn() { }

    internal static NoReturn Instance { get; } = new();
}

internal class RuleRequirementsBuilder : IRuleVisitor<RuleRequirementsContext, NoReturn>
{
    public NoReturn Visit(AndRule rule, RuleRequirementsContext context)
    {
        AppendSpace(context.Level, context.StringBuilder);
        context.StringBuilder.AppendLine("All the following conditions must be true:");
        foreach (var subRule in rule.Rules)
        {
            subRule.Accept(this, context with { Level = context.Level + 1 });
        }

        return NoReturn.Instance;
    }

    public NoReturn Visit(OrRule rule, RuleRequirementsContext context)
    {
        AppendSpace(context.Level, context.StringBuilder);
        context.StringBuilder.AppendLine("One of the following conditions must be true:");
        foreach (var subRule in rule.Rules)
        {
            subRule.Accept(this, context with { Level = context.Level + 1 });
        }

        return NoReturn.Instance;
    }

    public NoReturn Visit(MinLengthRule rule, RuleRequirementsContext context)
    {
        AppendSpace(context.Level, context.StringBuilder);

        context.StringBuilder
            .Append("The value must have at least ")
            .Append(rule.MinLength)
            .AppendLine(" characters");

        return NoReturn.Instance;
    }

    public NoReturn Visit(ContainsCharacterRule rule, RuleRequirementsContext context)
    {
        AppendSpace(context.Level, context.StringBuilder);

        context.StringBuilder
            .Append("The value must contain the character ")
            .Append(rule.Character)
            .AppendLine();

        return NoReturn.Instance;
    }

    public NoReturn Visit(ContainsAtLeastOneCharacterRule rule, RuleRequirementsContext context)
    {
        AppendSpace(context.Level, context.StringBuilder);

        context.StringBuilder
            .Append("The value must contain at least one of these characters: ")
            .AppendLine(rule.Characters);

        return NoReturn.Instance;
    }

    private static void AppendSpace(int level, StringBuilder sb)
    {
        for (var i = 0; i < level; i++)
        {
            sb.Append("  ");
        }

        if (level > 0)
        {
            sb.Append("- ");
        }
    }
}
