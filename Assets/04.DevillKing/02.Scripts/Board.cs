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

        public GameObject stonePrefab;

        private List<GameObject> normalPrefabs = new List<GameObject>();

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
                    normalPrefabs.Add(prefab); // 독 검사 없이 무조건 기본 조각으로 쏙!
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

            // 독 확률 계산 없이 바로 기본 조각 소환!
            if (normalPrefabs.Count > 0)
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

        public void SpawnStones(int count)
        {
            if (stonePrefab == null) return;

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

            for (int i = 0; i < count; i++)
            {
                if (validTargets.Count == 0) break;

                int randomIndex = Random.Range(0, validTargets.Count);
                Note targetNote = validTargets[randomIndex];
                validTargets.RemoveAt(randomIndex);

                int tx = targetNote.x;
                int ty = targetNote.y;

                Destroy(targetNote.gameObject);

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

                board[tx, ty] = newStone;
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
                    if (n != null && (n.instrumentType == NoteType.Shield || n.instrumentType == NoteType.Armor) && !n.isBroken)
                    {
                        targets.Add(n);
                    }
                }
            }

            int breakCount = Mathf.FloorToInt(targets.Count * 0.66f);
            Debug.Log($"🔨 방패 부수기! {breakCount}개를 박살냅니다.");

            for (int i = 0; i < breakCount; i++)
            {
                int randomIndex = Random.Range(0, targets.Count);
                Note targetNote = targets[randomIndex];
                targets.RemoveAt(randomIndex);

                targetNote.isBroken = true;
            }
        }
    }
}