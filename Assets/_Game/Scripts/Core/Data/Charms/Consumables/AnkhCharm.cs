using UnityEngine;
using _Game.Scripts.Core.Data;

[CreateAssetMenu(menuName = "Charms/Consumables/Ankh")]

    public class AnkhCharm : ConsumableCharm
    {
        public override bool OnPaymentCheck(int currentCoin, int currentDebt)
        {
            // Only trigger if the player is actually broke
            if (currentCoin < currentDebt)
            {
                Debug.Log($"[Ankh] Player saved! Debt remaining: {currentDebt}");
                
                // Destroy the Ankh
                // Consume(); 
                
                // Return TRUE to tell GameManager to stop the Game Over process
                return true;
            }
            return false; 
        }
}
