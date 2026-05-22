using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TM
{
    public class DirectionalPuzzle : MonoBehaviour
    {
        [Header("퍼즐 설정")]
        public int maxVisibleArrows = 8;
        private List<KeyCode> activeSequence = new List<KeyCode>();

        [Header("게이지 설정")]
        public Image gaugeFillImage;
        public float maxGauge = 100f;
        public float currentGauge = 0f;
        [Space]
        public float fillPerCorrect = 10f;
        public float penaltyPerWrong = 10f;
        public float drainPerSecond = 5f;

        [Header("악마 기믹 설정")]
        public Image devilImage;
        public Sprite devilEyesClosed;
        public Sprite devilEyesOpen;
        public float penaltyWhenCaught = 30f;
        [Space]
        public float minCloseTime = 2f;
        public float maxCloseTime = 5f;
        public float minOpenTime = 1f;
        public float maxOpenTime = 3f;

        public bool isDevilWatching = false;
        public float devilTimer = 0f;

        [Header("이벤트")]
        public UnityEvent<KeyCode> onArrowAdded;
        public UnityEvent onCorrectInput;
        public UnityEvent onWrongInput;
        public UnityEvent onGaugeFull;

        private bool isGameClear = false;

        void Start()
        {
            for (int i = 0; i < maxVisibleArrows; i++)
            {
                AddNewArrow();
            }

            UpdateGaugeUI();

            isDevilWatching = false;
            if (devilImage != null && devilEyesClosed != null)
            {
                devilImage.sprite = devilEyesClosed;
            }

            SetDevilTimer(false);
        }

        void Update()
        {
            if (isGameClear) return;

            // 1. 게이지 자동 감소
            if (currentGauge > 0)
            {
                currentGauge -= drainPerSecond * Time.deltaTime;
                currentGauge = Mathf.Max(currentGauge, 0f);
                UpdateGaugeUI();
            }

            // 2. 악마 상태 업데이트
            UpdateDevilState();

            // 3. 키 입력 감지 및 처리
            if (Input.anyKeyDown)
            {
                KeyCode pressedKey = GetPressedArrowKey();

                if (pressedKey != KeyCode.None)
                {
                    if (isDevilWatching)
                    {
                        // 🚨 악마가 보고 있을 때 움직임 -> 대참사! 페널티 적용
                        currentGauge -= penaltyWhenCaught;
                        currentGauge = Mathf.Max(currentGauge, 0f);
                        UpdateGaugeUI();
                        onWrongInput.Invoke();
                        Debug.Log("악마에게 걸렸습니다!");
                    }
                    else
                    {
                        // ✅ 평소대로 퍼즐 풀기
                        ProcessPuzzleInput(pressedKey);
                    }
                }
            }
        }

        private void UpdateDevilState()
        {
            devilTimer -= Time.deltaTime;

            if (devilTimer <= 0f)
            {
                isDevilWatching = !isDevilWatching;

                if (isDevilWatching)
                {
                    if (devilImage != null) devilImage.sprite = devilEyesOpen;
                    SetDevilTimer(true);
                }
                else
                {
                    if (devilImage != null) devilImage.sprite = devilEyesClosed;
                    SetDevilTimer(false);
                }
            }
        }

        private void SetDevilTimer(bool isOpeningEyes)
        {
            if (isOpeningEyes)
            {
                devilTimer = Random.Range(minOpenTime, maxOpenTime);
            }
            else
            {
                devilTimer = Random.Range(minCloseTime, maxCloseTime);
            }
        }

        // 정상적으로 퍼즐을 풀 때의 로직
        private void ProcessPuzzleInput(KeyCode pressedKey)
        {
            if (pressedKey == activeSequence[0])
            {
                // 맞췄을 때: 게이지 증가
                currentGauge += fillPerCorrect;

                activeSequence.RemoveAt(0);
                onCorrectInput.Invoke();
                AddNewArrow();

                // 싱글톤 매니저를 통해 정답 사운드 즉시 재생
                if (GameManager.instance != null)
                {
                    GameManager.instance.PlayCorrectSound();
                }

                if (currentGauge >= maxGauge)
                {
                    currentGauge = maxGauge;
                    isGameClear = true;
                    Debug.Log("게이지 100% 달성! 퍼즐 클리어!");
                    onGaugeFull.Invoke();

                    GameManager.instance.TriggerClearSequence();
                }
            }
            else
            {
                // 틀렸을 때: 페널티 적용
                currentGauge -= penaltyPerWrong;
                currentGauge = Mathf.Max(currentGauge, 0f);
                onWrongInput.Invoke();
                Debug.Log("틀렸습니다!");
            }

            UpdateGaugeUI();
        }

        private void UpdateGaugeUI()
        {
            if (gaugeFillImage != null)
            {
                gaugeFillImage.fillAmount = currentGauge / maxGauge;
            }
        }

        private void AddNewArrow()
        {
            KeyCode[] arrows = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Space };
            KeyCode newKey = arrows[Random.Range(0, arrows.Length)];

            activeSequence.Add(newKey);
            onArrowAdded.Invoke(newKey);
        }

        private KeyCode GetPressedArrowKey()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) return KeyCode.UpArrow;
            if (Input.GetKeyDown(KeyCode.DownArrow)) return KeyCode.DownArrow;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) return KeyCode.LeftArrow;
            if (Input.GetKeyDown(KeyCode.RightArrow)) return KeyCode.RightArrow;
            if (Input.GetKeyDown(KeyCode.Space)) return KeyCode.Space;

            return KeyCode.None;
        }
    }
}