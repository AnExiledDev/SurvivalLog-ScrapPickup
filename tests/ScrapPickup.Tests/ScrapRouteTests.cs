using ScrapPickup.Core;
using Xunit;

namespace ScrapPickup.Tests;

public class ScrapRouteTests
{
    private static readonly ScrapSettings Both = new(UseTrunk: true);

    [Fact]
    public void Exploring_with_a_vehicle_tries_the_trunk_before_the_bag()
    {
        var order = ScrapRoute.Order(new ScrapScene(IsByPlayer: true, IsExploring: true, HasVehicle: true), Both);

        Assert.Equal(new[] { Destination.Trunk, Destination.Bag }, order);
    }

    [Fact]
    public void At_home_the_trunk_is_out_of_reach_so_only_the_bag_is_tried()
    {
        var order = ScrapRoute.Order(new ScrapScene(IsByPlayer: true, IsExploring: false, HasVehicle: true), Both);

        Assert.Equal(new[] { Destination.Bag }, order);
    }

    [Fact]
    public void Exploring_without_a_vehicle_only_tries_the_bag()
    {
        var order = ScrapRoute.Order(new ScrapScene(IsByPlayer: true, IsExploring: true, HasVehicle: false), Both);

        Assert.Equal(new[] { Destination.Bag }, order);
    }

    [Fact]
    public void Scrap_by_anyone_but_the_player_stays_on_the_ground()
    {
        var order = ScrapRoute.Order(new ScrapScene(IsByPlayer: false, IsExploring: true, HasVehicle: true), Both);

        Assert.Empty(order);
    }

    [Fact]
    public void Turning_the_trunk_off_still_sends_scrap_to_the_bag()
    {
        var exploring = new ScrapScene(IsByPlayer: true, IsExploring: true, HasVehicle: true);

        Assert.Equal(new[] { Destination.Bag }, ScrapRoute.Order(exploring, new ScrapSettings(UseTrunk: false)));
    }

    [Fact]
    public void A_full_trunk_sends_the_item_to_the_bag()
    {
        var placed = ScrapRoute.Place(new[] { Destination.Trunk, Destination.Bag }, destination => destination == Destination.Bag);

        Assert.Equal(Destination.Bag, placed);
    }

    [Fact]
    public void The_trunk_wins_when_it_has_room_and_the_bag_is_never_asked()
    {
        var asked = new List<Destination>();

        var placed = ScrapRoute.Place(new[] { Destination.Trunk, Destination.Bag }, destination =>
        {
            asked.Add(destination);

            return true;
        });

        Assert.Equal(Destination.Trunk, placed);
        Assert.Equal(new[] { Destination.Trunk }, asked);
    }

    [Fact]
    public void Only_when_every_destination_is_full_does_the_item_go_to_the_ground()
    {
        var asked = new List<Destination>();

        var placed = ScrapRoute.Place(new[] { Destination.Trunk, Destination.Bag }, destination =>
        {
            asked.Add(destination);

            return false;
        });

        Assert.Null(placed);
        Assert.Equal(new[] { Destination.Trunk, Destination.Bag }, asked);
    }

    [Fact]
    public void The_tally_counts_each_destination_separately()
    {
        var tally = new ScrapTally();

        tally.Add(Destination.Trunk, 2);
        tally.Add(Destination.Bag, 1);
        tally.Add(null, 3);

        Assert.Equal((2, 1, 3, 6), (tally.Trunk, tally.Bag, tally.Ground, tally.Total));
        Assert.Equal("2 to the trunk, 1 to the bag, 3 on the ground", tally.ToString());
    }
}
