using System.Collections;
using System.Collections.Generic;
using DigitalRuby.SoundManagerNamespace;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController _instance;
    public static  AudioController Instance => _instance;

    public AudioSource music;

    public AudioSource walk, dash, jump, hit, collect, bullet, click;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        if (PlayerPrefs.HasKey("volume")) { PlayerPrefs.SetFloat("volume", 1f); }
    }

    public void PlayMusic() { music.PlayLoopingMusicManaged(.55f, 1.0f, true); }

    public void SfxWalk()
    {
        walk.pitch = Random.Range(.8f, 1.2f);
        
        walk.PlayOneShotSoundManaged(walk.clip);
    }
    
    public void SfxDash()
    {
        dash.PlayOneShotSoundManaged(dash.clip);
    }
    
    public void SfxJump()
    {
        jump.PlayOneShotSoundManaged(jump.clip);
    }
    
    public void SfxHit()
    {
        hit.PlayOneShotSoundManaged(hit.clip);
    }
    
    public void SfxCollect()
    {
        collect.PlayOneShotSoundManaged(collect.clip);
    }
    
    public void SfxBullet()
    {
        bullet.pitch = Random.Range(.8f, 1.2f);
        
        bullet.PlayOneShotSoundManaged(bullet.clip);
    }
    
    public void SfxClick()
    {
        click.PlayOneShotSoundManaged(click.clip);
    }
}