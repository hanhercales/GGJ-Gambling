using _Game.Scripts.Core.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "Charms/Symbol Modifier")]
public class SymbolModifierCharm : CharmData
{
    [Header("Family Settings")]
    public SymbolData targetSymbolID; // e.g., "Lemon"
    
    [Header("Buffs")]
    public int scoreAdder;        // For Common: "Increases symbol by base value"
    public float scoreMultiplier; // For Legendary: "Doubles current value"
    public int spawnWeightBonus;  // For Uncommon: "Appears more often"

    public override long ModifySymbolScore(long currentScore, SymbolData symbolId, string symbolName)
    {
        if (symbolId == targetSymbolID)
        {
            long newScore = (currentScore + scoreAdder);
            newScore = (long)(newScore * scoreMultiplier);
            return newScore;
        }
        return currentScore;
    }
}
