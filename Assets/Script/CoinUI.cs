using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    public int startCoinQuantity;
    public Text coinQuantity;

    public static int CurrentCoinQuantity;

    // Start is called before the first frame update
    void Start()
    {
        CurrentCoinQuantity = startCoinQuantity;
    }

    // Update is called once per frame
    void Update()
    {
        coinQuantity.text = CurrentCoinQuantity.ToString();
    }
}
