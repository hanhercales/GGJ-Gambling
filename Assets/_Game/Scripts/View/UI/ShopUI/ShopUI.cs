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
        [SerializeField] private Button btnPurchase; 
        [SerializeField] private Button btnReroll;
        [SerializeField] private Button btnBack;
        
        [SerializeField] private TextMeshProUGUI rerollPriceText;
        
        // (Optional) Nếu bạn muốn làm mờ nút bằng CanvasGroup thay vì Button Transition
        [SerializeField] private CanvasGroup purchaseBtnCanvasGroup; 

        private int _selectedIndex = -1; 

        private void Awake()
        {
            btnPurchase.onClick.AddListener(OnPurchaseClick);
            btnReroll.onClick.AddListener(OnRerollClick);
            btnBack.onClick.AddListener(OnBackClick);
        }

        private void OnEnable()
        {
            // Đảm bảo nút Purchase LUÔN HIỂN THỊ khi mở Shop
            if (btnPurchase != null) btnPurchase.gameObject.SetActive(true);

            // Nhưng mặc định là KHÔNG BẤM ĐƯỢC
            SetPurchaseInteractable(false);

            ResetSelectionLogic();

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopRefreshed += UpdateUI;
                UpdateUI(ShopManager.Instance.GetCurrentItems());
                
                UpdateRerollButton();
            }
        }

        private void OnDisable()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopRefreshed -= UpdateUI;
            }
        }
        
        private void UpdateRerollButton()
        {
            if (rerollPriceText != null && ShopManager.Instance != null)
            {
                int cost = ShopManager.Instance.GetRerollCost();
                rerollPriceText.text = $"Reroll ({cost} Coin)";
            }
        }

        private void UpdateUI(List<CharmData> items)
        {
            ResetSelectionLogic(); 
            UpdateRerollButton();

            for (int i = 0; i < shopSlots.Count; i++)
            {
                shopSlots[i].gameObject.SetActive(true);
                
                if (i < items.Count)
                    shopSlots[i].Setup(i, items[i], OnSlotSelected);
                else
                    shopSlots[i].Setup(i, null, OnSlotSelected);
            }
        }

        private void OnSlotSelected(int index)
        {
            Debug.Log($"[UI] Selected Slot: {index}");

            _selectedIndex = index;
            
            // 1. Highlight ô được chọn
            for (int i = 0; i < shopSlots.Count; i++)
            {
                shopSlots[i].SetSelected(i == index);
            }

            // 2. MỞ KHÓA NÚT MUA
            SetPurchaseInteractable(true);
        }

        private void OnPurchaseClick()
        {
            if (_selectedIndex != -1)
            {
                bool success = ShopManager.Instance.TryBuyItem(_selectedIndex);
                if (success)
                {
                    Debug.Log("[UI] Purchase Successful.");
                    // Mua xong -> Reset lại trạng thái (Khóa nút nhưng vẫn hiện)
                    ResetSelectionLogic();
                }
            }
        }

        private void ResetSelectionLogic()
        {
            _selectedIndex = -1;
            
            // Tắt highlight các slot
            foreach (var slot in shopSlots) slot.SetSelected(false);
            
            // KHÓA NÚT MUA (Thay vì ẩn đi)
            SetPurchaseInteractable(false);
        }

        // Hàm helper để quản lý trạng thái nút mua
        private void SetPurchaseInteractable(bool canInteract)
        {
            if (btnPurchase != null)
            {
                // 1. Luôn bật GameObject và Set trạng thái bấm
                btnPurchase.gameObject.SetActive(true);
                btnPurchase.interactable = canInteract;

                // 2. XỬ LÝ MÀU SẮC (Sửa lỗi tối màu tại đây)
                
                // CÁCH A: Nếu có CanvasGroup (Ưu tiên)
                if (purchaseBtnCanvasGroup != null)
                {
                    purchaseBtnCanvasGroup.alpha = canInteract ? 1f : 0.5f;
                }
                // CÁCH B: Nếu KHÔNG có CanvasGroup -> Tự đổi màu Image
                else 
                {
                    var img = btnPurchase.GetComponent<Image>();
                    if (img != null)
                    {
                        // Nếu bấm được -> Trả về màu Trắng tinh
                        // Nếu khóa -> Chuyển sang màu Xám
                        img.color = canInteract ? Color.white : Color.gray;
                    }
                }
            }
        }

        private void OnRerollClick()
        {
            ShopManager.Instance.RerollShop();
        }

        private void OnBackClick()
        {
            // UIManager.Instance.CloseShop();
            gameObject.SetActive(false);
        }
    }
}