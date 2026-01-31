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
        [SerializeField] private int rerollCost = 5; // COINS
        
        [Header("Runtime State")]
        [SerializeField] private int _globalDiscount = 0; // Reduces TICKET cost

        [Header("Probability Settings")]
        [SerializeField] private ShopProbabilitySO currentProbabilityProfile;

        private List<CharmData> _currentShopItems = new List<CharmData>();
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
            if (playerInventory != null)
            {
                playerInventory.OnCharmAdded += OnPlayerObtainedCharm;
                playerInventory.OnCharmRemoved += OnPlayerLostCharm;
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

        public void RerollShop(bool isFree = false)
        {
            if (!isFree)
            {
                if (!ResourceManager.Instance.TrySpendResource(ResourceType.Coin, rerollCost))
                {
                    Debug.Log("Not enough COINS to Reroll!");
                    return;
                }
            }

            _currentShopItems.Clear();
            List<CharmData> sessionPicks = new List<CharmData>();

            for (int i = 0; i < shopSlots; i++)
            {
                CharmData picked = RollUniqueCharm(sessionPicks);
                if (picked != null) sessionPicks.Add(picked);
                _currentShopItems.Add(picked);
            }

            OnShopRefreshed?.Invoke(_currentShopItems);
        }

        private CharmData RollUniqueCharm(List<CharmData> excludeList)
        {
            if (currentProbabilityProfile == null) return null;

            int maxAttempts = 10; 
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                TierWeight selectedTierInfo = WeightedRandomSelector.Select(currentProbabilityProfile.tierWeights, t => t.weight);
                CharmTier targetTier = selectedTierInfo.tier;
                
                if (_availablePool.TryGetValue(targetTier, out var pool) && pool.Count > 0)
                {
                    var candidates = pool.Where(c => !excludeList.Contains(c) && c.IsUnlockable(playerInventory)).ToList();
                    if (candidates.Count > 0)
                    {
                        return WeightedRandomSelector.Select(candidates, c => c.baseSpawnWeight);
                    }
                }
            }
            return null;
        }

        public void SetProbabilityProfile(ShopProbabilitySO newProfile) => currentProbabilityProfile = newProfile;

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
            
            int finalPrice = GetFinalPrice(item);
            
            var currentTicket = ResourceManager.Instance.GetResourceBigInt(ResourceType.Ticket);
            if (currentTicket < finalPrice) 
            {
                Debug.Log($"[Shop] Not enough TICKETS! (Have: {currentTicket}, Need: {finalPrice})");
                return false;
            }
            
            if (playerInventory.GetContent().Count >= playerInventory.GetSize()) 
            {
                Debug.Log("[Shop] Inventory Full!");
                return false;
            }
            
            if (ResourceManager.Instance.TrySpendResource(ResourceType.Ticket, finalPrice))
            {
                playerInventory.AddCharm(item);

                _currentShopItems[slotIndex] = null;
                OnShopRefreshed?.Invoke(_currentShopItems);
                
                Debug.Log($"[Shop] Bought '{item.charmName}' for {finalPrice} Tickets.");
                return true;
            }

            return false;
        }
        
        public int GetRerollCost() => rerollCost;
        public List<CharmData> GetCurrentItems() => _currentShopItems;
    }
}