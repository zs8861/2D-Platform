using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleHit : MonoBehaviour
{
    public GameObject sickle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            Instantiate(sickle, transform.position, transform.rotation);
        }
    }
}
