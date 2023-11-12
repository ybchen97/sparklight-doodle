using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotManager : MonoBehaviour {
    
    public Camera mainCamera;
    
    public GameObject starPrefab;

    public GameObject handManager;

    private Vector3 palmPosition;
    private Vector3 shotDirection; 

    public bool debugMode;

    public float timer=0;

    public void Shoot(Vector3 direction, Vector3 position) {
        
        GameObject star = Instantiate(starPrefab, position, Quaternion.identity);
        Star script = star.GetComponent<Star>();
        if (script != null) {
            script.SetDirection(direction);
        }
    }

    public void handShot(){
      Shoot(shotDirection, palmPosition);
    }

    private void updateShotParameter(){
      HandPoseListener _handPoseListener = handManager.GetComponent<HandPoseListener>();
      palmPosition = _handPoseListener.GetPalmPose();
      shotDirection = _handPoseListener.GetHandDirection();
      print("shotDirection");
      print( shotDirection);
    }

    void DebugShoot() {

        Vector3 direction = mainCamera.transform.forward;
        GameObject star = Instantiate(starPrefab, mainCamera.transform.position, Quaternion.identity);
        Star script = star.GetComponent<Star>();
        if (script != null) {  
        script.SetDirection(direction);
        }

    }

    void Start(){
    }

    void Update() {
      timer += Time.deltaTime;
      if (debugMode) {
        if (Input.GetKeyDown(KeyCode.Space)) {
          DebugShoot();
        }
      }
      if(timer>2.0f){
        updateShotParameter();
        handShot();
        timer = 0.0f;
      }
      
    }
    

}