using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharactorController : MonoBehaviour
{
    // public AnimatorController animatorControllerScript;
    public Animator mainCharacterAnimation;
    public GameObject cube;
    bool isAttacked = false;
    int cnt = 0;
    // Start is called before the first frame update
    void Start()
    {
        mainCharacterAnimation.SetInteger("animation", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (isAttacked) {
            cnt = 0;
            isAttacked = false;
        }
        cnt = cnt + 1;
        if (cnt > 800) {
            mainCharacterAnimation.SetInteger("animation", 1);
        }
        
    }

    void OnCollisionEnter(Collision col) {

        print(col.gameObject.name);
        
        if (col.gameObject.tag == "Monster") {
            // TODO: remove a heart

            // main character movement
            mainCharacterAnimation.SetInteger("animation", 4);
            Destroy(cube);
            isAttacked = true;
        }
    }
}
