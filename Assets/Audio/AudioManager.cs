using UnityEngine;

// adapted off of a tutorial on audio by BRACKEYS : https://www.youtube.com/watch?v=6OT43pvUyfY

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Sound[] sounds;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            var source = gameObject.AddComponent<AudioSource>();

            source.clip = s.clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = false;
            source.spatialBlend = 0f;
            s.source = source;
        }

    }

    private void Start()
    {
            Play("Ambient", loop: true);
    }
    public void Play(string name, bool loop = false)
    {
        var s = System.Array.Find(sounds, x => x.name == name);
        if (s == null || s.source == null) return;
        s.source.loop = loop;
        if (!s.source.isPlaying) s.source.Play();
    }

    public void Stop(string name)
    {
        var s = System.Array.Find(sounds, x => x.name == name);
        if (s == null || s.source == null) return;
        s.source.Stop();
    }

    public void SetPitch(string name, float pitch)
    {
        var s = System.Array.Find(sounds, x => x.name == name);
        if (s == null || s.source == null) return;
        s.source.pitch = pitch;
    }
}
