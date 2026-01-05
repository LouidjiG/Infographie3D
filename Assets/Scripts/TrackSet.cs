using UnityEngine;

[CreateAssetMenu(fileName = "TrackSet", menuName = "LickCombat/Track Set")]
public class TrackSet : ScriptableObject
{
    public const int LoopLength = 64;
    public AudioClip[] melody = new AudioClip[LoopLength];
    public AudioClip[] drums  = new AudioClip[LoopLength];
    public AudioClip[] bass   = new AudioClip[LoopLength];

    void OnValidate()
    {
        if (melody == null || melody.Length != LoopLength) melody = new AudioClip[LoopLength];
        if (drums  == null || drums.Length  != LoopLength) drums  = new AudioClip[LoopLength];
        if (bass   == null || bass.Length   != LoopLength) bass   = new AudioClip[LoopLength];
    }
}
