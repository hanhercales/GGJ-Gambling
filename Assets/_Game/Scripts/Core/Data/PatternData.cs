using System.Collections.Generic;
using UnityEngine;

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
        public float multiplier = 1.0f; // Hệ số nhân
        public int priority = 0;        // Độ ưu tiên (Số càng to càng được xét trước)
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
    }
}