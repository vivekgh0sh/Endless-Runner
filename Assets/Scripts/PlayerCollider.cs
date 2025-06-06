using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public PlayerMovement movement;
    private void OnCollisionEnter(Collision collisioninfo)
    {
        if (collisioninfo.gameObject.CompareTag("Collider"))
        {

            Debug.Log("Player collided with an obstacle!");
            movement.enabled = false;
            FindObjectOfType<gameManager>().EndGame();
        }
    }
}

