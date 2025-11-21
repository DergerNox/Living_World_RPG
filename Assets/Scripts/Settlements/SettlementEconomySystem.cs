using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Wires up settlements into simple supply chains and simulates the daily flow of resources.
/// Villages produce raw goods, towns refine them for profits, and castles turn refined goods into equipment.
/// </summary>
public static class SettlementEconomySystem
{
    private const float VillageExportRatio = 0.65f;
    private const float TownExportRatio = 0.5f;

    public static void InitializeEconomy(IEnumerable<Village> villages, IEnumerable<Town> towns, IEnumerable<Castle> castles, int warmupTicks = 3)
    {
        var villageList = FilterSettlements(villages);
        var townList = FilterSettlements(towns);
        var castleList = FilterSettlements(castles);

        if (villageList.Count == 0 && townList.Count == 0 && castleList.Count == 0)
        {
            return;
        }

        LinkSupplyChains(villageList, townList, castleList);
        warmupTicks = Mathf.Max(1, warmupTicks);
        ProgressEconomyInternal(villageList, townList, castleList, warmupTicks);
    }

    public static void ProgressEconomy(IEnumerable<Village> villages, IEnumerable<Town> towns, IEnumerable<Castle> castles, int days = 1)
    {
        var villageList = FilterSettlements(villages);
        var townList = FilterSettlements(towns);
        var castleList = FilterSettlements(castles);

        if (villageList.Count == 0 && townList.Count == 0 && castleList.Count == 0)
        {
            return;
        }

        days = Mathf.Max(1, days);
        ProgressEconomyInternal(villageList, townList, castleList, days);
    }

    private static List<T> FilterSettlements<T>(IEnumerable<T> source)
    {
        return source == null ? new List<T>() : source.Where(s => s != null).ToList();
    }

    private static void LinkSupplyChains(List<Village> villages, List<Town> towns, List<Castle> castles)
    {
        foreach (var village in villages)
        {
            village.ConnectedTown = null;
        }

        foreach (var town in towns)
        {
            town.ResetSupplyNetwork();
        }

        foreach (var castle in castles)
        {
            castle.ResetSupplyNetwork();
        }

        foreach (var village in villages)
        {
            Town closestTown = towns.Count > 0 ? FindClosest(towns, village.Position) : null;
            village.ConnectedTown = closestTown;
            closestTown?.RegisterVillageSupply(village);
        }

        foreach (var town in towns)
        {
            Castle closestCastle = castles.Count > 0 ? FindClosest(castles, town.Position) : null;
            town.ConnectedCastle = closestCastle;
            closestCastle?.RegisterSupplyingTown(town);
        }
    }

    private static T FindClosest<T>(IEnumerable<T> candidates, Vector3Int reference) where T : class, ISettlement
    {
        T best = null;
        float bestDistance = float.MaxValue;
        foreach (var candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            Vector3Int pos = new Vector3Int(candidate.PosX, candidate.PosY, 0);
            float sqr = (pos - reference).sqrMagnitude;
            if (sqr < bestDistance)
            {
                bestDistance = sqr;
                best = candidate;
            }
        }
        return best;
    }

    private static void ProgressEconomyInternal(List<Village> villages, List<Town> towns, List<Castle> castles, int days)
    {
        for (int i = 0; i < days; i++)
        {
            StepVillages(villages);
            StepTowns(towns);
            StepCastles(castles);
        }
    }

    private static void StepVillages(IEnumerable<Village> villages)
    {
        foreach (var village in villages)
        {
            if (village == null)
            {
                continue;
            }

            village.ProduceResources();
            TransferVillageExports(village);
        }
    }

    private static void TransferVillageExports(Village village)
    {
        Town town = village.ConnectedTown;
        if (town == null)
        {
            return;
        }

        foreach (var resourceType in village.ProductionTypes)
        {
            int available = village.ResourceLedger.GetBasic(resourceType);
            if (available <= 0)
            {
                continue;
            }

            int exportAmount = Mathf.Clamp(Mathf.RoundToInt(available * VillageExportRatio), 1, available);
            if (exportAmount <= 0)
            {
                continue;
            }

            village.ResourceLedger.ConsumeBasic(resourceType, exportAmount);
            town.ResourceLedger.AddBasic(resourceType, exportAmount);
        }
    }

    private static void StepTowns(IEnumerable<Town> towns)
    {
        foreach (var town in towns)
        {
            if (town == null)
            {
                continue;
            }

            town.ProcessEconomyTick();
            TransferTownExports(town);
        }
    }

    private static void TransferTownExports(Town town)
    {
        Castle castle = town.ConnectedCastle;
        if (castle == null)
        {
            return;
        }

        foreach (var resourceType in town.ResourceLedger.GetAdvancedResourceTypes())
        {
            int available = town.ResourceLedger.GetAdvanced(resourceType);
            if (available <= 0)
            {
                continue;
            }

            int exportAmount = Mathf.Clamp(Mathf.RoundToInt(available * TownExportRatio), 1, available);
            if (exportAmount <= 0)
            {
                continue;
            }

            town.ResourceLedger.ConsumeAdvanced(resourceType, exportAmount);
            castle.ResourceLedger.AddAdvanced(resourceType, exportAmount);
        }
    }

    private static void StepCastles(IEnumerable<Castle> castles)
    {
        foreach (var castle in castles)
        {
            castle?.ForgeEquipment();
        }
    }
}
