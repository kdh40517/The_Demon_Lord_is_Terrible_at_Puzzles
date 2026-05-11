using UnityEngine;
using UnityEngine.UI;

namespace SeoAhn
{
    // 스테이지 카드 선택 효과를 담당하는 스크립트입니다.
    // 선택되면:
    // 1. 카드가 살짝 커짐
    // 2. 카드 이미지 + Frame이 같이 반짝임
    public class StageCardSelectEffect : MonoBehaviour
    {
        [Header("카드 이미지")]
        [SerializeField] private Image cardImage; // VillageCard 자체의 Image

        [Header("프레임 이미지")]
        [SerializeField] private Image frameImage; // 자식 Frame 이미지

        [Header("크기 효과")]
        [SerializeField] private float selectedScaleMultiplier = 1.02f; // 선택 시 확대 배율
        [SerializeField] private float scaleSpeed = 8f; // 확대/축소 속도

        [Header("반짝임")]
        [SerializeField] private float glowSpeed = 4f; // 반짝이는 속도

        [Header("카드 이미지 밝기")]
        [SerializeField] private float cardMinAlpha = 0.85f; // 카드 최소 밝기
        [SerializeField] private float cardMaxAlpha = 1f; // 카드 최대 밝기

        [Header("프레임 밝기")]
        [SerializeField] private float frameMinAlpha = 0.35f; // 프레임 최소 밝기
        [SerializeField] private float frameMaxAlpha = 1f; // 프레임 최대 밝기
        [SerializeField] private float normalFrameAlpha = 0.75f; // 선택 안 됐을 때 프레임 밝기

        private bool isSelected; // 현재 선택 여부
        private Vector3 originalScale; // 원래 카드 크기
        private Vector3 targetScale; // 목표 카드 크기

        private Color originalCardColor; // 카드 원래 색상
        private Color originalFrameColor; // 프레임 원래 색상

        private void Awake()
        {
            // 현재 네가 Unity에서 지정한 카드 크기를 저장합니다.
            originalScale = transform.localScale;
            targetScale = originalScale;

            // 카드 이미지가 연결되지 않았으면 자동으로 자기 자신의 Image를 가져옵니다.
            if (cardImage == null)
            {
                cardImage = GetComponent<Image>();
            }

            if (cardImage != null)
            {
                originalCardColor = cardImage.color;
            }

            if (frameImage != null)
            {
                originalFrameColor = frameImage.color;
            }
        }

        private void Start()
        {
            // 시작 시 선택 해제 상태
            SetSelected(false);
        }

        private void Update()
        {
            // 카드 크기를 부드럽게 변경
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                scaleSpeed * Time.deltaTime
            );

            // 선택 중이면 카드 + 프레임 같이 반짝임
            if (isSelected)
            {
                float glow = (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f;

                if (cardImage != null)
                {
                    float alpha = Mathf.Lerp(cardMinAlpha, cardMaxAlpha, glow);

                    Color color = originalCardColor;
                    color.a = alpha;
                    cardImage.color = color;
                }

                if (frameImage != null)
                {
                    float alpha = Mathf.Lerp(frameMinAlpha, frameMaxAlpha, glow);

                    Color color = originalFrameColor;
                    color.a = alpha;
                    frameImage.color = color;
                }
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            // 선택되면 원래 크기 기준으로 살짝 확대
            targetScale = selected
                ? originalScale * selectedScaleMultiplier
                : originalScale;

            // 선택 해제 상태 원복
            if (!selected)
            {
                if (cardImage != null)
                {
                    cardImage.color = originalCardColor;
                }

                if (frameImage != null)
                {
                    Color color = originalFrameColor;
                    color.a = normalFrameAlpha;
                    frameImage.color = color;
                }
            }
        }
    }
}