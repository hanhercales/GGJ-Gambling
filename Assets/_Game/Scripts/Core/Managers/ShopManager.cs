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

            int maxAttempts = 10; // Giới hạn số lần thử để tránh treo máy nếu hết sạch đồ
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 1. Chọn Tier
                TierWeight selectedTierInfo = WeightedRandomSelector.Select(
                    currentProbabilityProfile.tierWeights, 
                    t => t.weight
                );
                CharmTier targetTier = selectedTierInfo.tier;

                // 2. Lấy danh sách khả dụng của Tier đó
                if (_availablePool.TryGetValue(targetTier, out var pool) && pool.Count > 0)
                {
                    // Lọc ra những cái chưa bị chọn trong mẻ này
                    // (Cách này hơi tốn hiệu năng nhưng đảm bảo an toàn với list nhỏ)
                    var candidates = pool.Where(c => !excludeList.Contains(c)).ToList();

                    if (candidates.Count > 0)
                    {
                        // Chọn 1 cái từ danh sách đã lọc
                        return WeightedRandomSelector.Select(candidates, c => c.baseSpawnWeight);
                    }
                }
                
                // Nếu Tier này hết đồ, vòng lặp sẽ tự thử lại (có thể ra Tier khác)
            }

            // Nếu quay mãi không ra (do hết sạch đồ ở mọi Tier khả thi) -> Trả về null
            return null;
        }

        public void SetProbabilityProfile(ShopProbabilitySO newProfile)
        {
            currentProbabilityProfile = newProfile;
        }

        public bool TryBuyItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _currentShopItems.Count) return false;
            CharmData item = _currentShopItems[slotIndex];
            if (item == null) return false;

            if (ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin) < item.price) return false;
            
            // Check full túi
            if (playerInventory.GetContent().Count >= playerInventory.GetSize()) return false;

            ResourceManager.Instance.TrySpendResource(ResourceType.Coin, item.price);
            
            // AddCharm sẽ kích hoạt Event -> Gọi RemoveFromPool tự động
            playerInventory.AddCharm(item);

            _currentShopItems[slotIndex] = null;
            OnShopRefreshed?.Invoke(_currentShopItems);
            return true;
        }
        
        public int GetRerollCost() => rerollCost;
        public List<CharmData> GetCurrentItems() => _currentShopItems;
    }
}