using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Note : MonoBehaviour
{
    [Header("Note Properties")]
    public int lane; // Assigned by the spawner
    public float speed = 5f;
    public NoteData noteData; // Reference to the note data
    
    [Header("Visual Settings")]
    public float missThresholdY = -6f;
    public float destroyThresholdY = -8f; // Extra buffer before destroying
    
    [Header("Hit Effects")]
    public GameObject hitEffect;
    public GameObject missEffect;
    public AudioSource noteHitSound;
    
    [Header("Sustained Note")]
    public GameObject sustainTail; // For long notes
    public bool isSustained = false;
    
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private bool hasPassedHitZone = false;
    private bool isDestroyed = false;
    
    // Events
    public System.Action<Note> OnNoteDestroyed;

    void Start()
    {
        InitializeNote();
    }
    
    void InitializeNote()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        // Set up visual properties
        SetupVisuals();
        
        // Set up sustained note if needed
        if (noteData != null && noteData.IsSustained())
        {
            SetupSustainedNote();
        }
    }
    
    void SetupVisuals()
    {
        if (spriteRenderer != null)
        {
            // Ensure sprite is visible
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 10;
            
            // Set color based on lane or note data
            if (noteData != null)
            {
                spriteRenderer.color = noteData.noteColor;
            }
            else
            {
                // Fallback color assignment
                SetColorByLane();
            }
        }
    }
    
    void SetColorByLane()
    {
        Color[] laneColors = { Color.green, Color.red, Color.yellow, Color.blue, Color.magenta };
        if (lane >= 0 && lane < laneColors.Length)
        {
            spriteRenderer.color = laneColors[lane];
        }
    }
    
    void SetupSustainedNote()
    {
        isSustained = true;
        
        if (sustainTail != null)
        {
            sustainTail.SetActive(true);
            
            // Scale the tail based on note duration
            float duration = noteData.duration;
            float tailLength = duration * speed; // Convert duration to visual length
            
            Vector3 tailScale = sustainTail.transform.localScale;
            tailScale.y = tailLength;
            sustainTail.transform.localScale = tailScale;
        }
    }

    void Update()
    {
        if (isDestroyed) return;
        
        // Move the note down
        MoveNote();
        
        // Check bounds
        CheckBounds();
    }
    
    void MoveNote()
    {
        Vector3 movement = Vector3.down * speed * Time.deltaTime;
        transform.position += movement;
        
        // Move sustain tail if it exists
        if (sustainTail != null && sustainTail.activeInHierarchy)
        {
            sustainTail.transform.position += movement;
        }
    }
    
    void CheckBounds()
    {
        float currentY = transform.position.y;
        
        // Check if note has passed the hit zone
        if (!hasPassedHitZone && currentY < missThresholdY)
        {
            hasPassedHitZone = true;
            
            // Only mark as missed if it hasn't been hit yet
            if (noteData != null && !noteData.hit && !noteData.missed)
            {
                Miss();
            }
        }
        
        // Check if note should be destroyed (completely off screen)
        if (currentY < destroyThresholdY || IsCompletelyOffScreen())
        {
            DestroyNote();
        }
    }
    
    bool IsCompletelyOffScreen()
    {
        if (mainCamera == null) return false;
        
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        
        // Note is off screen if it's below the bottom of the screen
        return screenPoint.y < -0.1f;
    }

    public void Hit()
    {
        if (isDestroyed) return;
        
        // Play hit effect
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up effect after 2 seconds
        }
        
        // Play hit sound
        if (noteHitSound != null)
        {
            noteHitSound.Play();
        }
        
        Debug.Log($"✅ Nota acertada en lane {lane}");
        
        // Handle sustained notes
        if (isSustained && sustainTail != null)
        {
            // For sustained notes, we might want to keep the tail visible
            // until the key is released or the sustain ends
            HandleSustainedHit();
        }
        else
        {
            DestroyNote();
        }
    }
    
    void HandleSustainedHit()
    {
        // Hide the note head but keep the tail
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // The tail will continue moving and be destroyed when it goes off screen
        // or when the key is released
    }

    public void Miss()
    {
        if (isDestroyed) return;
        
        // Play miss effect
        if (missEffect != null)
        {
            GameObject effect = Instantiate(missEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        Debug.Log($"❌ Nota fallada en lane {lane}");
        
        // Don't destroy immediately for visual feedback
        StartCoroutine(DelayedDestroy(0.5f));
    }
    
    System.Collections.IEnumerator DelayedDestroy(float delay)
    {
        // Fade out effect
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;
            
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / delay);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
        
        DestroyNote();
    }
    
    void DestroyNote()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // Notify that this note is being destroyed
        OnNoteDestroyed?.Invoke(this);
        
        // Clean up sustain tail
        if (sustainTail != null)
        {
            Destroy(sustainTail);
        }
        
        // Destroy the note
        Destroy(gameObject);
    }
    
    // Public method to check if note is in hit zone
    public bool IsInHitZone(float hitZoneY, float hitWindow)
    {
        float distance = Mathf.Abs(transform.position.y - hitZoneY);
        return distance <= hitWindow;
    }
    
    // Public method to get distance from hit zone
    public float GetDistanceFromHitZone(float hitZoneY)
    {
        return Mathf.Abs(transform.position.y - hitZoneY);
    }
    
    void OnDestroy()
    {
        // Clean up any remaining effects or sounds
        if (hitEffect != null) Destroy(hitEffect);
        if (missEffect != null) Destroy(missEffect);
    }
}
