using UnityEngine;
using UnityEngine.UI;

namespace TM
{
    // 핵심 1: 오브젝트에 이 스크립트를 넣으면 Image와 Button 컴포넌트가 알아서 찰떡같이 붙습니다.
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class Pipe : MonoBehaviour
    {
        // 핵심 2: 파이프가 어느 방향으로 뚫려있는지 기억하는 수첩입니다. [0]북, [1]동, [2]남, [3]서 순서입니다.
        [Header("파이프 설정 (북, 동, 남, 서)")]
        public bool[] isOpened = new bool[4];

        [Header("파이프 이미지")]
        public Sprite emptySprite;
        public Sprite waterSprite;

        private Image pipeImage;
        private Button button;

        // 물이 채워져 있는지 상태를 저장합니다.
        public bool hasWater = false;

        // 마우스 광클 방지를 위해 마지막으로 누른 시간을 기록해둡니다.
        private float lastClickTime = 0f;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip rotateSound;

        // 인스펙터 창에서 슬라이더로 소리 길이를 쉽게 조절할 수 있게 해줍니다.
        [Range(0.1f, 2.0f)]
        public float soundDuration = 0.5f;

        void Awake()
        {
            pipeImage = GetComponent<Image>();
            button = GetComponent<Button>();

            // 핵심 3: 유니티 에디터에서 일일이 연결할 필요 없이, 코드로 '버튼 누르면 OnPipeClicked 실행해!'라고 명령해둡니다.
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPipeClicked);

            button.targetGraphic = pipeImage;

            UpdateSprite();
        }

        // 파이프를 클릭했을 때 벌어지는 일들입니다.
        public void OnPipeClicked()
        {
            // 이미 퍼즐을 풀었다면 파이프가 더 이상 안 돌아가게 막습니다.
            if (PuzzleManager.instance.isCleared)
                return;

            // 0.2초 안에는 다시 클릭할 수 없도록 광클을 튕겨냅니다.
            if (Time.time - lastClickTime < 0.2f)
                return;

            lastClickTime = Time.time;

            RotatePipe(); // 1. 파이프를 돌리고
            PlayRotateSound(); // 2. 소리를 내고

            // 핵심 4: 파이프가 돌아갔으니 전체 물길이 변했겠죠? 매니저에게 물이 통하는지 다시 검사하라고 시킵니다.
            PuzzleManager.instance.CheckWaterFlow();
        }

        // 핵심 5: 파이프 회전의 비밀
        public void RotatePipe()
        {
            // 눈에 보이는 이미지를 시계방향(-90도)으로 휙 돌립니다.
            transform.Rotate(0, 0, -90);

            // 이미지가 시계방향으로 돌았으니, 뚫려있는 방향(isOpened) 데이터도 오른쪽으로 한 칸씩 밀어서 업데이트합니다.
            bool lastValue = isOpened[3];
            isOpened[3] = isOpened[2];
            isOpened[2] = isOpened[1];
            isOpened[1] = isOpened[0];
            isOpened[0] = lastValue;
        }

        // 매니저가 "물 들어왔어!"(true) 혹은 "물 빠졌어!"(false)라고 알려주면 실행됩니다.
        public void SetWater(bool fill)
        {
            hasWater = fill;
            UpdateSprite();
        }

        // [추가된 부분] 매니저가 파이프의 모양(일자, ㄱ자, T자)을 강제로 지정할 때 사용합니다.
        public void SetPipeType(bool n, bool e, bool s, bool w, Sprite newEmptySprite, Sprite newWaterSprite)
        {
            isOpened[0] = n;
            isOpened[1] = e;
            isOpened[2] = s;
            isOpened[3] = w;

            emptySprite = newEmptySprite;
            waterSprite = newWaterSprite;

            // [핵심 추가] 파이프 종류가 바뀔 때, 눈에 보이는 이미지의 회전 각도도 무조건 기본 상태(0도)로 초기화합니다!
            transform.rotation = Quaternion.identity;

            // 모양이 완전히 바뀌었으니 초기 상태(물이 없는 상태)로 리셋합니다.
            SetWater(false);
        }

        private void UpdateSprite()
        {
            if (pipeImage == null)
                return;

            // 물이 있으면 waterSprite, 없으면 emptySprite로 이미지를 교체합니다.
            pipeImage.sprite = hasWater ? waterSprite : emptySprite;
        }

        // 핵심 6: 깔끔한 효과음 재생 로직
        private void PlayRotateSound()
        {
            if (audioSource != null && rotateSound != null)
            {
                // 1. 다다닥 눌렀을 때 소리가 꼬이지 않도록 이전에 예약된 '소리 끄기' 명령을 취소합니다.
                CancelInvoke("StopSound");

                // 2. 나고 있던 소리가 있다면 뚝 끊고 처음부터 경쾌하게 다시 재생합니다.
                audioSource.Stop();
                audioSource.PlayOneShot(rotateSound);

                // 3. 우리가 정해둔 시간(soundDuration)이 지나면 소리를 끄라고(StopSound) 예약을 걸어둡니다.
                Invoke("StopSound", soundDuration);
            }
        }

        // 예약된 시간이 되면 얄짤없이 소리를 강제 종료합니다. (너무 긴 효과음 잘라내기 용도)
        private void StopSound()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }
}