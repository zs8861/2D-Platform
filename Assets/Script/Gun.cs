using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GameObject bullet;
    public Transform muzzleTransform;
    public Camera cam;

    private Vector3 mousePos;
    private Vector2 gunDirection;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        gunDirection = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(gunDirection.y, gunDirection.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            //Debug.Log("鼠标左键已经按下");
            //生成一颗子弹
            Instantiate(bullet, muzzleTransform.position, Quaternion.Euler(transform.eulerAngles));
        }
    }
}
