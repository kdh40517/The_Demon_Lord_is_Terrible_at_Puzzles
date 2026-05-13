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

        private System.Collections.IEnumerator ApplyGravity()
        {
            yield return new WaitForSeconds(0.1f); // 터지는 이펙트 대기

            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            Note[,] board = BoardManager.Instance.board;

            // 1. 논리적으로 조각들 밑으로 당기기 (데이터만 갱신)
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

            // 2. 맨 위 빈칸들에 새 조각을 '하늘(+600)'에 소환 (데이터 갱신)
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

            // 3. ★ 핵심: 모든 조각을 스르륵~ 떨어뜨리기!
            float duration = 0.25f; // 조각이 떨어지는데 걸리는 시간 (0.25초)
            float elapsedTime = 0f;

            // 출발점과 도착점 기록장
            Vector2[,] startPos = new Vector2[width, height];
            Vector2[,] targetPos = new Vector2[width, height];

            float cellSize = BoardManager.Instance.cellSize;
            float spacing = BoardManager.Instance.spacing;
            float startXOffset = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startYOffset = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;

            // 각 조각의 현재 위치와 가야 할 위치 계산
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Note note = board[x, y];
                    if (note != null)
                    {
                        RectTransform rect = note.GetComponent<RectTransform>();
                        startPos[x, y] = rect.anchoredPosition; // 지금 위치

                        float targetX = startXOffset + x * (cellSize + spacing);
                        float targetY = startYOffset + y * (cellSize + spacing);
                        targetPos[x, y] = new Vector2(targetX, targetY); // 가야 할 정답 위치
                    }
                }
            }

            // 진짜로 스르륵 움직이는 애니메이션 시작!
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = t * t * (3f - 2f * t); // 부드럽게 멈추는 가속도 효과 (Smoothstep)

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Note note = board[x, y];
                        if (note != null)
                        {
                            // 출발점에서 도착점까지 t 비율만큼 부드럽게 이동
                            note.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos[x, y], targetPos[x, y], t);
                        }
                    }
                }
                yield return null; // 다음 프레임까지 대기
            }

            // 4. 애니메이션 끝난 뒤, 1픽셀의 오차도 없이 정답 위치에 찰칵! 꽂아넣기
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

            // 보드 매니저와 똑같은 공식으로 오차 없이 타겟 위치 계산
            float startX = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startY = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;
            float targetX = startX + note.x * (cellSize + spacing);
            float targetY = startY + note.y * (cellSize + spacing);

            // 해당 조각을 타겟 위치로 쾅! 하고 이동시킵니다.
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
    }
}