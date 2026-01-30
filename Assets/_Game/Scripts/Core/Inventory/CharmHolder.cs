using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using UnityEngine;

namespace _Game.Scripts.Core.Inventory
{
    public class CharmHolder : MonoBehaviour
    {
        [SerializeField] private List<CharmData> content = new List<CharmData>();
        [SerializeField] private int size = 7;
        
        public event System.Action OnHolderChanged;
        public event System.Action<CharmData> OnCharmAdded;
        public event System.Action<CharmData> OnCharmRemoved;

        public void ModifyCapacity(int amount)
        {
            size += amount;
            if (size < 0) size = 0; 
            OnHolderChanged?.Invoke();
        }
        
        public bool AddCharm(CharmData charm)
        {
            if(content.Count >= size) return false;
            
            content.Add(charm);
            
            charm.OnEquip(this);
            
            OnHolderChanged?.Invoke();
            
            OnCharmAdded?.Invoke(charm);
            
            return true;
        }

        public bool RemoveCharm(CharmData charm)
        {
            if (content.Contains(charm))
            {
                charm.OnUnequip(this);
                
                content.Remove(charm);
                OnHolderChanged?.Invoke();
                
                OnCharmRemoved?.Invoke(charm);
                return true;
            }
            return false;
        }

        public bool ClearCharms()
        {
            // Cần loop để bắn event cho từng cái, đảm bảo trả hết về kho
            // Tạo bản sao list để tránh lỗi khi xóa
            var tempContent = new List<CharmData>(content);
            foreach(var c in tempContent)
            {
                RemoveCharm(c);
            }
            return true;
        }
        
        public int GetSize()
        {
            return size;
        }
        
        public List<CharmData> GetContent()
        {
            return content;
        }
    }
}

