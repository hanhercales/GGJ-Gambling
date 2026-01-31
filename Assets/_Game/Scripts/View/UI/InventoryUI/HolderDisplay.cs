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
            holderSlots.Clear();
            int targetSize = charmHolder.GetSize();
            
            while (slotParent.childCount > targetSize)
            {
                Transform childToRemove = slotParent.GetChild(slotParent.childCount - 1);
                DestroyImmediate(childToRemove.gameObject); 
            }
            
            for (int i = 0; i < targetSize; ++i)
            {
                GameObject slotObj;

                //Kiểm tra nếu parent đã có con thì dùng lại, không tạo mới
                if (i < slotParent.childCount)
                {
                    slotObj = slotParent.GetChild(i).gameObject;
                }
                else
                {
                    slotObj = Instantiate(holderSlotPrefab, slotParent);
                }

                // Setup các thành phần logic cho slot
                HolderSlotUI slotUI = slotObj.GetComponent<HolderSlotUI>();
                if (slotUI != null)
                {
                    holderSlots.Add(slotUI);
                    
                    Button slotButton = slotObj.GetComponent<Button>();
                    if(slotButton == null) slotButton = slotObj.AddComponent<Button>();

                    slotButton.onClick.RemoveAllListeners();

                    slotButton.onClick.AddListener(() => 
                    {
                        if (AudioManager.Instance != null) 
                            AudioManager.Instance.PlayClick();
                        
                        OnSlotUI_Clicked(slotUI);
                    });
                }
            }

            // Sau khi sinh xong thì cập nhật nội dung
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
            // 1. Kiểm tra xem số lượng slot trên UI có khớp với dữ liệu thực tế không
            // Nếu CharmHolder đã tăng size (ví dụ mua Estate), ta phải Setup lại UI từ đầu
            if (holderSlots.Count != charmHolder.GetSize())
            {
                SetupUI(); 
                return; // SetupUI sẽ gọi lại UpdateHolderUI ở cuối, nên ta return luôn để tránh chạy 2 lần
            }

            // 2. Cập nhật icon như bình thường
            List<CharmData> content = charmHolder.GetContent();

            for (int i = 0; i < holderSlots.Count; i++)
            {
                if(i < content.Count)
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

