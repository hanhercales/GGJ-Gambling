using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameplayButtonsUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isInteractable;
                
                // Tùy chọn: Giảm alpha một chút để người chơi biết nó đang bị khóa
                // _canvasGroup.alpha = isInteractable ? 1f : 0.6f; 
            }
        }
    }
}