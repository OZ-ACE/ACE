using Cysharp.Threading.Tasks;
using Unity.AppUI.UI;
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

    public string GetBGMPath(string clip)
    {
        return $"Audio/BGM/{clip}";
    }

    public string GetSFXPath(string clip)
    {
        return $"Audio/SFX/{clip}";
    }

    public void PlayBGM(string clip)
    {
        LoadAndPlayAudioClip(Audio_BGM, GetBGMPath(clip), true).Forget();
    }

    public void PlaySFX(string clip)
    {
        LoadAndPlayAudioClip(Audio_SFX, GetSFXPath(clip)).Forget();
    }

    public void PlayTypingSound()
    {
        LoadAndPlayAudioClip(Audio_SFX, GetSFXPath("Typing"), true).Forget();
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
