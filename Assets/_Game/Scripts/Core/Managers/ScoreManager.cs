using UnityEngine;

namespace _Game.Scripts.Core.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Global Multipliers")]
        [SerializeField] private float globalSymbolMultiplier = 1.0f;  
        [SerializeField] private float globalPatternMultiplier = 1.0f; 
        
        // --- NEW: The Greed Multiplier (Default 1.0) ---
        private float _greedFactor = 1.0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- API Buff/Debuff ---

        public void ModifySymbolMultiplier(float amount)
        {
            globalSymbolMultiplier += amount;
            if (globalSymbolMultiplier < 0f) globalSymbolMultiplier = 0f; // Allow 0, but usually not negative
        }

        public void ModifyPatternMultiplier(float amount)
        {
            globalPatternMultiplier += amount;
            if (globalPatternMultiplier < 0f) globalPatternMultiplier = 0f;
        }

        // --- NEW: Set Greed (Mask of Greed calls this) ---
        public void SetGreedMultiplier(float value)
        {
            _greedFactor = value;
            Debug.Log($"ScoreManager: Greed Factor set to {value}x");
        }

        public void ResetMultipliers()
        {
            globalSymbolMultiplier = 1.0f;
            globalPatternMultiplier = 1.0f;
            _greedFactor = 1.0f; // Reset Greed too
            Debug.Log("ScoreManager: All Multipliers reset.");
        }

        // --- UPDATED GETTERS (Apply Greed here) ---
        public float GetSymbolMult() => globalSymbolMultiplier * _greedFactor;
        public float GetPatternMult() => globalPatternMultiplier * _greedFactor;
    }
}