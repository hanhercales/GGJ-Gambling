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
        
        [Header("Game Config")]
        [SerializeField] private int startingCoin = 10; 
        [SerializeField] private int stagesPerDebtRound = 4;

        [Header("Shop Progression")]
        [Tooltip("Danh sách tỉ lệ Shop theo độ khó.")]
        [SerializeField] private List<ShopProbabilitySO> shopProfiles;
        
        [Header("Current State (Read Only)")]
        [SerializeField] private GameState currentState;
        [SerializeField] private int currentDebtRound = 1;
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int spinsRemaining = 0;
        [SerializeField] private int globalSpinModifier = 0;
        
        // Option hiện tại (để tính thưởng vé khi quay xong)
        [SerializeField] private SpinOption currentSpinOption; 
        
        public int SpinsRemaining => spinsRemaining;
        
        // --- [MỚI] Public Getter để UI có thể truy cập ---
        public GameState CurrentState => currentState;

        // Events
        public event Action<int> OnSpinsChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<int, int, int> OnRoundInfoChanged;
        
        // Event gửi 2 option (Trái/Phải) ra cho UI vẽ
        public event Action<SpinOption, SpinOption> OnSpinOptionsUpdated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            Debug.Log(">>> GAME STARTED <<<");

            // 1. Lắng nghe sự kiện từ DebtManager
            if (DebtManager.Instance != null)
            {
                DebtManager.Instance.OnDebtPaidSuccess += HandleDebtPaid;
                DebtManager.Instance.OnGameOver += HandleGameOver;
                Debug.Log("GameManager: Connected to DebtManager.");
            }
            
            // 2. Lắng nghe sự kiện Tiền thay đổi từ ResourceManager
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
            }

            StartNewGame();
        }

        private void OnDestroy()
        {
            if (DebtManager.Instance != null)
            {
                DebtManager.Instance.OnDebtPaidSuccess -= HandleDebtPaid;
                DebtManager.Instance.OnGameOver -= HandleGameOver;
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
            }
        }

        private void StartNewGame()
        {
            Debug.Log("--- Initializing New Game Data ---");
            currentDebtRound = 1;
            currentStage = 1;
            globalSpinModifier = 0;
            
            // 1. Reset Tài Nguyên
            ResourceManager.Instance.ResetAllData(startingCoin);
            Debug.Log($"Reset Resources: Coin = {startingCoin}");
                
            // 2. Reset Logic Game
            WeightManager.Instance.ResetAllWeights();
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetMultipliers();
            if (slotMachine != null) slotMachine.ResetPatternStats();
            
            // 3. Setup Shop
            UpdateShopDifficulty();
            if (ShopManager.Instance != null) ShopManager.Instance.RerollShop(true);

            // 4. Setup Nợ
            if (DebtManager.Instance != null)
            {
                DebtManager.Instance.SetupDebtForRound(currentDebtRound);
            }

            // 5. Kích hoạt Charm đầu game
            if (CharmManager.Instance != null)
                CharmManager.Instance.NotifyDeadlineStart(this);
            
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.ResetAllMasksForNewGame();
            }
            
            ChangeState(GameState.Preparation);
            NotifyRoundInfo();
        }
        
        public void ModifyGlobalSpinCount(int amount)
        {
            globalSpinModifier += amount;
            Debug.Log($"[GameManager] Global Spin Modifier changed: {globalSpinModifier}");
        }
        
        private void ApplySpinOption(SpinOption option)
        {
            currentSpinOption = option; 
            
            // --- [NEW] Apply the Modifier here! ---
            // Calculate final spins: Pack Amount + Modifier (e.g., 10 + (-2) = 8)
            int finalSpins = option.spinCount + globalSpinModifier;
            
            // Safety: Ensure at least 1 spin so the game doesn't softlock
            if (finalSpins < 1) finalSpins = 1; 

            Debug.Log($"[GameManager] Pack Selected: {option.spinCount} spins. Modifier: {globalSpinModifier}. Final: {finalSpins}");
            
            AddSpins(finalSpins);
            ChangeState(GameState.Spinning);
        }

        // --- XỬ LÝ KHI TÀI NGUYÊN THAY ĐỔI ---
        private void OnResourceChanged(ResourceType type, string value)
        {
            // Chỉ quan tâm nếu TIỀN thay đổi và đang ở giai đoạn chuẩn bị (mua gói)
            if (type == ResourceType.Coin && currentState == GameState.Preparation)
            {
                RequestSpinOptionsUpdate();
            }
        }

        // --- PUBLIC API: UI GỌI ĐỂ LẤY OPTION ---
        public void RequestSpinOptionsUpdate()
        {
            if (SpinManager.Instance != null)
            {
                var coin = ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);
                var options = SpinManager.Instance.GetSpinOptions(currentDebtRound, coin);
                
                OnSpinOptionsUpdated?.Invoke(options.option1, options.option2);
            }
            else
            {
                Debug.LogError("CRITICAL: SpinManager not found!");
            }
        }

        // --- PUBLIC API: UI GỌI KHI CHỌN GÓI ---
        public void SelectSpinOption(SpinOption option)
        {
            if (currentState != GameState.Preparation)
            {
                Debug.LogWarning($"Cannot select spin option. Wrong State: {currentState}");
                return;
            }
            
            if (!option.isAvailable)
            {
                Debug.LogWarning("Player tried to select an unavailable option.");
                return;
            }

            Debug.Log($"Player selecting option: {option.type} | Title: {option.title} | Cost: {option.coinCost}");

            if (option.coinCost > 0)
            {
                if (ResourceManager.Instance.TrySpendResource(ResourceType.Coin, option.coinCost))
                {
                    Debug.Log($"Purchase Successful! Spent {option.coinCost} coins.");
                    ApplySpinOption(option);
                }
                else
                {
                    Debug.LogError("FAILED: UI allows click but ResourceManager says Not Enough Coin!");
                }
            }
            else
            {
                Debug.Log("Free Spin / Bankruptcy Option Selected.");
                ApplySpinOption(option);
            }
        }

        public void TriggerSpin()
        {
            if (currentState != GameState.Spinning) return;
            if (spinsRemaining <= 0)
            {
                Debug.LogWarning("TriggerSpin called but No Spins Remaining!");
                return;
            }
            
            if (CharmManager.Instance != null) CharmManager.Instance.NotifySpinStart();
            if (MaskManager.Instance != null) 
                MaskManager.Instance.NotifySpinStart(slotMachine, luckManager);
            
            int calculatedLuck = LuckManager.Instance.CalculateLuckForSpin();
            Debug.Log($"SPINNING... (Luck Applied: {calculatedLuck})");

            slotMachine.PerformSpin(calculatedLuck, OnSpinCompleted);
        }
        
        private void OnSpinCompleted(float winAmount, List<MatchResult> results)
        {
            Debug.Log($"Spin Completed. Win Amount: {winAmount}");
            
            bool isWin = winAmount > 0;
            LuckManager.Instance.ReportSpinResult(isWin);
            
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.NotifySpinResult(winAmount, results);
                CharmManager.Instance.NotifySpinEnd();
            }

            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.NotifySpinResult(slotMachine, luckManager, winAmount);
                MaskManager.Instance.NotifySpinResultBuff(slotMachine, results);
                MaskManager.Instance.NotifySpinEnd(slotMachine, luckManager);
            }
            
            spinsRemaining--;
            OnSpinsChanged?.Invoke(spinsRemaining);

            if (spinsRemaining <= 0)
            {
                Debug.Log("Pack Finished (0 spins left). Checking Stage progression...");
                OnRoundFinished();
            }
        }
        
        private void OnRoundFinished()
        {
            if (currentSpinOption != null)
            {
                ResourceManager.Instance.AddResource(ResourceType.Ticket, currentSpinOption.ticketReward);
                Debug.Log($"Reward: Added {currentSpinOption.ticketReward} Tickets.");
            }
            
            if (CharmManager.Instance != null)
            {
                CharmManager.Instance.NotifyRoundCompleted(this);
            }

            if (currentStage < stagesPerDebtRound)
            {
                currentStage++;
                Debug.Log($">>> ADVANCING TO STAGE {currentStage}/{stagesPerDebtRound} <<<");
                ChangeState(GameState.Preparation); 
                NotifyRoundInfo();
            }
            else
            {
                Debug.Log("Stage Limit Reached. Resolving Debt Cycle...");
                ResolveDebtCycle();
            }
        }

        private void ResolveDebtCycle()
        {
            ChangeState(GameState.RoundEnd);
            if (DebtManager.Instance != null)
            {
                Debug.Log("Calling DebtManager.EvaluateRoundEnd()...");
                DebtManager.Instance.EvaluateRoundEnd();
            }
        }
        
        private void HandleDebtPaid()
        {
            Debug.Log("[Event Received] DebtManager says: Debt Paid / Clear!");

            int skippedStages = 0;
            if (currentState != GameState.RoundEnd)
            {
                skippedStages = (stagesPerDebtRound - currentStage) + 1;
            }

            if (skippedStages > 0)
            {
                int bonusTickets = skippedStages * 4;
                ResourceManager.Instance.AddResource(ResourceType.Ticket, bonusTickets);
                Debug.Log($"[Debt] Early Pay Bonus: +{bonusTickets} Tickets ({skippedStages} stages skipped).");
            }

            Debug.Log($"ROUND {currentDebtRound} COMPLETE! Preparing next round...");
            currentDebtRound++;
            currentStage = 1;
            
            // Reset state
            spinsRemaining = 0;
            currentSpinOption = null;
            OnSpinsChanged?.Invoke(0);
            
            if (CharmManager.Instance != null)
                CharmManager.Instance.NotifyDeadlineStart(this);
            
            luckManager.IncrementDebtCompleted();

            if (DebtManager.Instance != null)
            {
                DebtManager.Instance.SetupDebtForRound(currentDebtRound);
            }
            
            UpdateShopDifficulty();
            ChangeState(GameState.Preparation);
            NotifyRoundInfo();
        }
        
        public int GetCurrentEarlyPayBonus()
        {
            // Nếu đã hết vòng (RoundEnd) thì không còn khái niệm trả sớm
            if (currentState == GameState.RoundEnd) return 0;

            // Công thức: (Tổng Stage - Stage hiện tại + 1) * 4
            int remainingStages = (stagesPerDebtRound - currentStage) + 1;
            
            if (remainingStages < 0) remainingStages = 0;
            
            return remainingStages * 4;
        }

        private void HandleGameOver()
        {
            Debug.Log("[Event Received] DebtManager says: GAME OVER (Bankrupt).");
            ChangeState(GameState.GameOver);
        }

        // --- HELPER METHODS ---

        private void ChangeState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"[GameState] Changed to: {newState}");
            
            // Nếu vào giai đoạn chuẩn bị -> Tính giá tiền gói Spin
            if (newState == GameState.Preparation)
            {
                RequestSpinOptionsUpdate();
            }

            // Nếu đang quay hoặc Game Over -> Đóng hết Dialog UI
            if (newState == GameState.Spinning || newState == GameState.GameOver)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.CloseAllDialogs();
            }
            
            OnStateChanged?.Invoke(newState);
        }
        
        private void UpdateShopDifficulty()
        {
            if (shopProfiles == null || shopProfiles.Count == 0) return;
            
            int profileIndex = Mathf.Clamp((currentDebtRound - 1) / 3, 0, shopProfiles.Count - 1);
            
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.SetProbabilityProfile(shopProfiles[profileIndex]);
            }
        }

        public void AddSpins(int amount)
        {
            spinsRemaining += amount;
            Debug.Log($"AddSpins called: +{amount}. New Total: {spinsRemaining}");
            OnSpinsChanged?.Invoke(spinsRemaining); 
        }
        
        private void NotifyRoundInfo()
        {
            OnRoundInfoChanged?.Invoke(currentDebtRound, currentStage, stagesPerDebtRound);
        }
    }
}