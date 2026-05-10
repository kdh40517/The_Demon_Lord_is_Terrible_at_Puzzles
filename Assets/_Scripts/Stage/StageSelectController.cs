using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    // StageScene에서 일자로 배치된 스테이지 카드들을 좌우 방향키로 이동하고,
    // 잠금 상태에 따라 선택 가능 여부를 관리하는 스크립트입니다.
    public class StageSelectController : MonoBehaviour
    {
        [Header("카드 그룹")]
        [SerializeField] private RectTransform stageCardGroup; // 카드 전체를 담고 있는 부모 오브젝트

        [Header("스테이지 카드")]
        [SerializeField] private GameObject villageCard; // Village 카드
        [SerializeField] private GameObject forestCard;  // Forest 카드
        [SerializeField] private GameObject castleCard;  // Castle 카드
        [SerializeField] private GameObject devilCard;   // Devil 카드

        [Header("선택 표시")]
        [SerializeField] private GameObject villageSelectFrame; // Village 선택 테두리
        [SerializeField] private GameObject forestSelectFrame;  // Forest 선택 테두리
        [SerializeField] private GameObject castleSelectFrame;  // Castle 선택 테두리
        [SerializeField] private GameObject devilSelectFrame;   // Devil 선택 테두리

        [Header("카드 투명도")]
        [SerializeField] private CanvasGroup villageCanvasGroup; // Village 카드 투명도 조절
        [SerializeField] private CanvasGroup forestCanvasGroup;  // Forest 카드 투명도 조절
        [SerializeField] private CanvasGroup castleCanvasGroup;  // Castle 카드 투명도 조절
        [SerializeField] private CanvasGroup devilCanvasGroup;   // Devil 카드 투명도 조절

        [Header("씬 이름")]
        [SerializeField] private string villageSceneName = "01_VillageScene"; // Village 입장 씬
        [SerializeField] private string forestSceneName = "02_ForestScene";   // Forest 입장 씬
        [SerializeField] private string castleSceneName = "03_CastleScene";   // Castle 입장 씬
        [SerializeField] private string devilSceneName = "04_DevilScene";     // Devil 입장 씬

        [Header("카드 이동 설정")]
        [SerializeField] private float cardSpacing = 600f;       // 카드 사이 간격
        [SerializeField] private float slideSpeed = 8f;          // 카드 그룹이 이동하는 속도
        [SerializeField] private float lockedAlpha = 0.35f;      // 잠긴 카드 투명도
        [SerializeField] private float unlockedAlpha = 1f;       // 열린 카드 투명도

        [Header("스테이지 잠금 상태")]
        [SerializeField] private bool villageUnlocked = true; // 처음에는 Village만 열림
        [SerializeField] private bool forestUnlocked = false;
        [SerializeField] private bool castleUnlocked = false;
        [SerializeField] private bool devilUnlocked = false;

        private int currentIndex;           // 현재 화면 중앙에 있는 카드 번호
        private Vector2 targetGroupPosition; // 카드 그룹이 이동해야 할 목표 위치
        private bool isMoving;              // 카드 그룹이 이동 중인지 여부

        private void Start()
        {
            // 처음에는 Village 카드부터 보여줍니다.
            currentIndex = 0;

            // 카드 잠금 상태에 맞게 투명도를 적용합니다.
            UpdateCardLockVisuals();

            // 선택 표시를 갱신합니다.
            UpdateSelectFrames();

            // 카드 그룹 위치를 Village 기준으로 맞춥니다.
            MoveGroupInstantly();
        }

        private void Update()
        {
            HandleMoveInput();
            HandleSelectInput();
            MoveCardGroupSmoothly();
        }

        private void HandleMoveInput()
        {
            // 카드가 이동 중이면 추가 입력을 잠시 막습니다.
            if (isMoving)
            {
                return;
            }

            // 오른쪽 방향키: 다음 카드로 이동합니다.
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveToNextCard();
            }

            // 왼쪽 방향키: 이전 카드로 이동합니다.
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveToPreviousCard();
            }
        }

        private void HandleSelectInput()
        {
            // Space 키를 누르면 현재 카드가 열려 있을 때만 해당 씬으로 이동합니다.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EnterSelectedStage();
            }
        }

        private void MoveToNextCard()
        {
            // 마지막 카드보다 오른쪽으로는 이동하지 않습니다.
            if (currentIndex >= 3)
            {
                return;
            }

            currentIndex++;
            UpdateSelectFrames();
            SetTargetGroupPosition();
        }

        private void MoveToPreviousCard()
        {
            // 첫 번째 카드보다 왼쪽으로는 이동하지 않습니다.
            if (currentIndex <= 0)
            {
                return;
            }

            currentIndex--;
            UpdateSelectFrames();
            SetTargetGroupPosition();
        }

        private void SetTargetGroupPosition()
        {
            // currentIndex가 0이면 X = 0,
            // 1이면 X = -cardSpacing,
            // 2이면 X = -cardSpacing * 2,
            // 이런 방식으로 카드 그룹을 왼쪽으로 이동시켜 현재 카드를 중앙에 보이게 합니다.
            targetGroupPosition = new Vector2(-cardSpacing * currentIndex, stageCardGroup.anchoredPosition.y);
            isMoving = true;
        }

        private void MoveGroupInstantly()
        {
            // 시작할 때는 부드러운 이동 없이 바로 위치를 맞춥니다.
            if (stageCardGroup == null)
            {
                return;
            }

            targetGroupPosition = new Vector2(-cardSpacing * currentIndex, stageCardGroup.anchoredPosition.y);
            stageCardGroup.anchoredPosition = targetGroupPosition;
        }

        private void MoveCardGroupSmoothly()
        {
            // 카드 그룹이 없거나 이동 중이 아니면 처리하지 않습니다.
            if (stageCardGroup == null || !isMoving)
            {
                return;
            }

            // 목표 위치까지 부드럽게 이동합니다.
            stageCardGroup.anchoredPosition = Vector2.Lerp(
                stageCardGroup.anchoredPosition,
                targetGroupPosition,
                slideSpeed * Time.deltaTime
            );

            // 거의 도착하면 정확한 위치로 고정합니다.
            if (Vector2.Distance(stageCardGroup.anchoredPosition, targetGroupPosition) < 0.5f)
            {
                stageCardGroup.anchoredPosition = targetGroupPosition;
                isMoving = false;
            }
        }

        private void UpdateCardLockVisuals()
        {
            // 열린 카드는 선명하게, 잠긴 카드는 회색처럼 반투명하게 보이도록 합니다.
            SetCardAlpha(villageCanvasGroup, villageUnlocked);
            SetCardAlpha(forestCanvasGroup, forestUnlocked);
            SetCardAlpha(castleCanvasGroup, castleUnlocked);
            SetCardAlpha(devilCanvasGroup, devilUnlocked);
        }

        private void SetCardAlpha(CanvasGroup canvasGroup, bool isUnlocked)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = isUnlocked ? unlockedAlpha : lockedAlpha;
        }

        private void UpdateSelectFrames()
        {
            // 현재 선택된 카드에만 선택 표시를 켭니다.
            // 단, 잠긴 카드는 선택 표시가 켜지지 않게 합니다.
            SetSelectFrame(villageSelectFrame, currentIndex == 0 && villageUnlocked);
            SetSelectFrame(forestSelectFrame, currentIndex == 1 && forestUnlocked);
            SetSelectFrame(castleSelectFrame, currentIndex == 2 && castleUnlocked);
            SetSelectFrame(devilSelectFrame, currentIndex == 3 && devilUnlocked);
        }

        private void SetSelectFrame(GameObject selectFrame, bool active)
        {
            if (selectFrame == null)
            {
                return;
            }

            selectFrame.SetActive(active);
        }

        private void EnterSelectedStage()
        {
            // 현재 카드가 잠겨 있으면 입장하지 않습니다.
            if (!IsCurrentStageUnlocked())
            {
                Debug.Log("아직 열리지 않은 스테이지입니다.");
                return;
            }

            // 열린 카드라면 해당 씬으로 이동합니다.
            if (currentIndex == 0)
            {
                SceneManager.LoadScene(villageSceneName);
            }
            else if (currentIndex == 1)
            {
                SceneManager.LoadScene(forestSceneName);
            }
            else if (currentIndex == 2)
            {
                SceneManager.LoadScene(castleSceneName);
            }
            else if (currentIndex == 3)
            {
                SceneManager.LoadScene(devilSceneName);
            }
        }

        private bool IsCurrentStageUnlocked()
        {
            // 현재 선택된 스테이지가 열려 있는지 확인합니다.
            if (currentIndex == 0)
            {
                return villageUnlocked;
            }

            if (currentIndex == 1)
            {
                return forestUnlocked;
            }

            if (currentIndex == 2)
            {
                return castleUnlocked;
            }

            if (currentIndex == 3)
            {
                return devilUnlocked;
            }

            return false;
        }
    }
}