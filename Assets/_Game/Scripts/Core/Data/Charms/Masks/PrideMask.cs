using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Logic;

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Pride")]
public class MaskOfPrideCharm : MaskData // [THAY ĐỔI] Kế thừa MaskData
{
    [Header("Buff Settings")]
    public SymbolData crownSymbol;
    public string jackpotKeyword = "JACKPOT";

    [Header("The Tyrant's Curse")]
    [Tooltip("These symbols will pay 0 coins (Heart, Clover, Coin, Circle).")]
    public List<SymbolData> peasantSymbols; 
    
    public override bool OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results)
    {
        bool hitTheJackpot = false;
        foreach (var match in results)
        {
            if (match.symbol == crownSymbol && 
                match.pattern != null && 
                match.pattern.patternName.Contains(jackpotKeyword))
            {
                hitTheJackpot = true;
                break;
            }
        }

        if (hitTheJackpot)
        {
            Debug.Log("[Mask of Pride] KNEEL! Crown Jackpot hit. Doubling ALL patterns permanently.");
            if (machine != null && machine.allPatterns != null)
            {
                foreach(var pat in machine.allPatterns) 
                {
                    pat.AddPermanentValue(pat.baseMultiplier); 
                }
            }
            return true; // Kích hoạt hiệu ứng -> True
        }
        return false;
    }
    
    public override long ModifySymbolScore(long currentScore, SymbolData symbol, string symbolID)
    {
        if (peasantSymbols.Contains(symbol))
        {
            return 0;
        }
        return currentScore;
    }
}