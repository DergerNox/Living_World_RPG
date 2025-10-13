using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopCard : MonoBehaviour
{
    public Image troopImage;
    public TextMeshProUGUI troopName;
    public TextMeshProUGUI troopStats; // New field for displaying stats

    public void SetUpTroopCard(Troop troop)
    {
        troopName.text = troop.Name;
        troopImage.sprite = troop.GetTroopImage(troop.TroopType, troop.Rank);

        // Display key stats
        troopStats.text = $"Lvl: {troop.Level} | HP: {troop.Health} | DMG: {troop.Damage}\n" +
                         $"DEF: {troop.Defense} | SPD: {troop.Speed}";
    }
}
