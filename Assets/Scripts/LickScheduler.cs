using UnityEngine;

public class LickScheduler : MonoBehaviour
{
    [Range(40,240)] public int bpm = 120;
    [Range(1,32)]  public int subdivisionsPerBeat = 4; // 16 pas/mesure → 4 subdivisions par temps
    public TrackSet tracks;
    public InstrumentPlayer melodyPlayer;
    public InstrumentPlayer drumsPlayer;
    public InstrumentPlayer bassPlayer;
    public bool autoPlay = true;
    [Range(0.01f,0.2f)] public double lookahead = 0.08;

    double stepDuration;
    double loopStartDspTime;
    long nextStepToSchedule;
    bool playing;

    void Start()
    {
        stepDuration = 60.0 / (bpm * subdivisionsPerBeat);
        if (autoPlay) Play();
    }

    void Update()
    {
        if (!playing || tracks == null) return;
        double now = AudioSettings.dspTime;
        double windowEnd = now + lookahead;

        while (true)
        {
            int step = (int)(nextStepToSchedule % TrackSet.LoopLength);
            double stepTime = loopStartDspTime + (nextStepToSchedule * stepDuration);
            if (stepTime > windowEnd) break;

            var m = tracks.melody[step];
            var d = tracks.drums[step];
            var b = tracks.bass[step];

            if (m != null && melodyPlayer != null) melodyPlayer.PlayScheduled(m, stepTime);
            if (d != null && drumsPlayer  != null) drumsPlayer.PlayScheduled(d, stepTime);
            if (b != null && bassPlayer   != null) bassPlayer.PlayScheduled(b, stepTime);

            nextStepToSchedule++;
            if (nextStepToSchedule >= long.MaxValue - 1024)
            {
                double progress = now - loopStartDspTime;
                long stepsElapsed = (long)System.Math.Floor(progress / stepDuration);
                loopStartDspTime = loopStartDspTime + stepsElapsed * stepDuration;
                nextStepToSchedule = stepsElapsed;
            }
        }
    }

    public void Play()
    {
        double now = AudioSettings.dspTime;
        stepDuration = 60.0 / (bpm * subdivisionsPerBeat);
        loopStartDspTime = System.Math.Ceiling(now / stepDuration) * stepDuration;
        nextStepToSchedule = 0;
        playing = true;
    }

    public void Stop()
    {
        playing = false;
    }

    public int CurrentStep
    {
        get
        {
            double now = AudioSettings.dspTime;
            double t = now - loopStartDspTime;
            if (t < 0) return 0;
            int k = (int)System.Math.Floor(t / stepDuration) % TrackSet.LoopLength;
            if (k < 0) k += TrackSet.LoopLength;
            return k;
        }
    }
}
