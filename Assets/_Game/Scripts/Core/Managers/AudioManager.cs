using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Game.Scripts.Core.Managers
{
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
        public AudioClip patternMatchClip;
        
        public List<AudioClip> winClip;
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
    
        public void PlayPatternMatch()
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            PlaySFX(patternMatchClip);
            sfxSource.pitch = 1f;
            Debug.Log("Pattern match played");
        }
    
        public void PlayWin()
        {
            if (winClip != null && winClip.Count > 0)
            {
                PlaySFX(winClip[Random.Range(0, winClip.Count)]);
            }
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
}