using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Enemy")]
    public AudioClip enemyDeathClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.enabled = true;
        audioSource.mute = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.playOnAwake = false;

        Debug.Log("[AudioManager] Initialized, AudioSource=" + (audioSource != null) +
                  ", clip assigned=" + (enemyDeathClip != null));
    }

    private void OnEnable()
    {
        EnemyAI.OnEnemyKilled += PlayEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyAI.OnEnemyKilled -= PlayEnemyDeath;
    }

    public void PlayEnemyDeath()
    {
        if (enemyDeathClip == null)
        {
            Debug.LogWarning("[AudioManager] enemyDeathClip not assigned!");
            return;
        }
        if (audioSource == null)
        {
            Debug.LogError("[AudioManager] AudioSource is null!");
            return;
        }

        AudioListener.volume = 1f;

        Debug.Log("[AudioManager] Playing: " + enemyDeathClip.name + ", len=" + enemyDeathClip.length + "s");
        audioSource.PlayOneShot(enemyDeathClip);
    }
}
