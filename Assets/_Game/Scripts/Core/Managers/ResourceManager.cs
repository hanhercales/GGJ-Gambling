using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using _Game.Scripts.Core.Data;

namespace _Game.Scripts.Core.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        // Singleton Pattern để dễ truy cập từ bất cứ đâu
        public static ResourceManager Instance { get; private set; }

        private BigInteger coin = 0;
        private BigInteger currentDebt = 0;
            
        [Header("Debug Info (Read Only)")]
        [SerializeField] private string coinDisplay = "0";
        [SerializeField] private int ticket = 0;
        [SerializeField] private string debtDisplay = "0";
        
        [Header("Restrictions")]
        private bool _ticketsBlocked = false;
        
        public event Action<ResourceType, string> OnResourceChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        #region Getters
        public BigInteger GetResourceBigInt(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Coin: return coin;
                case ResourceType.Debt: return currentDebt;
                case ResourceType.Ticket: return ticket; // Implicit conversion int -> BigInt
                default: return 0;
            }
        }
        
        public int GetTicket() => ticket;
        #endregion

        #region Modifiers (Thay đổi số liệu)

        public void AddResource(ResourceType type, BigInteger amount)
        {
            switch (type)
            {
                case ResourceType.Coin:
                    coin += amount;
                    if (coin < 0) coin = 0;
                    OnResourceChanged?.Invoke(type, FormatBigInt(coin));
                    break;
                    
                case ResourceType.Debt:
                    currentDebt += amount;
                    OnResourceChanged?.Invoke(type, FormatBigInt(currentDebt));
                    break;
                    
                case ResourceType.Ticket:
                    if (_ticketsBlocked)
                    {
                        Debug.Log("[ResourceManager] Blocked Ticket gain due to active Curse.");
                        return; // Reject the deposit
                    }
                    ticket += (int)amount;
                    if (ticket < 0) ticket = 0;
                    OnResourceChanged?.Invoke(type, ticket.ToString());
                    break;
            }
            UpdateInspectorDisplay();
        }

        public void AddResource(ResourceType type, int amount)
        {
            AddResource(type, new BigInteger(amount));
        }

        public bool TrySpendResource(ResourceType type, BigInteger cost)
        {
            BigInteger currentAmount = GetResourceBigInt(type);
            if (currentAmount >= cost)
            {
                AddResource(type, -cost); 
                return true; 
            }
            return false; 
        }
        
        public void SetTicketBlock(bool isBlocked)
        {
            _ticketsBlocked = isBlocked;
            Debug.Log($"[ResourceManager] Ticket Block set to: {isBlocked}");
        }

        // Overload cho cost int
        public bool TrySpendResource(ResourceType type, int cost)
        {
            return TrySpendResource(type, new BigInteger(cost));
        }

        public void SetNewDebt(BigInteger amount)
        {
            currentDebt = amount;
            OnResourceChanged?.Invoke(ResourceType.Debt, FormatBigInt(currentDebt));
            
            UpdateInspectorDisplay();
        }
        
        public bool TryPayDebt()
        {
            if (coin >= currentDebt)
            {
                AddResource(ResourceType.Coin, -currentDebt);
                
                currentDebt = 0; 
                OnResourceChanged?.Invoke(ResourceType.Debt, FormatBigInt(currentDebt));
                UpdateInspectorDisplay();
                
                return true; 
            }
            return false;
        }
        
        // Reset game (Cho debug hoặc chơi lại)
        public void ResetAllData(int startCoin)
        {
            coin = startCoin;
            ticket = 0;
            currentDebt = 0;
            
            OnResourceChanged?.Invoke(ResourceType.Coin, FormatBigInt(coin));
            OnResourceChanged?.Invoke(ResourceType.Ticket, ticket.ToString());
            OnResourceChanged?.Invoke(ResourceType.Debt, FormatBigInt(currentDebt));
            
            UpdateInspectorDisplay();
        }

        #endregion
        
        #region Helper Format
        private void UpdateInspectorDisplay()
        {
#if UNITY_EDITOR
            // Format số đầy đủ để dễ debug chính xác
            coinDisplay = coin.ToString(); 
            debtDisplay = currentDebt.ToString();
#endif
        }
        
        private string FormatBigInt(BigInteger number)
        {
            if (number < 1000) return number.ToString();
            if (number < 1000000) return ((double)number / 1000).ToString("0.#") + "K";
            if (number < 1000000000) return ((double)number / 1000000).ToString("0.##") + "M";

            // Với số siêu lớn, dùng định dạng khoa học
            double d = (double)number;
            return d.ToString("0.##E+0"); 
        }
        #endregion
    }
}