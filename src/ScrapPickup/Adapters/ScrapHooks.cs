using System;
using System.Collections.Generic;
using BepInEx.Logging;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using ScrapPickup.Core;
using UnityEngine;

namespace ScrapPickup.Adapters;

/// <summary>
/// Furniture leaves items two ways. Demolish runs AE_DropFurnitureRemoveGet, which drops each item of the furniture's
/// RemoveGet list on the ground with DropItemManager.DropItemAtPosition; while that runs, each drop first tries the trunk
/// and the bag through the game's own ItemManager.TryAddItemToBag, which checks room and carry weight. Scrap runs
/// AE_OpenItemPackageByConfig, which hands each package to ItemManager.TryAddPackageItems; that already tries a given
/// owner, then the bag, then the ground, so while exploring the scrap gives it the trunk as that owner. Every hook fails
/// soft, back to the game's own behavior.
/// </summary>
internal static class ScrapHooks
{
    private static readonly HashSet<string> ReportedFailures = new();

    private static ManualLogSource? _log;
    private static ScrapSettings _settings = new(UseTrunk: true);
    private static Scrap? _open;
    private static bool _isPlacing;

    public static void Apply(Harmony harmony, ManualLogSource log)
    {
        _log = log;
        Patch(harmony, "the demolish action", () => AccessTools.Method(typeof(AE_DropFurnitureRemoveGet), nameof(AE_DropFurnitureRemoveGet.Run)), nameof(OpenDemolish), nameof(CloseScrap));
        Patch(harmony, "the ground drop", () => AccessTools.Method(typeof(DropItemManager), nameof(DropItemManager.DropItemAtPosition),
            new[] { typeof(Vector3), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) }), nameof(PlaceInstead), null);
        Patch(harmony, "the scrap action", () => AccessTools.Method(typeof(AE_OpenItemPackageByConfig), nameof(AE_OpenItemPackageByConfig.Run)), nameof(OpenScrap), nameof(CloseScrap));
        Patch(harmony, "the scrap package", () => AccessTools.Method(typeof(ItemManager), nameof(ItemManager.TryAddPackageItems)), nameof(PackageToTrunk), null);
    }

    public static void Use(ScrapSettings settings) => _settings = settings;

    private static void Patch(Harmony harmony, string what, Func<System.Reflection.MethodInfo?> find, string prefix, string? finalizer)
    {
        try
        {
            var original = find() ?? throw new MissingMethodException(what);

            harmony.Patch(original, prefix: new HarmonyMethod(typeof(ScrapHooks), prefix), finalizer: finalizer == null ? null : new HarmonyMethod(typeof(ScrapHooks), finalizer));
            _log?.LogInfo($"hooked {what}");
        }
        catch (Exception ex)
        {
            _log?.LogError($"could not hook {what}, it works as in the unmodded game: {ex.Message}");
        }
    }

    private static void OpenDemolish(EffectBtArgs __0) => Open(__0, Kind.Demolish);

    /// <summary>The same node opens item packages for other actions; only a scrap action counts.</summary>
    private static void OpenScrap(EffectBtArgs __0) => Open(__0, Kind.Scrap);

    private static void Open(EffectBtArgs args, Kind kind)
    {
        try
        {
            _open = null;

            var world = GameSingleton.Existing<BattleLogicWorld>();
            var role = world?._AgentManager?.GetLeadingRole();
            var items = world?._ItemManager;

            if (role == null || items == null) return;

            var parties = EffectBtArgsExtension.GetAttackerAndTarget(args);

            if (kind == Kind.Scrap && !IsScrapAction(parties.Item4)) return;

            var scrapper = parties.Item1;
            var scene = new ScrapScene(scrapper != null && scrapper.InstanceId == role.InstanceId, VehicleConfig.IsInExploreInstance(), VehicleConfig.HasVehicle());
            var order = ScrapRoute.Order(scene, _settings);

            if (order.Count == 0 || (kind == Kind.Scrap && order[0] != Destination.Trunk))
            {
                Note($"{kind} left to the game", $"a {kind} was left to the game ({scene}, {_settings})");

                return;
            }

            // CloseScrap runs as a finalizer, so even a scrap that throws inside the game closes its window.
            _open = new Scrap(kind, items, role.InstanceId, order, Time.frameCount, new ScrapTally());
        }
        catch (Exception ex)
        {
            _open = null;
            Report($"opening a {kind}", ex);
        }
    }

    /// <summary>
    /// The scrap actions (回收 and 努力拆除) run effects 127-131. Other effects that also remove furniture, such as taking
    /// a loaf of bread (拿走, 9083), are not scrap and keep the game's own handling.
    /// </summary>
    private static bool IsScrapAction(Config_Action? action) => action != null && action.EffectConfigID is >= 127 and <= 131;

    private static void CloseScrap()
    {
        var scrap = _open;

        _open = null;

        if (scrap != null && scrap.Tally.Total > 0) Note($"first {scrap.Kind}", $"first {scrap.Kind} this session: {scrap.Tally}");
    }

    /// <summary>False skips the game's drop because the item went to the trunk or the bag.</summary>
    private static bool PlaceInstead(int itemConfigId, int count, int startTime, int timeLeft, ref bool __result)
    {
        var scrap = _open;

        if (scrap == null || scrap.Kind != Kind.Demolish || _isPlacing || scrap.Frame != Time.frameCount) return true;

        try
        {
            _isPlacing = true;

            var placed = ScrapRoute.Place(scrap.Order, destination => TryAdd(scrap, destination, itemConfigId, count, startTime, timeLeft));

            scrap.Tally.Add(placed, count);

            if (placed == null) return true;

            __result = true;

            return false;
        }
        catch (Exception ex)
        {
            Report("placing a demolished item", ex);

            return true;
        }
        finally
        {
            _isPlacing = false;
        }
    }

    /// <summary>The start time and freshness the game's ground drop gives the item; a use count of 0 gives it the full uses from its config.</summary>
    private static bool TryAdd(Scrap scrap, Destination destination, int itemConfigId, int count, int startTime, int timeLeft)
    {
        var ownerId = destination == Destination.Trunk ? VehicleConfig.TrunkOwnerId : scrap.RoleId;

        return scrap.Items.TryAddItemToBag(ownerId, itemConfigId, count, startTime, timeLeft, 0, 1f, null, 0, null, null, 0, 0, false, false, false, false, 0);
    }

    /// <summary>
    /// Runs the package again with the trunk as its first owner; the game then tries the bag and the ground itself.
    /// False skips the original call, whose result the rerun supplies.
    /// </summary>
    private static bool PackageToTrunk(ItemManager __instance, int packageId, ref Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<int, int>> __result)
    {
        var scrap = _open;

        if (scrap == null || scrap.Kind != Kind.Scrap || _isPlacing || scrap.Frame != Time.frameCount) return true;

        try
        {
            _isPlacing = true;
            __result = __instance.TryAddPackageItems(packageId, new Il2CppSystem.Nullable<long>(VehicleConfig.TrunkOwnerId));
            Note("first scrap", "first scrap this session went to the trunk first");

            return false;
        }
        catch (Exception ex)
        {
            Report("sending a scrap package to the trunk", ex);

            return true;
        }
        finally
        {
            _isPlacing = false;
        }
    }

    private static void Note(string kind, string message)
    {
        if (ReportedFailures.Add("note: " + kind)) _log?.LogInfo(message);
    }

    private static void Report(string operation, Exception ex)
    {
        if (ReportedFailures.Add(operation)) _log?.LogError($"{operation} failed, the game handles the item as usual (each kind reported once): {ex}");
    }

    private enum Kind
    {
        Demolish,
        Scrap,
    }

    private sealed record Scrap(Kind Kind, ItemManager Items, long RoleId, IReadOnlyList<Destination> Order, int Frame, ScrapTally Tally);
}
