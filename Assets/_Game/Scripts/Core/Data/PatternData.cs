using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewPattern", menuName = "GameConfig/Pattern Data")]
    public class PatternData : ScriptableObject
    {
        #region Pattern Info
        [Header("Info")]
        public string patternName;
        #endregion

        #region Scoring
        [Header("Scoring")]
        [Tooltip("Base Intrinsic Value. E.g: HOR = 1.0")]
        public float baseMultiplier = 1.0f; 
        
        public int priority = 0;
        
        [Header("Runtime Info (Read Only)")]
        public float currentMultiplier; // The value used by the game
        
        // We keep this just for debugging (to see how much we grew)
        [System.NonSerialized] private float _runtimeValueBonus = 0f;
        #endregion

        #region Editor & Coordinates
        [Header("Editor Config")]
        [Min(1)] public int editorRows = 3; 
        [Min(1)] public int editorCols = 3;

        [Header("Coordinate Data")]
        public List<Vector2Int> relativeCoordinates = new List<Vector2Int>();
        #endregion
        
        // 1. RESET: Go back to base stats
        public void ResetStats()
        {
            currentMultiplier = baseMultiplier;
            _runtimeValueBonus = 0f;
        }
        
        public void AddPermanentValue(float amount)
        {
            _runtimeValueBonus += amount;
            currentMultiplier += amount; 
            Debug.Log($"[Pattern] {patternName} leveled up! Bonus: +{_runtimeValueBonus} | Total: {currentMultiplier}x");
        }
        
        public void ModifyMultiplier(float amount)
        {
            currentMultiplier += amount;
            if (currentMultiplier < 0) currentMultiplier = 0;
        }
    }
}