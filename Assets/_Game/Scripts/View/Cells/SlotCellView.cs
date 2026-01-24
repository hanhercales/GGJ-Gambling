using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace _Game.Scripts.View.Cells
{
    public class SlotCellView : MonoBehaviour
    {
        #region Components
        [Header("Components")]
        public Image iconImage;
        public Image backgroundHighlight;
        public RectTransform iconRect;
        public float spinSpeed = 0.5f;
        
        [Tooltip("Chiều cao ô (Nhập tay để fix lỗi Layout chưa load).")]
        public float fixedCellHeight = 0f; 
        #endregion

        private Tween _spinTween;
        private List<SymbolData> _possibleSymbols; // Để random tráo hình

        private void Awake()
        {
            if (iconImage != null) iconRect = iconImage.rectTransform;
        }

        public void SetData(SymbolData data)
        {
            if (data == null) return;
            iconImage.sprite = data.icon;
            ResetState();
        }

        #region Animation Control
        // Reset về trạng thái tĩnh
        public void ResetState()
        {
            if (backgroundHighlight != null)
            {
                backgroundHighlight.DOKill(); // Kill tween cũ ngay
                backgroundHighlight.color = Color.clear;
            }
            _spinTween?.Kill();
            iconRect.anchoredPosition = Vector2.zero;
            iconImage.color = Color.white;
        }

        // Bắt đầu hiệu ứng cuộn vô tận
        public void PlaySpinAnimation(List<SymbolData> animSymbols)
        {
            ResetState(); // Dọn dẹp trước khi quay
            _possibleSymbols = animSymbols;

            // 1. Lấy chiều cao chuẩn
            float h = (fixedCellHeight > 0) ? fixedCellHeight : GetComponent<RectTransform>().rect.height;
            if (h <= 0) h = 150f; // Fallback

            // 2. Đưa lên đỉnh
            iconRect.anchoredPosition = new Vector2(0, h);

            // 3. Loop trượt xuống -> Đổi hình -> Lặp lại
            _spinTween = iconRect.DOAnchorPosY(-h, spinSpeed)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() => 
                {
                    // Tráo hình ngẫu nhiên mỗi vòng
                    if (_possibleSymbols?.Count > 0)
                        iconImage.sprite = _possibleSymbols[Random.Range(0, _possibleSymbols.Count)].icon;
                });
        }

        // Dừng cuộn và hiện kết quả thật
        public void StopSpinAnimation(SymbolData finalResult)
        {
            _spinTween?.Kill();
            float h = (fixedCellHeight > 0) ? fixedCellHeight : GetComponent<RectTransform>().rect.height;
            if (h <= 0) h = 150f;

            iconImage.sprite = finalResult.icon;
            iconRect.anchoredPosition = new Vector2(0, h);
            
            // Rơi xuống và nảy nhẹ
            iconRect.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutBack);
        }

        // Nhấp nháy khi thắng
        public void HighlightWin()
        {
            if (backgroundHighlight != null)
            {
                backgroundHighlight.DOKill();
                backgroundHighlight.DOColor(new Color(1f, 1f, 0f, 0.5f), 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
        #endregion
    }
}