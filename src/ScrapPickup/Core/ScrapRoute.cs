using System;
using System.Collections.Generic;

namespace ScrapPickup.Core;

/// <summary>A place a scrapped item can go instead of the ground.</summary>
public enum Destination
{
    Trunk,
    Bag,
}

/// <summary>Where the player is and what they can reach when a piece of furniture is scrapped.</summary>
/// <param name="IsByPlayer">The player scrapped it. A robot or anyone else keeps the game's drop on the ground.</param>
/// <param name="IsExploring">Out on an exploration map rather than at home.</param>
/// <param name="HasVehicle">The player owns a trunk or shopping cart.</param>
public sealed record ScrapScene(bool IsByPlayer, bool IsExploring, bool HasVehicle);

/// <param name="UseTrunk">Send scrap to the trunk or shopping cart first while exploring.</param>
public sealed record ScrapSettings(bool UseTrunk);

/// <summary>
/// The order scrapped items try their destinations in: the trunk or cart first while exploring with one, then the bag.
/// At home there is no trunk in reach, so only the bag is tried. An item no destination takes falls to the ground as in
/// the unmodded game.
/// </summary>
public static class ScrapRoute
{
    private static readonly Destination[] None = Array.Empty<Destination>();

    public static IReadOnlyList<Destination> Order(ScrapScene scene, ScrapSettings settings)
    {
        if (!scene.IsByPlayer) return None;

        var order = new List<Destination>(2);

        if (settings.UseTrunk && scene.IsExploring && scene.HasVehicle) order.Add(Destination.Trunk);

        order.Add(Destination.Bag);

        return order;
    }

    /// <summary>The first destination in <paramref name="order"/> that takes the item, or null when it goes to the ground.</summary>
    public static Destination? Place(IReadOnlyList<Destination> order, Func<Destination, bool> tryAdd)
    {
        foreach (var destination in order)
        {
            if (tryAdd(destination)) return destination;
        }

        return null;
    }
}

/// <summary>How many items of one scrap went where, for the log.</summary>
public sealed class ScrapTally
{
    public int Trunk { get; private set; }

    public int Bag { get; private set; }

    public int Ground { get; private set; }

    public int Total => Trunk + Bag + Ground;

    public void Add(Destination? placed, int count)
    {
        switch (placed)
        {
            case Destination.Trunk:
                Trunk += count;
                break;
            case Destination.Bag:
                Bag += count;
                break;
            default:
                Ground += count;
                break;
        }
    }

    public override string ToString() => $"{Trunk} to the trunk, {Bag} to the bag, {Ground} on the ground";
}
