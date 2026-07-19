using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonBase<SoundManager>
{
    [SerializeField] private AudioMixer Audio_Mixer;
    [SerializeField] private AudioSource Audio_BGM;
    [SerializeField] private AudioSource Audio_SFX;

    public async UniTaskVoid LoadAndPlayAudioClip(AudioSource audioSource, string audioPath, bool isLoop = false)
    {
        AudioClip clip = await ResourceManager.Inst.LoadAsset<AudioClip>(audioPath);

        if (isLoop == true)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayBGM(string clip)
    {
        LoadAndPlayAudioClip(Audio_BGM, clip, true).Forget();
    }

    public void PlaySFX(string clip)
    {
        LoadAndPlayAudioClip(Audio_BGM, clip).Forget();
    }

    public void StopBGM()
    {
        Audio_BGM.Stop();
    }

    public void StopSFX()
    {
        Audio_SFX.Stop();
    }

    public AudioMixer GetAudioMixer()
    {
        return Audio_Mixer;
    }
}
