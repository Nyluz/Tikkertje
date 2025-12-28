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

    public void PlaySlap()
    {
        audioSource.PlayOneShot(slapSound, .5f);
    }

    public void PlayImpactSound(Vector3 position)
    {
        var clip = ragdollImpacts[Random.Range(0, ragdollImpacts.Length)];

        GameObject temp = new GameObject("ImpactSound");
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = Random.Range(0.65f, 1.35f); // pitch variation
        source.spatialBlend = 1f; // 3D sound
        source.Play();

        Destroy(temp, clip.length / source.pitch);
    }


}
