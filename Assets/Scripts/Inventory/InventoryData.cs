using System;
using System.Collections.Generic;
using LivingWorldRPG.Core;

[Serializable]
public class InventoryData
{
    public List<ItemData> items = new List<ItemData>();
    [NonSerialized] private IAudioService audioService;

    public void AddItem(ItemData item)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        if (item == null)
        {
            return;
        }

        items.Add(item);
        GetAudioService()?.PlayItemPickupSound();
    }

    public void RemoveItem(ItemData item)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        if (item == null)
        {
            return;
        }

        items.Remove(item);
    }

    public bool HasItem(ItemData item)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return false;
        }
        return item != null && items.Contains(item);
    }

    private IAudioService GetAudioService()
    {
        if (audioService == null)
        {
            if (!ServiceLocator.TryResolve(out audioService))
            {
                audioService = AudioManager.Instance;
            }
        }

        return audioService;
    }
}
