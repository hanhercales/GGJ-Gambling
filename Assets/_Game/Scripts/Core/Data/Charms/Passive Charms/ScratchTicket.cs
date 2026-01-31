using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Scratch Ticket")]
public class ScratchTicket : CharmData
{
    [Header("Settings")]
    public int bonusLevels = 1; // "+1 Level"
    [System.NonSerialized] private List<SymbolData> _activeTargets = new List<SymbolData>();

    private void OnEnable()
    {
        _activeTargets.Clear();
    }
    
    public override void OnEquip(CharmHolder holder)
    {
        ApplyEffect();
    }
    
    public override void OnRoundStart(GameManager gm)
    {
        RemoveEffect();
        ApplyEffect();
    }
    
    public override void OnUnequip(CharmHolder holder)
    {
        RemoveEffect();
    }
    
    private void ApplyEffect()
    {
        if (WeightManager.Instance == null) return;
        List<SymbolData> leaders = WeightManager.Instance.GetHighestWeightSymbols();
        foreach (var sym in leaders)
        {
            WeightManager.Instance.ApplyBuffLevel(sym, bonusLevels);
            _activeTargets.Add(sym);
            Debug.Log($"[LeaderCharm] Buffing Leader: {sym.idName}");
        }
    }

    private void RemoveEffect()
    {
        if (WeightManager.Instance == null) return;
        foreach (var sym in _activeTargets)
        {
            WeightManager.Instance.ApplyBuffLevel(sym, -bonusLevels);
        }
        _activeTargets.Clear();
    }
}