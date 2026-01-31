using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.UI
{
    public class SpinLeverUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image targetImage; // Ảnh của nút
        [SerializeField] private Button buttonComponent;

        [Header("Sprites")]
        [Tooltip("Sprite 1: Trạng thái nghỉ (Idle)")]
        [SerializeField] private Sprite spriteIdle; 
        
        [Tooltip("Sprite 2: Trạng thái giữa (Middle)")]
        [SerializeField] private Sprite spriteMiddle;
        
        [Tooltip("Sprite 3: Trạng thái gạt xuống (Pulled/Hold)")]
        [SerializeField] private Sprite spritePulled;

        [Header("Settings")]
        [SerializeField] private float frameDelay = 0.1f; // Tốc độ chuyển ảnh

        private void Start()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            if (buttonComponent == null) buttonComponent = GetComponent<Button>();
            
            // Đảm bảo bắt đầu ở trạng thái nghỉ
            ResetToIdle();
        }

        // --- GỌI KHI BẮT ĐẦU QUAY ---
        public void PlayPullAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(PullRoutine());
        }

        // --- GỌI KHI HOÀN TẤT MỌI THỨ ---
        public void PlayReleaseAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(ReleaseRoutine());
        }

        public void SetInteractable(bool state)
        {
            if (buttonComponent != null) buttonComponent.interactable = state;
        }

        private void ResetToIdle()
        {
            if (targetImage != null && spriteIdle != null)
                targetImage.sprite = spriteIdle;
        }

        private IEnumerator PullRoutine()
        {
            // 1. Đang ở Idle (Sprite 1)
            
            // 2. Chuyển sang Middle (Sprite 2)
            if (spriteMiddle != null) targetImage.sprite = spriteMiddle;
            yield return new WaitForSeconds(frameDelay);

            // 3. Chuyển sang Pulled (Sprite 3) và GIỮ NGUYÊN ở đây
            if (spritePulled != null) targetImage.sprite = spritePulled;
        }

        private IEnumerator ReleaseRoutine()
        {
            // 1. Đang ở Pulled (Sprite 3)

            // 2. Chuyển về Middle (Sprite 2)
            if (spriteMiddle != null) targetImage.sprite = spriteMiddle;
            yield return new WaitForSeconds(frameDelay);

            // 3. Chuyển về Idle (Sprite 1)
            if (spriteIdle != null) targetImage.sprite = spriteIdle;
        }
    }
}