using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instancia;
    public AudioSource source;

    [Header("Configuracion de Escena")]
    public bool esMusicaDeMenu = false;
    public bool esEscenaOpciones = false; 

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            
            // Aplicar volumen guardado al iniciar
            ActualizarVolumen(PlayerPrefs.GetFloat("VolumenMaster", 0.5f));
        }
        else
        {
            // SI ESTAMOS EN OPCIONES: No tocamos nada, dejamos que siga la música que ya suena
            if (esEscenaOpciones)
            {
                Destroy(gameObject);
                return;
            }

            // CAMBIO DE MÚSICA: De Menú a Nivel (o viceversa)
            if (esMusicaDeMenu != instancia.esMusicaDeMenu)
            {
                instancia.source.clip = GetComponent<AudioSource>().clip;
                instancia.source.Play();
                instancia.esMusicaDeMenu = esMusicaDeMenu;
            }
            
            Destroy(gameObject);
        }
    }

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
}