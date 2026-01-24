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
        
        [Tooltip("Chiều cao ô (Nhập tay để fix lỗi Layout chưa load).")]
        public float fixedCellHeight = 0f; 
        #endregion

        private Tween _spinTween;
        private int _currentIndex; 

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

        public void ResetState()
        {
            if (backgroundHighlight != null)
            {
                backgroundHighlight.DOKill();
                backgroundHighlight.color = Color.clear;
            }
            _spinTween?.Kill();
            iconRect.anchoredPosition = Vector2.zero;
            iconImage.color = Color.white;
        }

        #region Deterministic Animation
        public void SpinSequence(List<SymbolData> sequence, float timePerSymbol)
        {
            ResetState();
            _currentIndex = 0;

            if (sequence == null || sequence.Count == 0) return;

            iconImage.sprite = sequence[0].icon;
            
            float h = (fixedCellHeight > 0) ? fixedCellHeight : GetComponent<RectTransform>().rect.height;
            if (h <= 0) h = 150f;

            RunNextStep(sequence, h, timePerSymbol);
        }
        
        private void RunNextStep(List<SymbolData> sequence, float height, float speed)
        {
            iconRect.anchoredPosition = new Vector2(0, height);
            iconImage.sprite = sequence[_currentIndex].icon;

            _spinTween = iconRect.DOAnchorPosY(-height, speed)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _currentIndex++;
                    if (_currentIndex < sequence.Count - 1)
                    {
                        RunNextStep(sequence, height, speed);
                    }
                    else
                    {
                        FinishSpin(sequence[_currentIndex], height);
                    }
                });
        }
        
        private void FinishSpin(SymbolData finalSymbol, float height)
        {
            _spinTween?.Kill();
            iconImage.sprite = finalSymbol.icon;
            iconRect.anchoredPosition = new Vector2(0, height);
            iconRect.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutBack);
        }
        
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