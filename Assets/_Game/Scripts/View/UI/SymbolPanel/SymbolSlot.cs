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
        if (symbolData == null)
        {
            // Nếu null -> Ẩn icon hoặc xóa text để tránh hiện thông tin rác
            if (icon != null) icon.enabled = false;
            if (currentWeight != null) currentWeight.text = "-";
            if (value != null) value.text = "-";
            return; 
        }

        // Nếu có data -> Đảm bảo icon được bật
        if (icon != null) icon.enabled = true;
        
        float weight = (totalWeight > 0) ? ((float) symbolData.currentWeight / totalWeight * 100f) : 0f;
        
        if (icon != null) icon.sprite = symbolData.icon;
        
        if (currentWeight != null)
            currentWeight.text = weight.ToString("F1", CultureInfo.InvariantCulture) + '%';
        
        if (value != null)
            value.text = symbolData.currentValue.ToString();
    }
}
