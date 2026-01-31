using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core.Managers; // Cần dòng này để gọi UIManager

namespace _Game.Scripts.View.UI.BookUI
{
    public class BookUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button openButton; 
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (openButton != null) 
                openButton.onClick.AddListener(OpenBook);
            
            if (closeButton != null) 
                closeButton.onClick.AddListener(CloseBook);
        }

        // Logic thay đổi: Gọi UIManager
        public void OpenBook()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenBook();
            }
        }

        public void CloseBook()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseBook();
            }
        }
    }
}