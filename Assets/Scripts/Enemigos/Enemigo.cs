using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public float velocidad = 2f;
    public List<Vector3> waypoints;
    private int indiceWaypoint = 0;
    public int vida = 10;

    public delegate void LlegadaFinal();
    public event LlegadaFinal OnLlegadaFinal;

    private bool listoParaMover = false;

    void Start()
    {
        // Si ya tiene waypoints asignados, se activa inmediatamente
        if (waypoints != null && waypoints.Count > 0)
        {
            PrepararMovimiento();
        }
        else
        {
            // Espera a que el spawner le asigne los waypoints
            StartCoroutine(EsperarWaypoints());
        }
    }

    // ⚠ Importante: usar IEnumerator de System.Collections, NO genérico
    IEnumerator EsperarWaypoints()
    {
        // Esperamos hasta que waypoints tenga al menos un elemento
        while (waypoints == null || waypoints.Count == 0)
        {
            yield return null;
        }

        PrepararMovimiento();
    }

    void PrepararMovimiento()
    {
        // Posicionar en el primer waypoint
        transform.position = waypoints[0];
        indiceWaypoint = 0;
        listoParaMover = true;
    }

    void Update()
    {
        if (listoParaMover)
        {
            Mover();
        }
    }

    void Mover()
    {
        if (indiceWaypoint >= waypoints.Count) return;

        Vector3 objetivo = waypoints[indiceWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, objetivo) < 0.01f)
        {
            indiceWaypoint++;

            // Llegó al final
            if (indiceWaypoint >= waypoints.Count)
            {
                OnLlegadaFinal?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    public virtual void RecibirDanio(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0) Morir();
    }

    public virtual void Morir()
    {
        Destroy(gameObject);
    }
}
