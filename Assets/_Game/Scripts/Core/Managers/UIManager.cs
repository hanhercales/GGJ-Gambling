using UnityEngine;

namespace _Game.Scripts.Core.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject panelDialogContainer; // Cha của các dialog
        [SerializeField] private GameObject panelShop;            // Panel_Shop

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Mặc định ẩn container khi vào game
            CloseAllDialogs();
        }

        public void OpenShop()
        {
            panelDialogContainer.SetActive(true);
            panelShop.SetActive(true);
            
            // Có thể pause game hoặc chặn input khác tại đây nếu cần
        }

        public void CloseShop()
        {
            panelShop.SetActive(false);
            panelDialogContainer.SetActive(false); // Nếu không còn dialog nào khác
        }

        public void CloseAllDialogs()
        {
            panelShop.SetActive(false);
            panelDialogContainer.SetActive(false);
        }
    }
}