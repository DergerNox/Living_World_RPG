using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Aggregated per-faction economy data so AI and UI can make strategic decisions without scanning every settlement each frame.
/// </summary>
public class FactionEconomySnapshot
{
    public Dictionary<ResourceType, int> BasicResources { get; } = new Dictionary<ResourceType, int>();
    public Dictionary<AdvancedResourcesTypes, int> AdvancedResources { get; } = new Dictionary<AdvancedResourcesTypes, int>();
    public Dictionary<WeaponTypes, int> Armory { get; } = new Dictionary<WeaponTypes, int>();

    public bool HasFoodShortage { get; internal set; }
    public bool HasEquipmentShortage { get; internal set; }
    public string StrategicNeed { get; internal set; } = "Stable";

    public bool HasCriticalShortage => HasFoodShortage || HasEquipmentShortage;

    public int GetBasic(ResourceType type) => BasicResources.TryGetValue(type, out int value) ? value : 0;
    public int GetAdvanced(AdvancedResourcesTypes type) => AdvancedResources.TryGetValue(type, out int value) ? value : 0;
    public int GetArmoryCount(WeaponTypes type) => Armory.TryGetValue(type, out int value) ? value : 0;

    public int TotalArmoryCount => Armory.Values.Sum();

    public string BuildStatusLine()
    {
        return $"{StrategicNeed} | Food: {GetAdvanced(AdvancedResourcesTypes.Food)} | Arms: {TotalArmoryCount}";
    }
}

/// <summary>
/// Helper functions for computing snapshots and preparing AI-friendly data.
/// </summary>
public static class FactionEconomyUtility
{
    private const int FoodShortageThreshold = 120;
    private const int EquipmentShortageThreshold = 40;

    public static void RefreshFactionSnapshots(IList<Faction> factions)
    {
        if (factions == null)
        {
            return;
        }

        foreach (var faction in factions)
        {
            if (faction == null)
            {
                continue;
            }

            var snapshot = BuildSnapshot(faction);
            faction.ApplyEconomySnapshot(snapshot);
        }
    }

    public static FactionEconomySnapshot BuildSnapshot(Faction faction)
    {
        var snapshot = new FactionEconomySnapshot();
        if (faction == null)
        {
            return snapshot;
        }

        foreach (var village in faction.Villages)
        {
            if (village?.ResourceLedger == null)
            {
                continue;
            }

            AccumulateBasic(snapshot.BasicResources, village.ResourceLedger.BasicResources);
        }

        foreach (var town in faction.Towns)
        {
            if (town?.ResourceLedger == null)
            {
                continue;
            }

            AccumulateBasic(snapshot.BasicResources, town.ResourceLedger.BasicResources);
            AccumulateAdvanced(snapshot.AdvancedResources, town.ResourceLedger.AdvancedResources);
        }

        foreach (var castle in faction.Castles)
        {
            if (castle?.ResourceLedger == null)
            {
                continue;
            }

            AccumulateAdvanced(snapshot.AdvancedResources, castle.ResourceLedger.AdvancedResources);
            AccumulateArmory(snapshot.Armory, castle.Armory);
        }

        int foodStock = snapshot.GetBasic(ResourceType.Food) + snapshot.GetAdvanced(AdvancedResourcesTypes.Food) * 5;

        snapshot.HasFoodShortage = foodStock < FoodShortageThreshold;
        int armoryCount = snapshot.TotalArmoryCount;
        snapshot.HasEquipmentShortage = armoryCount < EquipmentShortageThreshold;

        if (snapshot.HasFoodShortage && snapshot.HasEquipmentShortage)
        {
            snapshot.StrategicNeed = "Critical: food & arms";
        }
        else if (snapshot.HasFoodShortage)
        {
            snapshot.StrategicNeed = "Needs food shipments";
        }
        else if (snapshot.HasEquipmentShortage)
        {
            snapshot.StrategicNeed = "Needs armaments";
        }
        else
        {
            snapshot.StrategicNeed = $"Stable (Armory {armoryCount})";
        }

        return snapshot;
    }

    private static void AccumulateBasic(IDictionary<ResourceType, int> target, IReadOnlyDictionary<ResourceType, int> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (var kvp in source)
        {
            if (kvp.Value <= 0)
            {
                continue;
            }

            target[kvp.Key] = target.TryGetValue(kvp.Key, out int existing)
                ? existing + kvp.Value
                : kvp.Value;
        }
    }

    private static void AccumulateAdvanced(IDictionary<AdvancedResourcesTypes, int> target, IReadOnlyDictionary<AdvancedResourcesTypes, int> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (var kvp in source)
        {
            if (kvp.Value <= 0)
            {
                continue;
            }

            target[kvp.Key] = target.TryGetValue(kvp.Key, out int existing)
                ? existing + kvp.Value
                : kvp.Value;
        }
    }

    private static void AccumulateArmory(IDictionary<WeaponTypes, int> target, IDictionary<WeaponTypes, int> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (var kvp in source)
        {
            if (kvp.Value <= 0)
            {
                continue;
            }

            target[kvp.Key] = target.TryGetValue(kvp.Key, out int existing)
                ? existing + kvp.Value
                : kvp.Value;
        }
    }
}
