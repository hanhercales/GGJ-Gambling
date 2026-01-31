using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.View.Cells;

namespace _Game.Scripts.View.UI
{
    public class BoardView : MonoBehaviour
    {
        #region Config
        [System.Serializable] 
        public class AnimationSettings
        {
            [Header("Spinning Feel")]
            [Tooltip("Thời gian trượt qua 1 ô (Càng nhỏ càng nhanh).")]
            [Range(0.01f, 0.2f)] public float timePerSymbol = 0.08f;

            [Tooltip("Thời gian chờ giữa các cột (Hiệu ứng lan truyền).")]
            [Range(0f, 1f)] public float delayPerColumn = 0.2f;

            [Header("Reel Length")]
            [Tooltip("Số lượng hình rác tối thiểu trong 1 lần quay.")]
            [Range(10, 50)] public int baseReelLength = 15;

            [Tooltip("Cột sau sẽ quay nhiều hơn cột trước bao nhiêu hình.")]
            [Range(0, 10)] public int reelLengthIncrement = 2;
        }

        [Header("Animation Config")]
        public AnimationSettings animSettings; 
        #endregion
        
        #region References
        [Header("References")]
        public SlotCellView cellPrefab;
        public Transform gridContainer;
        private List<SlotCellView> _spawnedCells = new List<SlotCellView>();
        
        private int _currentRows;
        private int _currentCols;
        #endregion

        #region Init Setup
        public void InitializeBoard(int rows, int cols)
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = null; 
#endif
            
            _currentRows = rows;
            _currentCols = cols;
            
            foreach (Transform child in gridContainer) Destroy(child.gameObject);
            _spawnedCells.Clear();

            for (int i = 0; i < rows * cols; i++)
            {
                _spawnedCells.Add(Instantiate(cellPrefab, gridContainer));
            }
        }

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
        public IEnumerator SpinSequenceRoutine(SymbolData[,] resultGrid, GridModel model, int rows, int cols, System.Action onComplete)
        {
            // Duyệt từng cột
            for (int x = 0; x < cols; x++)
            {
                // 1. Tính độ dài dải băng dựa trên Config
                int stripLength = animSettings.baseReelLength + (x * animSettings.reelLengthIncrement);
                List<SymbolData> reelStrip = model.CreateRandomStrip(stripLength); 

                // 2. Kích hoạt quay cho cột này
                for (int y = 0; y < rows; y++)
                {
                    int index = GetIndex(x, y, rows, cols);
                    if (index < _spawnedCells.Count)
                    {
                        List<SymbolData> cellSequence = new List<SymbolData>(reelStrip);
                        
                        // Gắn kết quả thật vào cuối
                        cellSequence.Add(resultGrid[x, y]);

                        // TRUYỀN TỐC ĐỘ TỪ CONFIG VÀO ĐÂY
                        _spawnedCells[index].SpinSequence(cellSequence, animSettings.timePerSymbol);
                    }
                }

                // TRUYỀN ĐỘ TRỄ TỪ CONFIG VÀO ĐÂY
                yield return new WaitForSeconds(animSettings.delayPerColumn); 
            }

            // Tính toán thời gian cột cuối cùng quay
            int lastColLength = animSettings.baseReelLength + (cols * animSettings.reelLengthIncrement);
            float lastColSpinDuration = lastColLength * animSettings.timePerSymbol; 
            
            yield return new WaitForSeconds(lastColSpinDuration + 0.5f);

            onComplete?.Invoke();
        }
        
        public void HighlightWinCells(List<Vector2Int> coordinates, int rows, int cols)
        {
            foreach (var c in coordinates)
            {
                int index = GetIndex(c.x, c.y, rows, cols);
                if (index < _spawnedCells.Count) _spawnedCells[index].HighlightWin();
            }
        }
        
        public void SetHighlightPattern(List<Vector2Int> coordinates, bool isActive)
        {
            foreach (var c in coordinates)
            {
                // Sử dụng _currentRows và _currentCols đã lưu
                int index = GetIndex(c.x, c.y, _currentRows, _currentCols);
                if (index >= 0 && index < _spawnedCells.Count)
                {
                    // Gọi hàm SetHighlightState mà chúng ta đã thêm vào SlotCellView
                    _spawnedCells[index].SetHighlightState(isActive);
                }
            }
        }

        private int GetIndex(int x, int y, int rows, int cols)
        {
            int visualY = (rows - 1) - y;
            return (visualY * cols) + x;
        }
        #endregion
    }
}