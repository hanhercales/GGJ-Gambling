using UnityEngine;
using _Game.Scripts.Core.Data;

public class ChanceCharm : CharmData
{
    [Header("Trigger Settings")]
    [Range(0f, 100f)] 
    public float triggerChance = 10f; // Editable in Inspector (e.g., 10%)

    // [SerializeField] private bool _playSfxOnTrigger = true;
    
    protected bool TryTrigger()
    {
        // 1. Roll the dice
        float roll = Random.Range(0f, 100f);
            
        // 2. Check success
        if (roll <= triggerChance)
        {
            // if (_playSfxOnTrigger)
            // {
            //     // TODO: Call your Audio Manager here
            // }
            return true;
        }

        return false;
    }
}
