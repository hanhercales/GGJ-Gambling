using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Logic;
using System.Numerics;

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Wrath")]
public class WrathMask : MaskData // [THAY ĐỔI] Kế thừa MaskData
{
    [Header("Buff Settings")]
    public int luckBonus = 5;

    [Header("Curse Settings")]
    public List<string> forbiddenPatterns = new List<string>() 
    { 
        "Above", "Below", "Eye", "Jackpot" 
    };
    
    [System.NonSerialized] private bool _buffApplied = false;
    [System.NonSerialized] private BigInteger _accumulatedRoundProfit = 0; 

    private void OnEnable()
    {
        _buffApplied = false;
        _accumulatedRoundProfit = 0;
    }

    public override void OnDeadlineStart(GameManager gm)
    {
        _accumulatedRoundProfit = 0;
        Debug.Log("[Mask of Wrath] New Round. Profit tracker reset to 0.");
    }
    
    public override void OnSpinStart(SlotMachineController machine, LuckManager luckManager)
    {
        luckManager.baseLuckFromCharms += luckBonus;
        _buffApplied = true;
    }
    
    public override void OnSpinResult(SlotMachineController machine, LuckManager luckManager, float winAmount)
    {
        if (winAmount > 0)
        {
            _accumulatedRoundProfit += (int)winAmount;
        }
    }
    
    public override void OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results)
    {
        // Did we hit a forbidden pattern?
        bool triggeredWrath = results.Any(match => forbiddenPatterns.Contains(match.pattern.patternName));

        if (triggeredWrath)
        {
            if (_accumulatedRoundProfit > 0)
            {
                ResourceManager.Instance.TrySpendResource(ResourceType.Coin, _accumulatedRoundProfit);

                Debug.Log($"[Mask of Wrath] CURSE! Forbidden Pattern found.");
                Debug.Log($"[Mask of Wrath] Voiding entire Round Profit: -{_accumulatedRoundProfit} Coins.");
                
                _accumulatedRoundProfit = 0;
                if (machine != null)
                {
                    machine.ForceStopVisuals();
                    if (machine.scoreText != null) machine.scoreText.text = "VOIDED";
                }
            }
        }
    }

    // 5. CLEANUP
    public override void OnSpinEnd(SlotMachineController machine, LuckManager luckManager)
    {
        if (_buffApplied)
        {
            luckManager.baseLuckFromCharms -= luckBonus;
            _buffApplied = false;
        }
    }
}