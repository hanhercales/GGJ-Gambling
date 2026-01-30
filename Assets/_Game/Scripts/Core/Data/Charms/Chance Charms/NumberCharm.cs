using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using UnityEngine;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Chance/Funny Number")]
public class NumberCharm : ChanceCharm
{
    [Header("Settings")]
    public int luckBonus = 6;
    public int maxUses = 9;
    
    // Runtime state
    [System.NonSerialized] private int _currentUses = 0;
    [System.NonSerialized] private bool _isActiveThisTurn = false;
    [System.NonSerialized] private CharmHolder _myHolder;
    
    // Đảm bảo reset khi game bắt đầu hoặc compile lại
    private void OnEnable()
    {
        _currentUses = 0;
        _isActiveThisTurn = false;
    }

    public override void OnEquip(CharmHolder holder)
    {
        _myHolder = holder;
        _currentUses = 0; 
    }

    public override void OnSpinStart(SlotMachineController machine, LuckManager luckManager)
    {
        _isActiveThisTurn = false;

        // 1. Kiểm tra giới hạn TRƯỚC khi trigger
        if (_currentUses >= maxUses) return;

        // 2. Roll trigger
        if (TryTrigger())
        {
            luckManager.baseLuckFromCharms += luckBonus;
            _isActiveThisTurn = true;
            _currentUses++; // Tăng số lần dùng
            
            Debug.Log($"[NumberCharm] Activated! ({_currentUses}/{maxUses})");
        }
    }

    public override void OnSpinEnd(SlotMachineController machine, LuckManager luckManager)
    {
        // Chỉ xử lý nếu turn này nó đã kích hoạt
        if (_isActiveThisTurn)
        {
            // Trả lại Luck cũ (Cleanup)
            luckManager.baseLuckFromCharms -= luckBonus;
            _isActiveThisTurn = false;
            
            // 3. Kiểm tra hủy bằng dấu >= để an toàn tuyệt đối
            if (_currentUses >= maxUses)
            {
                Debug.Log($"[NumberCharm] Expired ({_currentUses}/{maxUses}). Removing...");
                if (_myHolder != null)
                {
                    _myHolder.RemoveCharm(this);
                }
            }
        }
    }
}