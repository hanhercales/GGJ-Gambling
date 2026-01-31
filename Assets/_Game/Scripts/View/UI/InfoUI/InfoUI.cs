using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core.Managers;

namespace _Game.Scripts.View.UI.InfoUI
{
    public class InfoUI : MonoBehaviour
    {
        [Header("Internal UI")]
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null) 
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseInfo();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}