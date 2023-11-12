using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainCharactorController : MonoBehaviour
{
    // public AnimatorController animatorControllerScript;
    public Animator mainCharacterAnimation;
    public Button restartButton;
    bool isAttacked = false;
    public GameObject[] heartArray;
    int cnt = 0;
    int heartLeft = 5;
    // Start is called before the first frame update
    void Start()
    {
        // Initially hide the restart button
        // restartButton.gameObject.SetActive(false);
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
        if (cnt > 400) {
            mainCharacterAnimation.SetInteger("animation", 1);
        }
        
    }

    void OnCollisionEnter(Collision col) {
        print("collisionnnn");

        print(col.gameObject.name);
        
        if (col.gameObject.tag == "Monster") {
            // TODO: remove a heart

            // main character movement
            mainCharacterAnimation.SetInteger("animation", 4);
            col.gameObject.GetComponent<Monster>().Attack();
            isAttacked = true;
            heartLeft = heartLeft - 1;
            removeAHeart(heartLeft);
        }
    }

    void removeAHeart(int heartIndex) {
        if (heartIndex == 0){
            // restart game 
            heartArray[heartIndex].SetActive(false);
            // restartButton.gameObject.SetActive(true);
            RestartGame();
        }
        else {
            heartArray[heartIndex].SetActive(false);
        }
    }

    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
