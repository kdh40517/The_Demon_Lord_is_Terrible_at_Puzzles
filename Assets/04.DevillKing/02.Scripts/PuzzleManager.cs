using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DH
{
    public enum NoteType
    {
        Club,
        Armor,
        Shield,
        Stone,
        Broken
    }

    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance;

        [Header("연결된 노드")]
        public List<Note> connectedNotes = new List<Note>();

        [Header("선 연결 설정")]
        public LineRenderer lineRenderer;
        public LineRenderer dragLineRenderer;

        [Header("마우스 텍스트 설정")]
        public TextMeshProUGUI floatingText;
        public Vector3 textOffset = new Vector3(30f, 30f, 0f);

        [Header("효과음 설정")]
        public AudioSource sfxPlayer;
        public AudioClip[] instrumentSounds;

        private Note lastHoveredNote;
        private bool isDrawing = false;

        void Awake() { Instance = this; }

        public void StartDrawing(Note firstNote)
        {
            if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

            // ★ 유고수 추가: 돌멩이는 터치해서 이을 수 없습니다! 딴딴해요!
            if (firstNote.instrumentType == NoteType.Stone) return;

            connectedNotes.Clear();
            AddNoteToPath(firstNote);
            PlayInstrumentSound((int)firstNote.instrumentType);
            UpdateLine();

            UpdateFloatingText();
            isDrawing = true;
        }

        void Update()
        {
            if (isDrawing && connectedNotes.Count > 0)
            {
                Vector3 startPos = connectedNotes[connectedNotes.Count - 1].transform.position;
                startPos.z -= 1f;

                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z -= 1f;

                dragLineRenderer.positionCount = 2;
                dragLineRenderer.SetPosition(0, startPos);
                dragLineRenderer.SetPosition(1, endPos);

                if (floatingText != null)
                {
                    Vector3 textPos = endPos;
                    textPos.x += 0.8f;
                    textPos.y += 0.8f;
                    textPos.z -= 1f;
                    floatingText.transform.position = textPos;
                }
            }
        }

        public void OnNoteEnter(Note enteredNote)
        {
            if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

            // ★ 유고수 추가: 드래그 하다가 돌멩이를 만나면 연결 안 됨! 막혀버립니다!
            if (enteredNote.instrumentType == NoteType.Stone) return;

            lastHoveredNote = enteredNote;

            if (connectedNotes.Count == 0) return;

            if (connectedNotes.Count >= 2 && connectedNotes[connectedNotes.Count - 2] == enteredNote)
            {
                Note lastNoteToRemove = connectedNotes[connectedNotes.Count - 1];
                CanvasGroup cg = lastNoteToRemove.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;

                connectedNotes.RemoveAt(connectedNotes.Count - 1);
                UpdateLine();
                UpdateFloatingText();
                return;
            }

            if (connectedNotes.Contains(enteredNote)) return;

            Note lastNote = connectedNotes[connectedNotes.Count - 1];

            if (lastNote.instrumentType == enteredNote.instrumentType)
            {
                int distanceX = Mathf.Abs(lastNote.x - enteredNote.x);
                int distanceY = Mathf.Abs(lastNote.y - enteredNote.y);

                if (distanceX <= 1 && distanceY <= 1)
                {
                    AddNoteToPath(enteredNote);
                    PlayInstrumentSound((int)enteredNote.instrumentType);
                    UpdateLine();
                    UpdateFloatingText();
                }
            }
        }

        void UpdateFloatingText()
        {
            if (floatingText == null || connectedNotes.Count == 0) return;

            int validScore = 0;
            int poisonDamage = 0;

            foreach (Note note in connectedNotes)
            {
                if (!note.isBroken) validScore++; // 고장 안 난 조각만 점수로 인정!
                if (note.isPoisoned) poisonDamage++; // 독 조각은 플레이어 HP 데미지로 누적!
            }

            NoteType currentType = connectedNotes[0].instrumentType;
            string statName = "";

            if (currentType == NoteType.Club) statName = "DMG";
            else if (currentType == NoteType.Armor) statName = "HP";
            else if (currentType == NoteType.Shield) statName = "DEF";

            // 1. 기본 점수 텍스트 (예: DMG 5)
            string textOut = $"<color=white>{statName} {validScore}</color>";

            // 2. 독을 밟았다면 그 아래에 빨간 글씨로 덧붙이기 (예: (HP -1))
            if (poisonDamage > 0)
            {
                textOut += $"\n<size=60%><color=red>(HP -{poisonDamage})</color></size>";
            }

            floatingText.text = textOut;
            floatingText.gameObject.SetActive(true);
        }

        void AddNoteToPath(Note note)
        {
            connectedNotes.Add(note);
            CanvasGroup cg = note.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0.5f;
        }

        void UpdateLine()
        {
            lineRenderer.positionCount = connectedNotes.Count;
            for (int i = 0; i < connectedNotes.Count; i++)
            {
                lineRenderer.SetPosition(i, connectedNotes[i].transform.position);
            }
        }

        public void EndDrawing(Note releasedNote)
        {
            isDrawing = false;
            dragLineRenderer.positionCount = 0;

            if (floatingText != null) floatingText.gameObject.SetActive(false);

            if (connectedNotes.Count >= 3 && releasedNote == connectedNotes[connectedNotes.Count - 1])
            {
                int validScore = 0;
                int poisonDamage = 0;

                foreach (Note note in connectedNotes)
                {
                    if (!note.isBroken) validScore++;
                    if (note.isPoisoned) poisonDamage++;
                }

                if (GameManager.Instance != null)
                {
                    // 1. 독 데미지 먼저 입기!
                    if (poisonDamage > 0)
                    {
                        GameManager.Instance.TakePoisonDamage(poisonDamage);
                    }

                    // 2. 유효한 공격 점수가 있다면 종류에 맞게 발동! (★ 이 부분이 수정되었습니다!)
                    if (validScore > 0)
                    {
                        NoteType firstType = connectedNotes[0].instrumentType;

                        if (firstType == NoteType.Club)
                        {
                            GameManager.Instance.AttackBoss(validScore); // 몽둥이: 보스 공격
                        }
                        else if (firstType == NoteType.Armor)
                        {
                            GameManager.Instance.HealPlayer(validScore); // 갑옷: 내 피 회복
                        }
                        else if (firstType == NoteType.Shield)
                        {
                            GameManager.Instance.AddShield(validScore);  // 방패: 방어도 증가
                        }
                    }
                }

                BreakAdjacentStones(connectedNotes);

                foreach (Note note in connectedNotes)
                {
                    BoardManager.Instance.board[note.x, note.y] = null;
                    Destroy(note.gameObject);
                }

                if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
                {
                    GameManager.Instance.NextTurn();
                }

                StartCoroutine(ApplyGravity());
            }
            else
            {
                foreach (Note note in connectedNotes)
                {
                    CanvasGroup cg = note.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;
                }
            }

            connectedNotes.Clear();
            lineRenderer.positionCount = 0;
        }

        // ★ 유고수 추가: 이은 길 주변(8방향)에 돌멩이가 있으면 모조리 부수는 함수!
        void BreakAdjacentStones(List<Note> path)
        {
            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            Note[,] board = BoardManager.Instance.board;

            // ★ 대각선을 뺀 상, 하, 좌, 우 4방향 좌표값만 남깁니다.
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            foreach (Note node in path)
            {
                // 체크 횟수를 8회에서 4회로 변경!
                for (int i = 0; i < 4; i++)
                {
                    int nx = node.x + dx[i];
                    int ny = node.y + dy[i];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        Note target = board[nx, ny];
                        if (target != null && target.instrumentType == NoteType.Stone)
                        {
                            Debug.Log($"💥 쾅! ({nx}, {ny}) 위치의 돌이 직선 폭발로 파괴됨!");
                            Destroy(target.gameObject);
                            board[nx, ny] = null;
                        }
                    }
                }
            }
        }

        IEnumerator ApplyGravity()
        {
            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            float cellSize = BoardManager.Instance.cellSize;
            float spacing = BoardManager.Instance.spacing;
            Note[,] board = BoardManager.Instance.board;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        for (int upperY = y + 1; upperY < height; upperY++)
                        {
                            if (board[x, upperY] != null)
                            {
                                Note fallingNote = board[x, upperY];
                                board[x, y] = fallingNote;
                                board[x, upperY] = null;
                                fallingNote.y = y;

                                StartCoroutine(MoveBlock(fallingNote, x, y, cellSize, spacing, width, height));
                                break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        BoardManager.Instance.SpawnNote(x, y, 800f);
                        StartCoroutine(MoveBlock(board[x, y], x, y, cellSize, spacing, width, height));
                    }
                }
            }
        }

        IEnumerator MoveBlock(Note note, int x, int y, float cellSize, float spacing, int width, int height)
        {
            float startX = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startY = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;

            Vector2 targetPos = new Vector2(startX + x * (cellSize + spacing), startY + y * (cellSize + spacing));
            RectTransform rect = note.GetComponent<RectTransform>();

            while (Vector2.Distance(rect.anchoredPosition, targetPos) > 1f)
            {
                if (note == null) yield break;
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * 10f);
                yield return null;
            }
            rect.anchoredPosition = targetPos;
        }

        public void PlayInstrumentSound(int instrumentIndex)
        {
            if (sfxPlayer != null && instrumentIndex < instrumentSounds.Length)
            {
                AudioClip soundToPlay = instrumentSounds[instrumentIndex];
                if (soundToPlay != null)
                {
                    sfxPlayer.PlayOneShot(soundToPlay);
                }
            }
        }
    }
}