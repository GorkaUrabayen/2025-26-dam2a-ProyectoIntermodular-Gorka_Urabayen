using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        StartCoroutine(IniciarSpawner());
    }

    IEnumerator IniciarSpawner()
    {
        while (mapaScript == null || mapaScript.waypoints == null || mapaScript.waypoints.Count < 2)
        {
            yield return null;
        }

        waypoints = new List<Vector3>(mapaScript.waypoints);

        if (GameManager.instancia != null)
        {
            GameManager.instancia.enemigosRestantes = numeroOleadas * enemigosPorOleada;
        }

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
            for (int i = 0; i < enemigosPorOleada; i++)
            {
                GenerarEnemigo();
                yield return new WaitForSeconds(tiempoEntreEnemigos);
            }
            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }

    void GenerarEnemigo()
    {
        if (mapaScript == null || mapaScript.waypoints == null || mapaScript.waypoints.Count < 2) return;

        GameObject prefabAElegir = prefabEnemigoBase;
        if (oleadaActual >= oleadaMinimaParaExtra && prefabsExtra.Count > 0)
        {
            int index = Random.Range(0, prefabsExtra.Count + 1);
            if (index < prefabsExtra.Count) prefabAElegir = prefabsExtra[index];
        }

        Vector3 spawnPos = waypoints[0];
        spawnPos.z = 0f; 

        GameObject nuevoEnemigo = Instantiate(prefabAElegir, spawnPos, Quaternion.identity);

        Enemigo enemigoScript = nuevoEnemigo.GetComponent<Enemigo>();
        if (enemigoScript != null)
        {
            enemigoScript.SetWaypoints(new List<Vector3>(waypoints));
            enemigoScript.OnLlegadaFinal += EnemigoLlegadoAlFinal;
        }
    }

    void EnemigoLlegadoAlFinal()
    {
        if (GameManager.instancia != null)
        {
            GameManager.instancia.PerderVida(1);
        }
    }

    public void Pausar() => pausado = true;
    public void Reanudar() => pausado = false;
}