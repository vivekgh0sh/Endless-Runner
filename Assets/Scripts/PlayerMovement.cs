// --- START OF FILE PlayerMovement.cs ---
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    // public int forwardMovement = 2000; // We'll replace this with a target speed
    public float targetForwardSpeed = 10f; // Adjust this value to get desired speed
    public int sidewaysMovement = 25; // Keep this as is for now, or also experiment

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        // Ensure Rigidbody constraints are set (as a backup, though Inspector is primary)
        // rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        // --- FORWARD MOVEMENT: Target Velocity Approach ---
        // Calculate the desired velocity change for forward movement
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = new Vector3(currentVelocity.x, currentVelocity.y, targetForwardSpeed);
        Vector3 velocityChange = (targetVelocity - currentVelocity);

        // We only want to affect Z speed here, and keep existing Y and X speed from other physics (like falling or sideways input)
        velocityChange.x = 0; // Don't interfere with sideways input if it's also velocity based
        velocityChange.y = 0; // Don't interfere with gravity

        // Apply the change as an acceleration (ForceMode.VelocityChange is too abrupt for this continuous adjustment)
        // Or, more simply, directly set the Z velocity component, preserving X and Y
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, targetForwardSpeed);


        // --- SIDEWAYS MOVEMENT ---
        // Keep your existing sideways movement for now.
        // If the jump persists, we might need to change this too.
        if (Input.GetKey("d"))
        {
            // Using AddForce with VelocityChange for sideways might still be a bit jerky at seams.
            // Consider rb.AddForce(sidewaysMovement * Time.fixedDeltaTime, 0, 0, ForceMode.Force);
            // OR set target X velocity similar to forward, but that makes turning less snappy.
            rb.AddForce(sidewaysMovement * Time.fixedDeltaTime, 0, 0, ForceMode.VelocityChange);
        }
        if (Input.GetKey("a"))
        {
            rb.AddForce(-sidewaysMovement * Time.fixedDeltaTime, 0, 0, ForceMode.VelocityChange);
        }

        // --- DEBUGGING JUMP ---
        if (rb.linearVelocity.y > 0.1f)
        {
            Debug.Log($"Player is moving UP! Y Velocity: {rb.linearVelocity.y} at Time: {Time.time} at Z: {transform.position.z}");
        }

        if (rb.position.y < -1f)
        {
            FindObjectOfType<gameManager>().EndGame();
        }
    }
}
// --- END OF FILE PlayerMovement.cs ---