using UnityEngine;
using TMPro;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Data; // Để dùng ResourceType

namespace _Game.Scripts.View.UI
{
    public class GameStatusUI : MonoBehaviour
    {
        [Header("Progress Info")]
        [SerializeField] private TextMeshProUGUI stageText;    // VD: Stage: 1/4
        [SerializeField] private TextMeshProUGUI deadlineText; // VD: Deadline: 1
        [SerializeField] private TextMeshProUGUI spinText;     // VD: Spins: 5

        [Header("Resource Info")]
        [SerializeField] private TextMeshProUGUI coinText;     // VD: Coin: 10
        [SerializeField] private TextMeshProUGUI ticketText;   // VD: Ticket: 0
        [SerializeField] private TextMeshProUGUI debtText;     // VD: Debt: 25

        [Header("Bonus Info")]
        [SerializeField] private TextMeshProUGUI bonusText;    // VD: Early Pay Bonus: +16 Tickets

        private void Start()
        {
            // 1. Đăng ký sự kiện từ GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoundInfoChanged += UpdateRoundInfo;
                GameManager.Instance.OnSpinsChanged += UpdateSpinInfo;
                
                // Gọi cập nhật lần đầu
                UpdateSpinInfo(GameManager.Instance.SpinsRemaining);
                // (UpdateRoundInfo sẽ được GameManager gọi khi StartGame)
            }

            // 2. Đăng ký sự kiện từ ResourceManager
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += UpdateResourceInfo;
                
                // Cập nhật lần đầu
                if(coinText != null) coinText.text = $"Coin: {ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin)}";
                if(ticketText != null) ticketText.text = $"Ticket: {ResourceManager.Instance.GetTicket()}";
                if(debtText != null) debtText.text = $"Debt: {ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt)}";
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoundInfoChanged -= UpdateRoundInfo;
                GameManager.Instance.OnSpinsChanged -= UpdateSpinInfo;
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= UpdateResourceInfo;
            }
        }

        // --- CALLBACKS ---

        private void UpdateRoundInfo(int round, int currentStage, int maxStage)
        {
            if (stageText != null) stageText.text = $"Stage: {currentStage}/{maxStage}";
            if (deadlineText != null) deadlineText.text = $"Deadline: {round}";

            // Mỗi khi đổi Stage, số Bonus tiềm năng sẽ thay đổi -> Cập nhật luôn
            UpdateBonusText();
        }

        private void UpdateSpinInfo(int spins)
        {
            if (spinText != null) spinText.text = $"Spins: {spins}";
        }

        private void UpdateResourceInfo(ResourceType type, string value)
        {
            switch (type)
            {
                case ResourceType.Coin:
                    if (coinText != null) coinText.text = $"Coin: {value}";
                    break;
                case ResourceType.Ticket:
                    if (ticketText != null) ticketText.text = $"Ticket: {value}";
                    break;
                case ResourceType.Debt:
                    if (debtText != null) debtText.text = $"Debt: {value}";
                    break;
            }
        }

        private void UpdateBonusText()
        {
            if (bonusText != null && GameManager.Instance != null)
            {
                int bonus = GameManager.Instance.GetCurrentEarlyPayBonus();
                bonusText.text = $"Early Pay: <color=green>+{bonus} Tickets</color>";
            }
        }
    }
}