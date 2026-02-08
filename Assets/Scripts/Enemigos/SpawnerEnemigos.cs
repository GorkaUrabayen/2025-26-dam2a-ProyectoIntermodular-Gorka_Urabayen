using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerOleadas : MonoBehaviour
{
    [Header("Enemigos")]
    public GameObject prefabEnemigo;         // Prefab base
    public List<GameObject> prefabsExtra;    // Otros tipos opcionales

    [Header("Oleadas")]
    public int enemigosPorOleada = 5;
    public float tiempoEntreEnemigos = 1f;
    public float tiempoEntreOleadas = 5f;
    public int numeroOleadas = 3;

    [Header("Mapa")]
    public GenerarMapa mapaScript;           // Script del mapa procedural

    private List<Vector3> waypoints;
    private int oleadaActual = 0;
    private bool pausado = false;

    // Evento para cuando un enemigo llega al final
    public delegate void EnemigoAlFinal();
    public event EnemigoAlFinal OnEnemigoAlFinal;

    void Start()
    {
        // ⚠️ Comprobamos que el mapa y sus waypoints existan
        if (mapaScript == null)
        {
            Debug.LogError("No se ha asignado el script GenerarMapa!");
            return;
        }

        // Esperamos un frame para asegurarnos de que el mapa se haya generado
        StartCoroutine(IniciarSpawner());
    }

    IEnumerator IniciarSpawner()
    {
        // Esperar hasta que el mapa tenga waypoints
        while (mapaScript.waypoints == null || mapaScript.waypoints.Count == 0)
        {
            yield return null;
        }

        // Copiamos los waypoints del mapa procedural
        waypoints = new List<Vector3>(mapaScript.waypoints);

        // Iniciamos las oleadas
        StartCoroutine(IniciarOleadas());
    }

    IEnumerator IniciarOleadas()
    {
        while (oleadaActual < numeroOleadas)
        {
            if (pausado)
            {
                yield return null;
                continue;
            }

            oleadaActual++;
            Debug.Log("Oleada " + oleadaActual);

            for (int i = 0; i < enemigosPorOleada; i++)
            {
                GenerarEnemigo();
                yield return new WaitForSeconds(tiempoEntreEnemigos);
            }

            yield return new WaitForSeconds(tiempoEntreOleadas);
        }

        Debug.Log("Todas las oleadas completadas!");
    }

    void GenerarEnemigo()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        GameObject prefab = prefabEnemigo;

        // Elegir aleatoriamente otro prefab si existe
        if (prefabsExtra != null && prefabsExtra.Count > 0)
        {
            int index = Random.Range(0, prefabsExtra.Count + 1);
            if (index < prefabsExtra.Count) prefab = prefabsExtra[index];
        }

        // Instanciamos el enemigo en el primer waypoint
        GameObject nuevoEnemigo = Instantiate(prefab, waypoints[0], Quaternion.identity);

        // Asignamos los waypoints al enemigo directamente
        Enemigo enemigoScript = nuevoEnemigo.GetComponent<Enemigo>();
        if (enemigoScript != null)
        {
            enemigoScript.waypoints = waypoints;
            enemigoScript.OnLlegadaFinal += EnemigoLlegadoAlFinal;
        }
    }

    void EnemigoLlegadoAlFinal()
    {
        OnEnemigoAlFinal?.Invoke();
    }

    // Funciones para pausar y reanudar
    public void Pausar() => pausado = true;
    public void Reanudar() => pausado = false;
}
