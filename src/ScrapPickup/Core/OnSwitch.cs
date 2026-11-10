namespace ScrapPickup.Core;

/// <summary>
/// The [Scrap] Enabled setting. A new install starts off. A config file that a version before 0.1.2 wrote has no
/// Enabled key, and that player had the mod working, so the key is written as on for them.
/// </summary>
public static class OnSwitch
{
    public const string Section = "Scrap";

    public const string Key = "Enabled";

    public const bool Default = false;

    /// <summary>
    /// True when the config file was there before this start but has no [Scrap] Enabled line, so the player is updating
    /// from an older version. <paramref name="configText"/> is the file read before any setting is bound, or null when
    /// there was no file.
    /// </summary>
    public static bool IsUpdateFromOldVersion(string? configText) => configText != null && !HasKey(configText, Section, Key);

    /// <summary>Whether a BepInEx config file has a "Key = value" line in the section. Comment lines are skipped.</summary>
    public static bool HasKey(string text, string section, string key)
    {
        var isInSection = false;

        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();

            if (line.Length == 0 || line[0] == '#') continue;

            if (line[0] == '[' && line[line.Length - 1] == ']')
            {
                isInSection = line.Substring(1, line.Length - 2).Trim() == section;

                continue;
            }

            var equals = line.IndexOf('=');

            if (isInSection && equals > 0 && line.Substring(0, equals).Trim() == key) return true;
        }

        return false;
    }
}
