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
            return true;
        }

        public bool RemoveCharm(CharmData charm)
        {
            if (content.Contains(charm))
            {
                charm.OnUnequip(this);
                
                content.Remove(charm);
                OnHolderChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool ClearCharms()
        {
            foreach(var c in content) c.OnUnequip(this);
            content.Clear();
            OnHolderChanged?.Invoke();
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

