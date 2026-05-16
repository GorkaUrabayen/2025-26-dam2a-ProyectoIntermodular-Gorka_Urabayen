using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Clase encargada de la gestión y aparición (spawning) de enemigos.
// Controla el flujo de oleadas, intervalos de tiempo y la selección de tipos de enemigos.
public class SpawnerEnemigos : MonoBehaviour 
{
    [Header("Prefabs de Enemigos")]
    public GameObject prefabEnemigoBase;
    public List<GameObject> prefabsExtra;

    [Header("Configuración de Oleadas")]
    public int enemigosPorOleada = 5;
    public float tiempoEntreEnemigos = 1f;
    public float tiempoEntreOleadas = 5f;
    public int numeroOleadas = 3;
    
    [Range(1, 10)]
    public int oleadaMinimaParaExtra = 2;

    [Header("Referencias")]
    public GenerarMapa mapaScript;

    private List<Vector3> waypoints;
    private int oleadaActual = 0;
    private bool pausado = false;

    void Start()
    {
        // Iniciamos el proceso de espera y generación
        StartCoroutine(IniciarSpawner());
    }

    // Corrutina de espera activa. 
    // Asegura que el mapa procedimental esté listo antes de intentar generar enemigos.
    IEnumerator IniciarSpawner()
    {
        // Esperamos hasta que el mapa procedimental haya generado la ruta (waypoints)
        while (mapaScript == null || mapaScript.waypoints == null || mapaScript.waypoints.Count < 2)
        {
            yield return null;
        }

        waypoints = new List<Vector3>(mapaScript.waypoints);
        // Notificamos al GameManager el total de enemigos para gestionar las condiciones de victoria
        if (GameManager.instancia != null)
        {
            GameManager.instancia.enemigosRestantes = numeroOleadas * enemigosPorOleada;
        }
        // Una vez validada la ruta, iniciamos la lógica de oleadas
        StartCoroutine(IniciarOleadas());
    }
    // Ciclo principal que controla el paso del tiempo entre enemigos y oleadas.
    IEnumerator IniciarOleadas()
    {
        while (oleadaActual < numeroOleadas)
        {
            // Control de pausa del spawner (por ejemplo, para eventos o menús)
            if (pausado)
            {
                yield return null;
                continue;
            }

            oleadaActual++;
            // Ciclo de generación de la oleada actual
            for (int i = 0; i < enemigosPorOleada; i++)
            {
                GenerarEnemigo();
                yield return new WaitForSeconds(tiempoEntreEnemigos);
            }
            // Tiempo muerto entre el fin de una oleada y el inicio de la siguiente
            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }
    // Instancia un enemigo en el punto inicial de la ruta y configura sus parámetros.
    void GenerarEnemigo()
    {
        // Validación de seguridad para evitar errores de ejecución en la ruta
        if (mapaScript == null || mapaScript.waypoints == null || mapaScript.waypoints.Count < 2) return;
        // Lógica de selección de prefab: Mezcla aleatoria según la oleada actual
        GameObject prefabAElegir = prefabEnemigoBase;
        if (oleadaActual >= oleadaMinimaParaExtra && prefabsExtra.Count > 0)
        {
            // Sistema probabilístico simple para decidir si aparece un enemigo base o extra
            int index = Random.Range(0, prefabsExtra.Count + 1);
            if (index < prefabsExtra.Count) prefabAElegir = prefabsExtra[index];
        }
        // El punto de spawn siempre es el primer waypoint generado
        Vector3 spawnPos = waypoints[0];
        spawnPos.z = 0f; 
        // Instanciación del objeto en la jerarquía
        GameObject nuevoEnemigo = Instantiate(prefabAElegir, spawnPos, Quaternion.identity);
        // Configuración del script de IA del enemigo
        Enemigo enemigoScript = nuevoEnemigo.GetComponent<Enemigo>();
        if (enemigoScript != null)
        {
            // Pasamos la ruta de waypoints al enemigo recién creado
            enemigoScript.SetWaypoints(new List<Vector3>(waypoints));
            // Suscripción al evento de llegada para restar vida al jugador
            enemigoScript.OnLlegadaFinal += EnemigoLlegadoAlFinal;
        }
    }
    // Callback ejecutado mediante eventos cuando un enemigo cruza la meta.
    void EnemigoLlegadoAlFinal()
    {
        if (GameManager.instancia != null)
        {
            // Reducción de la salud del jugador a través del Singleton
            GameManager.instancia.PerderVida(1);
        }
    }
// Métodos públicos para controlar el flujo del spawner desde otros sistemas (UI, GameManager, etc.)
    public void Pausar() => pausado = true;
    public void Reanudar() => pausado = false;
}