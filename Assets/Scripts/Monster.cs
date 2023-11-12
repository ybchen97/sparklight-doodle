using UnityEngine;

public class Monster : MonoBehaviour
{
    public float spawnRate = 1.0f;
    public float scale = 1.0f;
    public float speed = 1.0f;
    public float health = 10;
    public int score = 10;
    public string fishName;
    public Vector3 forward;
    public float rotateSpeed = 1.0f; 
    Transform target;

    public void Awake()
    {
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        if (!target) {
            GetTarget();
        } else {
            RotateTowardsTarget();
        }

        //t += Time.deltaTime / speed; //increase time for Vector3.Lerp
        //transform.position = Vector3.Lerp(currentPos, pos, t); //the swimming
    }

    void GetTarget()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void RotateTowardsTarget()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = target.position - transform.position;
        // Reset value in y-axis to force rotation in xz-plane
        targetDirection[1] = 0.0f;

        // The step size is equal to speed times frame time.
        float singleStep = rotateSpeed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        MonsterSpawner.manager.DecrementMonsterCount();
        float multiplier = 1;
        if (gameObject.GetComponent<FishMovement>() != null)
        {
            multiplier = 1 + 1 / gameObject.GetComponent<FishMovement>().speed;
        }
        LevelManager.manager.CatchFish(fishName,  (int)(score * multiplier));
        Destroy(gameObject);
    }

}
