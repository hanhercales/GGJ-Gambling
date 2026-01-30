using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewSymbol", menuName = "GameConfig/Symbol Data")]
    public class SymbolData : RandomSOData
    {
        #region Thông số Gameplay
        [Header("Base Stats")]
        public int baseValue = 1;       // Điểm cơ bản
        // Trọng số hiện tại (Sau khi đã cộng trừ buff)
        // Biến này sẽ được GridModel dùng để tính toán
        [Header("Runtime Info (Read Only)")]
        public int currentWeight;
        public int currentValue;
        #endregion

        public void ResetStats()
        {
            currentWeight = baseSpawnWeight;
            currentValue = baseValue;
        }
        
        public void ModifyWeight(int amount)
        {
            currentWeight += amount;
            if (currentWeight < 0) currentWeight = 0;
        }
        
        public void ModifyValue(int amount)
        {
            currentValue += amount;
            // Tiền có thể âm (nếu có debuff), nhưng thường thì chặn ở 0
            // if (currentValue < 0) currentValue = 0; 
        }
    }
}