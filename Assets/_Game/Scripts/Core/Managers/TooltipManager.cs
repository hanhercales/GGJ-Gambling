using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace _Game.Scripts.Core.Managers
{
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private LayoutElement layoutElement;

        [Header("Settings")]
        [Tooltip("Chiều rộng tối đa cho phép. Nếu text ngắn hơn, khung sẽ co lại. Nếu dài hơn, text sẽ xuống dòng.")]
        [SerializeField] private float maxTooltipWidth = 300f; // Đổi từ int characterWrapLimit sang float width
        
        [SerializeField] private Vector2 offset = new Vector2(15, 15); 

        private RectTransform rectTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this.gameObject);
            else Instance = this;
        }

        private void Start()
        {
            rectTransform = tooltipPanel.GetComponent<RectTransform>();
            if (rectTransform != null) rectTransform.pivot = new Vector2(0, 0); 
            Hide();
        }

        private void Update()
        {
            if (tooltipPanel.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            Vector2 mousePos = Input.mousePosition;
            float pivotX = mousePos.x / Screen.width;
            float pivotY = mousePos.y / Screen.height;

            if (rectTransform != null)
            {
                rectTransform.pivot = new Vector2(pivotX > 0.5f ? 1 : 0, pivotY > 0.5f ? 1 : 0);
            }

            float offsetX = (rectTransform.pivot.x > 0.5f) ? -offset.x : offset.x;
            float offsetY = (rectTransform.pivot.y > 0.5f) ? -offset.y : offset.y;

            transform.position = mousePos + new Vector2(offsetX, offsetY);
        }

        public void Show(string content, string header = "")
        {
            if (string.IsNullOrEmpty(content) && string.IsNullOrEmpty(header)) return;

            // 1. Set nội dung trước
            if (string.IsNullOrEmpty(header))
            {
                headerText.gameObject.SetActive(false);
            }
            else
            {
                headerText.gameObject.SetActive(true);
                headerText.text = header;
            }

            contentText.text = content;

            // 2. Logic Co giãn thông minh
            // Tắt LayoutElement để text tự bung ra theo chiều ngang (Preferred Width)
            layoutElement.enabled = false;

            // Bắt buộc Unity tính toán lại layout ngay lập tức để lấy PreferredWidth mới nhất
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());

            // Kiểm tra xem Header hoặc Content có vượt quá giới hạn không
            float headerWidth = headerText.gameObject.activeSelf ? headerText.preferredWidth : 0;
            float contentWidth = contentText.preferredWidth;

            // Nếu một trong hai dài hơn Max Width -> Bật LayoutElement để ép xuống dòng
            if (headerWidth > maxTooltipWidth || contentWidth > maxTooltipWidth)
            {
                layoutElement.enabled = true;
                layoutElement.preferredWidth = maxTooltipWidth;
            }
            else
            {
                // Nếu ngắn hơn -> Giữ nguyên LayoutElement tắt (ContentSizeFitter sẽ tự co background lại)
                layoutElement.enabled = false;
            }

            tooltipPanel.SetActive(true);
            UpdatePosition(); 
        }

        public void Hide()
        {
            tooltipPanel.SetActive(false);
        }
    }
}