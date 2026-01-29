using System;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Extra Spins")]
public class ExtraSpinCharm : CharmData
{
    [Header("Settings")]
    public int extraSpins = 2;

    public override void OnRoundStart(GameManager gameManager)
    {
        gameManager.AddSpins(extraSpins);
        Debug.Log($"[Passive] Added {extraSpins} extra spins for this round.");
    }
}
