using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;

namespace _Game.Scripts.Core.Managers
{
    public class WeightManager : MonoBehaviour
    {
        public static WeightManager Instance { get; private set; }

        [Header("Data References")]
        [SerializeField] private List<SymbolData> allSymbols;
        
        private int _cachedTotalWeight = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        
        private void Start()
        {
            RecalculateTotalWeight();
        }

        // 1. Reset tất cả về zin (Gọi khi Start Game)
        public void ResetAllWeights()
        {
            foreach (var symbol in allSymbols)
            {
                symbol.ResetStats();
            }
            RecalculateTotalWeight();
            Debug.Log("WeightManager: Đã reset trọng số về gốc.");
        }

        // 2. API để Charm gọi vào (Buff trọng số)
        // amount: 8 nghĩa là +0.8
        public void ApplyBuff(SymbolData targetSymbol, int amount)
        {
            if (targetSymbol != null)
            {
                targetSymbol.ModifyWeight(amount);
                RecalculateTotalWeight();
            }
        }
        
        // HÀM MỚI: Dùng cho logic Buff (+1) = +0.8
        public void ApplyBuffLevel(SymbolData targetSymbol, int levels)
        {
            if (targetSymbol != null)
            {
                targetSymbol.AddWeightLevel(levels);
                RecalculateTotalWeight();
                Debug.Log($"Buff Level {targetSymbol.idName} +{levels} (Weight +{levels*8})");
            }
        }
        
        private void RecalculateTotalWeight()
        {
            _cachedTotalWeight = 0;
            foreach (var symbol in allSymbols)
            {
                _cachedTotalWeight += symbol.currentWeight;
            }
        }

        // Hàm này trả về chuỗi % để hiển thị UI (VD: "19.4%")
        public string GetChanceDisplay(SymbolData symbol)
        {
            if (_cachedTotalWeight <= 0) return "0%";
            
            // Công thức: (Weight / Total) * 100
            // Ép kiểu float để chia có số thập phân
            float percent = ((float)symbol.currentWeight / _cachedTotalWeight) * 100f;
            
            // Format lấy 1 số sau dấu phẩy (VD: 19.4)
            return percent.ToString("F1") + "%";
        }
        
        // Inside WeightManager.cs
        
        public List<SymbolData> GetHighestWeightSymbols()
        {
            List<SymbolData> leaders = new List<SymbolData>();
            int maxW = -1;

            foreach (var sym in allSymbols)
            {
                // Case A: Found a new highest
                if (sym.currentWeight > maxW)
                {
                    maxW = sym.currentWeight;
                    leaders.Clear(); // Discard previous leaders
                    leaders.Add(sym);
                }
                // Case B: It's a tie
                else if (sym.currentWeight == maxW)
                {
                    leaders.Add(sym);
                }
            }
            return leaders;
        }
    }
}