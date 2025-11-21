using System;
using System.Collections.Generic;

/// <summary>
/// Lightweight container that tracks the raw and refined resources produced by settlements.
/// Provides helpers for adding, consuming, and transferring resources without leaking dictionary logic everywhere.
/// </summary>
[Serializable]
public class SettlementResourceLedger
{
    private readonly Dictionary<ResourceType, int> basicResources = new Dictionary<ResourceType, int>();
    private readonly Dictionary<AdvancedResourcesTypes, int> advancedResources = new Dictionary<AdvancedResourcesTypes, int>();

    public IReadOnlyDictionary<ResourceType, int> BasicResources => basicResources;
    public IReadOnlyDictionary<AdvancedResourcesTypes, int> AdvancedResources => advancedResources;

    public int GetBasic(ResourceType type) => basicResources.TryGetValue(type, out int amount) ? amount : 0;

    public void AddBasic(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        basicResources[type] = GetBasic(type) + amount;
    }

    public bool ConsumeBasic(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int current = GetBasic(type);
        if (current < amount)
        {
            return false;
        }

        current -= amount;
        if (current <= 0)
        {
            basicResources.Remove(type);
        }
        else
        {
            basicResources[type] = current;
        }

        return true;
    }

    public IEnumerable<ResourceType> GetBasicResourceTypes()
    {
        return basicResources.Keys;
    }

    public int TransferBasicTo(SettlementResourceLedger target, ResourceType type, int amount)
    {
        if (target == null || amount <= 0)
        {
            return 0;
        }

        int moved = Math.Min(amount, GetBasic(type));
        if (moved <= 0)
        {
            return 0;
        }

        ConsumeBasic(type, moved);
        target.AddBasic(type, moved);
        return moved;
    }

    public int GetAdvanced(AdvancedResourcesTypes type) => advancedResources.TryGetValue(type, out int amount) ? amount : 0;

    public void AddAdvanced(AdvancedResourcesTypes type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        advancedResources[type] = GetAdvanced(type) + amount;
    }

    public bool ConsumeAdvanced(AdvancedResourcesTypes type, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int current = GetAdvanced(type);
        if (current < amount)
        {
            return false;
        }

        current -= amount;
        if (current <= 0)
        {
            advancedResources.Remove(type);
        }
        else
        {
            advancedResources[type] = current;
        }

        return true;
    }

    public IEnumerable<AdvancedResourcesTypes> GetAdvancedResourceTypes()
    {
        return advancedResources.Keys;
    }

    public int TransferAdvancedTo(SettlementResourceLedger target, AdvancedResourcesTypes type, int amount)
    {
        if (target == null || amount <= 0)
        {
            return 0;
        }

        int moved = Math.Min(amount, GetAdvanced(type));
        if (moved <= 0)
        {
            return 0;
        }

        ConsumeAdvanced(type, moved);
        target.AddAdvanced(type, moved);
        return moved;
    }
}
