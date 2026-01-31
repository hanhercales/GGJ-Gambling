using System.Text;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using UnityEngine;
using UnityEngine.UI;
// Nếu cần text, nhưng ở đây ta dùng Tooltip

// Để dùng StringBuilder tối ưu chuỗi

namespace _Game.Scripts.View.UI.BookUI
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(TooltipTrigger))]
    public class MaskSlotUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockIcon; // (Optional) Một hình ổ khóa nhỏ đè lên nếu thích

        private TooltipTrigger _tooltip;

        private void Awake()
        {
            if (iconImage == null) iconImage = GetComponent<Image>();
            _tooltip = GetComponent<TooltipTrigger>();
        }

        public void Setup(MaskData mask, CharmHolder playerInventory)
        {
            if (mask == null)
            {
                // Ẩn slot nếu không có dữ liệu
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            iconImage.sprite = mask.icon;

            // 1. Xử lý Trạng thái Mở khóa (Visual)
            if (mask.IsUnlocked)
            {
                // Đã mở: Màu sáng bình thường
                iconImage.color = Color.white;
                if (lockIcon != null) lockIcon.SetActive(false);
            }
            else
            {
                // Chưa mở: Màu tối đi
                iconImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); 
                if (lockIcon != null) lockIcon.SetActive(true);
            }

            // 2. Xử lý Tooltip (Thông tin nguyên liệu)
            BuildTooltipContent(mask, playerInventory);
        }

        private void BuildTooltipContent(MaskData mask, CharmHolder inventory)
        {
            if (_tooltip == null) return;

            _tooltip.header = mask.charmName;
            
            StringBuilder sb = new StringBuilder();

            // Mô tả gốc
            sb.AppendLine(mask.description);
            sb.AppendLine(); // Xuống dòng

            if (mask.IsUnlocked)
            {
                sb.Append("<color=yellow>STATUS: ACTIVE</color>");
            }
            else
            {
                sb.AppendLine("<color=orange>RECIPE (Requires in Inventory):</color>");
                
                // Duyệt qua từng nguyên liệu yêu cầu
                if (mask.requiredIngredients != null)
                {
                    foreach (var req in mask.requiredIngredients)
                    {
                        bool hasItem = inventory.HasCharm(req);
                        
                        if (hasItem)
                        {
                            // Có hàng: Màu xanh lá
                            sb.AppendLine($"<color=#00FF00><b>[V]</b> {req.charmName}</color>");
                        }
                        else
                        {
                            // Thiếu hàng: Màu đỏ
                            sb.AppendLine($"<color=#FF0000>[X] {req.charmName}</color>");
                        }
                    }
                }
            }

            _tooltip.content = sb.ToString();
        }
    }
}