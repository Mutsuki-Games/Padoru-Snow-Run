using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("元件")]
    public AudioSource bgmSource;
    public AudioSource effectSource;

    [Header("音訊")]
    public AudioClip title;
    public AudioClip gaming;
    public AudioClip dead;
    public AudioClip ding;

    private string nowBgm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void PlayDead()
    {
        effectSource.Stop();
        effectSource.clip = dead;
        effectSource.Play();
    }

    public void PlayDing()
    {
        effectSource.Stop();
        effectSource.clip = ding;
        effectSource.Play();
    }

    public void SetBgmNone()
    {
        bgmSource.Stop();
        nowBgm = string.Empty;
    }

    public void SetBgmTitle()
    {
        bgmSource.Stop();
        nowBgm = "title";
    }

    public void SetBgmGaming()
    {
        bgmSource.Stop();
        nowBgm = "gaming";
    }

    // Update is called once per frame
    void Update()
    {
        if (!bgmSource.isPlaying)
        {
            if (nowBgm == "title")
            {
                bgmSource.clip = title;
                bgmSource.Play();
            }
            else if (nowBgm == "gaming")
            {
                bgmSource.clip = gaming;
                bgmSource.Play();
            }
        }
    }
}
