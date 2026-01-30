using System;
using System.Numerics;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Inventory;

namespace _Game.Scripts.Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SlotMachineController slotMachine;
        [SerializeField] private LuckManager luckManager;
        [SerializeField] private CharmHolder charmHolder;
        
        [Header("Game Config")]
        [SerializeField] private DebtDifficultySO difficultyProfile; 
        [SerializeField] private int startingCoin = 10; 
        [SerializeField] private int stagesPerDebtRound = 4;

        [Header("Current State (Read Only)")]
        [SerializeField] private GameState currentState;
        [SerializeField] private int currentDebtRound = 1;
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int spinsRemaining = 0;
        [SerializeField] private SpinPackSO currentPack;

        // Events
        public event Action<int> OnSpinsChanged;
        public event Action<GameState> OnStateChanged;
        
        // Event cập nhật UI: (CurrentRound, CurrentStage, MaxStage)
        public event Action<int, int, int> OnRoundInfoChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            currentDebtRound = 1;
            currentStage = 1; 
            
            // 1. Reset Tiền & Nợ
            ResourceManager.Instance.ResetAllData(startingCoin);
                
            // 2. Reset Symbol (Weight & Value)
            WeightManager.Instance.ResetAllWeights();
            
            // 3. Reset Global Multipliers (MỚI)
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ResetMultipliers();
            
            // 4. Reset Patterns (MỚI)
            // Gọi thông qua SlotMachineController vì nó giữ list Pattern
            if (slotMachine != null)
                slotMachine.ResetPatternStats();

            // Setup Nợ ban đầu
            if (difficultyProfile != null)
            {
                BigInteger firstDebt = difficultyProfile.GetDebtForRound(currentDebtRound);
                ResourceManager.Instance.SetNewDebt(firstDebt);
            }
            else
            {
                Debug.LogError("CẢNH BÁO: Chưa gán DebtDifficultySO!");
                ResourceManager.Instance.SetNewDebt(25);
            }

            if (charmHolder != null)
            {
                foreach (var charm in charmHolder.GetContent())
                {
                    charm.OnRoundStart(this); 
                }
            }
            
            ChangeState(GameState.Preparation);
            NotifyRoundInfo();
        }

        // --- CORE LOOP ---

        // 1. Người chơi chọn gói Spin (Gọi từ UI Button)
        public void SelectSpinPack(SpinPackSO pack)
        {
            if (currentState != GameState.Preparation) return;

            // Kiểm tra ví tiền
            if (ResourceManager.Instance.TrySpendResource(ResourceType.Coin, pack.coinCost))
            {
                currentPack = pack;
                spinsRemaining = pack.spinCount;
                OnSpinsChanged?.Invoke(spinsRemaining);
                
                // Chuyển sang giai đoạn quay
                ChangeState(GameState.Spinning);
            }
            else
            {
                Debug.Log("Không đủ tiền mua gói này!");
            }
        }

        // 2. Người chơi bấm nút Spin (Gọi từ UI Button)
        public void TriggerSpin()
        {
            if (currentState != GameState.Spinning) return;
            if (spinsRemaining <= 0) return;

            // Lấy Luck từ manager
            int calculatedLuck = LuckManager.Instance.CalculateLuckForSpin();
            Debug.Log($"Spin {LuckManager.Instance.SpinCount + 1} - Luck Applied: {calculatedLuck}");

            // Truyền Luck vào slot machine
            slotMachine.PerformSpin(calculatedLuck, OnSpinCompleted);
        }
        
        private void OnSpinCompleted(float winAmount)
        {
            // Báo cáo kết quả để tính Pity
            bool isWin = winAmount > 0;
            LuckManager.Instance.ReportSpinResult(isWin);
            
            // Cộng tiền thắng
            if (winAmount > 0)
            {
                ResourceManager.Instance.AddResource(ResourceType.Coin, (int)winAmount);
            }

            // Trừ lượt quay
            spinsRemaining--;
            OnSpinsChanged?.Invoke(spinsRemaining);

            // Kiểm tra hết lượt chưa
            if (spinsRemaining <= 0)
            {
                OnSpinPackFinished();
            }
        }
        
        private void OnSpinPackFinished()
        {
            // 1. Thưởng Ticket ngay sau khi quay xong gói (bất kể đang ở stage nào)
            if (currentPack != null)
            {
                ResourceManager.Instance.AddResource(ResourceType.Ticket, currentPack.ticketReward);
                Debug.Log($"Pack Finished! Reward: {currentPack.ticketReward} Tickets.");
            }

            // 2. Kiểm tra tiến độ Stage
            if (currentStage < stagesPerDebtRound)
            {
                // Chưa đến lúc trả nợ -> Sang stage tiếp theo
                currentStage++;
                Debug.Log($"Moving to Stage {currentStage}/{stagesPerDebtRound}");
                
                ChangeState(GameState.Preparation); // Quay lại Shop mua gói tiếp
                NotifyRoundInfo();
            }
            else
            {
                // Đã xong stage cuối (4/4) -> Đến lúc trả nợ
                ResolveDebtCycle();
            }
        }

        // 4. Tổng kết vòng nợ
        private void ResolveDebtCycle()
        {
            ChangeState(GameState.RoundEnd);

            // Logic kiểm tra trả nợ
            if (ResourceManager.Instance.TryPayDebt())
            {
                Debug.Log($"DEBT ROUND {currentDebtRound} CLEARED!");

                // Tăng vòng Nợ lên
                currentDebtRound++;
                
                // Reset Stage về 1
                currentStage = 1;
                
                // Báo cho Luck Manager biết đã qua 1 deadline
                LuckManager.Instance.IncrementDebtCompleted();

                // Setup Nợ mới
                if (difficultyProfile != null)
                {
                    BigInteger nextDebt = difficultyProfile.GetDebtForRound(currentDebtRound);
                    ResourceManager.Instance.SetNewDebt(nextDebt);
                }
                
                ChangeState(GameState.Preparation);
                NotifyRoundInfo();
            }
            else
            {
                Debug.Log("Phá sản! Game Over.");
                ChangeState(GameState.GameOver);
            }
        }

        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
            Debug.Log($"Game State Changed: {newState}");
        }

        public void AddSpins(int amount)
        {
            spinsRemaining += amount;
            OnSpinsChanged?.Invoke(spinsRemaining); 
        }
        
        private void NotifyRoundInfo()
        {
            OnRoundInfoChanged?.Invoke(currentDebtRound, currentStage, stagesPerDebtRound);
        }
    }
}