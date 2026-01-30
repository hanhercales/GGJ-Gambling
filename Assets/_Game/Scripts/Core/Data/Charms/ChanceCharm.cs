using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Managers;

public class ChanceCharm : CharmData
{
    [Header("Trigger Settings")]
    [Range(0f, 100f)] 
    public float triggerChance = 10f; // Editable in Inspector (e.g., 10%)

    // [SerializeField] private bool _playSfxOnTrigger = true;
    
    protected bool TryTrigger()
    {
        float mult = 1f;
        if (LuckManager.Instance != null) 
        {
            mult = LuckManager.Instance.GetChanceMultiplier();
        }
        float finalChance = triggerChance * mult;
        
        float roll = Random.Range(0f, 100f);
            
        if (roll <= finalChance)
        {
            return true;
        }
        return false;
    }
}
