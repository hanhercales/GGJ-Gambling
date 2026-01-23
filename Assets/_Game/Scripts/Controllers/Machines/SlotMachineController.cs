using System.Collections;
using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.View.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;         

namespace _Game.Scripts.Controllers.Machines
{
    public class SlotMachineController : MonoBehaviour
    {
        [Header("Settings")]
        public int rows = 3;
        public int cols = 5;

        // BIẾN MỚI: Chỉ số may mắn
        [Range(0, 15)] 
        public int currentLuck = 3; 

        [Header("Data")]
        public List<SymbolData> allSymbols;
        public List<PatternData> allPatterns;

        [Header("UI References")]
        public BoardView boardView;
        public Button spinButton;
        public TextMeshProUGUI scoreText;

        private GridModel _gridModel;
        private PatternEvaluator _evaluator;
        private bool _isSpinning = false;

        private void Start()
        {
            _gridModel = new GridModel(allSymbols);
            _evaluator = new PatternEvaluator(allPatterns);
            
            boardView.InitializeBoard(rows, cols);

            // Bảng ban đầu thì không cần Luck (Luck = 0)
            SymbolData[,] initialGrid = _gridModel.GenerateMatrix(rows, cols);
            boardView.SetInitialState(initialGrid, rows, cols);
            
            spinButton.onClick.AddListener(OnSpinButtonClicked);
            scoreText.text = "READY"; 
        }

        private void OnSpinButtonClicked()
        {
            if (_isSpinning) return; 
            StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            _isSpinning = true;
            spinButton.interactable = false; 
            scoreText.text = "SPINNING...";

            // 1. Visual: Truyền list symbol vào để làm hiệu ứng tráo hình
            boardView.StartSpinning(rows, cols, allSymbols);

            // 2. Logic: Sinh bảng có tính năng Luck
            SymbolData[,] grid = _gridModel.GenerateLuckyMatrix(rows, cols, currentLuck);
            List<MatchResult> results = _evaluator.Evaluate(grid, cols, rows);

            // 3. Đợi quay (1 giây)
            yield return new WaitForSeconds(1.0f);

            // 4. Dừng quay từng cột
            bool animationDone = false;
            StartCoroutine(boardView.StopSpinningRoutine(grid, rows, cols, () => {
                animationDone = true;
            }));

            yield return new WaitUntil(() => animationDone);

            // 5. Tính điểm
            float totalWin = 0;
            foreach (var res in results)
            {
                totalWin += res.GetScore();
                boardView.HighlightWinCells(res.matchedCoordinates, rows, cols);
            }

            scoreText.text = totalWin > 0 ? $"WIN: {totalWin}" : "TRY AGAIN";
            
            _isSpinning = false;
            spinButton.interactable = true; 
        }
    }
}