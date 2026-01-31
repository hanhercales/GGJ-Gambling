using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI.ShopUI
{
    public class ShopSlotUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button selectButton; // Nút vô hình hoặc background để click chọn
        [SerializeField] private GameObject selectionHighlight; // Viền sáng khi được chọn
        [SerializeField] private GameObject soldOutOverlay;

        private int _myIndex;
        private System.Action<int> _onSelectedCallback;

        private void Start()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(OnClicked);
        }

        // Setup nhận thêm callback để báo cho ShopUI biết
        public void Setup(int index, CharmData data, System.Action<int> onSelected)
        {
            _myIndex = index;
            _onSelectedCallback = onSelected;
            SetSelected(false);

            if (data != null)
            {
                // Có hàng
                if (data.icon != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    // Tạm thời hiện 1 màu đỏ để biết là có data nhưng thiếu ảnh
                    iconImage.color = Color.red; 
                    iconImage.gameObject.SetActive(true);
                }

                priceText.text = data.price.ToString();
                nameText.text = data.charmName; 
                int finalPrice = data.price;
                
                if (ShopManager.Instance != null)
                {
                    finalPrice = ShopManager.Instance.GetFinalPrice(data);
                }
                
                if (finalPrice < data.price)
                {
                    priceText.color = Color.green; 
                }
                else
                {
                    priceText.color = Color.white; // Or your default color
                }

                priceText.text = finalPrice.ToString();
                
                if (selectButton) selectButton.interactable = true;
                if (soldOutOverlay) soldOutOverlay.SetActive(false);
            }
            else
            {
                // Hết hàng / Slot trống
                // Đừng ẩn toàn bộ, hãy để lại khung nền
                iconImage.gameObject.SetActive(false);
                priceText.text = "EMPTY"; // Hiện chữ EMPTY để dễ debug
                nameText.text = "";
                
                if (selectButton) selectButton.interactable = false;
                if (soldOutOverlay) soldOutOverlay.SetActive(true); // Đảm bảo Overlay có màu (vd: xám bán trong suốt)
            }
        }
        
        private void OnClicked()
        {
            _onSelectedCallback?.Invoke(_myIndex);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionHighlight != null)
                selectionHighlight.SetActive(isSelected);
        }
    }
}