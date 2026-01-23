using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using _Game.Scripts.Core.Data;
using _Game.Scripts.View.Cells;

namespace _Game.Scripts.View.UI
{
    public class BoardView : MonoBehaviour
    {
        [Header("References")]
        public SlotCellView cellPrefab;
        public Transform gridContainer;

        private List<SlotCellView> _spawnedCells = new List<SlotCellView>();

        public void InitializeBoard(int rows, int cols)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = null; // Fix lỗi Inspector
#endif
            foreach (Transform child in gridContainer) Destroy(child.gameObject);
            _spawnedCells.Clear();

            int totalCells = rows * cols;
            for (int i = 0; i < totalCells; i++)
            {
                SlotCellView cell = Instantiate(cellPrefab, gridContainer);
                _spawnedCells.Add(cell);
            }
        }

        // --- CẬP NHẬT: Truyền list symbol vào đây ---
        public void StartSpinning(int rows, int cols, List<SymbolData> animSymbols)
        {
            foreach (var cell in _spawnedCells)
            {
                cell.PlaySpinAnimation(animSymbols);
            }
        }

        public IEnumerator StopSpinningRoutine(SymbolData[,] grid, int rows, int cols, System.Action onComplete)
        {
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int visualY = (rows - 1) - y;
                    int listIndex = (visualY * cols) + x;

                    if (listIndex >= 0 && listIndex < _spawnedCells.Count)
                    {
                        var cell = _spawnedCells[listIndex];
                        cell.StopSpinAnimation(grid[x, y]);
                    }
                }
                yield return new WaitForSeconds(0.25f); 
            }
            yield return new WaitForSeconds(0.2f);
            onComplete?.Invoke();
        }

        public void HighlightWinCells(List<Vector2Int> coordinates, int rows, int cols)
        {
            foreach (var coord in coordinates)
            {
                int visualY = (rows - 1) - coord.y;
                int index = (visualY * cols) + coord.x;
                
                if (index < _spawnedCells.Count)
                    _spawnedCells[index].HighlightWin();
            }
        }
        
        public void SetInitialState(SymbolData[,] grid, int rows, int cols)
        {
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int visualY = (rows - 1) - y;
                    int listIndex = (visualY * cols) + x;
                    if (listIndex >= 0 && listIndex < _spawnedCells.Count)
                    {
                        _spawnedCells[listIndex].SetData(grid[x, y]);
                    }
                }
            }
        }
    }
}