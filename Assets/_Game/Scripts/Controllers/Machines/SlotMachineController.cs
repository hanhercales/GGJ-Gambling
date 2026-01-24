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

            // 1. VISUAL: Bắt đầu hiệu ứng cuộn (Tráo hình)
            boardView.StartSpinning(rows, cols, allSymbols);

            // 2. LOGIC: Tính toán kết quả ngầm (Có áp dụng Luck)
            SymbolData[,] grid = _gridModel.GenerateLuckyMatrix(rows, cols, currentLuck);
            List<MatchResult> results = _evaluator.Evaluate(grid, cols, rows);

            // 3. WAIT: Chờ cho hồi hộp
            yield return new WaitForSeconds(1.0f);

            // 4. VISUAL: Dừng từng cột và hiện hình thật
            bool animDone = false;
            StartCoroutine(boardView.StopSpinningRoutine(grid, rows, cols, () => animDone = true));
            yield return new WaitUntil(() => animDone);

            // 5. END: Tổng kết điểm
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