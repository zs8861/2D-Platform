using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatDestroy : MonoBehaviour
{
    public int batFlag;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        EasterEgg.Password += batFlag.ToString();
        //Debug.Log("蝙蝠" + batFlag + "死掉了");
        //Debug.Log(EasterEgg.Password);
    }
}
