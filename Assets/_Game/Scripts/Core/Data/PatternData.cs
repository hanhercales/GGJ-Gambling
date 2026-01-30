using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewPattern", menuName = "GameConfig/Pattern Data")]
    public class PatternData : ScriptableObject
    {
        #region Thông tin Pattern
        [Header("Info")]
        public string patternName;
        #endregion

        #region Cách tính điểm
        [Header("Scoring")]
        [Tooltip("Giá trị nội tại của Pattern (Gốc). VD: HOR = 1.0")]
        public float baseMultiplier = 1.0f; // Hệ số nhân
        
        public int priority = 0; // Độ ưu tiên (Số càng to càng được xét trước)
        
        [Header("Runtime Info (Read Only)")]
        public float currentMultiplier;
        #endregion

        #region Cấu hình Editor (Không dùng trong game)
        [Header("Editor Config")]
        [Min(1)] public int editorRows = 3; 
        [Min(1)] public int editorCols = 3;
        #endregion

        #region Dữ liệu tọa độ
        [Header("Coordinate Data")]
        // Danh sách tọa độ tương đối tạo nên hình dáng pattern
        public List<Vector2Int> relativeCoordinates = new List<Vector2Int>();
        #endregion
        
        public void ResetStats()
        {
            currentMultiplier = baseMultiplier;
        }

        // Dùng để buff giá trị nội tại của Pattern
        // VD: Charm "Các đường thẳng (HOR) được +0.5x giá trị"
        public void ModifyMultiplier(float amount)
        {
            currentMultiplier += amount;
            if (currentMultiplier < 0) currentMultiplier = 0;
        }
    }
}