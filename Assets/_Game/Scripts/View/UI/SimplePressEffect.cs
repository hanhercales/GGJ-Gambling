using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using _Game.Scripts.View.UI; // Để nhận diện TooltipTrigger

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(Image), typeof(Button))]
    public class SimplePressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [Tooltip("Sprite sẽ hiện khi bạn NHẤN GIỮ nút.")]
        [SerializeField] private Sprite pressedSprite;

        private Image _image;
        private Button _button;
        private Sprite _spriteBeforePress; // Lưu lại sprite trước khi bấm (có thể là Normal hoặc Highlight)
        private bool _isPressed = false;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 1. Nếu nút bị khóa (do SpinPackLocker khóa) -> Không làm gì cả
            if (_button == null || !_button.interactable) return;
            
            // 2. Nếu chưa gán ảnh Pressed -> Không làm gì cả
            if (pressedSprite == null) return;

            // 3. Lưu lại sprite hiện tại (để tí nữa trả lại)
            _spriteBeforePress = _image.sprite;
            
            // 4. Đổi sang ảnh Pressed
            _image.sprite = pressedSprite;
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Khi thả chuột ra: Trả lại ảnh cũ (thường là ảnh Highlight vì chuột vẫn đang hover)
            if (_isPressed)
            {
                RevertSprite();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPressed) return;

            // TRƯỜNG HỢP ĐẶC BIỆT: Kéo chuột ra ngoài khi đang giữ nút
            
            // Nếu nút này có TooltipTrigger: 
            // TooltipTrigger sẽ tự động trả về ảnh gốc (Idle) trong hàm OnPointerExit của nó.
            // Nên ta chỉ cần tắt cờ _isPressed và để TooltipTrigger lo phần hiển thị.
            if (GetComponent<TooltipTrigger>() != null)
            {
                _isPressed = false; 
                return;
            }

            // Nếu không có TooltipTrigger:
            // Ta tự trả về ảnh cũ.
            RevertSprite();
        }

        private void RevertSprite()
        {
            if (_image != null && _spriteBeforePress != null)
            {
                // Chỉ revert nếu nút vẫn đang mở (đề phòng trường hợp vừa bấm xong thì bị khóa)
                if (_button.interactable)
                {
                    _image.sprite = _spriteBeforePress;
                }
            }
            _isPressed = false;
        }
    }
}