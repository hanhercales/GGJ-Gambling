using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewSymbol", menuName = "GameConfig/Symbol Data")]
    public class SymbolData : ScriptableObject
    {
        #region Thông tin hiển thị
        [Header("Display Info")]
        public string idName;           // ID dùng trong code (VD: Lemon)
        public Sprite icon;             // Ảnh hiển thị
        [TextArea] public string description;
        #endregion

        #region Thông số Gameplay
        [Header("Base Stats")]
        public int baseValue = 1;       // Điểm cơ bản
        
        [Tooltip("Trọng số xuất hiện (Càng cao càng dễ ra).")]
        public float baseSpawnWeight = 10f; 
        #endregion
    }
}