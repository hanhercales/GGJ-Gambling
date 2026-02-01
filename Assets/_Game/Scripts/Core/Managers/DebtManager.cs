using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory; // Để dùng ResourceType
using TMPro; // Nếu nút có text hiển thị tiền

namespace _Game.Scripts.Core.Managers
{
    public class DebtManager : MonoBehaviour
    {
        public static DebtManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private DebtDifficultySO difficultyProfile;
        private float _debtMultiplier = 1.0f;
        
        [Header("UI References")]
        [SerializeField] private Button payDebtButton;
        [SerializeField] private TextMeshProUGUI payButtonText; // Optional: Để hiện "Pay [Amount]"
        [SerializeField] private CanvasGroup payButtonCanvasGroup;

        // Events
        public event Action OnDebtPaidSuccess; // Bắn ra khi trả xong nợ
        public event Action OnGameOver;        // Bắn ra khi không trả được

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Đăng ký sự kiện thay đổi tiền để cập nhật trạng thái nút
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
                UpdateButtonState(); // Check lần đầu
            }
            
            if (payDebtButton != null)
            {
                payDebtButton.onClick.AddListener(OnPayButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
            }
        }

        // --- PUBLIC API ---
        public void ModifyDebtMultiplier(float amount)
        {
            _debtMultiplier += amount;
            // Safety: Don't let multiplier go below 0.1 (10%)
            if (_debtMultiplier < 0.1f) _debtMultiplier = 0.1f;
            
            Debug.Log($"[DebtManager] Multiplier changed. Current: {_debtMultiplier:F2}x");
        }

        public void SetupDebtForRound(int roundIndex)
        {
            if (difficultyProfile != null)
            {
                BigInteger baseDebt = difficultyProfile.GetDebtForRound(roundIndex);
                BigInteger finalDebt = (BigInteger)((double)baseDebt * _debtMultiplier);

                Debug.Log($"[DebtManager] Setup Round {roundIndex}. Base: {baseDebt} x {_debtMultiplier} = {finalDebt}");

                // 3. Set it to Resource Manager
                ResourceManager.Instance.SetNewDebt(finalDebt);
                UpdateButtonState();
            }
            else
            {
                Debug.LogError("Chưa gắn DebtDifficultySO vào DebtManager!");
            }
        }

        public void EvaluateRoundEnd()
        {
            BigInteger currentDebt = ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt);
            BigInteger currentCoin = ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);

            // 1. Đã trả hết nợ từ trước (Do bấm nút sớm)
            if (currentDebt <= 0)
            {
                Debug.Log("[DebtManager] Nợ đã sạch. Auto-Pass!");
                OnDebtPaidSuccess?.Invoke(); // Tự động qua màn
                return;
            }

            // 2. Chưa trả nợ, nhưng ĐỦ tiền
            if (currentCoin >= currentDebt)
            {
                Debug.Log("[DebtManager] Đủ tiền. Đợi người chơi bấm PAY.");
                // Không làm gì cả, GameManager sẽ treo game ở trạng thái RoundEnd
                // Button Pay vẫn sáng để người chơi bấm.
            }
            // 3. Chưa trả nợ và KHÔNG đủ tiền
            else
            {
                // Thử kích hoạt Charm cứu mạng (Ankh)
                if (AttemptRescue())
                {
                    Debug.Log("[DebtManager] Được Charm cứu mạng!");
                    OnDebtPaidSuccess?.Invoke();
                }
                else
                {
                    Debug.Log("[DebtManager] Không đủ tiền và không có Charm cứu -> GAME OVER");
                    OnGameOver?.Invoke();
                }
            }
        }

        // --- INTERNAL LOGIC ---

        private void OnPayButtonClicked()
        {
            // Logic trả nợ thủ công
            if (ResourceManager.Instance.TryPayDebt())
            {
                // Trả thành công
                Debug.Log("Đã bấm nút trả nợ thành công!");
                
                // Cập nhật lại nút
                UpdateButtonState();

                // Báo cho GameManager biết
                OnDebtPaidSuccess?.Invoke();
            }
            else
            {
                return;
            }
        }

        private void OnResourceChanged(ResourceType type, string value)
        {
            if (type == ResourceType.Coin || type == ResourceType.Debt)
            {
                UpdateButtonState();
            }
        }

        private void UpdateButtonState()
        {
            if (payDebtButton == null) return;

            BigInteger coin = ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);
            BigInteger debt = ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt);

            // Nút chỉ bấm được khi: Có Nợ (>0)
            bool canPay = (debt > 0);

            payDebtButton.interactable = canPay;

            if (payButtonCanvasGroup != null)
            {
                payButtonCanvasGroup.alpha = canPay ? 1f : 0.5f;
            }

            // (Optional) Update text
            if (payButtonText != null)
            {
                if (debt <= 0) payButtonText.text = "PAID";
                else payButtonText.text = canPay ? "PAY DEBT" : "NEED COIN";
            }
        }

        private bool AttemptRescue()
        {
            if (CharmManager.Instance != null)
            {
                int coin = (int)ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);
                int debt = (int)ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt);
                return CharmManager.Instance.CheckPaymentSavior(coin, debt);
            }
            return false;
        }
    }
}