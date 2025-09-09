using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioComponent : MonoBehaviour
{
	public static string AUDIO_MASTER_SOUND = "AUDIO_MASTER_SOUND";
	public static string AUDIO_MUSIC_SOUND = "AUDIO_MUSIC_SOUND";
	public static string AUDIO_SFX_SOUND = "AUDIO_SFX_SOUND";
	public static string AUDIO_MUSIC_MUTE = "AUDIO_MUSIC_MUTE";
	public static string AUDIO_SFX_MUTE = "AUDIO_SFX_MUTE";

	public float delayInCrossfading = 0.3f;

	public List<MusicTrack> tracks = new List<MusicTrack>();
	public List<Sound> sounds = new List<Sound>();

	private bool sfxMute;
	private bool musicMute;

	[SerializeField] private AudioSource music;
	[SerializeField] private AudioSource sfx;

	private Sound GetSoundByName(string sName) => sounds.Find(x => x.name == sName);

	private static readonly List<string> mixBuffer = new List<string>();
	private const float mixBufferClearDelay = 0.05f;

	internal string currentTrack;

	private float m_SoundAllVolume, m_MusicVolume, m_SfxVolume;

	public float SoundAllVolume { get { return m_SoundAllVolume; } set { setMasterVolume(value); } }

	public float MusicVolume { get { return m_MusicVolume; } set { setMusicVolume(value); } }

	public float SfxVolume { get { return m_SfxVolume; } set { setSfxVolume(value); } }

	private void Awake()
	{
		Initialized();
	}

	void OnDestroy()
	{
		PlayerPrefs.Save();

	}

	private void Initialized()
	{
		// Configuring Audio Source For Playing Music And SFX
		music.loop = true;

		MusicVolume = PlayerPrefs.GetFloat(AUDIO_MUSIC_SOUND, 1f);
		SfxVolume = PlayerPrefs.GetFloat(AUDIO_SFX_SOUND, 1f);
		SoundAllVolume = PlayerPrefs.GetFloat(AUDIO_MASTER_SOUND, 1f);

		sfxMute = false;
		musicMute = false;


		// Checks If The sfxMute Is True Or Not
		if (PlayerPrefs.GetInt(AUDIO_SFX_MUTE, 0) == 1)
		{
			SfxToggle();
		}
		// Checks If The musicMute Is True Or Not
		if (PlayerPrefs.GetInt(AUDIO_MUSIC_MUTE, 0) == 1)
		{
			MusicToggle();
		}

		StartCoroutine(MixBufferRoutine());
	}

	// Responsible for limiting the frequency of playing sounds
	private IEnumerator MixBufferRoutine()
	{
		float time = 0;

		while (true)
		{
			time += Time.unscaledDeltaTime;
			yield return 0;
			if (time >= mixBufferClearDelay)
			{
				mixBuffer.Clear();
				time = 0;
			}
		}
	}

	// Play a music track with Cross fading
	public void PlayMusic(string trackName)
	{
		if (trackName != "")
			currentTrack = trackName;
		AudioClip to = null;
		foreach (MusicTrack track in tracks)
			if (track.name == trackName)
				to = track.track;

		StartCoroutine(CrossFade(to));
	}

	// Cross fading - Smooth Transition When Track Is Switched
	private IEnumerator CrossFade(AudioClip to)
	{
		if (music.clip != null)
		{
			while (delayInCrossfading > 0)
			{
				music.volume = delayInCrossfading * SoundAllVolume * MusicVolume;
				delayInCrossfading -= Time.unscaledDeltaTime;
				yield return 0;
			}
		}
		music.clip = to;
		if (to == null)
		{
			music.Stop();
			yield break;
		}
		delayInCrossfading = 0;

		if (!music.isPlaying)
			music.Play();

		while (delayInCrossfading < 1f)
		{
			music.volume = delayInCrossfading * SoundAllVolume * MusicVolume;
			delayInCrossfading += Time.unscaledDeltaTime;
			yield return 0;
		}
		music.volume = SoundAllVolume * MusicVolume;
	}

	// Sfx Button On/Off
	public void SfxToggle()
	{
		sfxMute = !sfxMute;
		sfx.mute = sfxMute;

		PlayerPrefs.SetInt(AUDIO_SFX_MUTE, RandomUtil.BoolToBinary(sfxMute));
		PlayerPrefs.Save();
	}

	// Music Button On/Off
	public void MusicToggle()
	{
		musicMute = !musicMute;
		music.mute = musicMute;

		PlayerPrefs.SetInt(AUDIO_MUSIC_MUTE, RandomUtil.BoolToBinary(musicMute));
		PlayerPrefs.Save();
	}

	// A single msound effect
	public void PlaySound(string clip)
	{
		Sound sound = GetSoundByName(clip);

		if (sound != null && !mixBuffer.Contains(clip))
		{
			if (sound.clips.Count == 0)
				return;
			mixBuffer.Add(clip);
			if (sound.clips != default)
			{
				sfx.PlayOneShot(sound.clips
					.GetRandom()); // Randomly Play Sound Each Time Through The Array Of clip
			}
		}
	}

	public void PlaySoundHasLoop(string clip)
	{
		Sound sound = GetSoundByName(clip);

		if (sound != null && !mixBuffer.Contains(clip))
		{
			if (sound.clips.Count == 0)
				return;
			mixBuffer.Add(clip);
			sfx.loop = true;
			sfx.clip = sound.clips.GetRandom();
			sfx.Play();
		}
	}

	public bool IsPlayingSoundLoop(string clip)
	{
		if (mixBuffer.Contains(clip) && sfx.loop) return true;
		return false;
	}

	public float GetDurationOfSound(string clip)
	{
		Sound sound = GetSoundByName(clip);
		return sound.clips[0].length;
	}

	public void StopSound()
	{
		sfx.loop = false;
		sfx.Stop();
	}

	public void StopMusic()
	{
		if (music != default) music.Stop();
	}

	public void PauseMusic()
	{
		music?.Pause();
	}

	public void ResumeMusic()
	{
		music?.Play();
	}

	private void setMasterVolume(float value)
	{
		m_SoundAllVolume = value;
		// Check If Sfx Volume Is Not 0
		if (Math.Abs(SoundAllVolume) > 0.05f)
		{
			// Set The Saved Value Of SFX Volume
			sfx.volume = SoundAllVolume * SfxVolume;
			music.volume = SoundAllVolume * MusicVolume;
		}
		// Set The Values To 0
		else
		{
			sfx.volume = 0;
			music.volume = 0;
		}
		PlayerPrefs.SetFloat(AUDIO_MASTER_SOUND, m_SoundAllVolume);
		PlayerPrefs.Save();
	}

	private void setMusicVolume(float value)
	{
		m_MusicVolume = value;
		// Check If Music Volume Is Not 0
		if (Math.Abs(MusicVolume) > 0.05f)
		{
			// Set The Saved Value Of Music Volume
			music.volume = MusicVolume * SoundAllVolume;
		}
		// Set The Values To 0
		else
		{
			music.volume = 0;
		}
		PlayerPrefs.SetFloat(AUDIO_MUSIC_SOUND, m_MusicVolume);
		PlayerPrefs.Save();
	}

	private void setSfxVolume(float value)
	{
		m_SfxVolume = value;
		// Check If sound all Volume Is Not 0
		if (Math.Abs(SfxVolume) > 0.05f)
		{
			sfx.volume = SfxVolume * SoundAllVolume;
		}
		// Set The Values To 0
		else
		{
			sfx.volume = 0;
		}
		PlayerPrefs.SetFloat(AUDIO_SFX_SOUND, m_SfxVolume);
		PlayerPrefs.Save();
	}



	[Serializable]
	public class MusicTrack
	{
		public string name;
		public AudioClip track;
	}

	[Serializable]
	public class Sound
	{
		public string name;
		public List<AudioClip> clips = new List<AudioClip>();
	}
}
