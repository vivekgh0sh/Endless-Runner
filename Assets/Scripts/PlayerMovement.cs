// --- START OF FILE PlayerMovement.cs ---
using UnityEngine;
using UnityEngine.EventSystems; // Required for checking if touch is over UI

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float targetForwardSpeed = 10f;
    // For ForceMode.VelocityChange, this value is a direct velocity addition.
    // A smaller value like 0.5f to 2f might be appropriate for a tap/hold effect.
    // If you want a "dash" on tap, it could be larger.
    // Let's assume you want a continuous effect while holding.
    public float sidewaysVelocityChangeMagnitude = 1f; // Adjust this value

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("PlayerMovement: Rigidbody component not found on this GameObject!", this);
                this.enabled = false;
                return;
            }
        }
        // Reminder: Set Rigidbody constraints in the Inspector for reliable behavior
        // e.g., Freeze Rotation X, Y, Z.
        // rb.constraints = RigidbodyConstraints.FreezeRotation;
        Debug.Log("PlayerMovement Start: Initialized. Ensure Rigidbody constraints are set in Inspector.");
    }

    // We'll get input in Update and apply physics changes in FixedUpdate.
    private float _processedHorizontalInputSignal = 0f; // -1 for left, 0 for none, 1 for right

    void Update()
    {
        // Reset processed input for this frame
        _processedHorizontalInputSignal = 0f;

        // --- KEYBOARD INPUT DETECTION ---
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _processedHorizontalInputSignal = 1f;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _processedHorizontalInputSignal = -1f;
        }

        // --- TOUCH INPUT DETECTION (Overrides keyboard if active touch for movement) ---
        if (Input.touchCount > 0)
        {
            bool touchIsForMovement = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // Check if touch is over UI (if EventSystem is present)
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    continue; // This touch is on UI, check next touch
                }

                // Process active touches (Began, Moved, Stationary) for movement
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    if (touch.position.x < Screen.width / 2) // Left half
                    {
                        _processedHorizontalInputSignal = -1f;
                        touchIsForMovement = true;
                        break; // Dominant touch for movement found
                    }
                    else if (touch.position.x >= Screen.width / 2) // Right half (use >= to cover exact middle)
                    {
                        _processedHorizontalInputSignal = 1f;
                        touchIsForMovement = true;
                        break; // Dominant touch for movement found
                    }
                }
            }
            // If a touch was for movement, _processedHorizontalInputSignal is now set by touch.
            // If all touches were on UI, _processedHorizontalInputSignal remains as set by keyboard (or 0).
        }
        // Debug.Log($"Update - Processed Horizontal Input: {_processedHorizontalInputSignal}");
    }


    void FixedUpdate()
    {
        // --- FORWARD MOVEMENT ---
        // Maintain target forward speed, allow physics to affect X (sideways) and Y (gravity).
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, targetForwardSpeed);

        // --- SIDEWAYS MOVEMENT APPLICATION ---
        if (_processedHorizontalInputSignal != 0)
        {
            // Apply an instantaneous velocity change for sideways movement.
            // The magnitude 'sidewaysVelocityChangeMagnitude' determines how much velocity is added *per physics step* this input is active.
            // If you want it to be "per second", you'd multiply by Time.fixedDeltaTime here,
            // but for VelocityChange, it's often more direct.
            // If _processedHorizontalInputSignal is held, this force is applied every FixedUpdate.
            Vector3 sidewaysVelocityChange = new Vector3(_processedHorizontalInputSignal * sidewaysVelocityChangeMagnitude, 0, 0);
            rb.AddForce(sidewaysVelocityChange, ForceMode.VelocityChange);
            // Debug.Log($"FixedUpdate - Applying Sideways VelocityChange: {sidewaysVelocityChange.x}");
        }


        // --- DEBUGGING JUMP (Optional) ---
        if (rb.linearVelocity.y > 0.5f) // Increased threshold slightly
        {
            // Debug.LogWarning($"Player Y Velocity > 0.5: {rb.velocity.y} (Possible jump/bump)");
        }

        // --- FALLING OFF CHECK ---
        if (rb.position.y < -1f)
        {
            // Ensure this gameManager reference is correct for the current game mode
            gameManager gm = FindObjectOfType<gameManager>();
            if (gm != null && !gm.gameHasEnded)
            {
                Debug.Log("PlayerMovement: Player fell, calling EndGame.");
                gm.EndGame();
                this.enabled = false; // Disable this script after game over to prevent further input
            }
        }
    }
}
// --- END OF FILE PlayerMovement.cs ---