using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace _Game.Scripts.View.Cells
{
    public class SlotCellView : MonoBehaviour
    {
        [Header("Components")]
        public Image iconImage;
        public Image backgroundHighlight;
        public RectTransform iconRect;

        [Tooltip("Chiều cao của ô. Nhập tay nếu Grid Layout Group chưa kịp tính.")]
        public float fixedCellHeight = 0f; 

        private Tween _spinTween;
        private List<SymbolData> _possibleSymbols;

        private void Awake()
        {
            if (iconImage != null)
            {
                iconRect = iconImage.rectTransform;
            }
        }

        public void SetData(SymbolData data)
        {
            if (data == null) return;
            iconImage.sprite = data.icon;
            ResetState();
        }

        // Hàm này reset mọi trạng thái về mặc định
        public void ResetState()
        {
            if (backgroundHighlight != null) 
            {
                // --- FIX QUAN TRỌNG ---
                // Phải Kill tween cũ trước khi set màu, nếu không nó sẽ tự bật lại màu vàng
                backgroundHighlight.DOKill(); 
                backgroundHighlight.color = Color.clear;
            }
            
            _spinTween?.Kill();
            iconRect.anchoredPosition = Vector2.zero;
            iconImage.color = Color.white;
        }

        public void PlaySpinAnimation(List<SymbolData> animSymbols)
        {
            // --- FIX QUAN TRỌNG ---
            // Gọi ResetState hoặc Kill background ngay khi bắt đầu quay
            if (backgroundHighlight != null)
            {
                backgroundHighlight.DOKill();
                backgroundHighlight.color = Color.clear;
            }
            // ----------------------

            _spinTween?.Kill();
            _possibleSymbols = animSymbols;

            // 1. Xác định chiều cao
            float heightToUse = fixedCellHeight;
            if (heightToUse <= 0) heightToUse = GetComponent<RectTransform>().rect.height;
            if (heightToUse <= 0) heightToUse = 150f; 

            // 2. Setup vị trí bắt đầu (Trên đỉnh)
            iconRect.anchoredPosition = new Vector2(0, heightToUse);

            // 3. Loop trượt xuống
            _spinTween = iconRect.DOAnchorPosY(-heightToUse, 0.12f)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() => 
                {
                    if (_possibleSymbols != null && _possibleSymbols.Count > 0)
                    {
                        var randomSym = _possibleSymbols[Random.Range(0, _possibleSymbols.Count)];
                        iconImage.sprite = randomSym.icon;
                    }
                }); 
        }

        public void StopSpinAnimation(SymbolData finalResult)
        {
            _spinTween?.Kill();
            
            float heightToUse = fixedCellHeight > 0 ? fixedCellHeight : GetComponent<RectTransform>().rect.height;
            if (heightToUse <= 0) heightToUse = 150f;

            iconImage.sprite = finalResult.icon;
            iconRect.anchoredPosition = new Vector2(0, heightToUse);
            iconRect.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutBack);
        }

        public void HighlightWin()
        {
            if (backgroundHighlight != null)
            {
                // Đảm bảo kill cái cũ trước khi tạo cái mới (tránh lỗi chồng chéo)
                backgroundHighlight.DOKill(); 
                
                backgroundHighlight.DOColor(new Color(1f, 1f, 0f, 0.5f), 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
    }
}