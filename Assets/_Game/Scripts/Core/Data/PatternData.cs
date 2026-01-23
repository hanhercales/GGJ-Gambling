using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewPattern", menuName = "GameConfig/Pattern Data")]
    public class PatternData : ScriptableObject
    {
        [Header("Display Info")]
        public string patternName;

        [Header("Calculate Score")]
        public float multiplier = 1.0f;
        public int priority = 0;

        [Header("Matrix Config (Editor Only)")]
        [Min(1)] public int editorRows = 3; 
        [Min(1)] public int editorCols = 3;

        [Header("Coordinate Config (Read Only)")]
        public List<Vector2Int> relativeCoordinates = new List<Vector2Int>();
    }
}