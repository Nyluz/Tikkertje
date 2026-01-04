using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [HideInInspector] public FMOD.Studio.EventInstance fuseEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartFuse()
    {
        LoopSound(ref fuseEvent);
    }

    public void StopFuse()
    {
        StopLoop(ref fuseEvent);
    }

    public static void PlaySound(Transform t, string eventName)
    {
        var evt = FMODUnity.RuntimeManager.CreateInstance(eventName);

        evt.setListenerMask(0xFFFFFFFF);
        evt.set3DAttributes(
            FMODUnity.RuntimeUtils.To3DAttributes(t)
        );
        evt.start();
        evt.release();
    }

    public void LoopSound(ref FMOD.Studio.EventInstance soundEvent)
    {
        soundEvent = FMODUnity.RuntimeManager.CreateInstance("event:/Fuse");
        soundEvent.start();
    }

    public void StopLoop(ref FMOD.Studio.EventInstance soundEvent)
    {
        soundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        soundEvent.release();
    }
}
