using System;
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
        [SerializeField] private CharmHolder charmHolder;
        
        [Header("Game Config")]
        // Thay thế biến startingDebt cứng bằng Profile mềm dẻo
        [SerializeField] private DebtDifficultySO difficultyProfile; 
        [SerializeField] private int startingCoin = 10; // Tiền khởi điểm

        [Header("Current State (Read Only)")]
        [SerializeField] private GameState currentState;
        [SerializeField] private int currentRound = 1;
        [SerializeField] private int spinsRemaining = 0;
        [SerializeField] private SpinPackSO currentPack;

        // Events
        public event Action<int> OnSpinsChanged;
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnRoundChanged; // Thêm event báo đổi vòng cho UI cập nhật

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
            currentRound = 1;
            
            // Reset toàn bộ dữ liệu về ban đầu
            ResourceManager.Instance.ResetAllData(startingCoin);

            if (difficultyProfile != null)
            {
                // Lấy nợ của vòng 1
                int firstDebt = difficultyProfile.GetDebtForRound(currentRound);
                ResourceManager.Instance.SetNewDebt(firstDebt);
            }
            else
            {
                Debug.LogError("CẢNH BÁO: Chưa gán DebtDifficultySO vào GameManager!");
                ResourceManager.Instance.SetNewDebt(25); // Giá trị chống lỗi (Fallback)
            }

            if (charmHolder != null)
            {
                foreach (var charm in charmHolder.GetContent())
                {
                    charm.OnRoundStart(this); 
                }
            }
            
            ChangeState(GameState.Preparation);
            
            // Báo cho UI biết đang ở Round 1
            OnRoundChanged?.Invoke(currentRound);
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

            // Gọi SlotMachine quay và chờ kết quả trả về
            slotMachine.PerformSpin(OnSpinCompleted);
        }

        // 3. Callback khi SlotMachine quay xong (Nhận kết quả từ Controller)
        private void OnSpinCompleted(float winAmount)
        {
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
                ResolveRound();
            }
        }

        // 4. Tổng kết vòng chơi
        private void ResolveRound()
        {
            ChangeState(GameState.RoundEnd);

            // Kiểm tra xem có đủ tiền trả nợ không
            if (ResourceManager.Instance.TryPayDebt())
            {
                Debug.Log($"ROUND {currentRound} CLEARED! - Debt Paid.");
                
                // Thưởng Ticket từ gói đã chọn (Logic phần thưởng phụ)
                if (currentPack != null)
                {
                    ResourceManager.Instance.AddResource(ResourceType.Ticket, currentPack.ticketReward);
                }
                
                currentRound++; // Lên vòng mới

                if (difficultyProfile != null)
                {
                    // Lấy nợ mới dựa trên số Round vừa tăng
                    int nextDebt = difficultyProfile.GetDebtForRound(currentRound);
                    ResourceManager.Instance.SetNewDebt(nextDebt);
                }
                
                ChangeState(GameState.Preparation); // Quay lại Shop/Chọn gói
                OnRoundChanged?.Invoke(currentRound); // Cập nhật UI Round
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
    }
}