using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymbolUI : MonoBehaviour
{
    // public Image[] prefabUIArray;
    public Image prefabUI;
    private Image  uiUse;

    // int GetRandomSymbol(float[] symbols)
    // {
    //     float total = 0;
    //     float randomChance = Random.Range(0f, 1f);
    //     for (int a = 0; a < symbols.Length; a++)
    //     {
    //         total += probs[a];
    //         if (randomChance <= total)
    //         {
    //             return a;
    //         }
    //     }
    //     return -1;
    // }

    // Start is called before the first frame update
    void Start()
    {
         uiUse = Instantiate(prefabUI, FindAnyObjectByType<Canvas>().transform).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        uiUse.transform.position = Camera.main.WorldToScreenPoint(transform.position);
    }
}
