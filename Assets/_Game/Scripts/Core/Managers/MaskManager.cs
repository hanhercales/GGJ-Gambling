using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Controllers.Machines; // Để tham chiếu SlotMachineController
using _Game.Scripts.Core.Logic;         // Để tham chiếu LuckManager

namespace _Game.Scripts.Core.Managers
{
    public class MaskManager : MonoBehaviour
    {
        public static MaskManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CharmHolder playerInventory;
        [Tooltip("Kéo tất cả các Mask SO vào đây")]
        [SerializeField] private List<MaskData> allGameMasks;

        // Danh sách các Mask đang hoạt động
        private List<MaskData> _activeMasks = new List<MaskData>();
        
        public List<MaskData> GetAllMasks() => allGameMasks;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (playerInventory != null)
            {
                // Lắng nghe sự kiện thay đổi túi đồ để check mở khóa Mask
                playerInventory.OnHolderChanged += CheckAllMasks;
            }
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.OnHolderChanged -= CheckAllMasks;
            }
        }

        // --- CORE LOGIC ---

        public void ResetAllMasksForNewGame()
        {
            _activeMasks.Clear();
            foreach (var mask in allGameMasks)
            {
                mask.ResetMaskState();
            }
            Debug.Log("[MaskManager] All masks reset for New Game.");
        }

        private void CheckAllMasks()
        {
            foreach (var mask in allGameMasks)
            {
                // Nếu mask chưa nằm trong danh sách Active
                if (!_activeMasks.Contains(mask))
                {
                    // Kiểm tra điều kiện (Hàm này đã bao gồm logic "Đã mở là giữ luôn")
                    if (mask.CheckUnlockCondition(playerInventory))
                    {
                        ActivateMask(mask);
                    }
                }
            }
        }

        private void ActivateMask(MaskData mask)
        {
            _activeMasks.Add(mask);
            
            // Có thể gọi sự kiện OnEquip nếu Mask cần setup gì đó ban đầu
            mask.OnEquip(playerInventory); 
            
            Debug.Log($"[MaskManager] ACTIVATED MASK: {mask.charmName}");
            
            // TODO: Bắn event để UI hiển thị thông báo "Mask Unlocked!"
        }

        // --- EVENT FORWARDING (Chuyển tiếp sự kiện Game sang Mask) ---
        // Các hàm này cần được gọi từ GameManager hoặc CharmManager
        
        public void NotifySpinStart(SlotMachineController machine, LuckManager luck)
        {
            foreach (var mask in _activeMasks) mask.OnSpinStart(machine, luck);
        }

        public void NotifySpinResult(SlotMachineController machine, LuckManager luck, float winAmount)
        {
            foreach (var mask in _activeMasks) mask.OnSpinResult(machine, luck, winAmount);
        }

        public void NotifySpinResultBuff(SlotMachineController machine, List<MatchResult> results)
        {
            foreach (var mask in _activeMasks) mask.OnSpinResultBuff(machine, results);
        }
        
        public void NotifySpinEnd(SlotMachineController machine, LuckManager luck)
        {
            foreach (var mask in _activeMasks) mask.OnSpinEnd(machine, luck);
        }

        public void NotifyRoundStart(GameManager gm)
        {
            foreach (var mask in _activeMasks) mask.OnRoundStart(gm);
        }

        // Getter cho UI hiển thị
        public List<MaskData> GetActiveMasks() => _activeMasks;
    }
}