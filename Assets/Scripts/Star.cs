using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour {

    public Vector3 moveDirection;
    public float timeToDestroy = 5f;
    private float timer;

    public float speed = 5f;
     
    public void SetDirection(Vector3 direction) {
        moveDirection = direction.normalized;
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
    }

}