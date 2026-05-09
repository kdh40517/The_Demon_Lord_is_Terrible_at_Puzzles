using UnityEngine;
using UnityEngine.UI;

namespace SeoAhn
{
    // 로딩씬에서 새가 StartPoint에서 EndPoint까지 날아가고,
    // 세 장의 이미지를 순서대로 바꿔 날갯짓처럼 보이게 하는 스크립트입니다.
    public class BirdFlyController : MonoBehaviour
    {
        [Header("이동 지점")]
        [SerializeField] private RectTransform startPoint; // 새가 출발할 위치
        [SerializeField] private RectTransform endPoint;   // 새가 도착할 위치

        [Header("이동 설정")]
        [SerializeField] private float flySpeed = 250f; // 새가 이동하는 속도
        [SerializeField] private bool loop = true;      // 도착 후 다시 시작할지 여부

        [Header("날갯짓 이미지")]
        [SerializeField] private Image birdImage;       // 새 이미지를 표시하는 Image 컴포넌트
        [SerializeField] private Sprite wingSpriteA;    // 날갯짓 첫 번째 이미지
        [SerializeField] private Sprite wingSpriteB;    // 날갯짓 두 번째 이미지
        [SerializeField] private Sprite wingSpriteC;    // 날갯짓 세 번째 이미지
        [SerializeField] private float wingChangeTime = 0.15f; // 이미지가 바뀌는 간격

        private RectTransform birdRectTransform; // 새 자신의 RectTransform
        private float wingTimer;                 // 날갯짓 시간 체크용
        private int currentWingIndex;            // 현재 표시 중인 날개 이미지 번호

        private void Awake()
        {
            // UI 위치 제어를 위해 RectTransform을 가져옵니다.
            birdRectTransform = GetComponent<RectTransform>();

            // Bird Image를 연결하지 않았을 경우 자기 자신의 Image를 자동으로 가져옵니다.
            if (birdImage == null)
            {
                birdImage = GetComponent<Image>();
            }
        }

        private void Start()
        {
            // 시작할 때 새를 StartPoint 위치로 이동시킵니다.
            ResetToStartPoint();
        }

        private void Update()
        {
            // 매 프레임 새 이동과 날갯짓 이미지를 처리합니다.
            MoveBird();
            AnimateWing();
        }

        private void MoveBird()
        {
            // 시작점이나 도착점이 연결되지 않았다면 이동하지 않습니다.
            if (startPoint == null || endPoint == null)
            {
                return;
            }

            // 현재 위치에서 EndPoint까지 이동합니다.
            birdRectTransform.anchoredPosition = Vector2.MoveTowards(
                birdRectTransform.anchoredPosition,
                endPoint.anchoredPosition,
                flySpeed * Time.deltaTime
            );

            // EndPoint에 도착했는지 확인합니다.
            if (Vector2.Distance(birdRectTransform.anchoredPosition, endPoint.anchoredPosition) <= 0.1f)
            {
                if (loop)
                {
                    ResetToStartPoint();
                }
            }
        }

        private void AnimateWing()
        {
            // 이미지 컴포넌트나 스프라이트가 없다면 날갯짓을 하지 않습니다.
            if (birdImage == null || wingSpriteA == null || wingSpriteB == null || wingSpriteC == null)
            {
                return;
            }

            wingTimer += Time.deltaTime;

            // 일정 시간이 지나면 다음 날개 이미지로 변경합니다.
            if (wingTimer >= wingChangeTime)
            {
                wingTimer = 0f;
                currentWingIndex++;

                if (currentWingIndex > 2)
                {
                    currentWingIndex = 0;
                }

                ApplyWingSprite();
            }
        }

        private void ApplyWingSprite()
        {
            // 현재 번호에 맞는 스프라이트를 표시합니다.
            if (currentWingIndex == 0)
            {
                birdImage.sprite = wingSpriteA;
            }
            else if (currentWingIndex == 1)
            {
                birdImage.sprite = wingSpriteB;
            }
            else if (currentWingIndex == 2)
            {
                birdImage.sprite = wingSpriteC;
            }
        }

        private void ResetToStartPoint()
        {
            // StartPoint가 연결되어 있으면 새 위치를 시작점으로 되돌립니다.
            if (startPoint == null)
            {
                return;
            }

            birdRectTransform.anchoredPosition = startPoint.anchoredPosition;

            // 시작할 때 첫 번째 날개 이미지로 맞춥니다.
            currentWingIndex = 0;
            ApplyWingSprite();
        }
    }
}