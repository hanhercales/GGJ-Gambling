using _Game.Scripts.View.UI; // Để dùng MenuMode
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
        [SerializeField] private GameObject panelInfo;            // Panel Info
        [SerializeField] private GameObject panelGameMenu;        // Panel chứa GameMenuUI
        private GameMenuUI _gameMenuScript;
        
        // Getter kiểm tra menu có mở không để GameManager chặn Esc
        public bool IsGameMenuOpen => panelGameMenu != null && panelGameMenu.activeSelf;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Lấy script UI từ panel
            if (panelGameMenu != null)
                _gameMenuScript = panelGameMenu.GetComponent<GameMenuUI>();
        }

        private void Start()
        {
            CloseAllDialogs();
            // Không mở gì ở đây cả, để GameManager quyết định
        }

        public void OpenShop()
        {
            panelDialogContainer.SetActive(true);
            
            if (panelShop != null) panelShop.SetActive(true);
            if (panelInfo != null) panelInfo.SetActive(false);
            if (panelBook != null) panelBook.SetActive(false);
            if (panelGameMenu != null) panelGameMenu.SetActive(false);
        }

        public void CloseShop()
        {
            if (panelShop != null) panelShop.SetActive(false);
            CheckCloseContainer();
        }

        public void OpenBook()
        {
            panelDialogContainer.SetActive(true);
            
            if (panelBook != null) panelBook.SetActive(true);
            if (panelInfo != null) panelInfo.SetActive(false);
            if (panelShop != null) panelShop.SetActive(false);
            if (panelGameMenu != null) panelGameMenu.SetActive(false);
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
            if (panelGameMenu != null) panelGameMenu.SetActive(false);
        }

        public void CloseInfo()
        {
            if (panelInfo != null) panelInfo.SetActive(false);
            CheckCloseContainer();
        }
        
        public void OpenGameMenu(MenuMode mode)
        {
            panelDialogContainer.SetActive(true);
            
            if (panelGameMenu != null)
            {
                panelGameMenu.SetActive(true);
                // Setup text và nút bấm tùy theo chế độ (MainMenu/Pause/GameOver)
                if (_gameMenuScript != null) _gameMenuScript.Setup(mode);
            }

            // Tắt các UI khác để tập trung vào Menu
            if (panelShop != null) panelShop.SetActive(false);
            if (panelBook != null) panelBook.SetActive(false);
            if (panelInfo != null) panelInfo.SetActive(false);
        }

        public void CloseGameMenu()
        {
            if (panelGameMenu != null) panelGameMenu.SetActive(false);
            CheckCloseContainer();
        }

        public void CloseAllDialogs()
        {
            if (panelShop != null) panelShop.SetActive(false);
            if (panelBook != null) panelBook.SetActive(false);
            if (panelInfo != null) panelInfo.SetActive(false);
            if (panelGameMenu != null) panelGameMenu.SetActive(false);
            
            if (panelDialogContainer != null) panelDialogContainer.SetActive(false);
        }
        
        private void CheckCloseContainer()
        {
            bool isShopActive = panelShop != null && panelShop.activeSelf;
            bool isBookActive = panelBook != null && panelBook.activeSelf;
            bool isInfoActive = panelInfo != null && panelInfo.activeSelf;
            bool isMenuActive = panelGameMenu != null && panelGameMenu.activeSelf;
            
            if (!isShopActive && !isBookActive && !isInfoActive && !isMenuActive)
            {
                if (panelDialogContainer != null) 
                    panelDialogContainer.SetActive(false);
            }
        }
    }
}