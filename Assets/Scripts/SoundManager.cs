using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private AudioSource audioSource;

    public AudioClip slapSound;
    public AudioClip[] ragdollImpacts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(Transform t, string eventName)
    {
        var evt = FMODUnity.RuntimeManager.CreateInstance(eventName);

        evt.setListenerMask(0xFFFFFFFF);
        //evt.setListenerMask(1u << playerIndex);
        evt.set3DAttributes(
            FMODUnity.RuntimeUtils.To3DAttributes(t)
        );
        evt.start();
        evt.release();
    }
}
