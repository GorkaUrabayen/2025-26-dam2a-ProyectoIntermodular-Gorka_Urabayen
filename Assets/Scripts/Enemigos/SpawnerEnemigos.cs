using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerOleadas : MonoBehaviour
{
    public GameObject prefabEnemigo;
    public List<GameObject> prefabsExtra;

    public int enemigosPorOleada = 5;
    public float tiempoEntreEnemigos = 1f;
    public float tiempoEntreOleadas = 5f;
    public int numeroOleadas = 3;

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
        if (waypoints == null || waypoints.Count < 2) return;

        GameObject prefab = prefabEnemigo;

        if (prefabsExtra != null && prefabsExtra.Count > 0)
        {
            int index = Random.Range(0, prefabsExtra.Count + 1);
            if (index < prefabsExtra.Count)
                prefab = prefabsExtra[index];
        }

        GameObject nuevoEnemigo = Instantiate(prefab, waypoints[0], Quaternion.identity);

        Enemigo enemigoScript = nuevoEnemigo.GetComponent<Enemigo>();
        if (enemigoScript != null)
        {
            enemigoScript.SetWaypoints(waypoints);
            enemigoScript.OnLlegadaFinal += EnemigoLlegadoAlFinal;
        }
    }

    void EnemigoLlegadoAlFinal()
    {
        if (GameManager.instancia != null)
            GameManager.instancia.PerderVida(1);
    }

    public void Pausar() => pausado = true;
    public void Reanudar() => pausado = false;
}
