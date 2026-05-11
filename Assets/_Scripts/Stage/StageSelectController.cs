using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    // 스테이지 선택 화면을 회전목마 방식으로 관리하는 스크립트입니다.
    // 좌/우 방향키로 카드가 순환 이동하고, Space 키로 선택한 스테이지에 입장합니다.
    public class StageSelectController : MonoBehaviour
    {
        [Header("스테이지 카드 RectTransform")]
        [SerializeField] private RectTransform villageCard;
        [SerializeField] private RectTransform forestCard;
        [SerializeField] private RectTransform castleCard;
        [SerializeField] private RectTransform devillCard;

        [Header("스테이지 카드 CanvasGroup")]
        [SerializeField] private CanvasGroup villageCanvasGroup;
        [SerializeField] private CanvasGroup forestCanvasGroup;
        [SerializeField] private CanvasGroup castleCanvasGroup;
        [SerializeField] private CanvasGroup devillCanvasGroup;

        [Header("스테이지 카드 이미지")]
        [SerializeField] private Image villageCardImage;
        [SerializeField] private Image forestCardImage;
        [SerializeField] private Image castleCardImage;
        [SerializeField] private Image devillCardImage;

        [Header("스테이지 카드 선택 효과")]
        [SerializeField] private StageCardSelectEffect villageEffect;
        [SerializeField] private StageCardSelectEffect forestEffect;
        [SerializeField] private StageCardSelectEffect castleEffect;
        [SerializeField] private StageCardSelectEffect devillEffect;

        [Header("흑백 Material")]
        [SerializeField] private Material grayscaleMaterial;

        [Header("잠금 안내 메시지")]
        [SerializeField] private GameObject lockedMessagePanel;
        [SerializeField] private CanvasGroup lockedMessageCanvasGroup;
        [SerializeField] private TMP_Text lockedMessageText;
        [SerializeField] private float lockedMessageFadeTime = 0.25f;
        [SerializeField] private float lockedMessageStayTime = 1.1f;

        [Header("선택 효과음")]
        [SerializeField] private AudioSource selectAudioSource; // Space로 스테이지 선택 시 재생할 AudioSource
        [SerializeField] private AudioClip selectClip; // 선택 효과음 파일
        [SerializeField] private float selectVolume = 0.5f; // 선택 효과음 볼륨
        [SerializeField] private float sceneLoadDelay = 0.25f; // 효과음이 들릴 시간을 주고 씬 이동

        [Header("이동할 씬 이름")]
        [SerializeField] private string villageSceneName = "01_VillageScene";
        [SerializeField] private string forestSceneName = "02_ForestScene";
        [SerializeField] private string castleSceneName = "03_CastleScene";
        [SerializeField] private string devillSceneName = "04_DevilScene";

        [Header("스테이지 잠금 상태")]
        [SerializeField] private bool villageUnlocked = true;
        [SerializeField] private bool forestUnlocked = false;
        [SerializeField] private bool castleUnlocked = false;
        [SerializeField] private bool devillUnlocked = false;

        [Header("회전목마 위치 설정")]
        [SerializeField] private Vector2 centerPosition = new Vector2(0f, -20f);
        [SerializeField] private Vector2 leftPosition = new Vector2(-560f, -60f);
        [SerializeField] private Vector2 rightPosition = new Vector2(560f, -60f);
        [SerializeField] private Vector2 backPosition = new Vector2(0f, -60f);

        [Header("회전목마 크기 설정")]
        [SerializeField] private float centerScale = 1.25f;
        [SerializeField] private float sideScale = 0.75f;
        [SerializeField] private float backScale = 0.1f;

        [Header("회전목마 투명도 설정")]
        [SerializeField] private float centerAlpha = 1f;
        [SerializeField] private float sideAlpha = 0.85f;
        [SerializeField] private float backAlpha = 0f;
        [SerializeField] private float lockedAlphaMultiplier = 0.55f;

        [Header("회전목마 이동 속도")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float scaleSpeed = 8f;
        [SerializeField] private float alphaSpeed = 8f;

        [Header("배경음")]
        [SerializeField] private AudioSource bgmAudioSource; // 스테이지 선택 화면 배경음

        private int currentIndex;
        private bool isMoving;
        private bool isEnteringStage;
        private Coroutine lockedMessageCoroutine;

        private RectTransform[] cardRects;
        private CanvasGroup[] cardCanvasGroups;
        private Image[] cardImages;
        private StageCardSelectEffect[] cardEffects;
        private bool[] unlockedStates;
        private string[] sceneNames;

        private Vector2[] targetPositions;
        private Vector3[] targetScales;
        private float[] targetAlphas;
        private bool[] shouldHideImmediately;

        private void Awake()
        {
            cardRects = new RectTransform[] { villageCard, forestCard, castleCard, devillCard };
            cardCanvasGroups = new CanvasGroup[] { villageCanvasGroup, forestCanvasGroup, castleCanvasGroup, devillCanvasGroup };
            cardImages = new Image[] { villageCardImage, forestCardImage, castleCardImage, devillCardImage };
            cardEffects = new StageCardSelectEffect[] { villageEffect, forestEffect, castleEffect, devillEffect };
            sceneNames = new string[] { villageSceneName, forestSceneName, castleSceneName, devillSceneName };

            targetPositions = new Vector2[4];
            targetScales = new Vector3[4];
            targetAlphas = new float[4];
            shouldHideImmediately = new bool[4];

            if (lockedMessagePanel != null && lockedMessageCanvasGroup == null)
            {
                lockedMessageCanvasGroup = lockedMessagePanel.GetComponent<CanvasGroup>();

                if (lockedMessageCanvasGroup == null)
                {
                    lockedMessageCanvasGroup = lockedMessagePanel.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            currentIndex = 0;
            isEnteringStage = false;

            RefreshUnlockedStates();

            if (lockedMessagePanel != null)
            {
                lockedMessagePanel.SetActive(false);
            }

            if (lockedMessageCanvasGroup != null)
            {
                lockedMessageCanvasGroup.alpha = 0f;
            }

            UpdateCardVisuals();
            CalculateCarouselTargets();
            ApplyTargetsInstantly();
            BringSelectedCardToFront();
            UpdateSelectionEffects();
        }

        private void Update()
        {
            if (isEnteringStage)
            {
                return;
            }

            HandleMoveInput();
            HandleSelectInput();
            MoveCardsSmoothly();
        }

        private void HandleMoveInput()
        {
            if (isMoving)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveRight();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLeft();
            }
        }

        private void HandleSelectInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EnterSelectedStage();
            }
        }

        private void MoveRight()
        {
            int nextIndex = currentIndex + 1;

            if (nextIndex > 3)
            {
                nextIndex = 0;
            }

            TryChangeSelection(nextIndex);
        }

        private void MoveLeft()
        {
            int previousIndex = currentIndex - 1;

            if (previousIndex < 0)
            {
                previousIndex = 3;
            }

            TryChangeSelection(previousIndex);
        }

        private void TryChangeSelection(int targetIndex)
        {
            if (targetIndex == currentIndex)
            {
                return;
            }

            if (!IsStageUnlocked(targetIndex))
            {
                ShowLockedMessage();
                return;
            }

            currentIndex = targetIndex;

            CalculateCarouselTargets();
            BringSelectedCardToFront();
            HideBackCardsImmediately();
            UpdateSelectionEffects();

            isMoving = true;
        }

        private void EnterSelectedStage()
        {
            if (!IsStageUnlocked(currentIndex))
            {
                ShowLockedMessage();
                return;
            }

            StartCoroutine(EnterSelectedStageRoutine());
        }

        private IEnumerator EnterSelectedStageRoutine()
        {
            // 스테이지 입장 중에는 추가 입력을 막습니다.
            isEnteringStage = true;

            // 배경음을 끕니다.
            if (bgmAudioSource != null)
            {
                bgmAudioSource.Stop();
            }

            // 선택 효과음만 재생합니다.
            PlaySelectSound();

            // 효과음이 들릴 시간을 줍니다.
            yield return new WaitForSeconds(sceneLoadDelay);

            // 로딩씬이 불러올 실제 목적지 씬 이름을 저장합니다.
            PlayerPrefs.SetString("NextSceneName", sceneNames[currentIndex]);
            PlayerPrefs.Save();

            // 로딩씬으로 이동합니다.
            SceneManager.LoadScene("99_LoadingScene");
        }

        private void PlaySelectSound()
        {
            if (selectAudioSource == null || selectClip == null)
            {
                return;
            }

            selectAudioSource.PlayOneShot(selectClip, selectVolume);
        }

        private void RefreshUnlockedStates()
        {
            unlockedStates = new bool[] { villageUnlocked, forestUnlocked, castleUnlocked, devillUnlocked };
        }

        private bool IsStageUnlocked(int index)
        {
            if (unlockedStates == null || index < 0 || index >= unlockedStates.Length)
            {
                return false;
            }

            return unlockedStates[index];
        }

        private void UpdateCardVisuals()
        {
            for (int i = 0; i < cardImages.Length; i++)
            {
                if (cardImages[i] == null)
                {
                    continue;
                }

                cardImages[i].material = IsStageUnlocked(i) ? null : grayscaleMaterial;
            }
        }

        private void CalculateCarouselTargets()
        {
            for (int i = 0; i < cardRects.Length; i++)
            {
                shouldHideImmediately[i] = false;

                int relativeIndex = GetRelativeIndex(i);

                if (relativeIndex == 0)
                {
                    SetTarget(i, centerPosition, centerScale, centerAlpha, false);
                }
                else if (relativeIndex == -1)
                {
                    SetTarget(i, leftPosition, sideScale, sideAlpha, false);
                }
                else if (relativeIndex == 1)
                {
                    SetTarget(i, rightPosition, sideScale, sideAlpha, false);
                }
                else
                {
                    SetTarget(i, backPosition, backScale, backAlpha, true);
                }
            }
        }

        private int GetRelativeIndex(int cardIndex)
        {
            int relative = cardIndex - currentIndex;

            if (relative > 2)
            {
                relative -= 4;
            }

            if (relative < -2)
            {
                relative += 4;
            }

            return relative;
        }

        private void SetTarget(int cardIndex, Vector2 position, float scale, float alpha, bool hideImmediately)
        {
            if (!IsStageUnlocked(cardIndex))
            {
                alpha *= lockedAlphaMultiplier;
            }

            targetPositions[cardIndex] = position;
            targetScales[cardIndex] = Vector3.one * scale;
            targetAlphas[cardIndex] = alpha;
            shouldHideImmediately[cardIndex] = hideImmediately;
        }

        private void BringSelectedCardToFront()
        {
            if (cardRects == null)
            {
                return;
            }

            int leftIndex = GetWrappedIndex(currentIndex - 1);
            int rightIndex = GetWrappedIndex(currentIndex + 1);
            int backIndex = GetWrappedIndex(currentIndex + 2);

            if (cardRects[backIndex] != null)
            {
                cardRects[backIndex].SetAsFirstSibling();
            }

            if (cardRects[leftIndex] != null)
            {
                cardRects[leftIndex].SetAsLastSibling();
            }

            if (cardRects[rightIndex] != null)
            {
                cardRects[rightIndex].SetAsLastSibling();
            }

            if (cardRects[currentIndex] != null)
            {
                cardRects[currentIndex].SetAsLastSibling();
            }
        }

        private int GetWrappedIndex(int index)
        {
            if (index < 0)
            {
                return 3;
            }

            if (index > 3)
            {
                return 0;
            }

            return index;
        }

        private void HideBackCardsImmediately()
        {
            for (int i = 0; i < cardCanvasGroups.Length; i++)
            {
                if (cardCanvasGroups[i] == null)
                {
                    continue;
                }

                if (shouldHideImmediately[i])
                {
                    cardCanvasGroups[i].alpha = 0f;
                }
            }
        }

        private void ApplyTargetsInstantly()
        {
            for (int i = 0; i < cardRects.Length; i++)
            {
                if (cardRects[i] == null)
                {
                    continue;
                }

                cardRects[i].anchoredPosition = targetPositions[i];
                cardRects[i].localScale = targetScales[i];
                cardRects[i].localRotation = Quaternion.identity;

                SetCardAlpha(i, targetAlphas[i]);
            }
        }

        private void MoveCardsSmoothly()
        {
            if (!isMoving)
            {
                return;
            }

            bool allArrived = true;

            for (int i = 0; i < cardRects.Length; i++)
            {
                if (cardRects[i] == null)
                {
                    continue;
                }

                cardRects[i].anchoredPosition = Vector2.Lerp(cardRects[i].anchoredPosition, targetPositions[i], moveSpeed * Time.deltaTime);
                cardRects[i].localScale = Vector3.Lerp(cardRects[i].localScale, targetScales[i], scaleSpeed * Time.deltaTime);
                cardRects[i].localRotation = Quaternion.identity;

                if (shouldHideImmediately[i])
                {
                    SetCardAlpha(i, 0f);
                }
                else
                {
                    float nextAlpha = Mathf.Lerp(GetCardAlpha(i), targetAlphas[i], alphaSpeed * Time.deltaTime);
                    SetCardAlpha(i, nextAlpha);
                }

                if (Vector2.Distance(cardRects[i].anchoredPosition, targetPositions[i]) > 0.5f)
                {
                    allArrived = false;
                }
            }

            if (allArrived)
            {
                ApplyTargetsInstantly();
                BringSelectedCardToFront();
                isMoving = false;
            }
        }

        private void SetCardAlpha(int index, float alpha)
        {
            if (cardCanvasGroups[index] == null)
            {
                return;
            }

            cardCanvasGroups[index].alpha = alpha;
        }

        private float GetCardAlpha(int index)
        {
            if (cardCanvasGroups[index] == null)
            {
                return 1f;
            }

            return cardCanvasGroups[index].alpha;
        }

        private void UpdateSelectionEffects()
        {
            for (int i = 0; i < cardEffects.Length; i++)
            {
                if (cardEffects[i] == null)
                {
                    continue;
                }

                cardEffects[i].SetSelected(i == currentIndex);
            }
        }

        private void ShowLockedMessage()
        {
            if (lockedMessagePanel == null || lockedMessageText == null || lockedMessageCanvasGroup == null)
            {
                return;
            }

            if (lockedMessageCoroutine != null)
            {
                StopCoroutine(lockedMessageCoroutine);
            }

            lockedMessageCoroutine = StartCoroutine(ShowLockedMessageRoutine());
        }

        private IEnumerator ShowLockedMessageRoutine()
        {
            lockedMessageText.text = "아직 스테이지가 열리지 않았어요!";
            lockedMessagePanel.SetActive(true);

            yield return StartCoroutine(FadeLockedMessage(0f, 1f));

            yield return new WaitForSeconds(lockedMessageStayTime);

            yield return StartCoroutine(FadeLockedMessage(1f, 0f));

            lockedMessagePanel.SetActive(false);
            lockedMessageCoroutine = null;
        }

        private IEnumerator FadeLockedMessage(float startAlpha, float endAlpha)
        {
            float timer = 0f;

            while (timer < lockedMessageFadeTime)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / lockedMessageFadeTime);

                if (lockedMessageCanvasGroup != null)
                {
                    lockedMessageCanvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (lockedMessageCanvasGroup != null)
            {
                lockedMessageCanvasGroup.alpha = endAlpha;
            }
        }
    }
}