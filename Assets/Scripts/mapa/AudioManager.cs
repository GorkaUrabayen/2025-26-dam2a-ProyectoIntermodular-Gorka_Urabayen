using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instancia;

    [Header("Configuración de Audio")]
    public AudioSource musicaSource;

    [Header("Clips de Música")]
    public AudioClip musicaMenu;
    public AudioClip musicaNivel;
    public AudioClip musicaVictoria;
    public AudioClip musicaDerrota;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            
            // Si no asignaste un AudioSource, intenta buscar uno en el objeto
            if (musicaSource == null) musicaSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Métodos que llama el GameManager ---

    public void PlayMusicaMenu() => CambiarMusica(musicaMenu, true);
    
    public void PlayMusicaNivel() => CambiarMusica(musicaNivel, true);
    
    public void PlayMusicaVictoria() => CambiarMusica(musicaVictoria, false);
    
    public void PlayMusicaDerrota() => CambiarMusica(musicaDerrota, false);

    // Función interna para evitar repetir código
    private void CambiarMusica(AudioClip clip, bool loop)
    {
        if (musicaSource == null || clip == null) return;

        // Si ya está sonando ese clip, no lo reinicies
        if (musicaSource.clip == clip) return;

        musicaSource.Stop();
        musicaSource.clip = clip;
        musicaSource.loop = loop;
        musicaSource.Play();
    }

    public void ActualizarVolumen(float volumen)
    {
        if (musicaSource != null) musicaSource.volume = volumen;
    }
}