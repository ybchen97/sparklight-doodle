using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharactorController : MonoBehaviour
{
    // public AnimatorController animatorControllerScript;
    public Animator mainCharacterAnimation;
    // Start is called before the first frame update
    void Start()
    {
        mainCharacterAnimation.SetInteger("animation", 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col) {

        print(col.gameObject.name);
        
        if (col.gameObject.name == "Monster") {
            // TODO: remove a heart

            // main character movement
            mainCharacterAnimation.SetInteger("animation", 4);
            for(int cnt = 0; cnt <= 30; cnt ++)
            {
                print("cnt is" + cnt);
            }
            mainCharacterAnimation.SetInteger("animation", 1);
        }
    }
}
