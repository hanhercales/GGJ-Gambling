using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Core.Utilities
{
    public static class WeightedRandomSelector
    {
        /// <summary>
        /// Chọn ngẫu nhiên 1 phần tử từ list dựa trên trọng số.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu (SymbolData, CharmData...)</typeparam>
        /// <param name="items">Danh sách các item</param>
        /// <param name="weightSelector">Hàm lambda để lấy trọng số từ item (VD: x => x.weight)</param>
        /// <returns>Item được chọn</returns>
        public static T Select<T>(IList<T> items, Func<T, int> weightSelector)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogError("WeightedRandomSelector: Danh sách rỗng!");
                return default;
            }

            // 1. Tính tổng trọng số
            int totalWeight = 0;
            foreach (var item in items)
            {
                int w = weightSelector(item);
                if (w < 0) w = 0; // Đảm bảo không âm
                totalWeight += w;
            }

            // Nếu tổng trọng số = 0, trả về phần tử đầu tiên hoặc random đều
            if (totalWeight <= 0) return items[Random.Range(0, items.Count)];

            // 2. Random một số từ 0 đến Total
            int randomValue = Random.Range(0, totalWeight);

            // 3. Quét và trừ dần (Linear Scan)
            foreach (var item in items)
            {
                int w = weightSelector(item);
                if (w <= 0) continue; // Bỏ qua item trọng số 0

                if (randomValue < w)
                {
                    return item; // Trúng giải!
                }
                
                randomValue -= w;
            }

            // Fallback (chống lỗi làm tròn số thực, dù ở đây dùng int thì ít gặp)
            return items[items.Count - 1];
        }

        /// <summary>
        /// Tạo một danh sách ngẫu nhiên (VD: tạo dải băng Reel Strip)
        /// </summary>
        public static List<T> SelectMultiple<T>(IList<T> items, Func<T, int> weightSelector, int count)
        {
            List<T> results = new List<T>();
            for (int i = 0; i < count; i++)
            {
                results.Add(Select(items, weightSelector));
            }
            return results;
        }
    }
}