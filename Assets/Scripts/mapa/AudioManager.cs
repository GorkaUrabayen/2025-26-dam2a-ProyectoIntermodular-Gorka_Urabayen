using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instancia;
    private AudioSource source;

    [Header("Configuración")]
    public bool esMusicaDeMenu = false;

    void Awake()
{
    if (instancia == null)
    {
        instancia = this;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
    }
    else
    {
        // SI YA EXISTE UNA INSTANCIA...
        // Pero este nuevo objeto tiene una canción diferente (la del nivel)
        if (!esMusicaDeMenu && instancia.esMusicaDeMenu)
        {
            // Detenemos la música del menú y ponemos la de este nivel
            instancia.source.clip = GetComponent<AudioSource>().clip;
            instancia.source.Play();
            instancia.esMusicaDeMenu = false; // Ya no estamos en el menú
        }
        
        Destroy(gameObject);
        return;
    }
}

    // Esta función permite cambiar el volumen desde el slider de opciones
    public void ActualizarVolumen(float nuevoVolumen)
    {
        if (source == null) source = GetComponent<AudioSource>();
        source.volume = nuevoVolumen;
    }

    public void DetenerMusicaYDestruir()
    {
        if (source != null) source.Stop();
        Destroy(gameObject);
    }

    public void PausarMusica() => source.Pause();
    public void ReanudarMusica() => source.UnPause();
}