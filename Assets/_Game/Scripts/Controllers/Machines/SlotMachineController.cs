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
        }
        
        private IEnumerator ShowWinSequence(List<MatchResult> results)
        {
            float currentDisplayedScore = 0;
            scoreText.text = "0";

            float gSymMult = ScoreManager.Instance.GetSymbolMult();
            float gPatMult = ScoreManager.Instance.GetPatternMult();

            // =========================================================================
            // BƯỚC QUAN TRỌNG: SẮP XẾP LẠI ĐỂ HIỂN THỊ
            // Logic tính toán trả về: Ưu tiên Cao -> Thấp (để lọc trùng).
            // Logic hiển thị cần: Ưu tiên Thấp -> Cao (để tạo kịch tính).
            // =========================================================================
            
            var displaySequence = results
                .OrderBy(r => r.pattern.priority) // 1. Xếp theo độ ưu tiên (0, 1, 2...) -> Nhỏ hiện trước
                .ThenBy(r => r.GetScore(gSymMult, gPatMult)) // 2. Nếu cùng ưu tiên, xếp theo điểm số (Ít tiền hiện trước)
                .ToList();
            
            yield return new WaitForSeconds(0.2f);

            // === GIAI ĐOẠN 1: HIGHLIGHT TỪNG PATTERN (TUẦN TỰ TỪ NHỎ ĐẾN LỚN) ===
            foreach (var match in displaySequence)
            {
                float matchScore = match.GetScore(gSymMult, gPatMult);
                
                // A. Bật Highlight các ô thắng của pattern này
                boardView.SetHighlightPattern(match.matchedCoordinates, true);

                // B. Cộng tiền vào ResourceManager
                ResourceManager.Instance.AddResource(ResourceType.Coin, (int)matchScore);
                
                // C. Cập nhật Text Score chạy lên
                currentDisplayedScore += matchScore;
                scoreText.text = $"WIN: {currentDisplayedScore}";

                // D. Chờ người chơi nhìn thấy
                yield return new WaitForSeconds(0.4f);

                // E. Tắt Highlight pattern này để nhường chỗ cho cái to hơn
                boardView.SetHighlightPattern(match.matchedCoordinates, false);
                
                yield return new WaitForSeconds(0.1f); 
            }

            // === GIAI ĐOẠN 2: HIGHLIGHT TỔNG (CHỈ CHẠY NẾU CÓ > 1 PATTERN) ===
            // Logic: Nếu chỉ thắng 1 pattern thì Giai đoạn 1 đã show rồi, không cần show lại.
            if (results.Count > 1) 
            {
                HashSet<Vector2Int> allCoords = new HashSet<Vector2Int>();
                foreach (var r in results) 
                foreach (var c in r.matchedCoordinates) allCoords.Add(c);

                // Highlight tất cả cùng lúc để chốt hạ
                boardView.HighlightWinCells(new List<Vector2Int>(allCoords), rows, cols);
                
                // Chờ ăn mừng tổng thể
                yield return new WaitForSeconds(1.5f); 
            }
        }
        #endregion
    }
}