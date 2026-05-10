using UnityEngine;
using System.Collections.Generic;

namespace DH
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance;

        [Header("보드 설정")]
        public Transform boardPanel;
        public GameObject[] instrumentPrefabs;

        // ★ 유고수 추가: 돌멩이 프리팹을 넣을 전용 가방!
        public GameObject stonePrefab;

        private List<GameObject> normalPrefabs = new List<GameObject>();
        private List<GameObject> poisonPrefabs = new List<GameObject>();

        [Header("확률 설정")]
        [Range(0, 100)]
        public int poisonChance = 15;

        public int width = 6;
        public int height = 6;
        public float cellSize = 100f;
        public float spacing = 5f;

        public Note[,] board;

        void Awake()
        {
            Instance = this;
            SortPrefabs();
        }

        void Start()
        {
            board = new Note[width, height];
            GenerateBoard();
        }

        void SortPrefabs()
        {
            foreach (GameObject prefab in instrumentPrefabs)
            {
                Note noteScript = prefab.GetComponent<Note>();
                if (noteScript != null)
                {
                    if (noteScript.isPoisoned) poisonPrefabs.Add(prefab);
                    else normalPrefabs.Add(prefab);
                }
            }
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
            GameObject newNoteObj = null;
            int dice = Random.Range(1, 101);
            bool isPoisonActive = GameManager.Instance != null && GameManager.Instance.poisonTurnsLeft > 0;

            if (isPoisonActive && dice <= poisonChance && poisonPrefabs.Count > 0)
            {
                int randomIndex = Random.Range(0, poisonPrefabs.Count);
                newNoteObj = Instantiate(poisonPrefabs[randomIndex], boardPanel);
            }
            else if (normalPrefabs.Count > 0)
            {
                int randomIndex = Random.Range(0, normalPrefabs.Count);
                newNoteObj = Instantiate(normalPrefabs[randomIndex], boardPanel);
            }

            if (newNoteObj == null) return;

            Note newNote = newNoteObj.GetComponent<Note>();
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

        // ★ 유고수 추가: 보스가 마법을 쓰면 멀쩡한 조각을 돌로 바꿔버리는 무시무시한 기능!
        public void SpawnStones(int count)
        {
            if (stonePrefab == null) return;

            // 1. 돌이 아닌 멀쩡한 조각들의 목록을 싹 다 모읍니다.
            List<Note> validTargets = new List<Note>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] != null && board[x, y].instrumentType != NoteType.Stone)
                    {
                        validTargets.Add(board[x, y]);
                    }
                }
            }

            // 2. 그 중에서 count(예: 3개)만큼 무작위로 골라서 돌로 바꿔치기합니다!
            for (int i = 0; i < count; i++)
            {
                if (validTargets.Count == 0) break; // 더 이상 바꿀 조각이 없으면 중지

                int randomIndex = Random.Range(0, validTargets.Count);
                Note targetNote = validTargets[randomIndex];
                validTargets.RemoveAt(randomIndex); // 한 번 돌로 바꾼 곳은 빼기

                int tx = targetNote.x;
                int ty = targetNote.y;

                // 원래 있던 몽둥이나 방패를 펑! 없앱니다.
                Destroy(targetNote.gameObject);

                // 그 자리에 무거운 돌멩이를 쿵! 떨어뜨립니다.
                GameObject stoneObj = Instantiate(stonePrefab, boardPanel);
                Note newStone = stoneObj.GetComponent<Note>();
                newStone.x = tx;
                newStone.y = ty;

                float startX = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
                float startY = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;
                float targetX = startX + tx * (cellSize + spacing);
                float targetY = startY + ty * (cellSize + spacing);

                RectTransform rect = stoneObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(targetX, targetY);

                board[tx, ty] = newStone; // 보드판에 돌멩이 등록 완료!
            }
        }
        public void BreakDefenseNotes()
        {
            List<Note> targets = new List<Note>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Note n = board[x, y];
                    // 아직 고장 나지 않은 멀쩡한 방패와 갑옷만 모읍니다.
                    if (n != null && (n.instrumentType == NoteType.Shield || n.instrumentType == NoteType.Armor) && !n.isBroken)
                    {
                        targets.Add(n);
                    }
                }
            }

            int breakCount = Mathf.FloorToInt(targets.Count * 0.66f);

            for (int i = 0; i < breakCount; i++)
            {
                int randomIndex = Random.Range(0, targets.Count);
                Note targetNote = targets[randomIndex];
                targets.RemoveAt(randomIndex);

                // ★ 종류(Type)는 바꾸지 않고, 고장 스티커만 찰칵!
                targetNote.isBroken = true;
                targetNote.GetComponent<UnityEngine.UI.Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f); // 회색으로 변색
            }
        }
    }
}