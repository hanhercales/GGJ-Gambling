using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Utilities;
using System.Linq; // Cần dùng để xử lý Dictionary hoặc List

namespace _Game.Scripts.Core.Managers
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CharmHolder playerInventory; // Kéo CharmHolder vào đây
        [SerializeField] private List<CharmData> allCharmsPool; // Kéo tất cả Charm SO vào đây

        [Header("Config")]
        [SerializeField] private int shopSlots = 6;
        [SerializeField] private int rerollCost = 5;

        [Header("Probability Settings")]
        // Profile tỉ lệ hiện tại (GameManager sẽ thay đổi cái này khi game khó lên)
        [SerializeField] private ShopProbabilitySO currentProbabilityProfile;

        // Data hiện tại của Shop (để hiển thị UI)
        private List<CharmData> _currentShopItems = new List<CharmData>();

        // Cache: Phân loại Charm theo Tier để truy xuất nhanh, không cần duyệt list mỗi lần roll
        private Dictionary<CharmTier, List<CharmData>> _charmsByTier = new Dictionary<CharmTier, List<CharmData>>();

        // Event báo cho UI vẽ lại
        public event System.Action<List<CharmData>> OnShopRefreshed;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Phân loại Charm vào các rổ Tier ngay khi khởi chạy
            InitializeCharmPools();
        }

        private void Start()
        {
            // Tự động roll shop lần đầu (Free)
            RerollShop(true);
        }

        // --- KHỞI TẠO DỮ LIỆU ---
        private void InitializeCharmPools()
        {
            _charmsByTier.Clear();

            // 1. Tạo list rỗng cho từng loại Tier
            foreach (CharmTier tier in System.Enum.GetValues(typeof(CharmTier)))
            {
                _charmsByTier[tier] = new List<CharmData>();
            }

            // 2. Phân loại từng Charm vào rổ tương ứng
            foreach (var charm in allCharmsPool)
            {
                if (_charmsByTier.ContainsKey(charm.tier))
                {
                    _charmsByTier[charm.tier].Add(charm);
                }
            }
            
            Debug.Log($"ShopManager: Đã phân loại {allCharmsPool.Count} charms vào các Tier.");
        }

        // Hàm để GameManager gọi khi muốn đổi tỉ lệ (VD: Qua màn 5 thì đổi profile tỉ lệ mới)
        public void SetProbabilityProfile(ShopProbabilitySO newProfile)
        {
            currentProbabilityProfile = newProfile;
            Debug.Log($"Shop Probability Updated: {newProfile.name}");
        }

        // --- CORE LOGIC: RANDOM 2 BƯỚC ---
        private CharmData RollSingleCharm()
        {
            if (currentProbabilityProfile == null)
            {
                Debug.LogError("LỖI: Chưa gán ShopProbabilitySO vào ShopManager!");
                return null;
            }

            // BƯỚC 1: Chọn Tier dựa trên trọng số của Profile hiện tại
            // VD: Profile Level 1 có (Common: 100, Rare: 0) -> Chắc chắn ra Common
            TierWeight selectedTierInfo = WeightedRandomSelector.Select(
                currentProbabilityProfile.tierWeights, 
                t => t.weight
            );
            
            CharmTier targetTier = selectedTierInfo.tier;

            // BƯỚC 2: Chọn Charm cụ thể trong Tier đó
            if (_charmsByTier.TryGetValue(targetTier, out List<CharmData> pool) && pool.Count > 0)
            {
                // Dùng tiếp WeightedRandomSelector chọn item dựa trên baseSpawnWeight của chính item đó
                // (Giúp phân biệt item xịn/dởm trong cùng 1 tier nếu cần)
                return WeightedRandomSelector.Select(pool, c => c.baseSpawnWeight);
            }

            // Fallback (Phòng hờ): Nếu quay vào Tier Rare mà chưa tạo charm Rare nào -> Lấy Common bù vào
            if (_charmsByTier[CharmTier.Common].Count > 0)
            {
                return _charmsByTier[CharmTier.Common][0];
            }

            return null; // Trường hợp xấu nhất (không có data)
        }

        // --- API: REROLL ---
        public void RerollShop(bool isFree = false)
        {
            // 1. Check tiền
            if (!isFree)
            {
                // TrySpendResource xử lý được cả BigInteger
                if (!ResourceManager.Instance.TrySpendResource(ResourceType.Coin, rerollCost))
                {
                    Debug.Log("Không đủ tiền Reroll!");
                    return;
                }
            }

            // 2. Xóa shop cũ
            _currentShopItems.Clear();
            
            // 3. Roll từng ô một theo logic mới
            for (int i = 0; i < shopSlots; i++)
            {
                CharmData newItem = RollSingleCharm();
                
                // Lưu ý: newItem có thể null nếu setup data lỗi, ta add null để UI xử lý ô trống
                _currentShopItems.Add(newItem);
            }

            // 4. Báo UI cập nhật
            OnShopRefreshed?.Invoke(_currentShopItems);
        }

        // --- API: BUY ---
        public bool TryBuyItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _currentShopItems.Count) return false;
            
            CharmData item = _currentShopItems[slotIndex];
            if (item == null) return false; // Ô này trống hoặc đã mua

            // 1. Check tiền (CharmData.price là int, ResourceManager dùng BigInt -> So sánh OK)
            if (ResourceManager.Instance.GetResourceBigInt(ResourceType.Coin) < item.price)
            {
                Debug.Log("Không đủ tiền mua Charm này!");
                return false;
            }

            // 2. Check túi (CharmHolder)
            if (playerInventory.GetContent().Count >= playerInventory.GetSize())
            {
                Debug.Log("Túi đồ đã đầy!");
                return false;
            }

            // 3. Thực hiện giao dịch
            ResourceManager.Instance.TrySpendResource(ResourceType.Coin, item.price);
            playerInventory.AddCharm(item);

            // 4. Xóa item khỏi shop (để trống ô đó, tránh mua lại)
            _currentShopItems[slotIndex] = null;
            
            // 5. Update UI (để ô đó biến mất hoặc hiện chữ SOLD)
            OnShopRefreshed?.Invoke(_currentShopItems);
            
            return true;
        }

        public int GetRerollCost() => rerollCost;
        public List<CharmData> GetCurrentItems() => _currentShopItems;
    }
}