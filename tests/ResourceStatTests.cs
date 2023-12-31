﻿namespace Tests;

public class ResourceStatTests
{
    [Test]
    public void TestIncrease()
    {
        ResourceStat<int> stat = new()
        {
            CurrentValue = 10,
            Max = 20,
            Min = 0
        };

        stat.Increase(5);

        Assert.That(stat.CurrentValue, Is.EqualTo(15));
        // Ensure upper bounds are respected
        stat.Increase(10);
        Assert.That(stat.CurrentValue, Is.EqualTo(stat.Max));
    }

    [Test]
    public void TestDecrease()
    {
        ResourceStat<int> stat = new()
        {
            CurrentValue = 10,
            Max = 20,
            Min = 0,
        };

        stat.Reduce(5);
        Assert.That(stat.CurrentValue, Is.EqualTo(5));

        stat.Reduce(10);
        Assert.That(stat.CurrentValue, Is.EqualTo(stat.Min));
    }

    [Test]
    public void TestDeplete()
    {
        ResourceStat<int> stat = new()
        {
            CurrentValue = 10,
            Max = 20,
            Min = 0,
        };

        stat.Deplete();
        Assert.That(stat.CurrentValue, Is.EqualTo(stat.Min));
    }

    [Test]
    public void TestReplenish()
    {
        ResourceStat<int> stat = new()
        {
            CurrentValue = 10,
            Max = 20,
            Min = 0,
        };

        stat.RestoreInFull();
        Assert.That(stat.CurrentValue, Is.EqualTo(stat.Max));
    }
}
