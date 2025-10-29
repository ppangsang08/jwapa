using UnityEngine;

//야무진 음악 알어서좀 찾아 넣으셈씹

[RequireComponent(typeof(AudioSource))]
public class SoundManager : GenericSingleton<SoundManager>
{
    private AudioSource audioSorce;

    internal override void Init()
    {
        audioSorce = GetComponent<AudioSource>();
    }

    internal void PlaySound(AudioClip audioClip)
    {
        audioSorce.clip = audioClip;
        audioSorce.Play();
    }
}
