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

        public bool AddCharm(CharmData charm)
        {
            if(content.Count >= size) return false;
            
            content.Add(charm);
            OnHolderChanged?.Invoke();
            return true;
        }

        public bool RemoveCharm(CharmData charm)
        {
            content.Remove(charm);
            OnHolderChanged?.Invoke();
            return true;
        }

        public bool ClearCharms()
        {
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

