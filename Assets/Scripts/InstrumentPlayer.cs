using UnityEngine;
using UnityEngine.Audio;

public class InstrumentPlayer : MonoBehaviour
{
    public AudioMixerGroup output;
    public int polyphony = 12;

    AudioSource[] pool;
    int head;

    void Awake()
    {
        pool = new AudioSource[polyphony];
        for (int i = 0; i < polyphony; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.outputAudioMixerGroup = output;
            pool[i] = src;
        }
    }

    public void PlayScheduled(AudioClip clip, double dspTime)
    {
        if (clip == null) return;
        var src = pool[head];
        head = (head + 1) % pool.Length;
        src.Stop();
        src.clip = clip;
        src.time = 0f;
        src.PlayScheduled(dspTime);
    }
}
