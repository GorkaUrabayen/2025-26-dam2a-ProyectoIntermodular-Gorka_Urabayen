using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instancia;
    private AudioSource source;

    [Header("Configuración")]
    // Marca esto SOLO en el AudioManager de la escena del Menú Principal
    public bool esMusicaDeMenu = false;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si entramos al Menú y venimos de un nivel, cambiamos la música
            if (esMusicaDeMenu)
            {
                instancia.DetenerMusicaYDestruir();
                instancia = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Si ya hay música sonando y no es el menú, no creamos otro manager
                Destroy(gameObject);
                return;
            }
        }

        source = GetComponent<AudioSource>();
    }

    public void DetenerMusicaYDestruir()
    {
        if (source != null) source.Stop();
        Destroy(gameObject);
    }

    public void PausarMusica() => source.Pause();
    public void ReanudarMusica() => source.UnPause();
}