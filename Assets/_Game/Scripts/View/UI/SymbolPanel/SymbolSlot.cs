using System.Globalization;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SymbolSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI currentWeight;
    [SerializeField] private TextMeshProUGUI value;
    
    public SymbolData symbolData;

    public void TakeInformation(float totalWeight)
    {
        float weight = (float) symbolData.currentWeight / totalWeight * 100f;
        
        icon.sprite = symbolData.icon;
        currentWeight.text = weight.ToString("F1", CultureInfo.InvariantCulture) + '%';
        value.text = symbolData.currentValue.ToString();
    }
}
