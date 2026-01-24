using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.View.UI;

namespace _Game.Scripts.Controllers.Machines
{
    public class SlotMachineController : MonoBehaviour
    {
        #region Config & Data
        [Header("Settings")]
        public int rows = 3;
        public int cols = 5;
        [Range(0, 15)] public int currentLuck = 3; // Chỉ số may mắn

        [Header("Data")]
        public List<SymbolData> allSymbols;
        public List<PatternData> allPatterns;

        [Header("UI")]
        public BoardView boardView;
        public Button spinButton;
        public TextMeshProUGUI scoreText;
        #endregion

        private GridModel _gridModel;
        private PatternEvaluator _evaluator;
        private bool _isSpinning = false;

        #region Game Loop
        private void Start()
        {
            // Init
            _gridModel = new GridModel(allSymbols);
            _evaluator = new PatternEvaluator(allPatterns);
            boardView.InitializeBoard(rows, cols);

            // Hiển thị bảng khởi đầu (Luck = 0)
            SymbolData[,] initGrid = _gridModel.GenerateMatrix(rows, cols);
            boardView.SetInitialState(initGrid, rows, cols);
            
            spinButton.onClick.AddListener(OnSpinClick);
            scoreText.text = "READY";
        }

        private void OnSpinClick()
        {
            if (_isSpinning) return;
            StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            _isSpinning = true;
            spinButton.interactable = false;
            scoreText.text = "SPINNING...";

            // 1. LOGIC: Tính toán kết quả trước
            SymbolData[,] finalGrid = _gridModel.GenerateLuckyMatrix(rows, cols, currentLuck);
            List<MatchResult> results = _evaluator.Evaluate(finalGrid, cols, rows);
            
            // 2. PASS RESULT: Gửi kết quả cho View
            // Truyền finalGrid vào, BoardView sẽ tự dựng kịch bản để dừng đúng hình đó
            bool animDone = false;
            StartCoroutine(boardView.SpinSequenceRoutine(finalGrid, _gridModel, rows, cols, () => 
            {
                animDone = true;
            }));

            // Đợi diễn hoạt xong
            yield return new WaitUntil(() => animDone);
            yield return new WaitForSeconds(0.5f); // Delay nhỏ cho mượt
            
            // 3. VIEW: Show kết quả
            float totalWin = 0;
            foreach (var r in results)
            {
                totalWin += r.GetScore();
                boardView.HighlightWinCells(r.matchedCoordinates, rows, cols);
            }

            scoreText.text = totalWin > 0 ? $"WIN: {totalWin}" : "TRY AGAIN";
            
            _isSpinning = false;
            spinButton.interactable = true;
        }
        #endregion
    }
}