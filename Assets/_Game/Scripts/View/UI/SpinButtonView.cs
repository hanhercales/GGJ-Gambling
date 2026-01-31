using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Data;

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(TooltipTrigger))] // Tích hợp sẵn Tooltip
    public class SpinButtonView : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Nếu tích, nút này sẽ hiển thị Option 2 (Bên phải/Gói ít lượt). Nếu bỏ tích, hiển thị Option 1 (Bên trái).")]
        [SerializeField] private bool isOption2;

        [Header("Visual Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI ticketText; // (Optional) Hiển thị số vé thưởng nếu muốn
        [SerializeField] private GameObject ticketIcon;      // (Optional) Icon vé

        [Header("State Sprites")]
        [Tooltip("Ảnh khi nút MỞ (Mua được)")]
        [SerializeField] private Sprite normalSprite;
        [Tooltip("Ảnh khi nút KHÓA (Đang còn lượt quay HOẶC Không đủ tiền)")]
        [SerializeField] private Sprite lockedSprite;

        // Internal References
        private Button _btn;
        private Image _img;
        private TooltipTrigger _tooltip;
        
        // Data State
        private SpinOption _currentOption;
        private bool _hasSpinsRemaining; // Đang còn lượt quay hay không?

        private void Awake()
        {
            _btn = GetComponent<Button>();
            _img = GetComponent<Image>();
            _tooltip = GetComponent<TooltipTrigger>();
            
            _btn.onClick.AddListener(OnClick);

            // Tự động lưu sprite hiện tại làm normal nếu chưa gán
            if (normalSprite == null && _img != null) normalSprite = _img.sprite;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                // Lắng nghe thay đổi về Option (Giá tiền/Gói)
                GameManager.Instance.OnSpinOptionsUpdated += OnOptionsUpdated;
                
                // Lắng nghe thay đổi về số lượt quay (Để khóa nút khi đang chơi)
                GameManager.Instance.OnSpinsChanged += OnSpinsChanged;

                // Khởi tạo trạng thái ban đầu
                _hasSpinsRemaining = GameManager.Instance.SpinsRemaining > 0;
                GameManager.Instance.RequestSpinOptionsUpdate();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpinOptionsUpdated -= OnOptionsUpdated;
                GameManager.Instance.OnSpinsChanged -= OnSpinsChanged;
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
            bool hasSpins = remaining > 0;
            
            // Chỉ refresh nếu trạng thái thực sự thay đổi để tối ưu performance
            if (_hasSpinsRemaining != hasSpins)
            {
                _hasSpinsRemaining = hasSpins;
                RefreshState();
            }
        }

        // --- CORE LOGIC: QUYẾT ĐỊNH HIỂN THỊ ---
        private void RefreshState()
        {
            if (_currentOption == null) return;

            // 1. Cập nhật Text
            if (titleText != null) titleText.text = _currentOption.title;
            
            if (costText != null) 
                costText.text = _currentOption.coinCost > 0 ? $"{_currentOption.coinCost}" : "FREE";
            
            if (ticketText != null) 
                ticketText.text = $"+{_currentOption.ticketReward}";

            if (_tooltip != null)
            {
                _tooltip.header = _currentOption.title;
                _tooltip.content = _currentOption.description;
            }

            // 2. Quyết định trạng thái
            bool isLockedByGameplay = _hasSpinsRemaining;
            bool isAvailable = _currentOption.isAvailable; 
            bool canInteract = !isLockedByGameplay && isAvailable;

            // 3. Cập nhật UI Button & Image
            _btn.interactable = canInteract;

            if (_img != null)
            {
                bool showLockSprite = isLockedByGameplay || !isAvailable;
                
                // Đổi ảnh
                _img.sprite = showLockSprite ? lockedSprite : normalSprite;

                // --- [THÊM ĐOẠN NÀY] ---
                // Báo cho Tooltip biết: "Đây là ảnh gốc mới nhé, đừng nhớ cái cũ nữa!"
                if (_tooltip != null)
                {
                    _tooltip.RefreshOriginalSprite();
                }
                // -----------------------
            }
        }

        private void OnClick()
        {
            // Bảo vệ 2 lớp: Check lại lần nữa trước khi gửi lệnh
            if (_currentOption != null && _btn.interactable)
            {
                GameManager.Instance.SelectSpinOption(_currentOption);
            }
        }
    }
}