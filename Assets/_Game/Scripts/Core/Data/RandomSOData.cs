using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    public class RandomSOData : ScriptableObject
    {
        #region Thông tin hiển thị
        [Header("Display Info")]
        public string idName;           // ID dùng trong code (VD: Lemon)
        public Sprite icon;             // Ảnh hiển thị
        #endregion

        #region Thông số Gameplay
        [Tooltip("Trọng số xuất hiện (Càng cao càng dễ ra).")]
        public int baseSpawnWeight; 
        #endregion
    }
}

