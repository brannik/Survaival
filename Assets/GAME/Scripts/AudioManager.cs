using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("----AUDIO SOURCE----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource SFXRepeatingSource;
    [Header("----AUDIO CLIP----")]
    public AudioClip background;
    public AudioClip coinPickup;
    public AudioClip openPage;
    public AudioClip armorPickup;
    public AudioClip warning;
    public AudioClip purchase;
    public AudioClip buttonPressed;


    private static AudioManager instance;

    // Static singleton property
    public static AudioManager Instance
    {
        // Here we use the ?? operator, to return 'instance' if 'instance' does not equal null
        // otherwise we assign instance to a new component and return that
        get { return instance ; }
    }
    private void Awake()
    {
        // Ensure there's only one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
    void Start(){
        musicSource.clip = background;
        musicSource.Play();
    }
    public void PlaySFX(AudioClip clip){
        SFXSource.PlayOneShot(clip);
    }
    public void StopSFX(){
        SFXSource.Stop();
    }
    public void PlayRepeatingSFX(AudioClip clip){
        SFXRepeatingSource.clip = clip;
        SFXRepeatingSource.Play();
    }
    public void StopRepeatingSFX(){
        if(SFXRepeatingSource.isPlaying) SFXRepeatingSource.Stop();
    }
}
