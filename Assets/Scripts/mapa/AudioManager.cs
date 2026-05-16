using UnityEngine;
// Gestiona el sistema de audio global del juego. 
// Utiliza un patrón Singleton persistente para permitir que la música continúe sonando sin interrupciones durante las transiciones de escena.
public class AudioManager : MonoBehaviour
{
    // Instancia estática para el acceso global (Patrón Singleton)
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
        // Implementación del Singleton con persistencia entre escenas
        if (instancia == null)
        {
            instancia = this;
            // Hace que este objeto no se destruya al cargar una nueva escena
            DontDestroyOnLoad(gameObject);
            
            // Autodetecta el AudioSource si no se asignó manualmente en el Inspector
            if (musicaSource == null) musicaSource = GetComponent<AudioSource>();
        }
        else
        {
            // Si ya existe una instancia de AudioManager, destruimos la nueva para evitar duplicados
            Destroy(gameObject);
        }
    }

    // --- Interfaz Pública para comunicación con el GameManager ---

    public void PlayMusicaMenu() => CambiarMusica(musicaMenu, true);
    
    public void PlayMusicaNivel() => CambiarMusica(musicaNivel, true);
    
    public void PlayMusicaVictoria() => CambiarMusica(musicaVictoria, false);
    
    public void PlayMusicaDerrota() => CambiarMusica(musicaDerrota, false);

    // Gestiona la transición lógica entre diferentes pistas de audio.
    // Incluye una comprobación de seguridad para no reiniciar la pista si ya está sonando.
    private void CambiarMusica(AudioClip clip, bool loop)
    {
        // Validación de nulidad para evitar errores en tiempo de ejecución
        if (musicaSource == null || clip == null) return;

        // Optimización: Evita reiniciar la música si se llama al método y el clip ya es el actual
        if (musicaSource.clip == clip) return;

        musicaSource.Stop();
        musicaSource.clip = clip;
        musicaSource.loop = loop;
        musicaSource.Play();
    }
    // Permite ajustar el volumen de la música en tiempo real.
    // Útil para la integración con sliders de volumen en el menú de opciones.
    public void ActualizarVolumen(float volumen)
    {
        if (musicaSource != null) musicaSource.volume = volumen;
    }
}