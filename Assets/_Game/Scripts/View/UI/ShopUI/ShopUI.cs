using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI.ShopUI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private List<ShopSlotUI> shopSlots; 

        [Header("Global Buttons")]
        [SerializeField] private Button btnPurchase; // Nút mua chung
        [SerializeField] private Button btnReroll;
        [SerializeField] private Button btnBack;
        
        [SerializeField] private TextMeshProUGUI rerollCostText;

        private int _selectedIndex = -1; // -1 nghĩa là chưa chọn gì

        private void Awake()
        {
            // Gán sự kiện click button ở đây (chỉ cần làm 1 lần)
            btnPurchase.onClick.AddListener(OnPurchaseClick);
            btnReroll.onClick.AddListener(OnRerollClick);
            btnBack.onClick.AddListener(OnBackClick);
        }
        
        private void OnEnable()
        {
            _selectedIndex = -1;
            btnPurchase.interactable = false;

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopRefreshed += UpdateUI;
                
                UpdateUI(ShopManager.Instance.GetCurrentItems());
                
                if (rerollCostText != null)
                    rerollCostText.text = $"Reroll: {ShopManager.Instance.GetRerollCost()}";
            }
        }

        private void OnDisable()
        {
            // Hủy đăng ký khi đóng Shop để tránh lỗi
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopRefreshed -= UpdateUI;
            }
        }

        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.OnShopRefreshed -= UpdateUI;
        }

        private void UpdateUI(List<CharmData> items)
        {
            // Reset selection khi shop refresh (reroll hoặc mua xong)
            _selectedIndex = -1;
            btnPurchase.interactable = false;

            for (int i = 0; i < shopSlots.Count; i++)
            {
                shopSlots[i].gameObject.SetActive(true);
                
                if (i < items.Count)
                {
                    // Truyền thêm hàm OnSlotSelected vào Setup
                    shopSlots[i].Setup(i, items[i], OnSlotSelected);
                }
                else
                {
                    shopSlots[i].Setup(i, null, OnSlotSelected);
                }
            }
        }

        // Callback khi click vào 1 slot
        private void OnSlotSelected(int index)
        {
            _selectedIndex = index;
            
            // Cập nhật visual (chỉ highlight ô được chọn)
            for (int i = 0; i < shopSlots.Count; i++)
            {
                shopSlots[i].SetSelected(i == index);
            }

            // Bật nút mua
            btnPurchase.interactable = true;
        }

        private void OnPurchaseClick()
        {
            if (_selectedIndex != -1)
            {
                bool success = ShopManager.Instance.TryBuyItem(_selectedIndex);
                if (success)
                {
                    // Mua thành công -> Reset chọn
                    _selectedIndex = -1;
                    btnPurchase.interactable = false;
                }
            }
        }

        private void OnRerollClick()
        {
            ShopManager.Instance.RerollShop();
        }

        private void OnBackClick()
        {
            UIManager.Instance.CloseShop();
        }
    }
}