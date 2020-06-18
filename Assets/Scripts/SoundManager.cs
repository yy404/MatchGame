using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayDestroyNormal()
    {
        playMySound(440.00f, 0.2f);
    }

    public void PlayDestroySpecial()
    {
        playMySound(880.00f, 0.2f);
    }

    private void playMySound(float frequency, float duration)
    {
        int sampleFreq = 44100;
        int samplesLength = Mathf.CeilToInt(sampleFreq * duration);
        float[] samples = new float[samplesLength];
        for(int i = 0; i < samplesLength; i++)
        {
            // sin
            samples[i] = Mathf.Sin(Mathf.PI*2*i*frequency/sampleFreq);
        }

        AudioClip ac = AudioClip.Create("Test", samplesLength, 1, sampleFreq, false);
        ac.SetData(samples, 0);
        audioSource.PlayOneShot(ac, 1.0f);
    }
}
