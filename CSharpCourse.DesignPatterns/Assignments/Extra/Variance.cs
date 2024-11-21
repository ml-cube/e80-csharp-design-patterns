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

// Covariant interface - something that provides power-ups
// The power-up is a covariant type parameter since the power-up
// provider only "gives out" items.
internal interface IPowerUpProvider<out T> where T : PowerUp
{
    T GetPowerUp();
}

// Contravariant interface - something that applies power-ups
// The power-up is a contravariant type parameter since the power-up
// applier only "consumes" items.
internal interface IPowerUpApplier<in T> where T : PowerUp
{
    void Apply(T powerUp);
}
