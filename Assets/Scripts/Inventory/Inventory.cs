using System.Collections.Generic;
using UnityEngine;
using LivingWorldRPG.Core;

[System.Serializable]
public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    private IAudioService audioService;

    public void AddItem(Item item)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        items.Add(item);
        // Play item pickup sound
        GetAudioService()?.PlayItemPickupSound();
    }

    public void RemoveItem(Item item)
    {
        if (!GameFeatureFlags.ItemsEnabled)
        {
            return;
        }
        items.Remove(item);
    }

    public bool HasItem(Item item)
    {
        return GameFeatureFlags.ItemsEnabled && items.Contains(item);
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
