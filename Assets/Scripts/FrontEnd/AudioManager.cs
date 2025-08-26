using UnityEngine;
using Sirenix.OdinInspector;

public class AudioManager : SingletonManager<AudioManager>{
    [BoxGroup("音源列表"), LabelText("背景音乐")] public AudioSource backgroundMusic;
    [BoxGroup("音源列表"), LabelText("音效")] public AudioSource sfxSource;

    [LabelText("默认背景音乐")] public AudioClip defaultBGM;
    [LabelText("NPC音效列表")] public AudioClip[] npcSFXs;

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    [Button("播放NPC音效")]
    public void PlayNPCSFX(float volume = 1f){
        int l = npcSFXs.Length;
        int randomIndex = Random.Range(0, l);
        PlaySFX(npcSFXs[randomIndex], volume);
    }

}