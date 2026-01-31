using UnityEngine;
using UnityEngine.EventSystems; // Cần thiết để bắt sự kiện chuột
using UnityEngine.UI;
using _Game.Scripts.Core.Managers;

namespace _Game.Scripts.View.UI
{
    // Script này cần gắn vào object có Image và Button
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Content")]
        public string header;
        [TextArea(3, 10)] public string content; // Cho phép nhập nhiều dòng trong Inspector

        [Header("Highlight Effect")]
        [SerializeField] private Image targetImage; // Ảnh của nút
        [SerializeField] private Sprite highlightSprite; // Sprite khi hover
        private Sprite _originalSprite; // Sprite gốc để trả lại khi chuột đi ra
        
        private Button _btn;

        private void Start()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            
            _btn = GetComponent<Button>();
            
            // Lưu lại sprite gốc
            if (targetImage != null)
            {
                _originalSprite = targetImage.sprite;
            }
        }

        // Khi chuột đi vào (Hover)
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_btn != null && !_btn.interactable) return;
            
            // 1. Gọi Manager để hiện bảng
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Show(content, header);
            }

            // 2. Đổi Sprite Highlight (nếu có setup)
            if (targetImage != null && highlightSprite != null)
            {
                targetImage.sprite = highlightSprite;
            }
        }

        // Khi chuột đi ra (Un-Hover)
        public void OnPointerExit(PointerEventData eventData)
        {
            // 1. Gọi Manager để ẩn bảng
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
            
            if (_btn != null && !_btn.interactable) return;

            // 2. Trả lại Sprite gốc
            if (targetImage != null && _originalSprite != null)
            {
                targetImage.sprite = _originalSprite;
            }
        }
        
        // Cập nhật lại sprite gốc nếu code khác thay đổi sprite (Optional)
        public void RefreshOriginalSprite()
        {
            if (targetImage != null) _originalSprite = targetImage.sprite;
        }
    }
}