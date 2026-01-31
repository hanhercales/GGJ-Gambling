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

        public void NotifySpinStart()
        {
            if (charmHolder == null) return;
            var charms = new List<CharmData>(charmHolder.GetContent());
            
            foreach (var charm in charms)
            {
                charm.OnSpinStart(slotMachine, luckManager);
            }
        }

        public void NotifySpinResult(float winAmount, List<MatchResult> results)
        {
            if (charmHolder == null) return;
            var charms = new List<CharmData>(charmHolder.GetContent());
            
            foreach (var charm in charms)
            {
                charm.OnSpinResult(slotMachine, luckManager, winAmount);
                charm.OnSpinResultBuff(slotMachine, results);
            }
        }

        public void NotifySpinEnd()
        {
            if (charmHolder == null) return;
            
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

        public bool CheckPaymentSavior(int currentCoin, int currentDebt)
        {
            if (charmHolder == null) return false;
            
            var charms = new List<CharmData>(charmHolder.GetContent());
            
            foreach (var charm in charms)
            {
                if (charm.OnPaymentCheck(currentCoin, currentDebt))
                {
                    Debug.Log($"[CharmManager] Player saved by {charm.charmName}!");
                    
                    if (charm is ConsumableCharm cons && cons.autoDiscard)
                    {
                        charmHolder.RemoveCharm(charm);
                        Debug.Log($"[CharmManager] {charm.charmName} consumed.");
                    }

                    return true;
                }
            }
            return false;
        }

        public List<CharmData> GetActiveCharms()
        {
            return charmHolder != null ? charmHolder.GetContent() : new List<CharmData>();
        }
    }
}