using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopSelectionPanel : MonoBehaviour
{
    public TextMeshProUGUI troopNameText;
    public Image troopImage;
    public TextMeshProUGUI unitCostText;
    public TMP_InputField quantityInput;
    public Slider quantitySlider;
    public TextMeshProUGUI totalCostText;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject settlementPanel;       // Reference to the main UI panel
    public GameObject recruitmentPanel;      // Reference to the recruitment panel

    private Troop selectedTroop;
    private int quantity = 1;

    public void Open(Troop troop)
    {
        selectedTroop = troop;
        troopNameText.text = troop.Name;
        troopImage.sprite = troop.GetTroopImage(troop.TroopType, troop.Rank);
        unitCostText.text = $"Cost per unit: {troop.Cost}";

        quantitySlider.minValue = 1;
        quantitySlider.maxValue = 100;
        quantitySlider.value = 1;

        quantityInput.text = "1";
        UpdateTotalCost();
        // Hide other panels
        if (settlementPanel != null) settlementPanel.SetActive(false);
        if (recruitmentPanel != null) recruitmentPanel.SetActive(false);
        gameObject.SetActive(true);
    }

    public void OnSliderChanged(float value)
    {
        quantity = Mathf.RoundToInt(value);
        quantityInput.text = quantity.ToString();
        UpdateTotalCost();
    }

    public void OnInputFieldChanged(string value)
    {
        if (int.TryParse(value, out quantity))
        {
            quantity = Mathf.Clamp(quantity, 1, 100);
            quantitySlider.value = quantity;
            UpdateTotalCost();
        }
    }

    public void UpdateTotalCost()
    {
        int totalCost = quantity * selectedTroop.Cost;
        totalCostText.text = $"Total Cost: {totalCost}";
    }

    public void OnConfirm()
    {
        Debug.Log($"Recruited {quantity} {selectedTroop.Name}");
        // TODO: Add to player party, deduct cost, etc.
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (recruitmentPanel != null) recruitmentPanel.SetActive(true);
    }
}
