using UnityEngine;

namespace _Game.Scripts.Core.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Global Multipliers")]
        [SerializeField] private float globalSymbolMultiplier = 1.0f;  
        [SerializeField] private float globalPatternMultiplier = 1.0f; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- API Buff/Debuff ---

        public void ModifySymbolMultiplier(float amount)
        {
            globalSymbolMultiplier += amount;
            // Chặn dưới là 1.0 (hoặc 0 tùy design, nhưng thường nhân thì tối thiểu là 1)
            if (globalSymbolMultiplier < 1f) globalSymbolMultiplier = 1f;
        }

        public void ModifyPatternMultiplier(float amount)
        {
            globalPatternMultiplier += amount;
            if (globalPatternMultiplier < 1f) globalPatternMultiplier = 1f;
        }

        public void ResetMultipliers()
        {
            globalSymbolMultiplier = 1.0f;
            globalPatternMultiplier = 1.0f;
            Debug.Log("ScoreManager: Reset Multipliers về 1.0");
        }

        // --- Getters ---
        public float GetSymbolMult() => globalSymbolMultiplier;
        public float GetPatternMult() => globalPatternMultiplier;
    }
}