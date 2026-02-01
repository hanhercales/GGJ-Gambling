using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Logic;

[CreateAssetMenu(menuName = "Charms/Family/Rare Family")]
public class RSymbolCharm : CharmData
{
    [Header("Settings")]
    public SymbolData targetSymbol; // e.g. "Cherry"
    [Tooltip("Growth Factor. 1.0 means add 100% of BASE value.")]
    public float growthFactor = 1.0f; 

    public override bool OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results)
    {
        bool triggered = false;

        foreach (var match in results)
        {
            if (match.symbol == targetSymbol)
            {
                float growthAmount = match.pattern.baseMultiplier * growthFactor;
                match.pattern.AddPermanentValue(growthAmount);
                Debug.Log($"[Scaler] {match.symbol.idName} buffed {match.pattern.patternName}! (+{growthAmount})");
                triggered = true;
            }
        }
        return triggered;
    }
}