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

            // Trigger Charm OnRoundStart (nếu có giữ lại charm từ game trước - tùy logic)
            if (charmHolder != null)
            {
                // Lưu ý: Thường StartNewGame sẽ clear charm, nhưng nếu game cho giữ thì gọi dòng này
                // charmHolder.ClearCharms(); // Nếu muốn xóa sạch túi
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
            
            if (charmHolder != null)
            {
                // Tạo bản sao list để tránh lỗi nếu charm tự hủy trong quá trình duyệt
                var charms = new List<CharmData>(charmHolder.GetContent());
                foreach (var charm in charms)
                {
                    charm.OnSpinStart(slotMachine, luckManager);
                }
            }
            
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
            
            // Cộng tiền thắng
            if (winAmount > 0)
            {
                ResourceManager.Instance.AddResource(ResourceType.Coin, (int)winAmount);
            }
            
            if (charmHolder != null)
            {
                // Tạo bản sao list (ToList) để tránh lỗi khi Charm tự xóa mình (NumberCharm, Lightbulb)
                var charms = new List<CharmData>(charmHolder.GetContent());
        
                foreach (var charm in charms)
                {
                    // 1. Xử lý kết quả thắng thua (ConsoPrizeCharm)
                    charm.OnSpinResult(slotMachine, luckManager, winAmount);
            
                    // 2. Xử lý Buff Symbol vĩnh viễn (CSymbolCharm) - QUAN TRỌNG
                    charm.OnSpinResultBuff(slotMachine, results);
            
                    // 3. Dọn dẹp cuối turn & Tự hủy (NumberCharm, Lightbulb)
                    charm.OnSpinEnd(slotMachine, luckManager);
                }
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
            if (charmHolder != null)
            {
                // Duyệt qua từng charm để tìm phao cứu sinh
                var charms = new List<CharmData>(charmHolder.GetContent());
                foreach (var charm in charms)
                {
                    int currentCoin = (int)ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);
                    int currentDebt = (int)ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt);

                    // Nếu Charm trả về true nghĩa là nó đã cứu
                    if (charm.OnPaymentCheck(currentCoin, currentDebt))
                    {
                        isSaved = true;
                        Debug.Log($"<color=green>SAVED BY CHARM: {charm.charmName}</color>");
                        
                        // Xử lý tiêu thụ charm Ankh (trong script AnkhCharm cần gọi holder.RemoveCharm)
                        // Trong AnkhCharm của bạn, bạn cần bỏ comment dòng Consume() hoặc xử lý logic đó.
                        if (charm is ConsumableCharm consumable && consumable.destroyOnUse)
                        {
                             charmHolder.RemoveCharm(charm);
                        }
                        
                        break; // Chỉ cần 1 cái cứu là đủ
                    }
                }
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
            
            // Trigger Charm OnRoundStart (ExtraSpinCharm)
            if (charmHolder != null)
            {
                foreach (var charm in charmHolder.GetContent())
                {
                    charm.OnRoundStart(this); 
                }
            }
            
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