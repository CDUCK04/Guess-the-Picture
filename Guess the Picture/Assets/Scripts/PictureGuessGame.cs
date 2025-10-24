using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class PictureGuessGame : MonoBehaviour
{
    [SerializeField] private SpriteScoreDisplay spriteScoreDisplay;
    [SerializeField] private SpriteScoreDisplay spriteTimerDisplay;

    [SerializeField] private SpriteScoreDisplay spriteCurrentRoundDisplay;
    [SerializeField] private SpriteScoreDisplay spriteTotalRoundsDisplay;

    [SerializeField] private GameObject restartMenu;

    [SerializeField] private RestartMenu restartMenuComponent;


    //teimo how did you do this w/o a single comment holy shit
    [Header("UI Refs")]
    [SerializeField] private Image pictureImage;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Prefabs & Folders")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private string picturesFolder = "Pictures";
    [SerializeField] private string lettersFolder = "Placeholder Prefabs/Letters";

    [Header("Provided Cover Blocks (drag all 25 here)")]
    [SerializeField] private List<GameObject> coverBlocks = new();

    [Header("Provided Letter Spots (drag all 20 here)")]
    [SerializeField] private List<RectTransform> letterSpots = new();

    [Header("Config")]
    [SerializeField] private int totalLetterButtons = 20;
    [SerializeField] private int totalRounds = MainMenu.NumOfRounds;
    [SerializeField] private int roundTimeSeconds = 25;
    [SerializeField] private int maxRoundPoints = 200;
    [SerializeField] private int penaltyStartAt = 20;
    [SerializeField] private int penaltyPerSec = 10;

    private readonly Dictionary<Button, RectTransform> buttonHomeSpot = new();

    private readonly System.Random rng = new System.Random();
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private List<Sprite> allPictures;
    private List<int> pictureOrder = new();
    private int currentRoundIndex = 0;

    private Sprite currentSprite;
    private string currentAnswer;

    private readonly List<AnswerSlot> slots = new();
    private readonly List<Button> letterButtons = new();

    private int score = 0;
    private float timer;
    private bool roundActive = false;
    private float coverTickAccumulator = 0f;

    void Start()
    {
        restartMenu.SetActive(false);

        totalRounds = MainMenu.NumOfRounds;

        if (letterSpots == null || letterSpots.Count == 0)
            Debug.LogWarning("Letter spots list is empty. Drag your 20 blank blocks into 'letterSpots'.");

        if (totalLetterButtons != letterSpots.Count)
            totalLetterButtons = letterSpots.Count;

        allPictures = Resources.LoadAll<Sprite>(picturesFolder).ToList();
        if (allPictures.Count == 0)
        {
            Debug.LogError("No sprites found under Resources/" + picturesFolder);
            return;
        }

        pictureOrder = Enumerable.Range(0, allPictures.Count)
                                 .OrderBy(_ => rng.Next())
                                 .Take(Mathf.Min(totalRounds, allPictures.Count))
                                 .ToList();

        UpdateScoreUI();
        StartRound();
    }

    void Update()
    {
        if (!roundActive) return;

        timer -= Time.deltaTime;
        int timeLeft = Mathf.CeilToInt(Mathf.Max(timer, 0));

        if (spriteTimerDisplay != null)
            spriteTimerDisplay.SetDisplay(timeLeft.ToString());
        else if (timerText)
            timerText.text = timeLeft.ToString();

        coverTickAccumulator += Time.deltaTime;
        while (coverTickAccumulator >= 1f)
        {
            coverTickAccumulator -= 1f;
            HideOneRandomCoverBlock();
        }

        if (timer <= 0f)
        {

            SetStatus("TIME'S UP!");
            StartCoroutine(RevealAndProceed(fillAnswer: true));
        }
    }

    private void StartRound()
    {
        ClearUI();

        if (currentRoundIndex >= pictureOrder.Count)
        {
            SetStatus($"Finished! Total Score: {score}");

            restartMenu.SetActive(true);
            restartMenuComponent.ShowRestartMenu();
            return;
        }

        currentSprite = allPictures[pictureOrder[currentRoundIndex]];
        pictureImage.sprite = currentSprite;

        currentAnswer = CleanAnswer(currentSprite.name);
        if (string.IsNullOrEmpty(currentAnswer))
        {
            currentRoundIndex++;
            StartRound();
            return;
        }

        float spacing = 90f;
        int count = currentAnswer.Length;

        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(slotPrefab, slotsParent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);

            var slot = go.GetComponent<AnswerSlot>() ?? go.AddComponent<AnswerSlot>();
            slot.Init();
            slots.Add(slot);
        }

        foreach (var block in coverBlocks) if (block) block.SetActive(true);

        SpawnLettersOntoSpots();

        timer = roundTimeSeconds;
        coverTickAccumulator = 0f;
        roundActive = true;

        SetStatus("Guess That Picture!");
        if (spriteCurrentRoundDisplay != null)
            spriteCurrentRoundDisplay.SetDisplay((currentRoundIndex + 1).ToString());
        if (spriteTotalRoundsDisplay != null)
            spriteTotalRoundsDisplay.SetDisplay(pictureOrder.Count.ToString());

    }

    private void SpawnLettersOntoSpots()
    {
        if (letterSpots == null || letterSpots.Count == 0) return;

        foreach (var spot in letterSpots)
        {
            if (!spot) continue;
            for (int i = spot.childCount - 1; i >= 0; i--)
                Destroy(spot.GetChild(i).gameObject);
        }
        letterButtons.Clear();

        var pool = BuildShuffledLetterPool(currentAnswer, totalLetterButtons);

        var shuffledSpots = letterSpots.OrderBy(_ => rng.Next()).ToList();

        for (int i = 0; i < Mathf.Min(pool.Count, shuffledSpots.Count); i++)
        {
            char c = pool[i];
            var prefab = Resources.Load<Button>($"{lettersFolder}/{c}");
            if (prefab == null)
            {
                Debug.LogError($"Missing letter prefab for '{c}' at Resources/{lettersFolder}/{c}");
                continue;
            }

            var btn = Instantiate(prefab, shuffledSpots[i]);
            var rt = btn.transform as RectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            buttonHomeSpot[btn] = shuffledSpots[i];
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = c.ToString();

            char picked = c;
            btn.onClick.AddListener(() => OnLetterPressed(btn, picked));
            letterButtons.Add(btn);
        }
    }

    public void Backspace()
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var s = slots[i];
            if (!s.IsEmpty && s.PlacedButton != null)
            {
                ReturnButtonToHome(s.PlacedButton);
                s.ClearPlaced();
                SetStatus("");
                break;
            }
        }
    }

    private void ReturnButtonToHome(Button btn)
    {
        RectTransform home;
        if (!buttonHomeSpot.TryGetValue(btn, out home) || home == null)
        {
            home = letterSpots.FirstOrDefault(s => s && s.childCount == 0);

            if (home == null)
                home = letterSpots.Where(s => s != null).OrderBy(s => s.childCount).FirstOrDefault();

            if (home == null)
                home = transform as RectTransform;
        }

        var rt = btn.transform as RectTransform;
        rt.SetParent(home, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        btn.interactable = true;
    }

    private void OnLetterPressed(Button btn, char letter)
    {
        if (!roundActive) return;

        var empty = slots.FirstOrDefault(s => s.IsEmpty);
        if (empty == null) return;

        PlaceButtonIntoSlot(btn, empty);

        if (slots.All(s => !s.IsEmpty))
        {
            string guess = new string(slots.Select(s => GetButtonLetter(s.PlacedButton)).ToArray());
            if (guess == currentAnswer)
            {
                int tRem = Mathf.CeilToInt(Mathf.Clamp(timer, 0, roundTimeSeconds));
                int points = CalculatePoints(tRem);
                score += points;
                UpdateScoreUI();
                SetStatus($"Correct! +{points}");

                StartCoroutine(RevealAndProceed(fillAnswer: false));
            }
            else
            {
                SetStatus("Wrong. Auto-clearing...");
                AutoClearSlots();
            }
        }
    }

    private void AutoClearSlots()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.PlacedButton != null)
            {
                var btn = slot.PlacedButton;
                slot.ClearPlaced();
                ReturnButtonToHome(btn);
            }
        }
    }

    private void PlaceButtonIntoSlot(Button btn, AnswerSlot slot)
    {
        btn.interactable = false;

        var t = btn.transform as RectTransform;
        t.SetParent(slot.transform, worldPositionStays: false);
        t.anchorMin = t.anchorMax = new Vector2(0.5f, 0.5f);
        t.pivot = new Vector2(0.5f, 0.5f);
        t.anchoredPosition = Vector2.zero;
        t.localScale = Vector3.one;

        slot.SetPlaced(btn);
    }

    private char GetButtonLetter(Button b)
    {
        var label = b.GetComponentInChildren<TMP_Text>();
        if (label && !string.IsNullOrEmpty(label.text)) return label.text[0];
        string n = b.gameObject.name.Trim();
        return string.IsNullOrEmpty(n) ? '?' : n[0];
    }

    private int CalculatePoints(int secondsRemaining)
    {
        if (secondsRemaining >= penaltyStartAt) return maxRoundPoints;
        int missing = penaltyStartAt - secondsRemaining;
        int pts = maxRoundPoints - penaltyPerSec * missing;
        return Mathf.Clamp(pts, 0, maxRoundPoints);
    }

    private void HideOneRandomCoverBlock()
    {
        var active = coverBlocks.Where(b => b && b.activeSelf).ToList();
        if (active.Count == 0) return;
        active[rng.Next(active.Count)].SetActive(false);
    }

    private void ProceedNextRound() => Invoke(nameof(NextRoundInternal), 0.5f);
    private void NextRoundInternal() { currentRoundIndex++; StartRound(); }

    private void ClearUI()
    {
        foreach (Transform t in slotsParent) Destroy(t.gameObject);
        slots.Clear();

        if (letterSpots != null)
        {
            foreach (var spot in letterSpots)
            {
                if (!spot) continue;
                for (int i = spot.childCount - 1; i >= 0; i--)
                    Destroy(spot.GetChild(i).gameObject);
            }
        }
        letterButtons.Clear();
        buttonHomeSpot.Clear();
    }

    private string CleanAnswer(string raw)
    {
        var letters = raw.ToUpper().Where(char.IsLetter).ToArray();
        return new string(letters);
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
        Debug.Log(msg);
    }

    private void UpdateScoreUI()
    {
        if (spriteScoreDisplay != null)
            spriteScoreDisplay.SetDisplay(score.ToString());
        else if (scoreText)
            scoreText.text = $"{score}";
    }

    private List<char> BuildShuffledLetterPool(string answer, int poolSize)
    {
        var pool = new List<char>(answer);
        while (pool.Count < poolSize)
            pool.Add(Alphabet[rng.Next(Alphabet.Length)]);

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool;
    }

    private void RevealAllCovers()
    {
        foreach (var b in coverBlocks) if (b) b.SetActive(false);
    }

    private void FillAnswerWithCorrectLetters()
    {
        foreach (var b in letterButtons) if (b) b.interactable = false;

        var used = new HashSet<Button>();

        for (int i = 0; i < currentAnswer.Length && i < slots.Count; i++)
        {
            char need = currentAnswer[i];

            if (!slots[i].IsEmpty && GetButtonLetter(slots[i].PlacedButton) == need)
            {
                used.Add(slots[i].PlacedButton);
                continue;
            }

            Button pick = letterButtons.FirstOrDefault(b =>
                b != null && !used.Contains(b) && GetButtonLetter(b) == need);

            if (pick != null)
            {
                PlaceButtonIntoSlot(pick, slots[i]);
                used.Add(pick);
            }
        }
    }

    private IEnumerator RevealAndProceed(bool fillAnswer)
    {
        roundActive = false;
        RevealAllCovers();
        if (fillAnswer) FillAnswerWithCorrectLetters();

        foreach (var b in letterButtons) if (b) b.interactable = false;

        yield return new WaitForSeconds(2.5f);
        ProceedNextRound();
    }
}
