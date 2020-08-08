using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPlatform : MonoBehaviour
{
    private BoxCollider2D bx2D;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        bx2D = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") 
            && other.GetType().ToString() == "UnityEngine.BoxCollider2D")
        {
            anim.SetTrigger("Collapse");
        }
    }

    void DisableBoxCollider()
    {
        bx2D.enabled = false;
    }

    void DestroyTrapPlatform()
    {
        Destroy(gameObject);
    }
}
