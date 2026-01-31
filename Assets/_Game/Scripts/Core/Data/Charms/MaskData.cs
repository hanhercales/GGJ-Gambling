using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Core.Inventory;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewMaskData", menuName = "Charms/Mask Data")]
    public class MaskData : CharmData
    {
        [Header("--- MASK REQUIREMENTS ---")]
        [Tooltip("Danh sách các Charm bắt buộc phải có trong kho để kích hoạt Mask này.")]
        public List<CharmData> requiredIngredients;

        // --- STATE LƯU VẾT ---
        // NonSerialized để không bị lưu cứng vào file asset trong Editor khi tắt game
        [System.NonSerialized] 
        private bool _hasBeenUnlocked = false;

        public bool IsUnlocked => _hasBeenUnlocked;

        /// <summary>
        /// Kiểm tra điều kiện mở khóa. 
        /// Logic: Nếu đã từng mở -> True luôn. Nếu chưa -> Check túi đồ.
        /// </summary>
        public bool CheckUnlockCondition(CharmHolder playerInventory)
        {
            // 1. Nếu đã mở khóa từ trước -> Giữ nguyên trạng thái (Bất tử với việc bán đồ)
            if (_hasBeenUnlocked) return true;

            // 2. Kiểm tra danh sách yêu cầu
            if (requiredIngredients == null || requiredIngredients.Count == 0)
            {
                return false;
            }

            foreach (var reqCharm in requiredIngredients)
            {
                // Nếu thiếu bất kỳ món nào -> Fail
                if (!playerInventory.HasCharm(reqCharm))
                {
                    return false; 
                }
            }

            // 3. Nếu đủ tất cả -> Đánh dấu ĐÃ MỞ KHÓA -> Return True
            _hasBeenUnlocked = true;
            Debug.Log($"[MaskData] Mask '{charmName}' unlocked permanently for this run!");
            return true;
        }

        /// <summary>
        /// Gọi hàm này khi bắt đầu New Game để khóa lại từ đầu.
        /// </summary>
        public void ResetMaskState()
        {
            _hasBeenUnlocked = false;
        }
    }
}