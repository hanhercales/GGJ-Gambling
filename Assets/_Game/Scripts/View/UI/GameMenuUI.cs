using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Managers;

namespace _Game.Scripts.View.UI
{
    public enum MenuMode
    {
        MainMenu,   
        Pause,      
        GameOver,
        WinGame     
    }

    public class GameMenuUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button btnNewGame;
        [SerializeField] private Button btnResume;
        [SerializeField] private TextMeshProUGUI resumeButtonText; 
        [SerializeField] private Button btnQuit;   

        private MenuMode _currentMode; 

        private void Awake()
        {
            if (btnNewGame != null) btnNewGame.onClick.AddListener(OnNewGameClick);
            if (btnResume != null) btnResume.onClick.AddListener(OnResumeClick);
            if (btnQuit != null) btnQuit.onClick.AddListener(OnQuitClick);
        }

        public void Setup(MenuMode mode)
        {
            _currentMode = mode; 
            
            if (resumeButtonText != null) resumeButtonText.text = "RESUME";

            switch (mode)
            {
                case MenuMode.MainMenu:
                    if (titleText != null) titleText.text = "WELCOME";
                    if (btnResume != null) btnResume.gameObject.SetActive(false);
                    break;

                case MenuMode.Pause:
                    if (titleText != null) titleText.text = "PAUSED";
                    if (btnResume != null) btnResume.gameObject.SetActive(true);
                    break;

                case MenuMode.GameOver:
                    if (titleText != null) titleText.text = "GAME OVER";
                    if (btnResume != null) btnResume.gameObject.SetActive(false);
                    break;
                
                case MenuMode.WinGame:
                    if (titleText != null) titleText.text = "YOU WIN!"; 
                    if (btnResume != null) 
                    {
                        btnResume.gameObject.SetActive(true);
                        // Đổi chữ thành Continue Endless
                        if (resumeButtonText != null) resumeButtonText.text = "CONTINUE"; 
                    }
                    break;
            }
        }

        private void OnNewGameClick()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            CloseMenu();
        }

        private void OnResumeClick()
        {
            if (_currentMode == MenuMode.WinGame)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ContinueEndlessMode();
                }
            }
            
            CloseMenu();
        }

        private void OnQuitClick()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void CloseMenu()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseGameMenu();
            }
        }
    }
}