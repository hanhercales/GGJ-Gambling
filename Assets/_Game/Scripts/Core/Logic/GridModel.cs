using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using System.Linq;
using _Game.Scripts.Core.Utilities;

namespace _Game.Scripts.Core.Logic
{
    public class GridModel
    {
        private List<SymbolData> _allSymbols;
        private int _totalWeight;

        public GridModel(List<SymbolData> symbols)
        {
            _allSymbols = symbols;
        }

        #region Logic Random
        // Chọn symbol dựa trên trọng số (Weighted Random)
        private SymbolData GetRandomSymbol()
        {
            // Cú pháp: Select(List, Hàm lấy trọng số)
            return WeightedRandomSelector.Select(_allSymbols, symbol => symbol.currentWeight);
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
            // Dùng hàm SelectMultiple có sẵn trong Utility
            return WeightedRandomSelector.SelectMultiple(_allSymbols, s => s.currentWeight, length);
        }
        #endregion
    }
}