using UnityEngine;
using UnityEngine.UI;

public class InteractDoor : MonoBehaviour
{
    [Header("Who can interact")]
    public string targetTag = "Player";
    
    [Header("When interact")]
    public string targetScene = "";

    [Header("Key & Message")]
    public KeyCode interactKey = KeyCode.E;
    public string promptText = "Press E to Interact";

    [Header("Visual Styling")]
    [Tooltip("Drag a font here, or it defaults to Arial.")]
    public Font customFont; 
    public Color promptTextColor = Color.white;

    [Header("World UI Settings")]
    public float heightOffset = 1.8f;
    public float worldScale = 0.01f;
    public Vector2 canvasPixelSize = new Vector2(800f, 200f);
    public int promptFontSize = 80;

    private bool isInside;
    private Canvas worldCanvas;
    private Transform cam;

    void Awake()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        
        if (customFont == null)
            customFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        SetupTrigger();
        CreateWorldUI();
        HideUI();
    }

    void SetupTrigger()
    {
        // Ensure there is a collider set as a trigger
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
        if (isInside)
        {
            ShowUI();
            if (Input.GetKeyDown(interactKey))
            {
                OnInteract();
            }
        }
        else
        {
            HideUI();
        }

        FaceCamera();
    }

    // --- YOUR CALLBACK IS HERE ---
    // This is where you will write your future side-hustle logic!
    void OnInteract()
    {
        if (LevelLoader.Instance != null)
            LevelLoader.Instance.LoadScene(targetScene);
    }

    void FaceCamera()
    {
        if (worldCanvas == null || cam == null) return;
        Vector3 dir = worldCanvas.transform.position - cam.position;
        if (dir.sqrMagnitude > 0.1f)
            worldCanvas.transform.rotation = Quaternion.LookRotation(dir);
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

    void ShowUI() => worldCanvas.gameObject.SetActive(true);
    void HideUI() => worldCanvas.gameObject.SetActive(false);

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag)) isInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag)) isInside = false;
    }
}
