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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // 1. Reset tất cả về zin (Gọi khi Start Game)
        public void ResetAllWeights()
        {
            foreach (var symbol in allSymbols)
            {
                symbol.ResetWeight();
            }
            Debug.Log("WeightManager: Đã reset trọng số về gốc.");
        }

        // 2. API để Charm gọi vào (Buff trọng số)
        // amount: 8 nghĩa là +0.8
        public void ApplyBuff(SymbolData targetSymbol, int amount)
        {
            if (targetSymbol != null)
            {
                targetSymbol.ModifyWeight(amount);
                Debug.Log($"Weight Buff: {targetSymbol.idName} += {amount} (New: {targetSymbol.currentWeight})");
            }
        }
        
        // 3. API Buff toàn bộ (Ví dụ Charm buff tất cả trái cây)
        public void ApplyGlobalBuff(int amount)
        {
            foreach (var symbol in allSymbols)
            {
                symbol.ModifyWeight(amount);
            }
        }
    }
}