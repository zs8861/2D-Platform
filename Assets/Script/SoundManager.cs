using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioSource audioSrc;
    public static AudioClip pickCoin;
    public static AudioClip throwCoin;
    

    // Start is called before the first frame update
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        pickCoin = Resources.Load<AudioClip>("PickCoin");
        throwCoin = Resources.Load<AudioClip>("ThrowCoin");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlayPickCoinClip()
    {
        audioSrc.PlayOneShot(pickCoin);
    }

    public static void PlayThrowCoinClip()
    {
        audioSrc.PlayOneShot(throwCoin);
    }
}
