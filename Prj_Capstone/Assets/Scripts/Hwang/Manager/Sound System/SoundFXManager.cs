using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Random = System.Random;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource soundFXObject;

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float pitchDeviation = 0.0f, float volume = 1.0f)
    {
        Random rand = new Random();

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.pitch = 1.0f + (float)rand.NextDouble() * pitchDeviation * 2.0f - pitchDeviation;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlaySoundFXClip(IEnumerable<AudioClip> audioClips, Transform spawnTransform, float pitchDeviation = 0.0f, float volume = 1.0f)
    {
        Random rand = new Random();
        int index = rand.Next(audioClips.Count());

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClips.ElementAt(index);

        audioSource.pitch = 1.0f + (float)rand.NextDouble() * pitchDeviation * 2.0f - pitchDeviation;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    public void SetMasterVolume(float volume) => audioMixer.SetFloat("masterVolume", Mathf.Log10(volume + float.Epsilon) * 20.0f);

    public void SetSoundFXVolume(float volume) => audioMixer.SetFloat("soundFXVolume", Mathf.Log10(volume + float.Epsilon) * 20.0f);

    public void SetBGMVolume(float volume) => audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume + float.Epsilon) * 20.0f);
}
