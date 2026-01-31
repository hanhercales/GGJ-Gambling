using System.Collections;
using _Game.Scripts.Core.Data;
using _Game.Scripts.View.UI; // Namespace chứa TooltipTrigger
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(TooltipTrigger))]
    public class HolderSlotUI : MonoBehaviour
    {
        [Header("Visual Components")]
        [Tooltip("Ảnh nền của ô (Slot BG)")]
        [SerializeField] private Image backgroundImage; 
        
        [Tooltip("Ảnh icon của Charm (nằm đè lên BG)")]
        [SerializeField] private Image charmIcon;
        
        [Tooltip("Khung sáng hiển thị khi Charm kích hoạt (nằm đè lên Icon)")]
        [SerializeField] private GameObject activationFrame;

        [Header("Selection (Optional)")]
        [SerializeField] private GameObject selectionBorder; // Viền khi click chọn trong kho

        // Data References
        private CharmData _currentCharm;
        private TooltipTrigger _tooltip;
        private Coroutine _activationCoroutine;

        private void Awake()
        {
            _tooltip = GetComponent<TooltipTrigger>();
            
            // Đảm bảo trạng thái ban đầu
            if (activationFrame != null) activationFrame.SetActive(false);
            if (selectionBorder != null) selectionBorder.SetActive(false);
        }

        // Hàm được gọi từ HolderDisplay để cập nhật dữ liệu
        public void UpdateSlot(CharmData charm)
        {
            _currentCharm = charm;

            if (charm != null)
            {
                // 1. Cập nhật Icon
                if (charmIcon != null)
                {
                    charmIcon.gameObject.SetActive(true);
                    charmIcon.sprite = charm.icon;
                }

                // 2. Cập nhật Tooltip (Kế thừa logic của TooltipTrigger)
                if (_tooltip != null)
                {
                    _tooltip.header = charm.charmName; // Hoặc charm.name
                    _tooltip.content = charm.description;
                    
                    // Nếu muốn đổi icon hover theo charm
                    // _tooltip.SetCustomSprite(charm.icon); 
                }
            }
            else
            {
                // Slot trống
                if (charmIcon != null) charmIcon.gameObject.SetActive(false);
                
                // Tooltip cho ô trống
                if (_tooltip != null)
                {
                    _tooltip.header = "Empty Slot";
                    _tooltip.content = "Purchase Charms from the Shop to fill this slot.";
                }
            }
            
            // Tắt hiệu ứng cũ nếu có
            if (activationFrame != null) activationFrame.SetActive(false);
        }

        // --- HIỆU ỨNG KÍCH HOẠT (FLASH 0.5s) ---
        public void PlayActivationEffect()
        {
            if (activationFrame == null) return;
            
            // Nếu đang chạy dở thì dừng lại chạy cái mới
            if (_activationCoroutine != null) StopCoroutine(_activationCoroutine);
            
            _activationCoroutine = StartCoroutine(ActivationRoutine());
        }

        private IEnumerator ActivationRoutine()
        {
            activationFrame.SetActive(true);
            
            // (Optional) Có thể thêm âm thanh kích hoạt tại đây
            // AudioManager.Instance.PlaySfx(triggerSound);

            yield return new WaitForSeconds(0.5f);
            
            activationFrame.SetActive(false);
            _activationCoroutine = null;
        }

        // --- LOGIC CHỌN (SELECTION) ---
        public void SetSelected(bool isSelected)
        {
            if (selectionBorder != null) selectionBorder.SetActive(isSelected);
        }

        public CharmData GetCurrentCharm() => _currentCharm;
    }
}