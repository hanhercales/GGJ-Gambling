using UnityEngine;

namespace _Game.Scripts.Core.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject panelDialogContainer; // Cha của các dialog
        [SerializeField] private GameObject panelShop;            // Panel_Shop
        [SerializeField] private GameObject panelBook;

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
            
            if (panelBook != null) panelBook.SetActive(false);
        }

        public void CloseShop()
        {
            panelShop.SetActive(false);
            panelDialogContainer.SetActive(false); // Nếu không còn dialog nào khác
        }
        
        public void OpenBook()
        {
            panelDialogContainer.SetActive(true); // QUAN TRỌNG: Bật cha lên
            
            if (panelBook != null) panelBook.SetActive(true);
            if (panelShop != null) panelShop.SetActive(false); // Tắt Shop đi nếu đang mở
        }

        public void CloseBook()
        {
            if (panelBook != null) panelBook.SetActive(false);
            CheckCloseContainer();
        }
        
        public void CloseAllDialogs()
        {
            panelShop.SetActive(false);
            panelDialogContainer.SetActive(false);
        }
        
        private void CheckCloseContainer()
        {
            bool isShopActive = panelShop != null && panelShop.activeSelf;
            bool isBookActive = panelBook != null && panelBook.activeSelf;

            if (!isShopActive && !isBookActive)
            {
                panelDialogContainer.SetActive(false);
            }
        }
    }
}