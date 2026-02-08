using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemigo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    private List<Vector3> waypoints;
    private int indiceWaypoint = 0;
    private bool listoParaMover = false;

    [Header("Vida")]
    public int vida = 10;
    private bool estaVivo = true;

    public delegate void LlegadaFinal();
    public event LlegadaFinal OnLlegadaFinal;

    [Header("Animaciones")]
    public Animator animator;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Muy importante para mover manualmente
    }

    void Update()
    {
        if (listoParaMover && estaVivo)
        {
            Mover();
        }
    }

    public void SetWaypoints(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2)
        {
            Debug.LogWarning("Waypoints inválidos para enemigo");
            return;
        }

        waypoints = new List<Vector3>(puntos);
        indiceWaypoint = 0;

        // Posicionar al enemigo en el primer waypoint
        Vector3 inicio = waypoints[0];
        inicio.z = 0; // importante para 2D
        rb.position = inicio;

        listoParaMover = true;

        if (animator != null)
            animator.SetBool("Caminando", true);

        Debug.Log("Waypoints asignados. Listo para moverse!");
    }

    void Mover()
    {
        if (indiceWaypoint >= waypoints.Count) return;

        Vector3 objetivo = waypoints[indiceWaypoint];
        objetivo.z = 0; // 2D

        // Mover usando Rigidbody2D
        Vector2 nuevaPos = Vector2.MoveTowards(rb.position, (Vector2)objetivo, velocidad * Time.deltaTime);
        rb.MovePosition(nuevaPos);

        // Debug para verificar movimiento
        Debug.Log($"Waypoint actual: {indiceWaypoint} | Pos: {rb.position} | Objetivo: {objetivo}");

        // Comprobar llegada al waypoint
        if (Vector2.Distance(rb.position, (Vector2)objetivo) < 0.1f)
        {
            indiceWaypoint++;

            if (indiceWaypoint >= waypoints.Count)
            {
                listoParaMover = false;
                OnLlegadaFinal?.Invoke();
                Morir();
            }
        }
    }

    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;

        vida -= cantidad;
        if (animator != null) animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            Morir();
        }
    }

    public void Morir()
    {
        if (!estaVivo) return;

        estaVivo = false;
        listoParaMover = false;

        if (animator != null) animator.SetTrigger("Morir");

        Destroy(gameObject, 0.5f); // Espera animación de muerte
    }
}
