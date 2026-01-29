using System; // Cần thêm thư viện này cho Action
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.View.UI;
using _Game.Scripts.Core.Inventory;

namespace _Game.Scripts.Controllers.Machines
{
    public class SlotMachineController : MonoBehaviour
    {
        #region Config & Data
        [Header("Settings")]
        public int rows = 3;
        public int cols = 5;
        [Range(0, 15)] public int currentLuck = 3;

        [Header("Data")]
        public List<SymbolData> allSymbols;
        public List<PatternData> allPatterns;

        [Header("UI")]
        public BoardView boardView;
        // Bỏ nút SpinButton ở đây, vì GameManager hoặc UI Manager sẽ quản lý nút bấm
        // public Button spinButton; 
        public TextMeshProUGUI scoreText;
        #endregion

        private GridModel _gridModel;
        private PatternEvaluator _evaluator;
        private bool _isSpinning = false;
        public CharmHolder charmHolder;

        #region Initialization
        private void Start()
        {
            // Init Logic & View
            _gridModel = new GridModel(allSymbols);
            _evaluator = new PatternEvaluator(allPatterns);
            boardView.InitializeBoard(rows, cols);

            // Hiển thị bảng khởi đầu
            SymbolData[,] initGrid = _gridModel.GenerateMatrix(rows, cols);
            boardView.SetInitialState(initGrid, rows, cols);
            
            scoreText.text = "READY";
        }
        #endregion

        #region Public API (Gọi từ GameManager)
        
        // GameManager sẽ gọi hàm này và truyền vào một hàm callback để nhận kết quả
        public void PerformSpin(Action<float> onSpinComplete)
        {
            if (_isSpinning) return;
            StartCoroutine(SpinRoutine(onSpinComplete));
        }

        #endregion

        #region Internal Logic
        private IEnumerator SpinRoutine(Action<float> onSpinComplete)
        {
            _isSpinning = true;
            scoreText.text = "SPINNING...";
    
            // Check buff đầu game
            if (charmHolder != null)
            {
                var charms = charmHolder.GetContent(); 
                for (int i = 0; i < charms.Count; i++)
                {
                    charms[i].OnSpinStart(this);
                }
            }
    
            // Core tính toán
            SymbolData[,] finalGrid = _gridModel.GenerateLuckyMatrix(rows, cols, currentLuck);
            List<MatchResult> results = _evaluator.Evaluate(finalGrid, cols, rows);
    
            // Hoạt ảnh
            bool animDone = false;
            StartCoroutine(boardView.SpinSequenceRoutine(finalGrid, _gridModel, rows, cols, () => 
            {
                animDone = true;
            }));

            yield return new WaitUntil(() => animDone);
            yield return new WaitForSeconds(0.2f); 
    
            // Kết quả
            float totalWin = 0;
            foreach (var r in results)
            {
                totalWin += r.GetScore();
                boardView.HighlightWinCells(r.matchedCoordinates, rows, cols);
            }

            scoreText.text = totalWin > 0 ? $"WIN: {totalWin}" : "0";
            
            // Dọn dẹp truớc khi rút
            if (charmHolder != null)
            {
                var charms = charmHolder.GetContent();
                for (int i = 0; i < charms.Count; i++)
                {
                    charms[i].OnSpinResult(this, totalWin);
                }
                
                for (int i = charms.Count - 1; i >= 0; i--)
                {
                    charms[i].OnSpinEnd(this);
                }
            }
    
            _isSpinning = false;
            onSpinComplete?.Invoke(totalWin);
        }
        #endregion
    }
}