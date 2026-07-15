using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonBase<SoundManager>
{
    [SerializeField] private AudioMixer Audio_Mixer;

    public AudioMixer GetAudioMixer()
    {
        return Audio_Mixer;
    }
}
