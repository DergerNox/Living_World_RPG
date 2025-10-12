using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopCard : MonoBehaviour
{
    public Image troopImage;
    public TextMeshProUGUI troopName;

    public void SetUpTroopCard(Troop troop)
    {
        troopName.text = troop.Name;
        troopImage.sprite = troop.GetTroopImage(troop.TroopType, troop.Rank);
    }
    
}
