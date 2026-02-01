using System.Collections.Generic;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using UnityEngine;
using _Game.Scripts.Core.Logic;

[CreateAssetMenu(menuName = "Charms/Family/Common Family")]
public class CSymbolCharm : ChanceCharm
{
    [Header("Settings")]
    [SerializeField] public SymbolData targetedSymbol;

    public override bool OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results)
    {
        bool triggered = false;

        foreach (var match in results)
        {
            if (match.symbol.idName == targetedSymbol.idName) 
            {
                if (TryTrigger())
                {
                    targetedSymbol.ModifyValue(targetedSymbol.baseValue);
                    Debug.Log($"[Scaler] {targetedSymbol.name} grew! Base is now {targetedSymbol.baseValue}");
                    triggered = true;
                }
            }
        }
        return triggered;
    }
}
