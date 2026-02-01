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
        [SerializeField] private LayoutElement layoutElement; // Gắn vào Panel cha chứa Text

        [Header("Settings")]
        [Tooltip("Chiều rộng tối đa của Tooltip. Nếu text dài hơn, nó sẽ tự xuống dòng.")]
        [SerializeField] private int maxTooltipWidth = 400; 
        
        [Tooltip("Khoảng cách giữa chuột và tooltip")]
        [SerializeField] private Vector2 offset = new Vector2(15, 15); 

        private RectTransform _rectTransform;
        private Canvas _parentCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this.gameObject);
            else Instance = this;
        }

        private void Start()
        {
            _rectTransform = tooltipPanel.GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>(); // Lấy Canvas để tính Scale
            
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
            // 1. Lấy vị trí chuột
            Vector2 mousePos = Input.mousePosition;

            // 2. Tính toán Pivot dựa trên vị trí chuột trên màn hình
            // Màn hình chia làm 4 góc:
            // pivotX = 0 (Left) nếu chuột ở nửa trái, = 1 (Right) nếu chuột ở nửa phải
            // pivotY = 0 (Bottom) nếu chuột ở nửa dưới, = 1 (Top) nếu chuột ở nửa trên
            float pivotX = (mousePos.x / Screen.width) > 0.5f ? 1f : 0f;
            float pivotY = (mousePos.y / Screen.height) > 0.5f ? 1f : 0f;

            if (_rectTransform != null)
            {
                _rectTransform.pivot = new Vector2(pivotX, pivotY);
            }

            // 3. Tính Offset thông minh
            // Nếu Pivot là 1 (Bên phải), ta cần trừ Offset để nó không đè lên chuột
            // Nếu Pivot là 0 (Bên trái), ta cần cộng Offset
            float finalOffsetX = (pivotX == 1f) ? -offset.x : offset.x;
            float finalOffsetY = (pivotY == 1f) ? -offset.y : offset.y;

            // 4. Cập nhật vị trí
            transform.position = mousePos + new Vector2(finalOffsetX, finalOffsetY);
            
            // [NÂNG CAO] Clamp vào màn hình (Phòng trường hợp tooltip quá to vẫn bị tràn)
            KeepOnScreen();
        }

        private void KeepOnScreen()
        {
            // Lấy 4 góc của Tooltip trong không gian màn hình
            Vector3[] corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);

            // corners[0] = Bottom Left, corners[2] = Top Right
            float bottomLeftX = corners[0].x;
            float topRightX = corners[2].x;
            float bottomLeftY = corners[0].y;
            float topRightY = corners[2].y;

            Vector3 shift = Vector3.zero;

            // Kiểm tra tràn bên Phải
            if (topRightX > Screen.width)
                shift.x -= (topRightX - Screen.width);
            
            // Kiểm tra tràn bên Trái
            if (bottomLeftX < 0)
                shift.x += (0 - bottomLeftX);

            // Kiểm tra tràn lên Trên
            if (topRightY > Screen.height)
                shift.y -= (topRightY - Screen.height);
            
            // Kiểm tra tràn xuống Dưới
            if (bottomLeftY < 0)
                shift.y += (0 - bottomLeftY);

            transform.position += shift;
        }

        public void Show(string content, string header = "")
        {
            if (string.IsNullOrEmpty(content) && string.IsNullOrEmpty(header)) return;

            // 1. Xử lý Header
            if (string.IsNullOrEmpty(header))
            {
                headerText.gameObject.SetActive(false);
            }
            else
            {
                headerText.gameObject.SetActive(true);
                headerText.text = header;
            }

            // 2. Xử lý Content
            contentText.text = content;

            // 3. Xử lý Max Width (Thay thế logic đếm ký tự cũ)
            // Bật LayoutElement và gán PreferredWidth
            if (layoutElement != null)
            {
                layoutElement.enabled = true;
                layoutElement.preferredWidth = maxTooltipWidth;
            }

            tooltipPanel.SetActive(true);
            
            // Cập nhật vị trí ngay lập tức để tránh bị nháy hình ở vị trí cũ (0,0)
            UpdatePosition(); 
        }

        public void Hide()
        {
            tooltipPanel.SetActive(false);
        }
    }
}