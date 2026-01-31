using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core.Managers;

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))] // Bắt buộc phải có Image để đổi Sprite
    public class SpinPackLocker : MonoBehaviour
    {
        [Header("Components")]
        private Button _myButton;
        private Image _myImage;
        [SerializeField] private CanvasGroup _visualGroup; // Vẫn giữ tùy chọn làm mờ nếu muốn

        [Header("Sprite Settings")]
        [Tooltip("Hình ảnh khi nút hoạt động bình thường (Sáng). Để trống sẽ dùng ảnh hiện tại làm mặc định.")]
        [SerializeField] private Sprite unlockedSprite;

        [Tooltip("Hình ảnh khi nút bị khóa (Tối/Ổ khóa).")]
        [SerializeField] private Sprite lockedSprite;

        // Lưu trữ sprite gốc phòng trường hợp không gán unlockedSprite
        private Sprite _defaultSprite;

        private void Awake()
        {
            _myButton = GetComponent<Button>();
            _myImage = GetComponent<Image>();
            
            // Lưu lại sprite ban đầu làm sprite mặc định
            if (_myImage != null)
            {
                _defaultSprite = _myImage.sprite;
            }

            if (_visualGroup == null) _visualGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpinsChanged += UpdateLockState;
                UpdateLockState(GameManager.Instance.SpinsRemaining);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnSpinsChanged -= UpdateLockState;
            }
        }

        private void UpdateLockState(int spinsRemaining)
        {
            // Logic: Còn lượt (spins > 0) -> BỊ KHÓA
            bool isLocked = (spinsRemaining > 0);

            // 1. Khóa chức năng bấm
            _myButton.interactable = !isLocked;

            // 2. Đổi hình ảnh (Sprite Swap)
            if (_myImage != null)
            {
                if (isLocked && lockedSprite != null)
                {
                    // Trạng thái KHÓA -> Dùng ảnh khóa
                    _myImage.sprite = lockedSprite;
                }
                else
                {
                    // Trạng thái MỞ -> Dùng ảnh mở (hoặc ảnh gốc)
                    _myImage.sprite = (unlockedSprite != null) ? unlockedSprite : _defaultSprite;
                }
            }

            // 3. (Tùy chọn) Xử lý Canvas Group
            // Nếu đã có Sprite khóa riêng rồi thì có thể không cần làm mờ nữa, hoặc giữ cả 2 tùy bạn
            if (_visualGroup != null)
            {
                // Ví dụ: Nếu có sprite khóa thì không cần làm mờ (alpha = 1), nếu không có thì làm mờ (alpha = 0.5)
                if (lockedSprite != null)
                {
                     _visualGroup.alpha = 1f; 
                }
                else
                {
                    _visualGroup.alpha = isLocked ? 0.5f : 1f;
                }
            }
        }
    }
}