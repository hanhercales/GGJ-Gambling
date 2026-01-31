using UnityEngine;

namespace _Game.Scripts.Core.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject panelDialogContainer; // Panel nền đen
        [SerializeField] private GameObject panelShop;            // Panel Shop
        [SerializeField] private GameObject panelBook;            // Panel BookMask
        [SerializeField] private GameObject panelInfo;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            CloseAllDialogs();
        }

        // --- SHOP LOGIC ---
        public void OpenShop()
        {
            panelDialogContainer.SetActive(true);
            
            if (panelShop != null) panelShop.SetActive(true);
            if (panelInfo != null) panelInfo.SetActive(false);
            if (panelBook != null) panelBook.SetActive(false);
        }

        public void CloseShop()
        {
            if (panelShop != null) panelShop.SetActive(false);
            CheckCloseContainer();
        }

        // --- BOOK LOGIC ---
        public void OpenBook()
        {
            panelDialogContainer.SetActive(true);
            
            if (panelBook != null) panelBook.SetActive(true);
            if (panelInfo != null) panelInfo.SetActive(false);
            if (panelShop != null) panelShop.SetActive(false);
        }

        public void CloseBook()
        {
            if (panelBook != null) panelBook.SetActive(false);
            CheckCloseContainer();
        }
        
        public void OpenInfo()
        {
            panelDialogContainer.SetActive(true);
            if (panelInfo != null) panelInfo.SetActive(true);
            
            if (panelShop != null) panelShop.SetActive(false);
            if (panelBook != null) panelBook.SetActive(false);
        }

        public void CloseInfo()
        {
            if (panelInfo != null) panelInfo.SetActive(false);
            CheckCloseContainer();
        }
        
        // --- SHARED ---
        public void CloseAllDialogs()
        {
            if (panelShop != null) panelShop.SetActive(false);
            if (panelBook != null) panelBook.SetActive(false);
            if (panelInfo != null) panelInfo.SetActive(false);
            if (panelDialogContainer != null) panelDialogContainer.SetActive(false);
        }
        
        // Chỉ tắt nền đen nếu CẢ Shop và Book đều đã đóng
        private void CheckCloseContainer()
        {
            bool isShopActive = panelShop != null && panelShop.activeSelf;
            bool isBookActive = panelBook != null && panelBook.activeSelf;
            bool isInfoActive = panelInfo != null && panelInfo.activeSelf;
            
            if (!isShopActive && !isBookActive)
            {
                if (panelDialogContainer != null) 
                    panelDialogContainer.SetActive(false);
            }
        }
    }
}