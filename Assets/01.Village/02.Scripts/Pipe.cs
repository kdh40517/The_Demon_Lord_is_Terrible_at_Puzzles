using UnityEngine;
using UnityEngine.UI;

namespace TM
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class Pipe : MonoBehaviour
    {
        [Header("파이프 설정 (북, 동, 남, 서)")]
        public bool[] isOpened = new bool[4];

        [Header("파이프 이미지")]
        public Sprite emptySprite;
        public Sprite waterSprite;

        private Image pipeImage;
        private Button button;

        public bool hasWater = false;

        private float lastClickTime = 0f;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip rotateSound;
        // ★ 새로 추가된 부분: 소리를 얼만큼만 들려줄지 정하는 변수 (기본값 0.5초)
        [Range(0.1f, 2.0f)]
        public float soundDuration = 0.5f;

        void Awake()
        {
            pipeImage = GetComponent<Image>();
            button = GetComponent<Button>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPipeClicked);

            button.targetGraphic = pipeImage;

            UpdateSprite();
        }

        public void OnPipeClicked()
        {
            if (PuzzleManager.instance.isCleared)
                return;

            if (Time.time - lastClickTime < 0.2f)
                return;

            lastClickTime = Time.time;

            RotatePipe();
            PlayRotateSound();
            PuzzleManager.instance.CheckWaterFlow();
        }

        public void RotatePipe()
        {
            transform.Rotate(0, 0, -90);

            bool lastValue = isOpened[3];
            isOpened[3] = isOpened[2];
            isOpened[2] = isOpened[1];
            isOpened[1] = isOpened[0];
            isOpened[0] = lastValue;
        }

        public void SetWater(bool fill)
        {
            hasWater = fill;
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (pipeImage == null)
                return;

            pipeImage.sprite = hasWater ? waterSprite : emptySprite;
        }

        // ★ 대폭 수정된 소리 재생 함수
        private void PlayRotateSound()
        {
            if (audioSource != null && rotateSound != null)
            {
                // 1. 혹시 이전에 예약된 '소리 끄기' 명령이 있다면 취소 (연속 클릭 시 오류 방지)
                CancelInvoke("StopSound");

                // 2. 기존에 나고 있던 소리가 있다면 끄고 처음부터 다시 재생
                audioSource.Stop();
                audioSource.PlayOneShot(rotateSound);

                // 3. soundDuration(초) 뒤에 StopSound 함수를 실행하라고 예약
                Invoke("StopSound", soundDuration);
            }
        }

        // ★ 새로 추가된 소리 끄기 함수
        private void StopSound()
        {
            if (audioSource != null)
            {
                audioSource.Stop(); // 소리 강제 종료!
            }
        }
    }
}