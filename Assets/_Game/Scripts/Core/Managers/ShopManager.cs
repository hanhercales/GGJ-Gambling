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
        [SerializeField] private List<CharmData> allCharmsPool;

        [Header("Config")]
        [SerializeField] private int shopSlots = 6;
        [SerializeField] private int rerollCost = 5; // Giá Reroll bằng Coin
        
        [Header("Runtime State")]
        [SerializeField] private int _globalDiscount = 0; // Giảm giá (nếu có charm giảm giá)

        [Header("Probability Settings")]
        [SerializeField] private ShopProbabilitySO currentProbabilityProfile;

        // Danh sách hiển thị trên UI
        private List<CharmData> _currentShopItems = new List<CharmData>();
        
        // Biến lưu tỉ lệ cộng thêm (Buff từ Lust Mask...)
        private Dictionary<CharmTier, float> _tierWeightMultipliers = new Dictionary<CharmTier, float>();
        
        // Kho hàng khả dụng (những món người chơi CHƯA có)
        private Dictionary<CharmTier, List<CharmData>> _availablePool = new Dictionary<CharmTier, List<CharmData>>();
        
        // Danh sách các slot được miễn phí (Buff từ Gluttony Mask...)
        private HashSet<int> _freeSlots = new HashSet<int>();

        // [MỚI] Thứ tự ưu tiên để fallback (Từ cao xuống thấp)
        // Nếu quay trúng Legendary mà hết hàng -> Nó sẽ tìm Rare -> Uncommon -> Common
        private readonly List<CharmTier> _tierFallbackOrder = new List<CharmTier>() 
        { 
            CharmTier.Legendary, 
            CharmTier.Rare, 
            CharmTier.Uncommon, 
            CharmTier.Common 
        };

        public event System.Action<List<CharmData>> OnShopRefreshed;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeCharmPools();
        }

        private void Start()
        {
            if (playerInventory != null)
            {
                playerInventory.OnCharmAdded += OnPlayerObtainedCharm;
                playerInventory.OnCharmRemoved += OnPlayerLostCharm;
                
                // Đồng bộ ngay lập tức phòng trường hợp Load Game
                SyncPoolWithInventory();
            }

            // Reroll lần đầu khi game chạy (Free)
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
            foreach (var charm in playerInventory.GetContent())
            {
                RemoveFromPool(charm);
            }
        }

        private void OnPlayerObtainedCharm(CharmData charm) => RemoveFromPool(charm);
        
        private void OnPlayerLostCharm(CharmData charm)
        {
            // Nếu là vật phẩm mua 1 lần duy nhất thì không trả lại pool
            if (charm.oneTimePurchase) return;

            AddToPool(charm);
        }
        
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

        // --- LOGIC REROLL THÔNG MINH (SMART REROLL) ---

        public void RerollShop(bool isFree = false)
        {
            if (!isFree)
            {
                // Kiểm tra tiền trước khi Reroll
                if (!ResourceManager.Instance.TrySpendResource(ResourceType.Coin, rerollCost))
                {
                    Debug.Log("Không đủ Coin để Reroll!");
                    return;
                }
            }

            _currentShopItems.Clear();
            _freeSlots.Clear(); // Reset trạng thái miễn phí
            
            // Danh sách tạm để đảm bảo không trùng lặp trong cùng 1 lần reroll
            List<CharmData> sessionPicks = new List<CharmData>();

            for (int i = 0; i < shopSlots; i++)
            {
                // [THAY ĐỔI] Dùng hàm RollSmartCharm thay vì RollUniqueCharm cũ
                CharmData picked = RollSmartCharm(sessionPicks);
                
                if (picked != null) 
                {
                    sessionPicks.Add(picked);
                }
                _currentShopItems.Add(picked);
            }
            
            // Bắn sự kiện cho các Mask (ví dụ Gluttony muốn free slot)
            if (CharmManager.Instance != null)
                CharmManager.Instance.NotifyShopRolled(this);

            OnShopRefreshed?.Invoke(_currentShopItems);
        }

        private CharmData RollSmartCharm(List<CharmData> excludeList)
        {
            if (currentProbabilityProfile == null) return null;

            // BƯỚC 1: Chọn Tier mục tiêu dựa trên tỉ lệ (Weighted Random)
            // Ví dụ: 5% ra Legendary, 95% ra Common
            TierWeight selectedTierInfo = WeightedRandomSelector.Select(
                    currentProbabilityProfile.tierWeights, 
                    t => Mathf.RoundToInt(GetEffectiveWeight(t)) 
                );
            CharmTier targetTier = selectedTierInfo.tier;

            // BƯỚC 2: Thử lấy đồ từ Tier mục tiêu
            CharmData result = TryPickFromTier(targetTier, excludeList);
            if (result != null) return result;

            // BƯỚC 3: Nếu Tier mục tiêu hết hàng -> FALLBACK (Hạ cấp độ tìm kiếm)
            // Logic: Nếu quay trúng Legendary nhưng hết Legendary -> Tìm Rare -> Uncommon -> Common
            
            int startIndex = _tierFallbackOrder.IndexOf(targetTier);
            
            // Quét xuống các tier thấp hơn trước
            for (int i = startIndex + 1; i < _tierFallbackOrder.Count; i++)
            {
                result = TryPickFromTier(_tierFallbackOrder[i], excludeList);
                if (result != null) return result;
            }

            // Nếu vẫn không có, quét ngược lên các tier cao hơn (trường hợp rất hiếm: chỉ còn đồ xịn)
            for (int i = startIndex - 1; i >= 0; i--)
            {
                result = TryPickFromTier(_tierFallbackOrder[i], excludeList);
                if (result != null) return result;
            }

            return null; // Thực sự hết sạch đồ trong game
        }

        private CharmData TryPickFromTier(CharmTier tier, List<CharmData> excludeList)
        {
            if (_availablePool.TryGetValue(tier, out var pool) && pool.Count > 0)
            {
                // Lọc danh sách ứng viên:
                // 1. Chưa được chọn trong lượt reroll này (excludeList)
                // 2. Thỏa mãn điều kiện mở khóa (IsUnlockable - check charm cha)
                var candidates = pool.Where(c => 
                        !excludeList.Contains(c) &&       
                        c.IsUnlockable(playerInventory)   
                ).ToList();

                if (candidates.Count > 0)
                {
                    // Chọn ngẫu nhiên theo trọng số spawn của từng charm
                    return WeightedRandomSelector.Select(candidates, c => c.baseSpawnWeight);
                }
            }
            return null;
        }

        // --- CÁC HÀM TIỆN ÍCH & API ---

        public void SetProbabilityProfile(ShopProbabilitySO newProfile)
        {
            currentProbabilityProfile = newProfile;
        }

        public int GetFinalPrice(CharmData item)
        {
            if (item == null) return 0;
            return Mathf.Max(0, item.price - _globalDiscount);
        }
        
        public void ModifyDiscount(int amount)
        {
            _globalDiscount += amount;
            OnShopRefreshed?.Invoke(_currentShopItems); 
        }
        
        public bool TryBuyItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _currentShopItems.Count) return false;

            CharmData item = _currentShopItems[slotIndex];
            if (item == null) return false;
            
            // 1. Tính giá Ticket (có áp dụng giảm giá hoặc miễn phí slot)
            int finalPrice = GetSlotPrice(slotIndex);
            
            // 2. Kiểm tra đủ Ticket không
            var currentTicket = ResourceManager.Instance.GetResourceBigInt(ResourceType.Ticket);
            
            if (currentTicket < finalPrice) 
            {
                Debug.Log($"[Shop] Không đủ Ticket! (Có: {currentTicket}, Cần: {finalPrice})");
                return false;
            }
            
            // 3. Kiểm tra túi đầy
            if (playerInventory.GetContent().Count >= playerInventory.GetSize()) 
            {
                Debug.Log("[Shop] Túi đồ đã đầy!");
                return false;
            }

            // 4. Trừ Ticket và thêm đồ
            if (ResourceManager.Instance.TrySpendResource(ResourceType.Ticket, finalPrice))
            {
                playerInventory.AddCharm(item);

                _currentShopItems[slotIndex] = null; // Xóa khỏi slot sau khi mua
                OnShopRefreshed?.Invoke(_currentShopItems);
                
                Debug.Log($"[Shop] Mua thành công '{item.charmName}' giá {finalPrice} Ticket.");
                return true;
            }

            return false;
        }
        
        public void SetSlotFree(int index)
        {
            if (!_freeSlots.Contains(index))
            {
                _freeSlots.Add(index);
            }
        }
        
        public int GetSlotPrice(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _currentShopItems.Count) return 0;
    
            // Nếu slot này được đánh dấu miễn phí (Gluttony Mask)
            if (_freeSlots.Contains(slotIndex)) return 0;

            CharmData item = _currentShopItems[slotIndex];
            return GetFinalPrice(item);
        }
        
        public void ModifyTierWeight(CharmTier tier, float multiplier)
        {
            if (!_tierWeightMultipliers.ContainsKey(tier))
            {
                _tierWeightMultipliers[tier] = 1.0f;
            }
    
            // Cộng dồn multiplier (ví dụ: +0.3)
            _tierWeightMultipliers[tier] += multiplier;
    
            Debug.Log($"[ShopManager] {tier} weight adjusted. Now: {_tierWeightMultipliers[tier]}x");
        }
        
        private float GetEffectiveWeight(TierWeight tw)
        {
            float mult = 1.0f;
            if (_tierWeightMultipliers.TryGetValue(tw.tier, out float m))
            {
                mult = m;
            }
            return tw.weight * mult;
        }
        
        public int GetRerollCost() => rerollCost;
        public List<CharmData> GetCurrentItems() => _currentShopItems;
    }
}