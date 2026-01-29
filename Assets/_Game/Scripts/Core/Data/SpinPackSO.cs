using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewSpinPack", menuName = "GameConfig/Spin Pack")]
    public class SpinPackSO : ScriptableObject
    {
        [Header("Display Info")]
        public string packName;
        [TextArea] public string description;

        [Header("Cost & Benefit")]
        [Tooltip("Số lượt quay nhận được.")]
        public int spinCount = 7;

        [Tooltip("Số Coin phải trả để mua gói này.")]
        public int coinCost = 10;

        [Tooltip("Số Ticket (Vé) được thưởng thêm sau khi quay xong gói này.")]
        public int ticketReward = 1;
    }
}