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
                if (data.icon != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.gameObject.SetActive(true);
                    iconImage.color = Color.aquamarine;
                }
                else
                {
                    iconImage.color = Color.red; 
                    iconImage.gameObject.SetActive(true);
                }

                nameText.text = data.charmName;
                int finalPrice = data.price;

                if (ShopManager.Instance != null)
                {
                    // Ask Manager for the real price (handling discounts)
                    finalPrice = ShopManager.Instance.GetFinalPrice(data);
                }

                priceText.text = finalPrice.ToString();
                
                if (selectButton) selectButton.interactable = true;
                if (soldOutOverlay) soldOutOverlay.SetActive(false);
            }
            else
            {
                // Empty / Sold Out Logic
                iconImage.gameObject.SetActive(false);
                priceText.text = "EMPTY"; 
                priceText.color = Color.gray; // Grey out text for empty slots
                nameText.text = "";
                
                if (selectButton) selectButton.interactable = false;
                if (soldOutOverlay) soldOutOverlay.SetActive(true);
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