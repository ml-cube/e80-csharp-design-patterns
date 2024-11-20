namespace CSharpCourse.DesignPatterns.Assignments.Extra;

internal class PowerUp
{
    public float Duration { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
}

internal class StatBoost : PowerUp
{
    public float BoostAmount { get; set; }
    public required string StatName { get; set; }
}
