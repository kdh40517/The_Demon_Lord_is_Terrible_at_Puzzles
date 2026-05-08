using UnityEngine;

namespace DH
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance;

        [Header("보드 설정")]
        public Transform boardPanel;
        public GameObject[] instrumentPrefabs;
        public int width = 6;
        public int height = 6;
        public float cellSize = 100f;
        public float spacing = 5f;

        public Note[,] board;

        void Awake() { Instance = this; }

        void Start()
        {
            board = new Note[width, height];
            GenerateBoard();
        }

        public void GenerateBoard()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SpawnNote(x, y, 0f);
                }
            }
        }

        public void SpawnNote(int x, int y, float dropOffset)
        {
            // 👇 유고수가 추가한 쉬운 확률 계산법 (주사위 굴리기!)
            int dice = Random.Range(1, 101); // 1부터 100까지 중에 하나를 뽑습니다.
            int randomIndex = 0;

            // 몽둥이(0번)가 나올 확률을 15%로 설정해 볼게요. (숫자는 원하시는 대로 더하거나 빼셔도 됩니다!)
            if (dice <= 10)
            {
                randomIndex = 0; // 1부터 15 사이의 숫자가 나오면 몽둥이 당첨!
            }
            else
            {
                // 16부터 100이 나오면, 몽둥이를 뺀 나머지 조각(1번부터 끝까지) 중에서만 뽑습니다.
                randomIndex = Random.Range(1, instrumentPrefabs.Length);
            }

            GameObject newNoteObj = Instantiate(instrumentPrefabs[randomIndex], boardPanel);
            Note newNote = newNoteObj.GetComponent<Note>();

            newNote.instrumentType = randomIndex;
            newNote.x = x;
            newNote.y = y;

            float startX = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startY = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;

            float targetX = startX + x * (cellSize + spacing);
            float targetY = startY + y * (cellSize + spacing);

            RectTransform rect = newNoteObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(targetX, targetY + dropOffset);

            board[x, y] = newNote;
        }
    }
}