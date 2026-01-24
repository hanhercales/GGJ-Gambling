using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.View.Cells;

namespace _Game.Scripts.View.UI
{
    public class BoardView : MonoBehaviour
    {
        #region References
        [Header("Setup")]
        public SlotCellView cellPrefab;
        public Transform gridContainer;
        private List<SlotCellView> _spawnedCells = new List<SlotCellView>();
        #endregion

        #region Init Setup
        public void InitializeBoard(int rows, int cols)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = null; // Fix lỗi Editor UI
#endif
            foreach (Transform child in gridContainer) Destroy(child.gameObject);
            _spawnedCells.Clear();

            for (int i = 0; i < rows * cols; i++)
            {
                _spawnedCells.Add(Instantiate(cellPrefab, gridContainer));
            }
        }

        // Set hình tĩnh (dùng lúc Start game)
        public void SetInitialState(SymbolData[,] grid, int rows, int cols)
        {
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int index = GetIndex(x, y, rows, cols);
                    if (index < _spawnedCells.Count) 
                        _spawnedCells[index].SetData(grid[x, y]);
                }
            }
        }
        #endregion

        #region Flow Control
        // 1. Cho tất cả các ô quay
        public void StartSpinning(int rows, int cols, List<SymbolData> animSymbols)
        {
            foreach (var cell in _spawnedCells)
                cell.PlaySpinAnimation(animSymbols);
        }

        // 2. Dừng lần lượt từng cột (Coroutine)
        public IEnumerator StopSpinningRoutine(SymbolData[,] grid, int rows, int cols, System.Action onComplete)
        {
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int index = GetIndex(x, y, rows, cols);
                    if (index < _spawnedCells.Count)
                        _spawnedCells[index].StopSpinAnimation(grid[x, y]);
                }
                yield return new WaitForSeconds(0.25f); // Delay giữa các cột
            }
            yield return new WaitForSeconds(0.2f); // Delay kết thúc
            onComplete?.Invoke();
        }

        // 3. Highlight ô thắng
        public void HighlightWinCells(List<Vector2Int> coordinates, int rows, int cols)
        {
            foreach (var c in coordinates)
            {
                int index = GetIndex(c.x, c.y, rows, cols);
                if (index < _spawnedCells.Count) _spawnedCells[index].HighlightWin();
            }
        }

        // Map tọa độ (x,y) sang index list phẳng
        private int GetIndex(int x, int y, int rows, int cols)
        {
            // Grid Layout Group xếp: Trên -> Dưới, Trái -> Phải
            // Logic: Dưới -> Trên, Trái -> Phải
            int visualY = (rows - 1) - y;
            return (visualY * cols) + x;
        }
        #endregion
    }
}