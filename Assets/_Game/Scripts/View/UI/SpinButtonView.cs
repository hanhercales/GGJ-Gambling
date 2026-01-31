using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Data;

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(TooltipTrigger))]
    public class SpinButtonView : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool isOption2;

        [Header("Visual Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI ticketText;
        [SerializeField] private GameObject ticketIcon;

        [Header("State Sprites")]
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite lockedSprite;

        private Button _btn;
        private Image _img;
        private TooltipTrigger _tooltip;
        
        private SpinOption _currentOption;

        private void Awake()
        {
            _btn = GetComponent<Button>();
            _img = GetComponent<Image>();
            _tooltip = GetComponent<TooltipTrigger>();
            
            _btn.onClick.AddListener(OnClick);

            if (normalSprite == null && _img != null) normalSprite = _img.sprite;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                // 1. Lắng nghe thay đổi giá/gói
                GameManager.Instance.OnSpinOptionsUpdated += OnOptionsUpdated;
                
                // 2. Lắng nghe thay đổi số lượt quay
                GameManager.Instance.OnSpinsChanged += OnSpinsChanged;

                // 3. [MỚI - QUAN TRỌNG] Lắng nghe thay đổi Trạng thái Game
                // Để khi chuyển sang SPINNING thì đổi sprite lock ngay
                GameManager.Instance.OnStateChanged += OnGameStateChanged;

                GameManager.Instance.RequestSpinOptionsUpdate();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpinOptionsUpdated -= OnOptionsUpdated;
                GameManager.Instance.OnSpinsChanged -= OnSpinsChanged;
                GameManager.Instance.OnStateChanged -= OnGameStateChanged; // [MỚI]
            }
        }

        // --- EVENT HANDLERS ---

        private void OnOptionsUpdated(SpinOption opt1, SpinOption opt2)
        {
            _currentOption = isOption2 ? opt2 : opt1;
            RefreshState();
        }

        private void OnSpinsChanged(int remaining)
        {
            RefreshState();
        }

        // [MỚI] Xử lý khi Game đổi trạng thái (Spinning <-> Preparation)
        private void OnGameStateChanged(GameState newState)
        {
            RefreshState();
        }

        // --- CORE LOGIC ---
        private void RefreshState()
        {
            if (_currentOption == null) return;

            // 1. Cập nhật Text
            if (titleText != null) titleText.text = _currentOption.title;
            if (costText != null) costText.text = _currentOption.coinCost > 0 ? $"{_currentOption.coinCost}" : "FREE";
            if (ticketText != null) ticketText.text = $"+{_currentOption.ticketReward}";

            if (_tooltip != null)
            {
                _tooltip.header = _currentOption.title;
                _tooltip.content = _currentOption.description;
            }

            // 2. Kiểm tra điều kiện khóa
            bool isLockedByGameplay = false;
            if (GameManager.Instance != null)
            {
                // CHỈ KHÓA KHI ĐANG QUAY
                // (Logic này cho phép cộng dồn spin ở phase Preparation)
                isLockedByGameplay = (GameManager.Instance.CurrentState == GameState.Spinning);
            }

            bool isAvailable = _currentOption.isAvailable; 
            
            // Nút bấm được khi: KHÔNG bị khóa bởi gameplay VÀ Gói này khả dụng
            bool canInteract = !isLockedByGameplay && isAvailable;

            // 3. Cập nhật UI
            _btn.interactable = canInteract;

            if (_img != null)
            {
                // Hiển thị ảnh khóa nếu: Bị khóa do đang quay HOẶC Không khả dụng (thiếu tiền/disable)
                bool showLockSprite = isLockedByGameplay || !isAvailable;
                
                _img.sprite = showLockSprite ? lockedSprite : normalSprite;

                if (_tooltip != null)
                {
                    _tooltip.RefreshOriginalSprite();
                }
            }
        }

        private void OnClick()
        {
            if (_currentOption != null && _btn.interactable)
            {
                GameManager.Instance.SelectSpinOption(_currentOption);
            }
        }
    }
}