using UnityEngine;

public class CubeMover : MonoBehaviour
{
    public Transform player; // Assign the player's transform here
    public float speed = 0.03f; // Speed at which the cube moves towards the player
    bool isMoving = true;

    void Update()
    {
        if (isMoving) {
            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime; // Calculate distance to move
            Vector3 rangeVect = new Vector3(0.5f, 0.5f, 0.5f);
            transform.position = Vector3.MoveTowards(transform.position, player.position - rangeVect, step);
        }

        
    }
}