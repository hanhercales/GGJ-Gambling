using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    public class HolderDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharmHolder charmHolder;
        [SerializeField] private GameObject holderSlotPrefab;
        [SerializeField] private Transform slotParent;
        
        // List quản lý UI Slot
        private List<HolderSlotUI> holderSlots = new List<HolderSlotUI>();
        
        // Dictionary để map từ Data -> UI (Giúp tìm slot nhanh để chạy hiệu ứng)
        private Dictionary<CharmData, HolderSlotUI> _charmToSlotMap = new Dictionary<CharmData, HolderSlotUI>();

        private HolderSlotUI selectedSlot;

        private void Awake()
        {
            if(charmHolder == null) charmHolder = FindFirstObjectByType<CharmHolder>();
            
            if(charmHolder == null) return;

            charmHolder.OnHolderChanged += UpdateHolderUI;
        }
        
        private void Start()
        {
            SetupUI();
        }

        private void OnDestroy()
        {
            if (charmHolder != null)
                charmHolder.OnHolderChanged -= UpdateHolderUI;
        }

        // Sinh ra các ô slot (chỉ làm khi khởi tạo hoặc thay đổi số lượng túi)
        public void SetupUI()
        {
            // Clear map cũ
            _charmToSlotMap.Clear();
            holderSlots.Clear();
            
            // Xóa các object con cũ (nếu có)
            foreach (Transform child in slotParent)
            {
                Destroy(child.gameObject);
            }
            
            int targetSize = charmHolder.GetSize();
            
            for (int i = 0; i < targetSize; ++i)
            {
                GameObject slotObj = Instantiate(holderSlotPrefab, slotParent);
                HolderSlotUI slotUI = slotObj.GetComponent<HolderSlotUI>();

                if (slotUI != null)
                {
                    holderSlots.Add(slotUI);
                    
                    // Thêm Button để click chọn (nếu cần xem chi tiết)
                    Button slotButton = slotObj.GetComponent<Button>();
                    if(slotButton == null) slotButton = slotObj.AddComponent<Button>();
                    
                    // Chỉnh transition button thành None để không xung đột màu sắc
                    slotButton.transition = Selectable.Transition.None; 

                    slotButton.onClick.RemoveAllListeners();
                    slotButton.onClick.AddListener(() => OnSlotUI_Clicked(slotUI));
                }
            }

            UpdateHolderUI();
        }

        // Cập nhật nội dung bên trong các slot
        public void UpdateHolderUI()
        {
            // Nếu size thay đổi thì phải setup lại layout
            if (holderSlots.Count != charmHolder.GetSize())
            {
                SetupUI();
                return;
            }

            _charmToSlotMap.Clear();
            List<CharmData> content = charmHolder.GetContent();

            for (int i = 0; i < holderSlots.Count; i++)
            {
                if (i < content.Count)
                {
                    CharmData charm = content[i];
                    holderSlots[i].UpdateSlot(charm);
                    
                    // Map Charm này vào Slot này để sau này tìm cho dễ
                    if (charm != null && !_charmToSlotMap.ContainsKey(charm))
                    {
                        _charmToSlotMap[charm] = holderSlots[i];
                    }
                }
                else
                {
                    holderSlots[i].UpdateSlot(null);
                }
            }
        }
        
        // --- API ĐỂ KÍCH HOẠT HIỆU ỨNG TỪ BÊN NGOÀI ---
        // Gọi hàm này từ GameManager hoặc CharmManager khi một Charm kích hoạt
        public void PlayCharmEffect(CharmData charm)
        {
            if (charm == null) return;

            if (_charmToSlotMap.TryGetValue(charm, out HolderSlotUI slotUI))
            {
                slotUI.PlayActivationEffect();
            }
        }

        // Logic click chọn
        private void OnSlotUI_Clicked(HolderSlotUI clickedSlot)
        {
            if (selectedSlot == clickedSlot)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }
            else
            {
                if(selectedSlot != null) selectedSlot.SetSelected(false);
                
                selectedSlot = clickedSlot;
                selectedSlot.SetSelected(true);
            }
        }
    }
}