using ScrapPickup.Core;
using Xunit;

namespace ScrapPickup.Tests;

public class OnSwitchTests
{
    /// <summary>The file 0.1.1 writes: a [Scrap] section with only ToTrunk, and BepInEx's comment lines.</summary>
    private const string OldVersionConfig =
        "## Settings file was created by plugin Scrap Pickup v0.1.1\n" +
        "## Plugin GUID: com.dev.survivallog.scrappickup\n" +
        "\n" +
        "[Scrap]\n" +
        "\n" +
        "## When true, items from furniture you scrap or demolish while exploring go to your trunk first.\n" +
        "# Setting type: Boolean\n" +
        "# Default value: true\n" +
        "ToTrunk = false\n";

    [Fact]
    public void A_new_install_starts_off()
    {
        Assert.False(OnSwitch.Default);
        Assert.False(OnSwitch.IsUpdateFromOldVersion(configText: null));
    }

    [Fact]
    public void An_update_from_a_version_without_the_setting_turns_it_on()
    {
        Assert.True(OnSwitch.IsUpdateFromOldVersion(OldVersionConfig));
    }

    [Fact]
    public void A_player_who_turned_it_off_keeps_it_off()
    {
        var turnedOff = OldVersionConfig + "\n# Setting type: Boolean\n# Default value: false\nEnabled = false\n";

        Assert.False(OnSwitch.IsUpdateFromOldVersion(turnedOff));
    }

    [Fact]
    public void A_commented_out_line_does_not_count_as_the_setting()
    {
        Assert.True(OnSwitch.IsUpdateFromOldVersion(OldVersionConfig + "# Enabled = false\n"));
    }

    [Fact]
    public void An_Enabled_key_in_another_section_does_not_count()
    {
        Assert.True(OnSwitch.IsUpdateFromOldVersion(OldVersionConfig + "\n[Other]\nEnabled = false\n"));
    }

    [Fact]
    public void Windows_line_endings_and_spaces_around_the_key_are_read()
    {
        Assert.True(OnSwitch.HasKey("[ Scrap ]\r\n  Enabled  =  true\r\n", "Scrap", "Enabled"));
    }

    [Fact]
    public void The_setting_is_named_Scrap_Enabled()
    {
        Assert.Equal(("Scrap", "Enabled"), (OnSwitch.Section, OnSwitch.Key));
    }
}
