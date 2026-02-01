using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Managers;

namespace _Game.Scripts.View.UI
{
    // Enum để xác định trạng thái hiển thị của Menu
    public enum MenuMode
    {
        MainMenu,   // Mới mở game (Chỉ có New Game, Quit)
        Pause,      // Đang chơi bấm Esc (Có thêm Resume)
        GameOver    // Thua cuộc (Chỉ có New Game, Quit)
    }

    public class GameMenuUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button btnNewGame;
        [SerializeField] private Button btnResume; // Nút tắt bảng (dấu X hoặc chữ Resume)
        [SerializeField] private Button btnQuit;   // (Optional)

        private void Awake()
        {
            if (btnNewGame != null) btnNewGame.onClick.AddListener(OnNewGameClick);
            if (btnResume != null) btnResume.onClick.AddListener(OnResumeClick);
            if (btnQuit != null) btnQuit.onClick.AddListener(OnQuitClick);
        }

        public void Setup(MenuMode mode)
        {
            switch (mode)
            {
                case MenuMode.MainMenu:
                    if (titleText != null) titleText.text = "WELCOME";
                    if (btnResume != null) btnResume.gameObject.SetActive(false); // Bắt buộc chọn New Game
                    break;

                case MenuMode.Pause:
                    if (titleText != null) titleText.text = "PAUSED";
                    if (btnResume != null) btnResume.gameObject.SetActive(true); // Cho phép tắt để chơi tiếp
                    break;

                case MenuMode.GameOver:
                    if (titleText != null) titleText.text = "GAME OVER";
                    if (btnResume != null) btnResume.gameObject.SetActive(false); // Thua rồi, không thể Resume
                    break;
            }
        }

        private void OnNewGameClick()
        {
            // Báo GameManager bắt đầu game mới
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            
            // Đóng menu sau khi bấm
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseGameMenu();
            }
        }

        private void OnResumeClick()
        {
            // Chỉ đóng menu, game sẽ tiếp tục
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseGameMenu();
            }
        }

        private void OnQuitClick()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}