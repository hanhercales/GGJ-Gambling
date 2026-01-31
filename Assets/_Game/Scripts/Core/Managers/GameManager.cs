using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Logic;

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

        [Header("Shop Progression")]
        [Tooltip("Danh sách tỉ lệ Shop theo độ khó. VD: Element 0 = Early, Element 1 = Mid...")]
        [SerializeField] private List<ShopProbabilitySO> shopProfiles;
        
        [Header("Current State (Read Only)")]
        [SerializeField] private GameState currentState;
        [SerializeField] private int currentDebtRound = 1;
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int spinsRemaining = 0;
        [SerializeField] private SpinPackSO currentPack;
        
        public int SpinsRemaining => spinsRemaining;

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
            
            // Cập nhật tỉ lệ Shop về Early Game
            UpdateShopDifficulty();
            
            // Reroll Shop miễn phí cho khởi đầu mới
            if (ShopManager.Instance != null)
                ShopManager.Instance.RerollShop(true);

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

            if (CharmManager.Instance != null)
                CharmManager.Instance.NotifyRoundStart(this);
            
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
                spinsRemaining += pack.spinCount;
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
            
            if (CharmManager.Instance != null)
                CharmManager.Instance.NotifySpinStart();
            
            // Lấy Luck từ manager
            int calculatedLuck = LuckManager.Instance.CalculateLuckForSpin();
            Debug.Log($"Spin {LuckManager.Instance.SpinCount + 1} - Luck Applied: {calculatedLuck}");

            // Truyền Luck vào slot machine
            slotMachine.PerformSpin(calculatedLuck, OnSpinCompleted);
        }
        
        private void OnSpinCompleted(float winAmount, List<MatchResult> results)
        {
            // Báo cáo kết quả để tính Pity
            bool isWin = winAmount > 0;
            LuckManager.Instance.ReportSpinResult(isWin);
            
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.NotifySpinResult(winAmount, results);
                CharmManager.Instance.NotifySpinEnd();
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

            // Bước 1: Thử trả nợ
            if (ResourceManager.Instance.TryPayDebt())
            {
                OnDebtPaidSuccess();
                return;
            }

            // Bước 2: Nếu không đủ tiền -> Hỏi Charm xem có ai cứu không? (AnkhCharm)
            bool isSaved = false;
            if (CharmManager.Instance != null)
            {
                int coin = (int)ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);
                int debt = (int)ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt);
                
                isSaved = CharmManager.Instance.CheckPaymentSavior(coin, debt);
            }

            if (isSaved)
            {
                // Nếu được cứu -> Coi như thành công qua màn
                OnDebtPaidSuccess();
            }
            else
            {
                // Bước 3: Chết thật
                Debug.Log("Phá sản! Game Over.");
                ChangeState(GameState.GameOver);
                if (UIManager.Instance != null) UIManager.Instance.CloseAllDialogs();
            }
        }
        
        private void OnDebtPaidSuccess()
        {
            Debug.Log($"DEBT ROUND {currentDebtRound} CLEARED!");

            currentDebtRound++;
            currentStage = 1;
            
            if (CharmManager.Instance != null)
                CharmManager.Instance.NotifyRoundStart(this);
            
            luckManager.IncrementDebtCompleted();

            if (difficultyProfile != null)
            {
                BigInteger nextDebt = difficultyProfile.GetDebtForRound(currentDebtRound);
                ResourceManager.Instance.SetNewDebt(nextDebt);
            }
            
            // Update Shop Probability (Logic bạn đã thêm trước đó)
            UpdateShopDifficulty();

            ChangeState(GameState.Preparation);
            NotifyRoundInfo();
        }

        private void ChangeState(GameState newState)
        {
            currentState = newState;
            
            // Khi bắt đầu quay, đóng tất cả Shop/Dialog để người chơi tập trung
            if (newState == GameState.Spinning || newState == GameState.GameOver)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.CloseAllDialogs();
            }
            
            OnStateChanged?.Invoke(newState);
            Debug.Log($"Game State Changed: {newState}");
        }
        
        private void UpdateShopDifficulty()
        {
            if (shopProfiles == null || shopProfiles.Count == 0) return;

            // Logic ví dụ: Cứ mỗi 3 Round thì tăng độ khó shop lên 1 bậc
            // Round 1-3: Index 0
            // Round 4-6: Index 1
            // ...
            int profileIndex = Mathf.Clamp((currentDebtRound - 1) / 3, 0, shopProfiles.Count - 1);
            
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.SetProbabilityProfile(shopProfiles[profileIndex]);
            }
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