using _Game.Scripts.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SymbolSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI symbolName;
    public TextMeshProUGUI currentWeight;
    public TextMeshProUGUI value;
    public SymbolData symbolData;

    public void TakeInformation()
    {
        icon.sprite = symbolData.icon;
        symbolName.text = symbolData.idName;
        currentWeight.text = symbolData.currentWeight.ToString();
        value.text = symbolData.currentValue.ToString();
    }
}
