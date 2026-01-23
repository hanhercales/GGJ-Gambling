using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using System.Linq;

namespace _Game.Scripts.Core.Logic
{
    public class GridModel
    {
        private List<SymbolData> _allSymbols;
        private float _totalWeight;

        public GridModel(List<SymbolData> symbols)
        {
            _allSymbols = symbols;
            CalculateTotalWeight();
        }

        private void CalculateTotalWeight()
        {
            _totalWeight = 0;
            foreach (var sym in _allSymbols)
            {
                _totalWeight += sym.baseSpawnWeight;
            }
        }

        // --- LOGIC MỚI: Sinh Grid có can thiệp bởi Luck ---
        public SymbolData[,] GenerateLuckyMatrix(int rows, int cols, int luckValue)
        {
            SymbolData[,] matrix = new SymbolData[cols, rows];
            
            // 1. Chọn ngẫu nhiên 1 Symbol làm "Lucky Symbol"
            SymbolData luckySymbol = _allSymbols[Random.Range(0, _allSymbols.Count)];
            
            // 2. Tạo danh sách tọa độ của toàn bộ bàn cờ
            List<Vector2Int> allCoords = new List<Vector2Int>();
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    allCoords.Add(new Vector2Int(x, y));
                }
            }

            // 3. Tráo trộn danh sách tọa độ (Shuffle) để Luck nằm rải rác
            allCoords = ShuffleList(allCoords);

            // 4. Gán Lucky Symbol vào N vị trí đầu tiên (N = luckValue)
            // Clamp để đảm bảo không crash nếu luck > tổng số ô
            int guaranteedCount = Mathf.Clamp(luckValue, 0, allCoords.Count);

            for (int i = 0; i < guaranteedCount; i++)
            {
                Vector2Int coord = allCoords[i];
                matrix[coord.x, coord.y] = luckySymbol;
            }

            // 5. Random các ô còn lại theo tỷ lệ trọng số bình thường
            for (int i = guaranteedCount; i < allCoords.Count; i++)
            {
                Vector2Int coord = allCoords[i];
                matrix[coord.x, coord.y] = GetRandomSymbol();
            }

            // Log ra console để bạn dễ kiểm tra
            if (luckValue > 0)
            {
                Debug.Log($"<color=cyan>[LUCK SYSTEM] Applied: {luckValue} x {luckySymbol.idName}</color>");
            }
            
            return matrix;
        }

        // Hàm trộn ngẫu nhiên (Fisher-Yates Shuffle)
        private List<T> ShuffleList<T>(List<T> inputList)
        {
            List<T> list = new List<T>(inputList);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]); // Swap cú pháp mới C#
            }
            return list;
        }

        // Hàm random theo trọng số (Giữ nguyên)
        private SymbolData GetRandomSymbol()
        {
            float randomValue = Random.Range(0, _totalWeight);
            float currentSum = 0;

            foreach (var sym in _allSymbols)
            {
                currentSum += sym.baseSpawnWeight;
                if (randomValue <= currentSum)
                {
                    return sym;
                }
            }
            return _allSymbols.Last(); 
        }

        // Wrapper cho trường hợp không dùng Luck (Luck = 0)
        public SymbolData[,] GenerateMatrix(int rows, int cols)
        {
            return GenerateLuckyMatrix(rows, cols, 0); 
        }
    }
}