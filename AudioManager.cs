using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    bool sfxMuted;
    bool musicMuted;
    bool ambienceMuted;
    //
    int maxVolume = 0;
    int muteVolume = -80;
    float minSFXPitch = 0.88f;
    float maxSFXPitch = 1.03f;
    int crossfadeDuration = 3;
    //
    public AudioMixer mainMixer;
    public AudioSource sfxSource;
    public AudioSource ogPitchSFX;
    public AudioSource musicSource;
    public AudioSource dialogueSource;
    public AudioSource ambienceSource;
    public AudioSource crossFadeSource;
    //
    string intro = "introLoop";
    string deathSFX = "deathSFX";
    string deathLoop = "deathLoop";
    string ambience = "dungeonsAmbience";
    string forestLoop = "forestAmbience";
    //
    string sfxVolume = "SFXVolume";
    string musicVolume = "MusicVolume";
    string ambienceVolume = "AmbienceVolume";
    string dialogueVolume = "DialogueVolume";
    //
    public ClipHolder[] uiSFX;
    public ClipHolder[] generalSFX;
    public ClipHolder[] ambienceTracks;
    public ClipHolder[] backgroundMusic;
    public ClipHolder[] brokenWoodSFX;
    public ClipHolder[] brokenVaseSFX;
    //
    WaitForSeconds waitToStopSource = new WaitForSeconds(3);
    //
    Dictionary<AudioType, AudioSource> source = new Dictionary<AudioType, AudioSource>();
    public Dictionary<string, ClipHolder> uiSounds = new Dictionary<string, ClipHolder>();
    public Dictionary<string, ClipHolder> generalSounds = new Dictionary<string, ClipHolder>();
    public Dictionary<string, ClipHolder> ambienceSounds = new Dictionary<string, ClipHolder>();
    public Dictionary<string, ClipHolder> musicDictionary = new Dictionary<string, ClipHolder>();

    //

    void Awake()
    {
        source.Add(AudioType.SFX, sfxSource);
        source.Add(AudioType.Music, musicSource);
        source.Add(AudioType.Dialogue, dialogueSource);
        source.Add(AudioType.Ambience, ambienceSource);
        for (int i = 0; i < uiSFX.Length; i++) uiSounds.Add(uiSFX[i].name, uiSFX[i]);
        for (int i = 0; i < generalSFX.Length; i++) generalSounds.Add(generalSFX[i].name, generalSFX[i]);
        for (int i = 0; i < ambienceTracks.Length; i++) ambienceSounds.Add(ambienceTracks[i].name, ambienceTracks[i]);
        for (int i = 0; i < backgroundMusic.Length; i++) musicDictionary.Add(backgroundMusic[i].name, backgroundMusic[i]);
    }
    //
    void OnEnable() => GM.i.events.OnPlayerDied += HandlePlayerDeath;
    void FadeOutAmbience() => RunAudioJob(ambienceSounds[ambience], AudioAction.Stop, true);
    public void FadeOutMusic() => RunAudioJob(musicDictionary[intro], AudioAction.Stop, true);
    public void FadeInAmbience()
    {
        if (GM.i.inSanctuary)
        {
            if (GM.i.saveManager.unlockedEnchantedRealm) RunAudioJob(ambienceSounds[forestLoop], AudioAction.Play, true);
            else RunAudioJob(ambienceSounds[ambience], AudioAction.Play, true);
        }
        else RunAudioJob(ambienceSounds[ambience], AudioAction.Play, true);
    }
    public void Play(ClipHolder clip, bool fade = false, float volume = 0) => RunAudioJob(clip, AudioAction.Play, fade, volume);
    public void Stop(ClipHolder clip, bool fade = false) => RunAudioJob(clip, AudioAction.Stop, fade);
    public void PlaySFXWithoutPitchShift(ClipHolder clip, float volume = 0)
    {
        if (volume == 0) ogPitchSFX.PlayOneShot(clip.clip, clip.clipVolume);
        else ogPitchSFX.PlayOneShot(clip.clip, volume);
    }
    //
    void RunAudioJob(ClipHolder clipHolder, AudioAction action, bool fade = false, float vol = 0)
    {
        switch (action)
        {
            case AudioAction.Play:
                if (clipHolder.audioType == AudioType.Music || clipHolder.audioType == AudioType.Ambience)
                {
                    var index = clipHolder.audioType;
                    source[index].clip = clipHolder.clip;
                    source[index].volume = clipHolder.clipVolume;
                    source[index].Play();
                }
                //
                else if (clipHolder.audioType == AudioType.SFX)
                {
                    var sfxPlayer = source[AudioType.SFX];
                    sfxPlayer.pitch = Random.Range(minSFXPitch, maxSFXPitch);
                    if (vol == 0) sfxPlayer.PlayOneShot(clipHolder.clip, clipHolder.clipVolume);
                    else sfxPlayer.PlayOneShot(clipHolder.clip, vol);
                }
                else source[clipHolder.audioType].PlayOneShot(clipHolder.clip, clipHolder.clipVolume);
                break;
            //
            case AudioAction.Stop when !fade:
                source[clipHolder.audioType].Stop();
                break;
        }
        //
        if (fade)
        {
            AudioSource targetSource = source[clipHolder.audioType];
            float targetVolume = clipHolder.clipVolume;
            //
            if (action == AudioAction.Play) targetSource.volume = 0;
            else
            {
                targetVolume = 0;
                StartCoroutine(WaitToStopSource(targetSource));
            }
            targetSource.DOFade(targetVolume, 1).SetEase(Ease.InCubic);
        }
    }
    //
    public void CrossFadeMusic(ClipHolder clipHolder = null)
    {
        AudioSource crossSource;
        AudioSource playingSource;
        //
        if (musicSource.isPlaying)
        {
            playingSource = musicSource;
            crossSource = crossFadeSource;
        }
        else
        {
            crossSource = musicSource;
            playingSource = crossFadeSource;
        }
        //
        if (clipHolder == null)
        {
            playingSource.DOFade(0, 1).SetEase(Ease.InCubic);
            StartCoroutine(WaitToStopSource(playingSource));
            return;
        }
        //
        float target = clipHolder.clipVolume;
        //
        crossSource.clip = clipHolder.clip;
        crossSource.volume = 0;
        crossSource.Play();
        //
        playingSource.DOFade(0, crossfadeDuration).SetEase(Ease.InCubic);
        crossSource.DOFade(target, crossfadeDuration).SetEase(Ease.InCubic);
        StartCoroutine(WaitToStopSource(playingSource));
    }
    //
    public IEnumerator WaitToStopSource(AudioSource sourceToStop)
    {
        yield return waitToStopSource;
        sourceToStop.Stop();
    }
    //
    void HandlePlayerDeath()
    {
        FadeOutAmbience();
        Play(musicDictionary[deathLoop], true);
        PlaySFXWithoutPitchShift(generalSounds[deathSFX]);
        if (GM.i.pManager.inBossArena) GM.i.realmManager.bossMusic.Stop();
    }
    //
    enum AudioAction
    {
        Play,
        Stop
    }
    //
    bool ToggleAudioBool(bool muted) => muted ? false : true;
    public void ToggleMusic()
    {
        if (musicMuted) mainMixer.SetFloat(musicVolume, 0);
        else mainMixer.SetFloat(musicVolume, -80);
        //
        musicMuted = ToggleAudioBool(musicMuted);
    }
    //
    public void ToggleSFX()
    {
        if (sfxMuted)
        {
            mainMixer.SetFloat(sfxVolume, maxVolume);
            mainMixer.SetFloat(dialogueVolume, maxVolume);
        }
        else
        {
            mainMixer.SetFloat(sfxVolume, muteVolume);
            mainMixer.SetFloat(dialogueVolume, muteVolume);
        }
        //
        sfxMuted = ToggleAudioBool(sfxMuted);
    }
    //
    public void ToggleAmbience()
    {
        if (ambienceMuted) mainMixer.SetFloat(ambienceVolume, maxVolume);
        else mainMixer.SetFloat(ambienceVolume, muteVolume);
        //
        ambienceMuted = ToggleAudioBool(ambienceMuted);
    }
    //
    public void PlayBrokenPropSFX(bool isWood, Vector3 pos)
    {
        ClipHolder randSound;
        if (isWood) randSound = brokenWoodSFX[Random.Range(0, brokenWoodSFX.Length)];
        else randSound = brokenVaseSFX[Random.Range(0, brokenWoodSFX.Length)];
        RunAudioJob(randSound, AudioAction.Play);
    }
    //
    void OnDisable() => GM.i.events.OnPlayerDied -= HandlePlayerDeath;
}

//

public enum AudioType
{
    Dialogue,
    Ambience,
    Music,
    SFX
}
//
[System.Serializable]
public class ClipHolder
{
    public string name;
    public AudioClip clip;
    public float clipVolume;
    public AudioType audioType;
}
