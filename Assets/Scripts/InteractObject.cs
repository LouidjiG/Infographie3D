using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoInteractPrompt : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("References")]
    [Tooltip("Drag the child object with the image/model here.")]
    public Transform visualChild;

    [Header("Who can interact")]
    public string targetTag = "Player";

    [Header("Key & Message")]
    public KeyCode interactKey = KeyCode.E;
    public string promptText = "Press E";
    [TextArea(3, 10)]
    public string dialogueContent = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    
    [Header("Whoosh Whoosh (Oscillating Movement)")]
    public bool enableMovement = true;
    public RotationAxis spinAxis = RotationAxis.Y;
    [Tooltip("How far the object swings left and right (e.g. 45 degrees).")]
    public float maxRotationAngle = 45f;
    [Tooltip("How fast the rotation oscillates.")]
    public float rotationSpeed = 2f;
    public float bobHeight = 0.5f;
    public float bobSpeed = 2f;

    [Header("Visual Styling")]
    public Font customFont; 
    public Color promptTextColor = Color.white;
    public Color dialogueTextColor = Color.white;
    public Color dialogueBackgroundColor = new Color(0, 0, 0, 0.85f);

    [Header("Animation Settings")]
    public float typingSpeed = 0.03f;

    [Header("World UI Settings")]
    public float heightOffset = 1.8f;
    public float worldScale = 0.01f;
    public Vector2 canvasPixelSize = new Vector2(800f, 200f);
    public int promptFontSize = 80;

    [Header("Dialogue Box Settings")]
    public int dialogueFontSize = 36;
    public Vector2 dialogueBoxSize = new Vector2(1200, 200);

    private bool isInside;
    private bool isDialogueActive;
    private Coroutine typingCoroutine;
    
    // Starting States
    private Vector3 visualStartLocalPos;
    private Quaternion visualStartLocalRot;
    private Transform targetTransform;

    private Canvas worldCanvas;
    private Text dialogueTextUI;
    private GameObject dialoguePanel;
    private Transform cam;

    void Awake()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        
        targetTransform = (visualChild != null) ? visualChild : transform;
        visualStartLocalPos = targetTransform.localPosition;
        visualStartLocalRot = targetTransform.localRotation;
        
        if (customFont == null)
            customFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        SetupTrigger();
        CreateWorldUI();
        CreateScreenSpaceUI();
        
        HideUI();
        HideDialogue();
    }

    void SetupTrigger()
    {
        var col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            (col as BoxCollider).size = new Vector3(3, 3, 3);
        }
        col.isTrigger = true;
    }

    void Update()
    {
        HandleInteraction();
        HandleMovement();
        FaceCamera();
    }

    void HandleMovement()
    {
        if (!enableMovement || targetTransform == null) return;

        // 1. OSCILLATING ROTATION (Ping-Pong)
        // We use Sin to calculate an angle between -maxRotationAngle and +maxRotationAngle
        float angle = Mathf.Sin(Time.time * rotationSpeed) * maxRotationAngle;
        
        Vector3 rotationAxisVector = Vector3.zero;
        switch (spinAxis)
        {
            case RotationAxis.X: rotationAxisVector = Vector3.right; break;
            case RotationAxis.Y: rotationAxisVector = Vector3.up; break;
            case RotationAxis.Z: rotationAxisVector = Vector3.forward; break;
        }

        // Apply rotation relative to the initial start rotation
        targetTransform.localRotation = visualStartLocalRot * Quaternion.AngleAxis(angle, rotationAxisVector);

        // 2. BOBBING
        float newY = visualStartLocalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        targetTransform.localPosition = new Vector3(targetTransform.localPosition.x, newY, targetTransform.localPosition.z);
    }

    void HandleInteraction()
    {
        if (isInside && Input.GetKeyDown(interactKey))
        {
            if (!isDialogueActive)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                typingCoroutine = StartCoroutine(DisplayDialogue());
            }
            else HideDialogue();
        }

        if (isInside && !isDialogueActive) ShowUI();
        else HideUI();
    }

    IEnumerator DisplayDialogue()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        dialogueTextUI.text = "";

        foreach (char letter in dialogueContent.ToCharArray())
        {
            dialogueTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    void FaceCamera()
    {
        if (worldCanvas == null || cam == null) return;
        Vector3 dir = worldCanvas.transform.position - cam.position;
        if (dir.sqrMagnitude > 0.1f)
            worldCanvas.transform.rotation = Quaternion.LookRotation(dir);
    }

    void ShowUI() => worldCanvas.gameObject.SetActive(true);
    void HideUI() => worldCanvas.gameObject.SetActive(false);

    void HideDialogue()
    {
        isDialogueActive = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    void CreateWorldUI()
    {
        GameObject canvasGO = new GameObject("InteractCanvas_World");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = Vector3.up * heightOffset;
        canvasGO.transform.localScale = Vector3.one * worldScale;

        worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.GetComponent<RectTransform>().sizeDelta = canvasPixelSize;

        GameObject textGO = new GameObject("InteractText");
        textGO.transform.SetParent(canvasGO.transform, false);

        Text t = textGO.AddComponent<Text>();
        t.text = promptText;
        t.fontSize = promptFontSize;
        t.color = promptTextColor;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = customFont;
        
        RectTransform rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void CreateScreenSpaceUI()
    {
        GameObject screenGO = new GameObject("InteractCanvas_Screen");
        Canvas screenCanvas = screenGO.AddComponent<Canvas>();
        screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        screenCanvas.sortingOrder = 999;

        CanvasScaler scaler = screenGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        screenGO.AddComponent<GraphicRaycaster>();

        dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(screenGO.transform, false);
        
        Image bgImage = dialoguePanel.AddComponent<Image>();
        bgImage.color = dialogueBackgroundColor;

        RectTransform panelRT = dialoguePanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0); 
        panelRT.anchorMax = new Vector2(0.5f, 0);
        panelRT.pivot = new Vector2(0.5f, 0);
        panelRT.sizeDelta = dialogueBoxSize; 
        panelRT.anchoredPosition = new Vector2(0, 80);

        GameObject textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dialoguePanel.transform, false);

        dialogueTextUI = textGO.AddComponent<Text>();
        dialogueTextUI.font = customFont;
        dialogueTextUI.fontSize = dialogueFontSize;
        dialogueTextUI.color = dialogueTextColor;
        dialogueTextUI.alignment = TextAnchor.UpperLeft;
        dialogueTextUI.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogueTextUI.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform textRT = dialogueTextUI.rectTransform;
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(40, 20); 
        textRT.offsetMax = new Vector2(-40, -20);
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag(targetTag)) isInside = true; }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            isInside = false;
            HideDialogue();
        }
    }
}