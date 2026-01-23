using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewSymbol", menuName = "GameConfig/Symbol Data")]
    public class SymbolData : ScriptableObject
    {
        #region Thông tin hiển thị
        [Header("Display Info")]
        public string idName; 
        public Sprite icon;
        [TextArea] public string description;
        #endregion

        #region Thông số Gameplay
        [Header("Base Stats")]
        public int baseValue = 1;
        
        [Tooltip("Đây là trọng số cơ bản, không phải phần trăm cứng.")]
        public float baseSpawnWeight = 10f; 
        #endregion
    }
}