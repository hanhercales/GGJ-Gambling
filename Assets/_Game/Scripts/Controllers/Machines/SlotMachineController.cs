using System; // Cần thêm thư viện này cho Action
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.Core.Managers;
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

        [Header("Data")]
        public List<SymbolData> allSymbols;
        public List<PatternData> allPatterns;

        [Header("UI")]
        public BoardView boardView;
        public SpinLeverUI spinLever;
        public TextMeshProUGUI scoreText;
        [Tooltip("Quản lý nhóm nút Shop và Pack")]
        #endregion

        private GridModel _gridModel;
        private PatternEvaluator _evaluator;
        private bool _isSpinning = false;

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
        public void PerformSpin(int luckValue, System.Action<float, List<MatchResult>> onSpinComplete)
        {
            if (_isSpinning) return;
            StartCoroutine(SpinRoutine(luckValue, onSpinComplete));
        }
        public void ResetPatternStats()
        {
            foreach (var pattern in allPatterns)
            {
                pattern.ResetStats();
            }
            Debug.Log("Patterns: Đã reset stats về gốc.");
        }
        #endregion

        #region Internal Logic
        private IEnumerator SpinRoutine(int luckValue, System.Action<float, List<MatchResult>> onSpinComplete)
        {
            _isSpinning = true;
            scoreText.text = "SPINNING...";
            
            if (spinLever != null)
            {
                spinLever.SetInteractable(false); // Khóa nút
                spinLever.PlayPullAnimation();    // Chạy hoạt ảnh gạt (1->2->3)
            }
            
            // 1. LOGIC: Tính toán kết quả
            SymbolData[,] finalGrid = _gridModel.GenerateLuckyMatrix(rows, cols, luckValue);
            List<MatchResult> results = _evaluator.Evaluate(finalGrid, cols, rows);
            
            // 2. VIEW: Diễn hoạt
            bool animDone = false;
            StartCoroutine(boardView.SpinSequenceRoutine(finalGrid, _gridModel, rows, cols, () => 
            {
                animDone = true;
            }));

            yield return new WaitUntil(() => animDone);
            yield return new WaitForSeconds(0.2f); 
            
            // 3. SHOW RESULT
            float totalWin = results.Sum(r => r.GetScore(ScoreManager.Instance.GetSymbolMult(), ScoreManager.Instance.GetPatternMult()));

            if (totalWin > 0)
            {
                // Chạy diễn hoạt cộng tiền tuần tự
                yield return StartCoroutine(ShowWinSequence(results));
                
                // Hiển thị tổng cuối cùng cho chắc chắn
                scoreText.text = $"WIN: {totalWin}";
            }
            else
            {
                scoreText.text = "0";
            }
            
            _isSpinning = false;
            
            if (spinLever != null)
            {
                spinLever.PlayReleaseAnimation(); // Chạy hoạt ảnh nhả (3->2->1)
                spinLever.SetInteractable(true);  // Mở lại nút
            }

            // 4. BÁO CÁO KẾT QUẢ VỀ GAMEMANAGER
            onSpinComplete?.Invoke(totalWin, results);

            if(results.Count() > 2)
            {
                AudioManager.Instance.PlayWin();
            }
        }
        
        public IEnumerator HandleSingleWinVisual(MatchResult match, float duration = 1.5f)
        {
            float gSymMult = ScoreManager.Instance.GetSymbolMult();
            float gPatMult = ScoreManager.Instance.GetPatternMult();
            float matchScore = match.GetScore(gSymMult, gPatMult);
            
            boardView.HighlightWinCells(match.matchedCoordinates, rows, cols);
            AudioManager.Instance.PlayPatternMatch();
            if (matchScore > 0)
            {
                ResourceManager.Instance.AddResource(ResourceType.Coin, (int)matchScore);
            }

            yield return new WaitForSeconds(duration);
        }
        
        private IEnumerator HandleComboStepVisual(MatchResult match, float displayScore)
        {
            float gSymMult = ScoreManager.Instance.GetSymbolMult();
            float gPatMult = ScoreManager.Instance.GetPatternMult();
            float matchScore = match.GetScore(gSymMult, gPatMult);
            
            boardView.SetHighlightPattern(match.matchedCoordinates, true);
            AudioManager.Instance.PlayPatternMatch();
            
            ResourceManager.Instance.AddResource(ResourceType.Coin, (int)matchScore);
            scoreText.text = $"WIN: {displayScore}";

            yield return new WaitForSeconds(0.4f);
            
            boardView.SetHighlightPattern(match.matchedCoordinates, false);
            yield return new WaitForSeconds(0.1f);
        }

        public void ForceStopVisuals()
        {
            StopAllCoroutines();
            if (boardView != null)
            {
                boardView.HighlightWinCells(new List<Vector2Int>(), rows, cols);
            }
        }

        private IEnumerator ShowWinSequence(List<MatchResult> results)
        {
            float currentDisplayedScore = 0;
            scoreText.text = "0";
            
            float gSymMult = ScoreManager.Instance.GetSymbolMult();
            float gPatMult = ScoreManager.Instance.GetPatternMult();

            var displaySequence = results
                .OrderBy(r => r.pattern.priority)
                .ThenBy(r => r.GetScore(gSymMult, gPatMult))
                .ToList();

            yield return new WaitForSeconds(0.2f);
            
            if (displaySequence.Count == 1)
            {
                var match = displaySequence[0];
                float score = match.GetScore(gSymMult, gPatMult);
                scoreText.text = $"WIN: {score}";
                
                yield return StartCoroutine(HandleSingleWinVisual(match, 1.5f));
            }
            else
            {
                foreach (var match in displaySequence)
                {
                    float matchScore = match.GetScore(gSymMult, gPatMult);
                    currentDisplayedScore += matchScore;
                    
                    // REUSE: Call the shared step function
                    yield return StartCoroutine(HandleComboStepVisual(match, currentDisplayedScore));
                }
                
                HashSet<Vector2Int> allCoords = new HashSet<Vector2Int>();
                foreach (var r in results) 
                    foreach (var c in r.matchedCoordinates) allCoords.Add(c);

                boardView.HighlightWinCells(new List<Vector2Int>(allCoords), rows, cols);
                yield return new WaitForSeconds(1.5f); 
            }
        }
        #endregion
    }
}