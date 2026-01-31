using UnityEngine;

namespace _Game.Scripts.View.UI
{
    public class HUDToggleUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Kéo Panel_Left vào đây")]
        [SerializeField] private GameObject panelLeft;
        
        [Tooltip("Kéo Panel_Right vào đây")]
        [SerializeField] private GameObject panelRight;

        // Trạng thái hiện tại (Mặc định là true - đang hiện)
        private bool _isVisible = true;

        private void Update()
        {
            // Bắt sự kiện nhấn phím TAB
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleHUD();
            }
        }

        private void ToggleHUD()
        {
            // Đảo ngược trạng thái
            _isVisible = !_isVisible;

            // Bật/Tắt GameObject dựa trên trạng thái mới
            if (panelLeft != null) 
                panelLeft.SetActive(_isVisible);
            
            if (panelRight != null) 
                panelRight.SetActive(_isVisible);
        }
    }
}