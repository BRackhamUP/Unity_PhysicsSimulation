using UnityEngine;

// adapted from a tutorial by Brackey - https://www.youtube.com/watch?v=6OT43pvUyfY
[System.Serializable]
public class Sound
{
    // audio clip and name of audio clip in the inspector
    public string name;
    public  AudioClip clip;

    // ensure pitch and volume can be adjusted in a range from 0-1
    [Range(0f, 1f)] 
    public float volume;
    [Range(0.1f, 3f)] 
    public float pitch;

    [HideInInspector]
    public AudioSource source;
}
