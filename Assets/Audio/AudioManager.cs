using UnityEngine;
// adapted from a tutorial on audio by BRACKEYS : https://www.youtube.com/watch?v=6OT43pvUyfY

public class AudioManager : MonoBehaviour
{
    // creating a reference to an instance of the audiomanager
    public static AudioManager Instance { get; private set; } 

    // list of sounds to easily add more in inspector
    public Sound[] sounds;

    void Awake()
    {
        // singleton to ensure only first audio manager is present
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // creating an audio source for every sound and applying defaults
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

    // play ambient on game start and loop 
    private void Start()
    {
            Play("Ambient", loop: true);
    }

    // Play method to call in vehicle interaction/controller scripts 
    public void Play(string name, bool loop = false)
    {
        // find the sound by name
        var s = System.Array.Find(sounds, x => x.name == name);
        s.source.loop = loop;

        // safety check to see if sound is playing
        if (!s.source.isPlaying) 
            s.source.Play();
    }

    // Stop method to call in vehicle interaction script
    public void Stop(string name)
    {
        // find sound by name
        var s = System.Array.Find(sounds, x => x.name == name);

        s.source.Stop();
    }

    // Pitch method to access and change the pitch of the sound
    public void SetPitch(string name, float pitch)
    {
        var s = System.Array.Find(sounds, x => x.name == name);
        s.source.pitch = pitch;
    }

    // method for changing the volume of the sounds
    public void SetVolume(string name, float volume)
    {
        var s = System.Array.Find(sounds, x => x.name == name);
        s.source.volume = Mathf.Clamp01(volume);
    }
}
