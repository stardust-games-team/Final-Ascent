using System;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] AudioMixerSnapshot _patrolSnapshot;
    [SerializeField] AudioMixerSnapshot _combatSnapshot;
    [SerializeField] AudioMixerSnapshot _gameOverSnapshot;
    [SerializeField] AudioClip[] _patrolMusic;
    [SerializeField] AudioClip[] _combatMusic;
    [SerializeField] AudioSource _patrolAudioSource;
    [SerializeField] AudioSource _combatAudioSource;
    [SerializeField] AudioSource _gameOverAudioSource;

    int _patrolMusicIndex, _combatMusicIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayCombatMusic()
    {
        _combatAudioSource.Stop();
        _combatAudioSource.clip = _combatMusic[_combatMusicIndex];
        _combatAudioSource.Play();
        _combatSnapshot.TransitionTo(1f);
        _combatMusicIndex = (_combatMusicIndex + 1) % _combatMusic.Length;

        
    }

    public void PlayPatrolMusic()
    {
        _patrolAudioSource.Stop();
        _patrolAudioSource.clip = _patrolMusic[_patrolMusicIndex];
        _patrolAudioSource.Play();
        _patrolSnapshot.TransitionTo(1f);
        _patrolMusicIndex = (_patrolMusicIndex + 1) % _patrolMusic.Length;
    }

    public void PlayGameOverMusic()
    {
         _gameOverAudioSource.gameObject.SetActive(true);  
    _gameOverAudioSource.enabled = true; 
        SafePlay(_gameOverAudioSource);
        _gameOverSnapshot.TransitionTo(1f);
    }

    void SafePlay(AudioSource source)
{
    if (!source.gameObject.activeInHierarchy)
        source.gameObject.SetActive(true);

    if (!source.enabled)
        source.enabled = true;

    source.Play();
}



}
