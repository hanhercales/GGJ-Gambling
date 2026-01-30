using _Game.Scripts.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    public class HolderSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image selectedBorder;
        [SerializeField] private TextMeshProUGUI nameText;
        
        private CharmData currentCharm;
        private bool isSelected = false;

        public void UpdateSlot(CharmData charm)
        {
            if (charm != null)
            {
                nameText.text = charm.name;
                currentCharm = charm;
                icon.sprite = charm.icon;
                icon.enabled = true;
            }
            else
            {
                nameText.text = "";
                icon.sprite = null;
                icon.enabled = false;
            }
            
            selectedBorder.enabled = false;
            nameText.enabled = false;
        }

        public void SetSelected(bool selected)
        {
            this.isSelected = selected;
            UpdateSelectionUI(selected);
        }

        private void UpdateSelectionUI(bool selected)
        {
            selectedBorder.enabled = selected;
            nameText.enabled = selected;
        }

        public CharmData GetCurrentCharm()
        {
            return currentCharm;
        }
    }
}

