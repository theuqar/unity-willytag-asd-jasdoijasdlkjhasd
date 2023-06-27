using System;
using GorillaNetworking;
using UnityEngine;

public class SynchedMusicController : MonoBehaviour
{
	public int mySeed;

	public System.Random randomNumberGenerator = new System.Random();

	public long minimumWait = 900000L;

	public int randomInterval = 600000;

	public long[] songStartTimes;

	public int[] audioSourcesForPlaying;

	public int[] audioClipsForPlaying;

	public AudioSource audioSource;

	public AudioSource[] audioSourceArray;

	public AudioClip[] songsArray;

	public int lastPlayIndex;

	public long currentTime;

	public bool isMuted;

	public long totalLoopTime;

	public GorillaPressableButton muteButton;

	public bool usingMultipleSongs;

	public bool usingMultipleSources;

	public bool isPlayingCurrently;

	public bool testPlay;

	public bool twoLayer;

	public string locationName;

	private void Start()
	{
		totalLoopTime = 0L;
		AudioSource[] array = audioSourceArray;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mute = PlayerPrefs.GetInt(locationName + "Muted", 0) != 0;
		}
		audioSource.mute = PlayerPrefs.GetInt(locationName + "Muted", 0) != 0;
		muteButton.isOn = audioSource.mute;
		muteButton.UpdateColor();
		randomNumberGenerator = new System.Random(mySeed);
		GenerateSongStartRandomTimes();
	}

	private void Update()
	{
		isPlayingCurrently = audioSource.isPlaying;
		if (testPlay)
		{
			testPlay = false;
			if (usingMultipleSources && usingMultipleSongs)
			{
				audioSource = audioSourceArray[UnityEngine.Random.Range(0, audioSourceArray.Length)];
				audioSource.clip = songsArray[UnityEngine.Random.Range(0, songsArray.Length)];
				audioSource.time = 0f;
			}
			if (twoLayer)
			{
				StartPlayingSongs(0L, 0L);
			}
			else
			{
				audioSource.Play();
			}
		}
		if (GorillaComputer.instance.startupMillis == 0L)
		{
			return;
		}
		currentTime = (GorillaComputer.instance.startupMillis + (long)(Time.realtimeSinceStartup * 1000f)) % totalLoopTime;
		if (audioSource.isPlaying)
		{
			return;
		}
		if (songStartTimes[lastPlayIndex] < currentTime && currentTime < songStartTimes[(lastPlayIndex + 1) % songStartTimes.Length])
		{
			if (twoLayer)
			{
				if (songStartTimes[lastPlayIndex] + (long)(audioSource.clip.length * 1000f) > currentTime)
				{
					StartPlayingSongs(songStartTimes[lastPlayIndex], currentTime);
				}
			}
			else if (usingMultipleSongs && usingMultipleSources)
			{
				if (songStartTimes[lastPlayIndex] + (long)(songsArray[audioClipsForPlaying[lastPlayIndex]].length * 1000f) > currentTime)
				{
					StartPlayingSong(songStartTimes[lastPlayIndex], currentTime, songsArray[audioClipsForPlaying[lastPlayIndex]], audioSourceArray[audioSourcesForPlaying[lastPlayIndex]]);
				}
			}
			else if (songStartTimes[lastPlayIndex] + (long)(audioSource.clip.length * 1000f) > currentTime)
			{
				StartPlayingSong(songStartTimes[lastPlayIndex], currentTime);
			}
			return;
		}
		for (int i = 0; i < songStartTimes.Length; i++)
		{
			if (songStartTimes[i] > currentTime)
			{
				lastPlayIndex = (i - 1) % songStartTimes.Length;
				break;
			}
		}
	}

	private void StartPlayingSong(long timeStarted, long currentTime)
	{
		audioSource.Play();
		audioSource.time = (float)(currentTime - timeStarted) / 1000f;
	}

	private void StartPlayingSongs(long timeStarted, long currentTime)
	{
		AudioSource[] array = audioSourceArray;
		foreach (AudioSource obj in array)
		{
			obj.Play();
			obj.time = (float)(currentTime - timeStarted) / 1000f;
		}
	}

	private void StartPlayingSong(long timeStarted, long currentTime, AudioClip clipToPlay, AudioSource sourceToPlay)
	{
		audioSource = sourceToPlay;
		sourceToPlay.clip = clipToPlay;
		sourceToPlay.Play();
		sourceToPlay.time = (float)(currentTime - timeStarted) / 1000f;
	}

	private void GenerateSongStartRandomTimes()
	{
		songStartTimes = new long[500];
		audioSourcesForPlaying = new int[500];
		audioClipsForPlaying = new int[500];
		songStartTimes[0] = minimumWait + randomNumberGenerator.Next(randomInterval);
		for (int i = 1; i < songStartTimes.Length; i++)
		{
			songStartTimes[i] = songStartTimes[i - 1] + minimumWait + randomNumberGenerator.Next(randomInterval);
		}
		if (usingMultipleSources)
		{
			for (int j = 0; j < audioSourcesForPlaying.Length; j++)
			{
				audioSourcesForPlaying[j] = randomNumberGenerator.Next(audioSourceArray.Length);
			}
		}
		if (usingMultipleSongs)
		{
			for (int k = 0; k < audioClipsForPlaying.Length; k++)
			{
				audioClipsForPlaying[k] = randomNumberGenerator.Next(songsArray.Length);
			}
		}
		if (usingMultipleSongs)
		{
			totalLoopTime = songStartTimes[songStartTimes.Length - 1] + (long)(songsArray[audioClipsForPlaying[audioClipsForPlaying.Length - 1]].length * 1000f);
		}
		else
		{
			totalLoopTime = songStartTimes[songStartTimes.Length - 1] + (long)(audioSource.clip.length * 1000f);
		}
	}

	public void MuteAudio(GorillaPressableButton pressedButton)
	{
		if (audioSource.mute)
		{
			PlayerPrefs.SetInt(locationName + "Muted", 0);
			PlayerPrefs.Save();
			audioSource.mute = false;
			AudioSource[] array = audioSourceArray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].mute = false;
			}
			pressedButton.isOn = false;
			pressedButton.UpdateColor();
		}
		else
		{
			PlayerPrefs.SetInt(locationName + "Muted", 1);
			PlayerPrefs.Save();
			audioSource.mute = true;
			AudioSource[] array = audioSourceArray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].mute = true;
			}
			pressedButton.isOn = true;
			pressedButton.UpdateColor();
		}
	}
}
