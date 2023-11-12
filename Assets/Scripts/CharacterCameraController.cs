using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraController : MonoBehaviour
{
    public Camera camera;
    public GameObject mainCharacter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camera_up = camera.transform.up;
        Vector3 camera_right = camera.transform.right;
        Vector3 camera_forward = camera.transform.forward;
        mainCharacter.transform.right = camera_right;
        mainCharacter.transform.forward = camera_forward;
        mainCharacter.transform.up = camera_up;
        
    }
}
