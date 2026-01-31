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
        [SerializeField] private int characterWrapLimit = 80;
        
        // Offset: Đẩy Tooltip ra xa chuột một chút (X: sang phải, Y: lên trên)
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
            
            // QUAN TRỌNG: Set Pivot về góc dưới-trái (0, 0)
            // Để khi set vị trí tại chuột, bảng sẽ mọc sang phải và lên trên
            if (rectTransform != null)
            {
                rectTransform.pivot = new Vector2(0, 0); 
            }
            
            Hide();
        }

        private void Update()
        {
            if (tooltipPanel.activeSelf)
            {
                // Lấy vị trí chuột
                Vector2 mousePos = Input.mousePosition;
                
                // Logic kiểm tra biên màn hình (để không bị tràn ra ngoài)
                float pivotX = mousePos.x / Screen.width;
                float pivotY = mousePos.y / Screen.height;

                // Nếu chuột ở nửa phải màn hình -> Tooltip hiện sang trái chuột
                // Nếu chuột ở nửa trên màn hình -> Tooltip hiện xuống dưới chuột
                // (Chuyển Pivot linh hoạt)
                if (rectTransform != null)
                {
                    rectTransform.pivot = new Vector2(pivotX > 0.5f ? 1 : 0, pivotY > 0.5f ? 1 : 0);
                }

                // Cập nhật vị trí + Offset an toàn
                // Nếu pivot là 1 (bên phải), ta cần trừ offset, ngược lại thì cộng
                float offsetX = (rectTransform.pivot.x > 0.5f) ? -offset.x : offset.x;
                float offsetY = (rectTransform.pivot.y > 0.5f) ? -offset.y : offset.y;

                transform.position = mousePos + new Vector2(offsetX, offsetY);
            }
        }

        public void Show(string content, string header = "")
        {
            if (string.IsNullOrEmpty(content) && string.IsNullOrEmpty(header)) return;

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

            int headerLength = headerText.text.Length;
            int contentLength = contentText.text.Length;

            layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);

            tooltipPanel.SetActive(true);
            
            // Gọi Update 1 lần ngay lập tức để tránh bị nháy ở vị trí cũ
            Update(); 
        }

        public void Hide()
        {
            tooltipPanel.SetActive(false);
        }
    }
}