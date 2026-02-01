using UnityEngine;
using _Game.Scripts.Core.Data;     
using _Game.Scripts.Core.Managers;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameplayButtonsUI : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private Image shopButtonImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite lockedSprite;
        
        private CanvasGroup _canvasGroup;
        private Button _shopButton; 

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // Tìm component Button
            if (shopButtonImage != null) _shopButton = shopButtonImage.GetComponent<Button>();
            if (_shopButton == null) _shopButton = GetComponentInChildren<Button>();
        }

        private void Update()
        {
            // [STRICT MODE] Kiểm tra liên tục mỗi khung hình
            if (GameManager.Instance != null)
            {
                GameState state = GameManager.Instance.CurrentState;
                
                // Chỉ mở khi đang ở Preparation
                bool shouldBeInteractable = (state == GameState.Preparation);
                
                // Cập nhật trạng thái
                ForceUpdateState(shouldBeInteractable);
            }
        }

        private void ForceUpdateState(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                // 1. Chặn Raycast (Chuột không bấm được)
                if (_canvasGroup.blocksRaycasts != isInteractable)
                    _canvasGroup.blocksRaycasts = isInteractable;

                // 2. Chặn Interactable (Logic hệ thống)
                if (_canvasGroup.interactable != isInteractable)
                    _canvasGroup.interactable = isInteractable;

                // [THAY ĐỔI] Luôn giữ độ sáng 100% (Alpha = 1), không làm mờ nữa
                if (_canvasGroup.alpha != 1f)
                    _canvasGroup.alpha = 1f;
            }

            // 3. Đổi Sprite (Khóa/Mở)
            if (shopButtonImage != null)
            {
                Sprite targetSprite = isInteractable ? normalSprite : lockedSprite;
                
                // Chỉ gán lại nếu sprite thay đổi (để tối ưu)
                if (shopButtonImage.sprite != targetSprite)
                    shopButtonImage.sprite = targetSprite;
                
                // [THAY ĐỔI] Luôn giữ màu trắng (gốc) để LockedSprite hiển thị đúng màu thiết kế
                // Không phủ màu xám (Color.gray) đè lên nữa
                if (shopButtonImage.color != Color.white)
                    shopButtonImage.color = Color.white;
            }
            
            // 4. Khóa cứng component Button (Lớp bảo vệ cuối cùng)
            if (_shopButton != null && _shopButton.interactable != isInteractable)
            {
                _shopButton.interactable = isInteractable;
            }
        }
    }
}