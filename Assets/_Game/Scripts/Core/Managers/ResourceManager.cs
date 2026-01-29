using System;
using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;

namespace _Game.Scripts.Core.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        // Singleton Pattern để dễ truy cập từ bất cứ đâu
        public static ResourceManager Instance { get; private set; }

        [Header("Debug Info (Read Only)")]
        [SerializeField] private int coin = 0;
        [SerializeField] private int ticket = 0;
        [SerializeField] private int currentDebt = 0;

        // Sự kiện để UI tự động cập nhật (Observer Pattern)
        // Tham số 1: Loại tài nguyên, Tham số 2: Giá trị mới
        public event Action<ResourceType, int> OnResourceChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        #region Getters
        public int GetResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Coin: return coin;
                case ResourceType.Ticket: return ticket;
                case ResourceType.Debt: return currentDebt;
                default: return 0;
            }
        }
        #endregion

        #region Modifiers (Thay đổi số liệu)

        // Hàm tổng quát để cộng/trừ tài nguyên
        public void AddResource(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Coin:
                    coin += amount;
                    if (coin < 0) coin = 0; // Không cho âm tiền
                    break;
                case ResourceType.Ticket:
                    ticket += amount;
                    if (ticket < 0) ticket = 0;
                    break;
                case ResourceType.Debt:
                    currentDebt += amount; 
                    // Debt có thể tăng lên (khi qua màn) hoặc giảm đi (nếu có item giảm nợ)
                    break;
            }

            // Báo cho UI biết là số liệu đã thay đổi
            OnResourceChanged?.Invoke(type, GetResource(type));
        }

        // Hàm kiểm tra và tiêu tiền (dùng khi mua Pack hoặc Charm)
        public bool TrySpendResource(ResourceType type, int cost)
        {
            int currentAmount = GetResource(type);
            if (currentAmount >= cost)
            {
                AddResource(type, -cost); // Trừ tiền
                return true; // Mua thành công
            }
            return false; // Không đủ tiền
        }

        // Setup lại nợ cho vòng mới
        public void SetNewDebt(int amount)
        {
            currentDebt = amount;
            OnResourceChanged?.Invoke(ResourceType.Debt, currentDebt);
        }
        
        // Logic trả nợ cuối vòng
        // Trả về TRUE nếu đủ tiền trả nợ, FALSE nếu phá sản
        public bool TryPayDebt()
        {
            if (coin >= currentDebt)
            {
                AddResource(ResourceType.Coin, -currentDebt);
                return true; // Trả nợ thành công, sống sót qua màn
            }
            return false; // Game Over
        }
        
        // Reset game (Cho debug hoặc chơi lại)
        public void ResetAllData(int startCoin)
        {
            coin = startCoin;
            ticket = 0;
            currentDebt = 0;
            
            OnResourceChanged?.Invoke(ResourceType.Coin, coin);
            OnResourceChanged?.Invoke(ResourceType.Ticket, ticket);
            OnResourceChanged?.Invoke(ResourceType.Debt, currentDebt);
        }

        #endregion
    }
}