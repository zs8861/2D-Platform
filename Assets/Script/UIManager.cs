using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform UI_Element;
    public RectTransform CanvasRect;
    public Transform trashBinPos;
    public float xOffset;
    public float yOffset;
    public Text coinNumber;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(trashBinPos.position);
        Vector2 worldOjectScreenPos
            = new Vector2((viewportPos.x * CanvasRect.sizeDelta.x) -
                           (CanvasRect.sizeDelta.x * 0.5f) + xOffset,
                           (viewportPos.y * CanvasRect.sizeDelta.y) -
                           (CanvasRect.sizeDelta.y * 0.5f) + yOffset);
        UI_Element.anchoredPosition = worldOjectScreenPos;


    }
}
