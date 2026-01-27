using System;
using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    public class HolderDisplay : MonoBehaviour
    {
        [SerializeField] private CharmHolder charmHolder;
        [SerializeField] private GameObject holderSlotPrefab;
        [SerializeField] private Transform slotParent;
        
        private List<HolderSlotUI> holderSlots =  new List<HolderSlotUI>();
        
        private HolderSlotUI selectedSlot;

        private void Awake()
        {
            if(charmHolder == null) charmHolder = FindFirstObjectByType<CharmHolder>();
            
            if(charmHolder == null) return;

            charmHolder.OnHolderChanged += UpdateHolderUI;

            SetupUI();
        }

        private void OnDestroy()
        {
            if (charmHolder != null)
                charmHolder.OnHolderChanged -= UpdateHolderUI;
        }

        public void SetupUI()
        {
            foreach (Transform slot in slotParent)
            {
                Destroy( slot.gameObject);
            }
            holderSlots.Clear();

            for (int i = 0; i < charmHolder.GetSize(); ++i)
            {
                GameObject slot = Instantiate(holderSlotPrefab, slotParent);
                HolderSlotUI slotUI = slot.GetComponent<HolderSlotUI>();
                if (slotUI != null)
                {
                    holderSlots.Add(slotUI);
                    Button slotButton = slot.GetComponent<Button>();
                    if(slotButton == null)  slotButton = slot.AddComponent<Button>();
                    slotButton.onClick.AddListener(() => OnSlotUI_Clicked(slotUI));
                }
            }

            UpdateHolderUI();
        }

        private void OnSlotUI_Clicked(HolderSlotUI clickedSlot)
        {
            if (selectedSlot == clickedSlot)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }
            else
            {
                if(selectedSlot != null)
                    selectedSlot.SetSelected(false);
                
                selectedSlot = clickedSlot;
                selectedSlot.SetSelected(true);
            }
        }

        public void UpdateHolderUI()
        {
            List<CharmData> content = charmHolder.GetContent();

            for (int i = 0; i < holderSlots.Count; i++)
            {
                if(i <  content.Count)
                    holderSlots[i].UpdateSlot(content[i]);
                else
                    holderSlots[i].UpdateSlot(null);
            }
        }

        public HolderSlotUI GetSelectedSlot()
        {
            return selectedSlot;
        }

        public void SetSelectedSlot(HolderSlotUI slot)
        {
            if (selectedSlot == null) return;
            selectedSlot = slot;
        }
    }
}

