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
        public GameplayButtonsUI gameplayButtons;
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
            
            if (gameplayButtons != null)
            {
                gameplayButtons.SetInteractable(false);
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
            
            if (gameplayButtons != null)
            {
                gameplayButtons.SetInteractable(true);
            }

            // 4. BÁO CÁO KẾT QUẢ VỀ GAMEMANAGER
            onSpinComplete?.Invoke(totalWin, results);

            if(results.Count() > 2)
            {
                AudioManager.Instance.PlayWin(results.Count() > 7);
            }
        }
        
        private IEnumerator ShowWinSequence(List<MatchResult> results)
        {
            float currentDisplayedScore = 0;
            scoreText.text = "0";

            float gSymMult = ScoreManager.Instance.GetSymbolMult();
            float gPatMult = ScoreManager.Instance.GetPatternMult();

            // Sắp xếp: Nhỏ hiện trước, Lớn hiện sau
            var displaySequence = results
                .OrderBy(r => r.pattern.priority)
                .ThenBy(r => r.GetScore(gSymMult, gPatMult))
                .ToList();
            
            yield return new WaitForSeconds(0.2f);

            // === TRƯỜNG HỢP 1: CHỈ ĂN ĐÚNG 1 PATTERN ===
            // Logic: Bật Highlight "xịn" (nhấp nháy/loop) ngay lập tức và cộng tiền luôn
            if (displaySequence.Count == 1)
            {
                var match = displaySequence[0];
                float matchScore = match.GetScore(gSymMult, gPatMult);

                // Dùng hàm HighlightWinCells (loại nhấp nháy) thay vì SetHighlightPattern (loại tĩnh)
                boardView.HighlightWinCells(match.matchedCoordinates, rows, cols);

                ResourceManager.Instance.AddResource(ResourceType.Coin, (int)matchScore);
                scoreText.text = $"WIN: {matchScore}";
                
                AudioManager.Instance.PlayPatternMatch();

                // Giữ nguyên trạng thái này trong 1.5s để người chơi tận hưởng
                yield return new WaitForSeconds(1.5f);
            }
            // === TRƯỜNG HỢP 2: ĂN NHIỀU PATTERN (COMBO) ===
            // Logic: Chạy tuần tự từng cái (Highlight tĩnh -> tắt) rồi mới chốt hạ bằng Highlight tổng
            else
            {
                // A. Giai đoạn tuần tự (Cộng dồn cảm xúc)
                foreach (var match in displaySequence)
                {
                    float matchScore = match.GetScore(gSymMult, gPatMult);
                    
                    // Bật Highlight thường (tĩnh)
                    boardView.SetHighlightPattern(match.matchedCoordinates, true);

                    ResourceManager.Instance.AddResource(ResourceType.Coin, (int)matchScore);
                    
                    currentDisplayedScore += matchScore;
                    scoreText.text = $"WIN: {currentDisplayedScore}";

                    yield return new WaitForSeconds(0.4f);
                    
                    AudioManager.Instance.PlayPatternMatch();

                    // Tắt đi để chuyển sang cái tiếp theo
                    boardView.SetHighlightPattern(match.matchedCoordinates, false);
                    
                    yield return new WaitForSeconds(0.1f); 
                }

                // B. Giai đoạn chốt hạ (Show tất cả)
                HashSet<Vector2Int> allCoords = new HashSet<Vector2Int>();
                foreach (var r in results) 
                    foreach (var c in r.matchedCoordinates) allCoords.Add(c);

                // Bật Highlight xịn (nhấp nháy) cho toàn bộ
                boardView.HighlightWinCells(new List<Vector2Int>(allCoords), rows, cols);
                
                yield return new WaitForSeconds(1.5f); 
            }
        }
        #endregion
    }
}