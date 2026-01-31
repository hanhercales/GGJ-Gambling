using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Logic;

namespace _Game.Scripts.Core.Managers
{
    public class CharmManager : MonoBehaviour
    {
        public static CharmManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CharmHolder charmHolder;
        [SerializeField] private LuckManager luckManager;
        [SerializeField] private SlotMachineController slotMachine; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- PUBLIC API: Gọi từ GameManager ---

        public void NotifySpinStart()
        {
            if (charmHolder == null) return;

            // Tạo bản sao list để tránh lỗi nếu Charm tự hủy giữa chừng
            var charms = new List<CharmData>(charmHolder.GetContent());
            
            foreach (var charm in charms)
            {
                // Gọi hàm kích hoạt
                charm.OnSpinStart(slotMachine, luckManager);
            }
        }

        public void NotifySpinResult(float winAmount, List<MatchResult> results)
        {
            if (charmHolder == null) return;

            var charms = new List<CharmData>(charmHolder.GetContent());
            
            foreach (var charm in charms)
            {
                // 1. Xử lý logic thắng thua (ConsoPrize)
                charm.OnSpinResult(slotMachine, luckManager, winAmount);

                // 2. Xử lý logic buff Symbol (CSymbolCharm)
                charm.OnSpinResultBuff(slotMachine, results);
            }
        }

        public void NotifySpinEnd()
        {
            if (charmHolder == null) return;

            // Duyệt ngược để an toàn hơn khi xóa
            var charms = new List<CharmData>(charmHolder.GetContent());
            for (int i = charms.Count - 1; i >= 0; i--)
            {
                charms[i].OnSpinEnd(slotMachine, luckManager);
            }
        }

        public void NotifyRoundStart(GameManager gm)
        {
            if (charmHolder == null) return;
            foreach (var charm in charmHolder.GetContent())
            {
                charm.OnRoundStart(gm);
            }
        }

        // Kiểm tra xem có charm nào cứu mạng không (Ankh)
        public bool CheckPaymentSavior(int currentCoin, int currentDebt)
        {
            if (charmHolder == null) return false;

            var charms = new List<CharmData>(charmHolder.GetContent());
            foreach (var charm in charms)
            {
                if (charm.OnPaymentCheck(currentCoin, currentDebt))
                {
                    // Nếu là Consumable dùng 1 lần, phải xóa nó đi
                    if (charm is ConsumableCharm consumable && consumable.destroyOnUse)
                    {
                        charmHolder.RemoveCharm(charm);
                    }
                    return true; // Được cứu!
                }
            }
            return false;
        }

        // Helper để SlotMachine lấy list charm tính điểm
        public List<CharmData> GetActiveCharms()
        {
            return charmHolder != null ? charmHolder.GetContent() : new List<CharmData>();
        }
    }
}