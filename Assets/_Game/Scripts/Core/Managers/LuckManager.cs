using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Core.Managers
{
    public class LuckManager : MonoBehaviour
    {
        public static LuckManager Instance { get; private set; }

        [Header("State Info (Read Only)")]
        [SerializeField] public int baseLuckFromCharms = 0;
        
        [Tooltip("Tổng Luck thực tế (Base + Bonus) được áp dụng ở lần quay gần nhất.")]
        [SerializeField] private int lastCalculatedTotalLuck = 0;
        
        [SerializeField] private int spinCount = 0;     // Tổng số lượt đã quay
        [SerializeField] private int loseStreak = 0;    // Chuỗi thua liên tiếp (Pity)
        [SerializeField] private int debtCompleted = 0; // Số vòng nợ đã hoàn thành (để tính OLS)
        
        [Header("Charm Counters")]
        [SerializeField] private int _horseshoeCount = 0;
        
        public int SpinCount => spinCount;      
        public int LoseStreak => loseStreak;   
        public int DebtCompleted => debtCompleted;
        
        // ILS Algorithm Variables
        private int _ilsOffset;
        
        // OLS Algorithm Variables
        private int _nextOLSSpinTarget = -1;

        // Events cho Visual (Hiệu ứng tia lửa)
        public event Action OnSparkTriggered; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Khởi tạo ILS Offset (0 đến 4) như tài liệu
            _ilsOffset = Random.Range(0, 5);
            Debug.Log($"LuckManager Init: ILS Offset = {_ilsOffset}");
        }

        // --- CORE API: GỌI TRƯỚC KHI QUAY ---
        public int CalculateLuckForSpin()
        {
            int finalLuck = baseLuckFromCharms;
            bool sparkTriggered = false;

            // 1. Base Luck Bonus (Nếu Luck thấp 1-7)
            finalLuck += CalculateLowBaseLuckBonus(finalLuck);

            // 2. Spontaneous Luck (ILS / OLS)
            int spontaneousBonus = 0;
            
            if (debtCompleted == 0) // Đang ở Debt đầu tiên -> Dùng ILS
            {
                if (CheckILS(spinCount + 1)) // +1 vì spinCount tính lượt sắp quay
                {
                    // "Luck increases by a random amount between 4 and 8"
                    // 6 is twice as likely -> Custom logic or simple random
                    spontaneousBonus = Random.Range(4, 9); 
                    sparkTriggered = true;
                    Debug.Log($"<color=yellow>[ILS TRIGGERED] Bonus: {spontaneousBonus}</color>");
                }
            }
            else // Đã qua Debt đầu tiên -> Dùng OLS
            {
                // Nếu chưa tính target OLS tiếp theo hoặc đã vượt qua -> Tính lại
                if (_nextOLSSpinTarget == -1 || spinCount >= _nextOLSSpinTarget)
                {
                    CalculateNextOLS();
                }

                if (spinCount + 1 == _nextOLSSpinTarget)
                {
                    spontaneousBonus = CalculateOLSBonus(spinCount + 1);
                    sparkTriggered = true;
                    Debug.Log($"<color=orange>[OLS TRIGGERED] Bonus: {spontaneousBonus}</color>");
                    
                    // Reset để tính lần sau
                    CalculateNextOLS(); 
                }
            }
            finalLuck += spontaneousBonus;

            // 3. Pity Spins (Thua 4 lần liên tiếp trở lên)
            // "Happens after previous steps"
            if (loseStreak >= 4)
            {
                int pityBonus = 5 + (loseStreak - 4); // +5, then +1 per extra miss
                finalLuck += pityBonus;
                sparkTriggered = true;
                Debug.Log($"<color=red>[PITY TRIGGERED] Streak: {loseStreak} | Bonus: {pityBonus}</color>");
            }

            // Trigger Visual Effect nếu có
            if (sparkTriggered) OnSparkTriggered?.Invoke();

            lastCalculatedTotalLuck = finalLuck;
            
            return finalLuck;
        }

        // --- CORE API: GỌI SAU KHI CÓ KẾT QUẢ ---
        public void ReportSpinResult(bool isWin)
        {
            spinCount++;

            if (isWin)
            {
                loseStreak = 0;
            }
            else
            {
                loseStreak++;
            }
        }
        
        public void IncrementDebtCompleted()
        {
            debtCompleted++;
            // Reset OLS target khi qua vòng mới để tính lại cho chuẩn
            _nextOLSSpinTarget = -1; 
        }
        
        public void UpdateBaseLuck(int amount)
        {
            baseLuckFromCharms = amount;
        }
        
        public void UpdateHorseshoeCount(int amount)
        {
            _horseshoeCount += amount;
            // Safety clamp: never let it go below 0
            if (_horseshoeCount < 0) _horseshoeCount = 0;
        }
        
        public float GetChanceMultiplier()
        {
            return _horseshoeCount > 0 ? 2.0f : 1.0f;
        }

        #region Internal Algorithms (Theo tài liệu Clover Pit)

        // Initial Luck Spins Algorithm
        private bool CheckILS(int currentSpinNum)
        {
            if (currentSpinNum == 1) return false;

            // Formula: mod = 4 + floor(spin_num / 6)
            int mod = 4 + Mathf.FloorToInt(currentSpinNum / 6f);
            
            // Check: (spin_num + offset + 1) % mod == 0
            return (currentSpinNum + _ilsOffset + 1) % mod == 0;
        }

        // Occasional Luck Spins Scheduler
        private void CalculateNextOLS()
        {
            // Formula: Next = Current + 4 + Deadlines + 50% chance 1
            int randomAdd = Random.value > 0.5f ? 1 : 0;
            int gap = 4 + debtCompleted + randomAdd;
            
            _nextOLSSpinTarget = spinCount + gap;
            // Đảm bảo không trùng ngay lập tức (Logic an toàn)
            if (_nextOLSSpinTarget <= spinCount) _nextOLSSpinTarget = spinCount + 5; 
        }

        private int CalculateOLSBonus(int spinNum)
        {
            int bonus = 0;
            // Stackable bonuses
            if (spinNum % 7 == 0) bonus += Random.Range(7, 10); // +7 to +9
            
            if (spinNum % 3 == 0) bonus += Random.Range(5, 8); // +5 to +7
            else bonus += Random.Range(3, 6); // +3 to +5

            return bonus;
        }

        // Base Luck Boost (1-7 Range)
        private int CalculateLowBaseLuckBonus(int currentLuck)
        {
            if (currentLuck >= 1 && currentLuck <= 4)
            {
                // 1/2 chance +1 or +2
                if (Random.value < 0.5f) return Random.Range(1, 3);
            }
            else if (currentLuck >= 5 && currentLuck <= 6)
            {
                // 1/4 chance +1
                if (Random.value < 0.25f) return 1;
            }
            else if (currentLuck == 7)
            {
                // 1/6 chance +1
                if (Random.value < 0.166f) return 1;
            }
            return 0;
        }

        #endregion
    }
}