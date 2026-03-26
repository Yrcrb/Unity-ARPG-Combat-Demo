using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public OnAttackEvent onAttackEvent;
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
        onAttackEvent.onAttack += AttackAudio;
        AudioControl();
    }
    private void AudioControl()//订阅滑动监听函数
    {
        foreach (var slider in sliders)
        {
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
                slider.value = value;
            }
        }
    }
    private void OnDestroy()
    {
        onAttackEvent.onAttack -= AttackAudio;
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
