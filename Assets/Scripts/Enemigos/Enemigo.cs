using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemigo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2f;
    protected List<Vector3> waypoints;
    private int indiceWaypoint = 0;
    private bool listoParaMover = false;

    [Header("Vida")]
    public int vida = 10;
    private bool estaVivo = true;

    public delegate void LlegadaFinal();
    public event LlegadaFinal OnLlegadaFinal;

    [Header("Animaciones")]
    public Animator animator;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        // Pausa lógica
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        if (listoParaMover && estaVivo)
            Mover();
    }

    public void SetWaypoints(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2) return;

        waypoints = new List<Vector3>(puntos);
        indiceWaypoint = 0;

        // Forzamos posición inicial con Z en 0
        Vector3 inicio = waypoints[0];
        inicio.z = 0;
        rb.position = inicio;

        listoParaMover = true;

        if (animator != null)
            animator.SetBool("Caminando", true);
    }

    protected virtual void Mover()
    {
        if (indiceWaypoint >= waypoints.Count || !estaVivo) return;

        // IMPORTANTE: Ignoramos la Z del waypoint para el cálculo
        Vector3 puntoObjetivo = waypoints[indiceWaypoint];
        Vector2 objetivo2D = new Vector2(puntoObjetivo.x, puntoObjetivo.y);
        
        Vector2 posicionActual2D = rb.position;
        Vector2 direccion = objetivo2D - posicionActual2D;
        float distancia = direccion.magnitude;

        // Si estamos muy cerca del punto (margen de 0.1)
        if (distancia < 0.1f)
        {
            indiceWaypoint++;

            if (indiceWaypoint >= waypoints.Count)
            {
                LlegarAlFinal();
                return;
            }
        }
        else
        {
            float paso = velocidad * Time.deltaTime;
            // Usamos MovePosition para un movimiento limpio en Kinematic
            rb.MovePosition(posicionActual2D + direccion.normalized * Mathf.Min(paso, distancia));
        }
    }

    private void LlegarAlFinal()
    {
        if (!estaVivo) return;

        estaVivo = false; // Evitamos que procese más daño o movimiento
        listoParaMover = false;

        if (OnLlegadaFinal != null)
            OnLlegadaFinal.Invoke();

        Morir(false);
    }

    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;

        vida -= cantidad;

        if (animator != null)
            animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            if (GameManager.instancia != null)
                GameManager.instancia.GanarDinero(5);
            
            Morir(true);
        }
    }

    public void Morir(bool muertoPorJugador)
    {
        estaVivo = false;
        listoParaMover = false;

        if (animator != null)
            animator.SetTrigger("Morir");

        Destroy(gameObject);
    }
}