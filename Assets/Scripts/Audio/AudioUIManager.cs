using System.Collections;
using UnityEngine;

public class AudioUIManager : MonoBehaviour
{
    public static AudioUIManager Instance;

    [Header("General")]
    public AudioClip menuBackground;
    public AudioClip menuButtonClick;
    public AudioClip textPopup;
    public AudioClip dialogueReminder;
    public AudioClip buttonClick;
    public AudioClip scorePageBGM;

    private AudioSource _sfxSource;
    private AudioSource _bgmSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        var sources = GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _sfxSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            _bgmSource = sources[0];
            _sfxSource = sources[1];
        }

        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _sfxSource.playOnAwake = false;
    }


    public void PlayScorePageBGM(float volume = 0.2f, float delay = 0f)
    {
        if (scorePageBGM == null) return;
        if (_bgmSource.clip == scorePageBGM && _bgmSource.isPlaying) return;
        _bgmSource.clip = scorePageBGM;
        _bgmSource.volume = volume;
        _bgmSource.loop = false;
        _bgmSource.Play();
    }


    public void PlayMenuButtonClick(float volume = 1f, float delay = 0f)
    {
        PlaySFX(menuButtonClick, volume, delay);
    }

    public void PlayCommonButtonClick(float volume = 1f, float delay = 0f)
    {
        PlaySFX(buttonClick, volume, delay);
    }

    public void PlayTextPopup(float volume = 1f, float delay = 0f)
    {
        PlaySFX(textPopup, volume, delay);
    }

    public void PlayDialogueReminder(float volume = 1f, float delay = 0f)
    {
        PlaySFX(dialogueReminder, volume, delay);
    }

    private void PlaySFX(AudioClip clip, float volume = 1f, float delay = 0f)
    {
        if (clip == null) return;

        if (delay <= 0f)
        {
            _sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            StartCoroutine(PlaySFXDelayed(clip, volume, delay));
        }
    }
    private IEnumerator PlaySFXDelayed(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        _sfxSource.PlayOneShot(clip, volume);
    }
}
