using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.View.UI;

namespace _Game.Scripts.Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SlotMachineController slotMachine;
        [SerializeField] private LuckManager luckManager;
        [SerializeField] private CharmHolder playerInventory;
        
        [Header("Game Config")]
        [SerializeField] private int startingCoin = 10; 
        [SerializeField] private int startingTicket = 0;
        [SerializeField] private int stagesPerDebtRound = 4;
        [SerializeField] private int maxRoundsToWin = 12;

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
        
        private bool _isGameRunning = false;
        
        public int SpinsRemaining => spinsRemaining;
        
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
            
            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<CharmHolder>();
        }

        private void Start()
        {
            Debug.Log(">>> GAME STARTED <<<");

            if (DebtManager.Instance != null)
            {
                DebtManager.Instance.OnDebtPaidSuccess += HandleDebtPaid;
                DebtManager.Instance.OnGameOver += HandleGameOver;
                Debug.Log("GameManager: Connected to DebtManager.");
            }
            
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
            }

            OpenMainMenu();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!_isGameRunning || currentState == GameState.GameOver) return;

                if (UIManager.Instance != null)
                {
                    if (UIManager.Instance.IsGameMenuOpen)
                    {
                        UIManager.Instance.CloseGameMenu();
                    }
                    else
                    {
                        UIManager.Instance.OpenGameMenu(MenuMode.Pause);
                    }
                }
            }
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
        
        private void OpenMainMenu()
        {
            _isGameRunning = false;
            if (UIManager.Instance != null)
                UIManager.Instance.OpenGameMenu(MenuMode.MainMenu);
        }

        public void StartNewGame()
        {
            Debug.Log("--- Initializing New Game Data ---");
            
            _isGameRunning = true;
            
            if (playerInventory != null)
            {
                playerInventory.ClearCharms();
                Debug.Log("Inventory Cleared for New Game.");
            }
            
            currentDebtRound = 1;
            currentStage = 1;
            globalSpinModifier = 0;
            
            // 1. Reset Tài Nguyên
            ResourceManager.Instance.ResetAllData(startingCoin, startingTicket);
            Debug.Log($"Reset Resources: Coin = {startingCoin}, Ticket = {startingTicket}");
                
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
            
            Debug.Log($"[Spin Calculation] Base Pack: {option.spinCount} | Global Modifier: {globalSpinModifier}");

            int finalSpins = option.spinCount + globalSpinModifier;
            
            if (finalSpins < 1) finalSpins = 1; 

            Debug.Log($"[Spin Calculation] FINAL SPINS: {finalSpins}");
            
            AddSpins(finalSpins);
            ChangeState(GameState.Spinning);
        }

        private void OnResourceChanged(ResourceType type, string value)
        {
            if (type == ResourceType.Coin && currentState == GameState.Preparation)
            {
                RequestSpinOptionsUpdate();
            }
        }

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
            Debug.Log("[GameManager] Event Received: Debt Paid!");

            // 1. Tính toán Bonus Ticket (Trả sớm)
            int skippedStages = 0;
            
            // Chỉ tính bonus nếu chưa đến giai đoạn kết sổ (RoundEnd)
            // Tức là người chơi bấm nút Pay Debt khi vẫn đang ở Preparation của stage nào đó
            if (currentState != GameState.RoundEnd)
            {
                // Ví dụ: Tổng 4 stage. Đang ở Stage 2 (chuẩn bị quay stage 2).
                // Skipped = (4 - 2) + 1 = 3 stage (Stage 2, 3, 4 đều chưa quay).
                skippedStages = (stagesPerDebtRound - currentStage) + 1;
            }

            if (skippedStages > 0)
            {
                int bonusTickets = skippedStages * 4; // 4 vé mỗi stage skip
                
                Debug.Log($"[Debt Bonus] Skipped {skippedStages} stages (Current: {currentStage}/{stagesPerDebtRound}). Adding {bonusTickets} Tickets.");
                
                // Gọi ResourceManager để cộng
                ResourceManager.Instance.AddResource(ResourceType.Ticket, bonusTickets);
            }
            else
            {
                Debug.Log("[Debt Bonus] No stages skipped. No bonus tickets.");
            }

            // 2. Logic Win Game
            if (currentDebtRound == maxRoundsToWin)
            {
                HandleWinGame();
            }
            else
            {
                PrepareNextRound();
            }
        }
        
        private void HandleWinGame()
        {
            Debug.Log(">>> VICTORY! Max Round Reached. <<<");
            
            _isGameRunning = false; 
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenGameMenu(MenuMode.WinGame);
            }
        }
        
        public void ContinueEndlessMode()
        {
            Debug.Log(">>> CONTINUING TO ENDLESS MODE <<<");
            _isGameRunning = true;
            PrepareNextRound();
        }
        
        private void PrepareNextRound()
        {
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

            if (ShopManager.Instance != null)
            {
                Debug.Log("[GameManager] Auto-rerolling shop for new round...");
                ShopManager.Instance.RerollShop(isFree: true);
            }
            
            ChangeState(GameState.Preparation);
            NotifyRoundInfo();
        }
        
        public int GetCurrentEarlyPayBonus()
        {
            if (currentState == GameState.RoundEnd) return 0;

            int remainingStages = (stagesPerDebtRound - currentStage) + 1;
            
            if (remainingStages < 0) remainingStages = 0;
            
            return remainingStages * 4;
        }

        private void HandleGameOver()
        {
            Debug.Log("[Event Received] DebtManager says: GAME OVER (Bankrupt).");
            ChangeState(GameState.GameOver);
            
            _isGameRunning = false;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenGameMenu(MenuMode.GameOver);
            }
        }

        private void ChangeState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"[GameState] Changed to: {newState}");
            
            if (newState == GameState.Preparation)
            {
                RequestSpinOptionsUpdate();
            }

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

            int profileIndex;
            
            if (currentDebtRound > 12)
            {
                profileIndex = shopProfiles.Count - 1;
            }
            else
            {
                profileIndex = Mathf.Clamp((currentDebtRound - 1) / 3, 0, shopProfiles.Count - 1);
            }
            
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