using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mouvements : MonoBehaviour
{
    [Header("Paramètres de Mouvement")]
    public float moveSpeed = 5f;

    [Header("Références")]
    public Transform cameraTransform;
    public MeshRenderer meshRenderer;

    [Header("Animation (Flipbook)")]
    public Texture idleFrame;
    public Texture walkFrame1;
    public Texture walkFrame2;
    public Texture walkFrame3;
    public Texture walkFrame4;

    [Header("Animation - Réglages")]
    public float idleFps = 1f;
    public float walkFps = 12f;
    public float runThreshold = 0.1f;
    public bool resetToFirstWalkFrameOnStartMoving = true;
    public bool keepLastWalkFrameWhenStopping = false;

    [Header("Animation - Shader Property")]
    public string texturePropertyName = "_MainTex";

    private Rigidbody rb;
    private Vector3 movementInput;

    private MaterialPropertyBlock mpb;
    private int frameIndex;
    private float frameTimer;
    private bool wasMoving;
    public bool isAnimatingOverride = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.useGravity = true;
        }

        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        movementInput = (forward * moveVertical + right * moveHorizontal).normalized;

        HandleMeshFlip(-moveHorizontal);
        if (!isAnimatingOverride)
        {
            HandleFlipbookAnimation(moveHorizontal, moveVertical);
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            transform.position += movementInput * moveSpeed * Time.fixedDeltaTime;
            return;
        }

        Vector3 desiredVelocity = movementInput * moveSpeed;
        rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);
    }

    void HandleMeshFlip(float horizontalInput)
    {
        if (meshRenderer == null) return;

        if (horizontalInput > 0.1f && meshRenderer.transform.localScale.x < 0)
        {
            Vector3 newScale = meshRenderer.transform.localScale;
            newScale.x = Mathf.Abs(newScale.x);
            meshRenderer.transform.localScale = newScale;
        }
        else if (horizontalInput < -0.1f && meshRenderer.transform.localScale.x > 0)
        {
            Vector3 newScale = meshRenderer.transform.localScale;
            newScale.x = -Mathf.Abs(newScale.x);
            meshRenderer.transform.localScale = newScale;
        }
    }

    void HandleFlipbookAnimation(float moveHorizontal, float moveVertical)
    {
        if (meshRenderer == null) return;

        Vector3 v = rb != null ? rb.linearVelocity : movementInput * moveSpeed;
        float speed = new Vector2(v.x, v.z).magnitude;
        bool isMoving = speed > runThreshold;

        Texture[] walkFrames = GetWalkFrames();
        Texture targetTexture;

        if (!isMoving)
        {
            if (!keepLastWalkFrameWhenStopping)
            {
                targetTexture = idleFrame != null ? idleFrame : (walkFrames.Length > 0 ? walkFrames[0] : null);
                ApplyTexture(targetTexture);
            }

            frameTimer = 0f;
            if (!keepLastWalkFrameWhenStopping) frameIndex = 0;

            wasMoving = false;
            return;
        }

        if (!wasMoving && resetToFirstWalkFrameOnStartMoving)
        {
            frameIndex = 0;
            frameTimer = 0f;
        }

        float fps = Mathf.Max(0.01f, walkFps);
        float frameTime = 1f / fps;

        frameTimer += Time.deltaTime;
        while (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            if (walkFrames.Length > 0) frameIndex = (frameIndex + 1) % walkFrames.Length;
        }

        targetTexture = walkFrames.Length > 0 ? walkFrames[frameIndex] : idleFrame;
        ApplyTexture(targetTexture);

        wasMoving = true;
    }

    Texture[] GetWalkFrames()
    {
        int count = 0;
        if (walkFrame1 != null) count++;
        if (walkFrame2 != null) count++;
        if (walkFrame3 != null) count++;
        if (walkFrame4 != null) count++;

        if (count == 0) return new Texture[0];

        Texture[] frames = new Texture[count];
        int i = 0;
        if (walkFrame1 != null) frames[i++] = walkFrame1;
        if (walkFrame2 != null) frames[i++] = walkFrame2;
        if (walkFrame3 != null) frames[i++] = walkFrame3;
        if (walkFrame4 != null) frames[i++] = walkFrame4;

        return frames;
    }

    void ApplyTexture(Texture tex)
    {
        if (tex == null) return;

        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetTexture(texturePropertyName, tex);
        meshRenderer.SetPropertyBlock(mpb);
    }
}
