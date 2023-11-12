using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotManager : MonoBehaviour {
    
    public Camera mainCamera;
    
    public GameObject starPrefab;

    public bool debugMode;

    public void Shoot(Vector3 direction, Vector3 position) {
        
        GameObject star = Instantiate(starPrefab, position, Quaternion.identity);
        Star script = star.GetComponent<Star>();
        if (script != null) {
            script.SetDirection(direction);
        }
    }

    void DebugShoot() {

        Vector3 direction = mainCamera.transform.forward;
        GameObject star = Instantiate(starPrefab, mainCamera.transform.position, Quaternion.identity);
        Star script = star.GetComponent<Star>();
        if (script != null) {  
        script.SetDirection(direction);
        }

  }

    void Update() {
    if (debugMode) {
      if (Input.GetKeyDown(KeyCode.Space)) {
        DebugShoot();
      }
    }
  }

}