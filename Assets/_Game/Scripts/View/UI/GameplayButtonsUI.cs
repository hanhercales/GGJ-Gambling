using UnityEngine;
using _Game.Scripts.Core.Data;     // Để dùng GameState
using _Game.Scripts.Core.Managers;
using UnityEngine.UI; // Để gọi GameManager

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameplayButtonsUI : MonoBehaviour
    {
        [SerializeField] private Image shopButtonImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite lockedSprite;
        
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            // Lắng nghe sự kiện thay đổi trạng thái game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
                
                // Cập nhật ngay trạng thái ban đầu (để tránh UI bị sai khi mới vào game)
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        // --- LOGIC TỰ ĐỘNG KHÓA/MỞ ---
        private void OnGameStateChanged(GameState newState)
        {
            // Chỉ cho phép tương tác (bấm nút Shop, Book, Mua gói...) khi đang ở giai đoạn Chuẩn Bị
            bool canInteract = (newState == GameState.Preparation);
            
            SetInteractable(canInteract);
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isInteractable;
                _canvasGroup.blocksRaycasts = isInteractable; // Chặn chuột hoàn toàn
                shopButtonImage.sprite = isInteractable ? normalSprite : lockedSprite;
                
                // Làm mờ để người chơi biết là đang bị khóa
                _canvasGroup.alpha = isInteractable ? 1f : 0.6f; 
            }
        }
    }
}