using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Utilities;
using System.Linq;

namespace _Game.Scripts.Core.Managers
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CharmHolder playerInventory;
        [SerializeField] private List<CharmData> allCharmsPool; // Danh sách gốc

        [Header("Config")]
        [SerializeField] private int shopSlots = 6;
        [SerializeField] private int rerollCost = 5;

        [Header("Probability Settings")]
        [SerializeField] private ShopProbabilitySO currentProbabilityProfile;

        // Danh sách hiển thị trên UI
        private List<CharmData> _currentShopItems = new List<CharmData>();
        
        // Kho hàng khả dụng (những món người chơi CHƯA có)
        private Dictionary<CharmTier, List<CharmData>> _availablePool = new Dictionary<CharmTier, List<CharmData>>();

        public event System.Action<List<CharmData>> OnShopRefreshed;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeCharmPools();
        }

        private void Start()
        {
            // Đăng ký sự kiện từ CharmHolder
            if (playerInventory != null)
            {
                playerInventory.OnCharmAdded += OnPlayerObtainedCharm;
                playerInventory.OnCharmRemoved += OnPlayerLostCharm;
                
                // Đồng bộ ngay lập tức (phòng trường hợp Load Game đã có đồ)
                SyncPoolWithInventory();
            }

            RerollShop(true);
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.OnCharmAdded -= OnPlayerObtainedCharm;
                playerInventory.OnCharmRemoved -= OnPlayerLostCharm;
            }
        }

        // --- QUẢN LÝ KHO (POOL SYSTEM) ---

        private void InitializeCharmPools()
        {
            _availablePool.Clear();
            foreach (CharmTier tier in System.Enum.GetValues(typeof(CharmTier)))
            {
                _availablePool[tier] = new List<CharmData>();
            }

            // Nạp tất cả vào kho
            foreach (var charm in allCharmsPool)
            {
                if (_availablePool.ContainsKey(charm.tier))
                {
                    _availablePool[charm.tier].Add(charm);
                }
            }
        }

        private void SyncPoolWithInventory()
        {
            // Loại bỏ những món đang nằm trong túi người chơi
            foreach (var charm in playerInventory.GetContent())
            {
                RemoveFromPool(charm);
            }
        }

        private void OnPlayerObtainedCharm(CharmData charm) => RemoveFromPool(charm);
        private void OnPlayerLostCharm(CharmData charm) => AddToPool(charm);

        private void RemoveFromPool(CharmData charm)
        {
            if (_availablePool.TryGetValue(charm.tier, out var list))
            {
                if (list.Contains(charm)) list.Remove(charm);
            }
        }

        private void AddToPool(CharmData charm)
        {
            if (_availablePool.TryGetValue(charm.tier, out var list))
            {
                if (!list.Contains(charm)) list.Add(charm);
            }
        }

        // --- LOGIC REROLL (KHÔNG TRÙNG LẶP) ---

        public void RerollShop(bool isFree = false)
        {
            if (!isFree)
            {
                if (!ResourceManager.Instance.TrySpendResource(ResourceType.Coin, rerollCost))
                {
                    Debug.Log("Không đủ tiền Reroll!");
                    return;
                }
            }

            _currentShopItems.Clear();
            
            // Danh sách tạm để kiểm tra trùng lặp TRONG MẺ QUAY NÀY
            List<CharmData> sessionPicks = new List<CharmData>();

            for (int i = 0; i < shopSlots; i++)
            {
                CharmData picked = RollUniqueCharm(sessionPicks);
                
                // Nếu pick được thì thêm vào danh sách tạm để ô sau không pick trúng nữa
                if (picked != null)
                {
                    sessionPicks.Add(picked);
                }
                
                _currentShopItems.Add(picked);
            }

            OnShopRefreshed?.Invoke(_currentShopItems);
        }

        // Hàm roll đảm bảo không trùng với những gì đã roll trước đó trong cùng mẻ
        private CharmData RollUniqueCharm(List<CharmData> excludeList)
        {
            if (currentProbabilityProfile == null) return null;

            int maxAttempts = 10; 
    
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 1. Pick Tier
                TierWeight selectedTierInfo = WeightedRandomSelector.Select(
                    currentProbabilityProfile.tierWeights, 
                    t => t.weight
                );
                CharmTier targetTier = selectedTierInfo.tier;
                
                if (_availablePool.TryGetValue(targetTier, out var pool) && pool.Count > 0)
                {
                    var candidates = pool.Where(c => 
                            !excludeList.Contains(c) &&       // 1. Not picked in this reroll yet
                            c.IsUnlockable(playerInventory)   // 2. NEW: Requirements met?
                    ).ToList();

                    if (candidates.Count > 0)
                    {
                        return WeightedRandomSelector.Select(candidates, c => c.baseSpawnWeight);
                    }
                }
            }

            return null;
        }

        public void SetProbabilityProfile(ShopProbabilitySO newProfile)
        {
            currentProbabilityProfile = newProfile;
        }

        public bool TryBuyItem(int slotIndex)
        {
            // 1. Kiểm tra Slot hợp lệ
            if (slotIndex < 0 || slotIndex >= _currentShopItems.Count) 
            {
                Debug.LogError($"[Shop] Invalid Slot Index: {slotIndex}");
                return false;
            }

            CharmData item = _currentShopItems[slotIndex];
            if (item == null)
            {
                Debug.LogWarning($"[Shop] Slot {slotIndex} is empty (already bought).");
                return false;
            }

            // 2. Kiểm tra tiền
            var currentCoin = ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin);
            if (currentCoin < item.price) 
            {
                Debug.Log($"[Shop] Mua thất bại: Không đủ tiền! (Có: {currentCoin}, Cần: {item.price})");
                return false;
            }
            
            // 3. Kiểm tra túi đồ (Inventory Full)
            // LƯU Ý: Đây là nguyên nhân phổ biến nhất khiến bấm vào không có gì xảy ra
            if (playerInventory.GetContent().Count >= playerInventory.GetSize()) 
            {
                Debug.Log("[Shop] Mua thất bại: Túi đồ đã đầy (Inventory Full)!");
                return false;
            }

            // 4. Thực hiện giao dịch
            ResourceManager.Instance.TrySpendResource(ResourceType.Coin, item.price);
            playerInventory.AddCharm(item);

            _currentShopItems[slotIndex] = null;
            OnShopRefreshed?.Invoke(_currentShopItems);
            
            Debug.Log($"[Shop] Mua thành công: {item.name}");
            return true;
        }
        
        public int GetRerollCost() => rerollCost;
        public List<CharmData> GetCurrentItems() => _currentShopItems;
    }
}