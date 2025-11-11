using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    private IAudioService audioService;

    public void AddItem(Item item)
    {
        items.Add(item);
        // Play item pickup sound
        GetAudioService()?.PlayItemPickupSound();
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public bool HasItem(Item item)
    {
        return items.Contains(item);
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
