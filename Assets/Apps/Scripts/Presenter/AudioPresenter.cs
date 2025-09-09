using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AudioPresenter : IDisposable
{
    private IDIContainer container = default;

    private AudioComponent objAudio = default;
    private CancellationTokenSource cts = default;

    private string _currentMusic;

    public AudioPresenter(IDIContainer container)
    {
        this.container = container;
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();

        ObjectLoader.SafeRelease("Object", ref objAudio);

    }

    public async UniTaskVoid ShowAudioAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        var main = Main.Instance.gameObject;

        objAudio ??= await ObjectLoader.Instantiate<AudioComponent>("Object", main.transform, token: cts.Token);
    }

    public void PlayMusic(string music)
    {
        if (_currentMusic != music)
        {
            _currentMusic = music;
            objAudio.PlayMusic(music);
        }
    }

    public void PlaySFX(string sound)
    {
        objAudio.PlaySound(sound);
    }

    public void StopMusic()
    {
        objAudio.StopMusic();
    }

    public float GetMusicVolumn() { return objAudio != null ? objAudio.MusicVolume : 0; }
    public void SetMusicVolumn(float val) { if ( objAudio != null ) objAudio.MusicVolume = val; }
    public float GetSFXVolumn() { return objAudio != null ? objAudio.SfxVolume : 0; }
    public void SetSFXVolumn(float val) { if (objAudio != null) objAudio.SfxVolume = val; }
}
