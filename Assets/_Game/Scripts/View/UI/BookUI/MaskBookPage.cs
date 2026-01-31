using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;

namespace _Game.Scripts.View.UI.BookUI
{
    public class MaskBookPage : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Danh sách 7 Slot UI trong Book")]
        [SerializeField] private List<MaskSlotUI> maskSlots;

        [SerializeField] private CharmHolder playerInventory;

        private void OnEnable()
        {
            RefreshBook();
        }

        public void RefreshBook()
        {
            if (MaskManager.Instance == null) return;
            if (playerInventory == null) playerInventory = FindFirstObjectByType<CharmHolder>();

            List<MaskData> allMasks = MaskManager.Instance.GetAllMasks();

            // Duyệt qua các slot UI
            for (int i = 0; i < maskSlots.Count; i++)
            {
                if (i < allMasks.Count)
                {
                    // Có dữ liệu -> Setup
                    maskSlots[i].Setup(allMasks[i], playerInventory);
                }
                else
                {
                    // Hết dữ liệu -> Tắt slot thừa
                    maskSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}