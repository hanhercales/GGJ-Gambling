using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewSymbol", menuName = "GameConfig/Symbol Data")]
    public class SymbolData : RandomSOData
    {
        #region Thông số Gameplay
        [Header("Base Stats")]
        public int baseValue = 1;       // Điểm cơ bản
        
        // QUY ƯỚC: Weight nhập trong Inspector đã nhân 10.
        // VD: Lemon 1.3 -> Nhập 13.
        // VD: Seven 0.5 -> Nhập 5.
        
        [Header("Runtime Info (Read Only)")]
        public int currentWeight;
        public int currentValue;
        #endregion

        // HẰNG SỐ: Mỗi 1 Level buff (+1) tương ứng tăng 0.8 weight -> Int là 8
        private const int WEIGHT_PER_LEVEL = 8;
        
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
        
        // Gọi hàm này: symbol.AddWeightLevel(1) -> Nó tự cộng 8
        public void AddWeightLevel(int levels)
        {
            int amountToAdd = levels * WEIGHT_PER_LEVEL;
            ModifyWeight(amountToAdd);
        }
        
        public void ModifyValue(int amount)
        {
            currentValue += amount;
            // Tiền có thể âm (nếu có debuff), nhưng thường thì chặn ở 0
            // if (currentValue < 0) currentValue = 0; 
        }
    }
}