using UnityEngine;

namespace LunasisGame
{
    public class TitleLogoDrop : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;

        [Header("위치 설정")]
        [SerializeField] private float startY = 800f;
        [SerializeField] private float targetY = 220f;

        [Header("내려오는 설정")]
        [SerializeField] private float dropSpeed = 3000f;

        [Header("도착 후 튕김 설정")]
        [SerializeField] private float bounceHeight = 25f;
        [SerializeField] private float bounceDuration = 0.15f;
        [SerializeField] private int bounceCount = 3;

        private int currentBounceCount;
        private float bounceTimer;
        private bool isDropping = true;
        private bool isBouncingUp;
        private bool isBouncingDown;

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            Vector2 position = rectTransform.anchoredPosition;
            position.y = startY;
            rectTransform.anchoredPosition = position;
        }

        private void Update()
        {
            if (isDropping)
            {
                DropLogo();
                return;
            }

            if (isBouncingUp || isBouncingDown)
            {
                BounceLogo();
            }
        }

        private void DropLogo()
        {
            Vector2 position = rectTransform.anchoredPosition;

            position.y = Mathf.MoveTowards(
                position.y,
                targetY,
                dropSpeed * Time.deltaTime
            );

            rectTransform.anchoredPosition = position;

            if (Mathf.Approximately(position.y, targetY))
            {
                isDropping = false;
                isBouncingUp = true;
                bounceTimer = 0f;
                currentBounceCount = 0;
            }
        }

        private void BounceLogo()
        {
            bounceTimer += Time.deltaTime;

            float progress = bounceTimer / bounceDuration;
            progress = Mathf.Clamp01(progress);

            Vector2 position = rectTransform.anchoredPosition;

            if (isBouncingUp)
            {
                position.y = Mathf.Lerp(targetY, targetY + bounceHeight, progress);

                if (progress >= 1f)
                {
                    isBouncingUp = false;
                    isBouncingDown = true;
                    bounceTimer = 0f;
                }
            }
            else if (isBouncingDown)
            {
                position.y = Mathf.Lerp(targetY + bounceHeight, targetY, progress);

                if (progress >= 1f)
                {
                    currentBounceCount++;

                    if (currentBounceCount >= bounceCount)
                    {
                        isBouncingDown = false;
                        position.y = targetY;
                    }
                    else
                    {
                        isBouncingDown = false;
                        isBouncingUp = true;
                        bounceTimer = 0f;
                    }
                }
            }

            rectTransform.anchoredPosition = position;
        }
    }
}