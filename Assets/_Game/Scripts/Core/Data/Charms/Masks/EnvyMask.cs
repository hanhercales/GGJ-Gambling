using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Logic;
using System.Linq; // Needed for Sorting

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Envy")]
public class MaskOfEnvyCharm : CharmData
{
    [Header("Settings")]
    [Range(0f, 1f)] public float triggerChance = 0.3f; // 30%
    public float bonusMultiplier = 1.5f; // x1.5

    public override void OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results)
    {
        if (results.Count == 0) return;

        // 1. Roll Logic (30%)
        if (Random.value > triggerChance) return;

        // 2. Determine Fate (50/50)
        bool isGoodOutcome = Random.value > 0.5f;

        if (isGoodOutcome)
        {
            ApplyEnvyBonus(results);
        }
        else
        {
            ApplyEnvyCurse(results);
        }
    }

    private void ApplyEnvyBonus(List<MatchResult> results)
    {
        // A. FIND THE "IDOL" (Symbol with highest value)
        // We use currentValue to decide who is "Best"
        var bestMatch = results
            .OrderByDescending(r => r.symbol.currentValue) 
            .FirstOrDefault();

        if (bestMatch == null) return;
        SymbolData bestSymbol = bestMatch.symbol;

        Debug.Log($"[Mask of Envy] JEALOUSY! Transforming all wins into {bestSymbol.idName}...");
        
        
        float gSymMult = ScoreManager.Instance.GetSymbolMult();
        float gPatMult = ScoreManager.Instance.GetPatternMult();
        float extraMoneyToAdd = 0;

        foreach (var match in results)
        {
            // 1. Swap the symbol!
            // The previous 'Lemon' match now thinks it is a 'Diamond' match.
            match.symbol = bestSymbol;

            // 2. Calculate the NEW score
            float newScore = match.GetScore(gSymMult, gPatMult);

            // 3. We want 1.5x total. 
            // The Machine handles 1.0x. We handle the 0.5x.
            extraMoneyToAdd += newScore * (bonusMultiplier - 1.0f);
        }

        // C. PAY THE BONUS
        if (extraMoneyToAdd > 0)
        {
            ResourceManager.Instance.AddResource(ResourceType.Coin, (int)extraMoneyToAdd);
            Debug.Log($"[Mask of Envy] Bonus Multiplier applied: +{extraMoneyToAdd} coins.");
        }
    }

    private void ApplyEnvyCurse(List<MatchResult> results)
    {
        Debug.Log("[Mask of Envy] BACKFIRE! Taking half the earnings...");

        float gSymMult = ScoreManager.Instance.GetSymbolMult();
        float gPatMult = ScoreManager.Instance.GetPatternMult();

        // Calculate what the machine is ABOUT to pay
        float projectedWin = results.Sum(r => r.GetScore(gSymMult, gPatMult));

        // Calculate the penalty (50%)
        int penalty = (int)(projectedWin * 0.5f);

        if (penalty > 0)
        {
            ResourceManager.Instance.TrySpendResource(ResourceType.Coin, penalty);
        }
    }
}