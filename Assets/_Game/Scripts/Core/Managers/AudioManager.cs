using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource spinSource;
    
    public AudioClip backgroundMusic;
    public AudioClip clickClip;
    public AudioClip leverPullClip;
    public AudioClip spinningLoopClip;
    public AudioClip reelStopClip;
    
    public AudioClip winSmallClip;
    public AudioClip winBigClip;
    public AudioClip maskUnlockClip;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (musicSource.isPlaying) musicSource.Stop();
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("Playing BGM");
        }
    }

    public void PlayClick()
    {
        PlaySFX(clickClip);
    }

    public void PlayLeverPull()
    {
        PlaySFX(leverPullClip, 1.2f);
    }

    public void PlayReelStop()
    {
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        PlaySFX(reelStopClip);
        sfxSource.pitch = 1f;
    }

    public void PlayWin(bool isBigWin)
    {
        PlaySFX(isBigWin ? winBigClip : winSmallClip);
    }

    public void PlayUnlockMask()
    {
        PlaySFX(maskUnlockClip);
    }

    private void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }
    
    public void StartSpinningSound()
    {
        if (spinningLoopClip != null)
        {
            spinSource.clip = spinningLoopClip;
            spinSource.loop = true;
            spinSource.Play();
        }
    }

    public void StopSpinningSound()
    {
        spinSource.Stop();
    }
}
