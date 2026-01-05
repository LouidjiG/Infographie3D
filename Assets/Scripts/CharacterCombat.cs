using UnityEngine;
using System.Collections;

public class CharacterCombat : MonoBehaviour
{
    [Header("Animation Frames")]
    public Texture[] attackFrames;
    public float attackFps = 12f;

    [Header("References")]
    public MeshRenderer meshRenderer;
    public BoxCollider hitbox;
    public string texturePropertyName = "_MainTex";

    [Header("Hitbox Timing")]
    public int activeFrameStart = 1;
    public int activeFrameEnd = 2;

    private MaterialPropertyBlock mpb;
    private bool isAttacking = false;
    private Mouvements moveScript;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        moveScript = GetComponent<Mouvements>();
        
        if (hitbox != null) hitbox.enabled = false;
    }

    void Update()
    {
        // Only trigger if Mouse 0 (Left Click) is pressed AND we aren't already attacking
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(PlayAttackAnimation());
        }
    }

    IEnumerator PlayAttackAnimation()
    {
        isAttacking = true;

        // Tell the movement script to stop its own flipbook
        if (moveScript != null) moveScript.isAnimatingOverride = true;

        float frameTime = 1f / attackFps;

        for (int i = 0; i < attackFrames.Length; i++)
        {
            ApplyTexture(attackFrames[i]);

            // Hitbox Logic
            if (hitbox != null)
            {
                if (i == activeFrameStart) hitbox.enabled = true;
                if (i == activeFrameEnd + 1) hitbox.enabled = false;
            }

            yield return new WaitForSeconds(frameTime);
        }

        // Reset everything
        if (hitbox != null) hitbox.enabled = false;
        
        // Give control back to the movement script
        if (moveScript != null) moveScript.isAnimatingOverride = false;
        
        isAttacking = false;
    }

    void ApplyTexture(Texture tex)
    {
        if (tex == null || meshRenderer == null) return;
        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetTexture(texturePropertyName, tex);
        meshRenderer.SetPropertyBlock(mpb);
    }
}