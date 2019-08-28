using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CollisionSound : MonoBehaviour
{
    public List<AudioClip> collisionSounds = new List<AudioClip>();
    private List<AudioSource> audioSources;
    public float maxVolume = 0.5f;

    float lastSoundPlayTime;
    float minPauseBetweenSoundPlayTime = 1f;

    void Awake()
    {
        audioSources = new List<AudioSource>(GetComponents<AudioSource>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlaycollisionSound();
    }

    private void OnCollisionStay(Collision collision)
    {
        PlaycollisionSound();
    }

    void PlaycollisionSound() {
        AudioSource audioSource = audioSources[0];
        if (audioSource.isPlaying){
            if ((Time.time - lastSoundPlayTime) < minPauseBetweenSoundPlayTime) return;
            var audioFull = true;
            foreach (var otherSource in audioSources) {
                if (otherSource.isPlaying == false) {
                    audioFull = false;
                    audioSource = otherSource;
                    break;
                }
            }
            if (audioFull) return;
        }
        if (collisionSounds == null || collisionSounds.Count == 0) return;
        var clip = collisionSounds[Random.Range(0, collisionSounds.Count - 1)];
        audioSource.clip = clip;
        audioSource.volume = Random.Range(maxVolume/2f, maxVolume);
        audioSource.Play();
        lastSoundPlayTime = Time.time;
        //TODO use velocity to increase new sound probability
        minPauseBetweenSoundPlayTime = Random.Range(0.2f, 1.5f);
    }

    private void OnCollisionExit(Collision collision)
    {
        foreach (var audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }
}
