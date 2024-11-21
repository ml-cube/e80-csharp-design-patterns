using CSharpCourse.DesignPatterns.Assignments.Extra;
using Moq;

namespace CSharpCourse.DesignPatterns.Tests.AssignmentTests.Extra;

public class VarianceTests
{
    [Fact]
    public void PowerUps()
    {
        // The chest gives out objects of type StatBoost
        var chest = new Mock<IPowerUpProvider<StatBoost>>();
        
        chest
            .Setup(x => x.GetPowerUp())
            .Returns(
            new StatBoost
            {
                Name = "Random Boost",
                Duration = 10f,
                BoostAmount = Random.Shared.Next(10, 30),
                StatName = "Strength"
            });

        // Covariance
        // We can use IPowerUpProvider<StatBoost> as IPowerUpProvider<PowerUp>
        // The generalChest promises to give out at least a PowerUp, but it
        // actually gives out a StatBoost (derived class).
        IPowerUpProvider<PowerUp> generalChest = chest.Object;

        PowerUp powerUp = generalChest.GetPowerUp();

        Assert.IsType<StatBoost>(powerUp);

        var handler = new Mock<IPowerUpApplier<PowerUp>>();
        
        handler
            .Setup(x => x.Apply(It.IsAny<PowerUp>()))
            .Callback<PowerUp>(powerUp => {
                powerUp.IsActive = true;
                Console.WriteLine($"Applied {powerUp.Name} for {powerUp.Duration} seconds!");

                if (powerUp is StatBoost statBoost)
                {
                    Console.WriteLine($"Boosted {statBoost.StatName} by {statBoost.BoostAmount}");
                }
            }
        );

        // Contravariance example
        // We can use IPowerUpApplier<PowerUp> as IPowerUpApplier<StatBoost>
        // The handler can apply any power-up, therefore it definitely knows
        // how to apply a StatBoost, which is a more specific type.
        IPowerUpApplier<StatBoost> specificHandler = handler.Object;

        specificHandler.Apply((StatBoost)powerUp);

        Assert.True(powerUp.IsActive);
    }

    // From MSDN docs
    // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/

    [Fact]
    public void Covariance()
    {
        // IEnumerable<out T> is covariant, so we can do this
        IEnumerable<object> objects = Enumerable.Repeat(string.Empty, 10);

        Assert.IsAssignableFrom<IEnumerable<string>>(objects);
    }

    [Fact]
    public void Contravariance()
    {
        static void SetObject(object o)
        {
            // Implementation
        }

        // Action<in T> is contravariant, so we can do this
        Action<string> actString = SetObject;

        Assert.IsType<Action<string>>(actString);
    }

#pragma warning disable S2094 // Classes should not be empty
    // Base classes
    public class Animal { }
    public class Dog : Animal { }
    public class GermanShepherd : Dog { }

    // Result classes
    public class AnimalInfo { }
    public class DogInfo : AnimalInfo { }
#pragma warning restore S2094 // Classes should not be empty

    [Fact]
    public void FuncExample()
    {
        // Func<in T, out TResult> is both contravariant and covariant
        Func<Dog, DogInfo> getDogInfo = (dog) => new DogInfo();

        // 1. Covariance example with TResult (out parameter)

        // A function that takes a Dog and returns a DogInfo
        // can be assigned to a function that takes Dog but returns
        // the more general AnimalInfo
        Func<Dog, AnimalInfo> getAnimalInfo = getDogInfo;

        Assert.IsType<Func<Dog, DogInfo>>(getAnimalInfo);

        // 2. Contravariance example with T (in parameter)

        // A function that takes a Dog and returns DogInfo
        // can be assigned to a function that takes the more specific
        // GermanShepherd and returns DogInfo
        Func<GermanShepherd, DogInfo> processDog = getDogInfo;

        Assert.IsType<Func<Dog, DogInfo>>(processDog);

        // 3. Combined example

        // A function that takes a Dog and returns a DogInfo
        // can be assigned to a function that takes a more specific
        // GermanShepherd and returns a more general AnimalInfo
        Func<GermanShepherd, AnimalInfo> func = getDogInfo;

        Assert.IsType<Func<Dog, DogInfo>>(func);

        // 4. What if it was like Func<out T, in TResult>?

        // Then, we would be able to assign getDogInfo to

        // - Func<Animal, DogInfo> (which means we would be able to pass
        // a Cat to a function that actually expects a Dog)
        
        // - Func<Dog, GermanShepherdInfo> (which means we could be
        // expecting a GermanShepherdInfo but actually get a HuskyInfo)
    }
}
