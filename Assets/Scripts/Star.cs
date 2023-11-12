using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour {

    public Vector3 moveDirection;
    public float timeToDestroy = 5f;
    private float timer;
    public GameObject sparkPrefab;

    public float speed = 5f;
    private float initY;
    
    public void SetDirection(Vector3 direction) {
        moveDirection = direction.normalized;
        moveDirection[1] = 0f;
        initY = transform.position[1];

    }

    void Update() {
        if (moveDirection != Vector3.zero) {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
        timer += Time.deltaTime;

        if(timer >= timeToDestroy)
        {
            Destroy(gameObject);
        }
        transform.position = new Vector3(transform.position[0], initY, transform.position[2]);
    }

    void OnCollisionEnter(Collision col) {

        print(col.gameObject.name);
        
        if (col.gameObject.tag == "Monster") {

            // Call hurt on monster
            col.gameObject.GetComponent<Monster>().TakeDamage();  

            // Destroy star 
            Destroy(gameObject);
            
            // Instantiate spark prefab
            Instantiate(sparkPrefab, transform.position, Quaternion.identity);
            print("colided with monster");
        }

    }

}