using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using System.Linq;

namespace _Game.Scripts.Core.Logic
{
    public class GridModel
    {
        private List<SymbolData> _allSymbols;
        private int _totalWeight;

        public GridModel(List<SymbolData> symbols)
        {
            _allSymbols = symbols;
            CalculateTotalWeight();
        }

        #region Logic Random
        private void CalculateTotalWeight()
        {
            _totalWeight = 0;
            foreach (var sym in _allSymbols) _totalWeight += sym.baseSpawnWeight;
        }

        // Chọn symbol dựa trên trọng số (Weighted Random)
        private SymbolData GetRandomSymbol()
        {
            float randomValue = Random.Range(0, _totalWeight);
            foreach (var sym in _allSymbols)
            {
                if (randomValue < sym.baseSpawnWeight) return sym;
                randomValue -= sym.baseSpawnWeight;
            }
            return _allSymbols[0]; 
        }

        // Tráo trộn danh sách (Fisher-Yates Shuffle)
        private List<T> ShuffleList<T>(List<T> inputList)
        {
            List<T> list = new List<T>(inputList);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
            return list;
        }
        #endregion

        #region API chính
        // Sinh Grid thường (Luck = 0)
        public SymbolData[,] GenerateMatrix(int rows, int cols)
        {
            return GenerateLuckyMatrix(rows, cols, 0); 
        }

        // Sinh Grid có can thiệp bởi Luck System
        public SymbolData[,] GenerateLuckyMatrix(int rows, int cols, int luckValue)
        {
            SymbolData[,] matrix = new SymbolData[cols, rows];
            
            // Chọn lucky symbol theo trọng số
            SymbolData luckySymbol = GetRandomSymbol(); 

            List<Vector2Int> allCoords = new List<Vector2Int>();
            for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                allCoords.Add(new Vector2Int(x, y));

            allCoords = ShuffleList(allCoords);
            
            int guaranteedCount = Mathf.Clamp(luckValue, 0, allCoords.Count);

            // 1. Gán Lucky Symbol vào các vị trí chỉ định
            for (int i = 0; i < guaranteedCount; i++)
            {
                Vector2Int c = allCoords[i];
                matrix[c.x, c.y] = luckySymbol;
            }

            // 2. Random các ô còn lại theo trọng số
            for (int i = guaranteedCount; i < allCoords.Count; i++)
            {
                Vector2Int c = allCoords[i];
                matrix[c.x, c.y] = GetRandomSymbol();
            }
            
            // Log
            if (luckValue > 0)
            {
                Debug.Log($"<color=cyan>[LUCK] Symbol: {luckySymbol.idName} | Count: {guaranteedCount}</color>");
            }

            return matrix;
        }
        
        // Tạo ra 1 dải băng chứa N symbol ngẫu nhiên
        public List<SymbolData> CreateRandomStrip(int length)
        {
            List<SymbolData> strip = new List<SymbolData>();
            for (int i = 0; i < length; i++)
            {
                strip.Add(GetRandomSymbol());
            }
            return strip;
        }
        #endregion
    }
}