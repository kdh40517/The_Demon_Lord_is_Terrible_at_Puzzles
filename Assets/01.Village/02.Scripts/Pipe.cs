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
        //private int rotationState = 0;

        void Awake()
        {
            // 컴포넌트 가져오기
            pipeImage = GetComponent<Image>();
            button = GetComponent<Button>();

            // 버튼 이벤트 등록
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPipeClicked);

            // Button 색 변화 대상 지정
            button.targetGraphic = pipeImage;

            // 초기 회전값
            //transform.localRotation = Quaternion.identity;

            // 시작 이미지 설정
            UpdateSprite();
        }

        public void OnPipeClicked()
        {
            // 더블클릭 방지
            if (Time.time - lastClickTime < 0.2f)
                return;

            lastClickTime = Time.time;

            RotatePipe();

            PuzzleManager.instance.CheckWaterFlow();
        }

        public void RotatePipe()
        {
            // 현재 각도에서 무조건 시계방향으로 90도 회전
            transform.Rotate(0, 0, -90);

            // 방향 정보 한 칸씩 밀기 (이건 아주 완벽합니다!)
            bool lastValue = isOpened[3];
            isOpened[3] = isOpened[2];
            isOpened[2] = isOpened[1];
            isOpened[1] = isOpened[0];
            isOpened[0] = lastValue;
        }

        // 물 상태 변경
        public void SetWater(bool fill)
        {
            hasWater = fill;
            UpdateSprite();
        }

        // 스프라이트 갱신
        private void UpdateSprite()
        {
            if (pipeImage == null)
                return;

            pipeImage.sprite = hasWater
                ? waterSprite
                : emptySprite;
        }
    }
}