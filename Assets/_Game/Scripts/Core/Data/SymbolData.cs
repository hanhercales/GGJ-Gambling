using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewSymbol", menuName = "GameConfig/Symbol Data")]
    public class SymbolData : RandomSOData
    {
        #region Thông số Gameplay
        [Header("Base Stats")]
        public int baseValue = 1;       // Điểm cơ bản
        public int currentWeight; 
        #endregion

        public void ResetWeight()
        {
            currentWeight = baseSpawnWeight;
        }
    }
}