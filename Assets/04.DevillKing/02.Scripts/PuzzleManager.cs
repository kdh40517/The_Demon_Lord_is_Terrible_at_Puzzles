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

        [Header("연결 완성 사운드")]
        public AudioClip finishAttackSound; // 몽둥이 완성 소리
        public AudioClip finishArmorSound;  // 갑옷 완성 소리
        public AudioClip finishShieldSound; // 방패 완성 소리

        private Note lastHoveredNote;
        private bool isDrawing = false;

        void Awake() { Instance = this; }

        public void StartDrawing(Note firstNote)
        {
            if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

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
            // ★ 유고수 추가: 테이프 무늬를 시간에 따라 옆으로 쓱쓱 밀어줍니다! (-2f 숫자를 바꾸면 속도가 바뀝니다)
            if (lineRenderer != null) lineRenderer.material.mainTextureOffset = new Vector2(Time.time * -5000f, 0f);
            if (dragLineRenderer != null) dragLineRenderer.material.mainTextureOffset = new Vector2(Time.time * -2f, 0f);

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

            foreach (Note note in connectedNotes)
            {
                if (!note.isBroken) validScore++;
            }

            NoteType currentType = connectedNotes[0].instrumentType;
            string statName = "";

            if (currentType == NoteType.Club) statName = "DMG";
            else if (currentType == NoteType.Armor) statName = "HP";
            else if (currentType == NoteType.Shield) statName = "DEF";

            string textOut = $"<color=white>{statName} {validScore}</color>";

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

                foreach (Note note in connectedNotes)
                {
                    if (!note.isBroken) validScore++;
                }

                if (GameManager.Instance != null)
                {
                    if (validScore > 0)
                    {
                        NoteType firstType = connectedNotes[0].instrumentType;

                        if (firstType == NoteType.Club)
                        {
                            GameManager.Instance.AttackBoss(validScore);
                            // 👇 몽둥이 소리 추가!
                            if (sfxPlayer != null && finishAttackSound != null) sfxPlayer.PlayOneShot(finishAttackSound);
                        }
                        else if (firstType == NoteType.Armor)
                        {
                            GameManager.Instance.HealPlayer(validScore);
                            // 👇 갑옷(회복) 소리 추가!
                            if (sfxPlayer != null && finishArmorSound != null) sfxPlayer.PlayOneShot(finishArmorSound);
                        }
                        else if (firstType == NoteType.Shield)
                        {
                            GameManager.Instance.AddShield(validScore);
                            // 👇 방패 소리 추가!
                            if (sfxPlayer != null && finishShieldSound != null) sfxPlayer.PlayOneShot(finishShieldSound);
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

        void BreakAdjacentStones(List<Note> path)
        {
            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            Note[,] board = BoardManager.Instance.board;

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            foreach (Note node in path)
            {
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

        private System.Collections.IEnumerator ApplyGravity()
        {
            yield return new WaitForSeconds(0.1f);

            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            Note[,] board = BoardManager.Instance.board;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        for (int ny = y + 1; ny < height; ny++)
                        {
                            if (board[x, ny] != null)
                            {
                                board[x, y] = board[x, ny];
                                board[x, ny] = null;
                                board[x, y].x = x;
                                board[x, y].y = y;
                                break;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        BoardManager.Instance.SpawnNote(x, y, 600f);
                    }
                }
            }

            float duration = 0.25f;
            float elapsedTime = 0f;

            Vector2[,] startPos = new Vector2[width, height];
            Vector2[,] targetPos = new Vector2[width, height];

            float cellSize = BoardManager.Instance.cellSize;
            float spacing = BoardManager.Instance.spacing;
            float startXOffset = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startYOffset = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Note note = board[x, y];
                    if (note != null)
                    {
                        RectTransform rect = note.GetComponent<RectTransform>();
                        startPos[x, y] = rect.anchoredPosition;

                        float targetX = startXOffset + x * (cellSize + spacing);
                        float targetY = startYOffset + y * (cellSize + spacing);
                        targetPos[x, y] = new Vector2(targetX, targetY);
                    }
                }
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = t * t * (3f - 2f * t);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Note note = board[x, y];
                        if (note != null)
                        {
                            note.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos[x, y], targetPos[x, y], t);
                        }
                    }
                }
                yield return null;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Note note = board[x, y];
                    if (note != null)
                    {
                        note.GetComponent<RectTransform>().anchoredPosition = targetPos[x, y];
                    }
                }
            }
        }

        void UpdateNotePosition(Note note)
        {
            if (note == null) return;

            float cellSize = BoardManager.Instance.cellSize;
            float spacing = BoardManager.Instance.spacing;
            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;

            float startX = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startY = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;
            float targetX = startX + note.x * (cellSize + spacing);
            float targetY = startY + note.y * (cellSize + spacing);

            note.GetComponent<RectTransform>().anchoredPosition = new Vector2(targetX, targetY);
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

        public void ShuffleBoard()
        {
            StartCoroutine(ShuffleBoardRoutine());
        }

        private System.Collections.IEnumerator ShuffleBoardRoutine()
        {
            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            Note[,] board = BoardManager.Instance.board;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int randomX = Random.Range(0, width);
                    int randomY = Random.Range(0, height);

                    Note temp = board[x, y];
                    board[x, y] = board[randomX, randomY];
                    board[randomX, randomY] = temp;

                    if (board[x, y] != null) { board[x, y].x = x; board[x, y].y = y; }
                    if (board[randomX, randomY] != null) { board[randomX, randomY].x = randomX; board[randomX, randomY].y = randomY; }
                }
            }

            float cellSize = BoardManager.Instance.cellSize;
            float spacing = BoardManager.Instance.spacing;
            float startXOffset = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startYOffset = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;

            Vector2[,] startPositions = new Vector2[width, height];
            Vector2[,] targetPositions = new Vector2[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] != null)
                    {
                        startPositions[x, y] = board[x, y].GetComponent<RectTransform>().anchoredPosition;

                        float targetX = startXOffset + x * (cellSize + spacing);
                        float targetY = startYOffset + y * (cellSize + spacing);
                        targetPositions[x, y] = new Vector2(targetX, targetY);
                    }
                }
            }

            float duration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = t * t * (3f - 2f * t);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (board[x, y] != null)
                        {
                            board[x, y].GetComponent<RectTransform>().anchoredPosition =
                                Vector2.Lerp(startPositions[x, y], targetPositions[x, y], t);
                        }
                    }
                }
                yield return null;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] != null)
                    {
                        board[x, y].GetComponent<RectTransform>().anchoredPosition = targetPositions[x, y];
                    }
                }
            }
        }
    }
}