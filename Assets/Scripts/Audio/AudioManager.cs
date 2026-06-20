using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip attackClip;
    public List<Slider> sliders = new List<Slider>();
    private float value;
    private void Awake()
    {
        if (attackClip != null)
        {
            audioSource.clip = attackClip;
        }
        EventBus.Instance.Add(E.OnAttack, AttackAudio);
        AudioControl();
    }
    private void AudioControl()
    {
        foreach (var slider in sliders)
        {
            if (slider != null)
                slider.onValueChanged.AddListener(AudioChange);
        }
    }
    private void AudioChange(float newValue)
    {
        if (Mathf.Approximately(value, newValue)) return;
        value = newValue;
        if (audioSource != null)
        {
            audioSource.volume = value;
            foreach (var slider in sliders)
            {
                if (slider != null)
                    slider.value = value;
            }
        }
    }
    private void OnDestroy()
    {
        EventBus.Instance.Remove(E.OnAttack, AttackAudio);
        foreach (var slider in sliders)
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(AudioChange);
            }
        }
    }
    private void AttackAudio()
    { 
        audioSource.Play();
    }

}
