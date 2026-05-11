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
        public Image devilImage;               // 씬에 있는 Devil UI 이미지 컴포넌트
        public Sprite devilEyesClosed;         // 눈 감은 이미지 (안전)
        public Sprite devilEyesOpen;           // 눈 뜬 이미지 (위험)
        public float penaltyWhenCaught = 30f;  // 눈 떴을 때 누르면 깎이는 뼈아픈 페널티
        [Space]
        public float minCloseTime = 2f;        // 눈 감고 있는 최소 시간
        public float maxCloseTime = 5f;        // 눈 감고 있는 최대 시간
        public float minOpenTime = 1f;         // 눈 뜨고 있는 최소 시간
        public float maxOpenTime = 3f;         // 눈 뜨고 있는 최대 시간

        private bool isDevilWatching = false;  // 현재 악마가 보고 있는지 여부
        private float devilTimer = 0f;         // 악마 상태 전환 타이머

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

            // 악마 기믹 초기화 (처음엔 눈 감은 상태로 시작)
            isDevilWatching = false;
            if (devilImage != null && devilEyesClosed != null)
            {
                devilImage.sprite = devilEyesClosed;
            }
            devilTimer = Random.Range(minCloseTime, maxCloseTime);
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

                // 지정된 방향키나 스페이스바가 눌렸을 때만 반응
                if (pressedKey != KeyCode.None)
                {
                    if (isDevilWatching)
                    {
                        // 🚨 악마가 보고 있을 때 움직임 -> 대참사!
                        currentGauge -= penaltyWhenCaught;
                        currentGauge = Mathf.Max(currentGauge, 0f);
                        UpdateGaugeUI();
                        onWrongInput.Invoke(); // 틀림 효과음 재생용
                        Debug.Log("악마에게 걸렸습니다! 멈춰!");
                    }
                    else
                    {
                        // ✅ 평소대로 퍼즐 풀기
                        ProcessPuzzleInput(pressedKey);
                    }
                }
            }
        }

        // 악마가 무작위 시간마다 눈을 뜨고 감게 하는 함수
        private void UpdateDevilState()
        {
            devilTimer -= Time.deltaTime;

            if (devilTimer <= 0f)
            {
                isDevilWatching = !isDevilWatching; // 상태 반전 (눈 뜸 <-> 눈 감음)

                if (isDevilWatching)
                {
                    // 눈 뜸!
                    if (devilImage != null) devilImage.sprite = devilEyesOpen;
                    devilTimer = Random.Range(minOpenTime, maxOpenTime);
                }
                else
                {
                    // 눈 감음!
                    if (devilImage != null) devilImage.sprite = devilEyesClosed;
                    devilTimer = Random.Range(minCloseTime, maxCloseTime);
                }
            }
        }

        // 정상적으로 퍼즐을 풀 때의 로직
        private void ProcessPuzzleInput(KeyCode pressedKey)
        {
            if (pressedKey == activeSequence[0])
            {
                currentGauge += fillPerCorrect;
                activeSequence.RemoveAt(0);
                onCorrectInput.Invoke();
                AddNewArrow();

                if (currentGauge >= maxGauge)
                {
                    currentGauge = maxGauge;
                    isGameClear = true;
                    Debug.Log("게이지 100% 달성! 퍼즐 클리어!");
                    onGaugeFull.Invoke();
                }
            }
            else
            {
                currentGauge -= penaltyPerWrong;
                currentGauge = Mathf.Max(currentGauge, 0f);
                onWrongInput.Invoke();
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