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

    // Delegado y Evento para avisar al Spawner
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
    // Si el tiempo está detenido, el enemigo no ejecuta su lógica de movimiento
    if (Mathf.Approximately(Time.timeScale, 0f)) return;

    if (listoParaMover && estaVivo)
        Mover();
}

    public void SetWaypoints(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2)
            return;

        waypoints = new List<Vector3>(puntos);
        indiceWaypoint = 0;

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

        Vector3 objetivo = waypoints[indiceWaypoint];
        objetivo.z = 0;

        Vector2 direccion = (Vector2)objetivo - rb.position;
        float distancia = direccion.magnitude;

        // Margen de error de 0.1f para detectar que llegó al punto
        if (distancia < 0.1f)
        {
            indiceWaypoint++;

            // Si ya no hay más puntos, ha llegado a la meta
            if (indiceWaypoint >= waypoints.Count)
            {
                LlegarAlFinal();
                return;
            }
        }
        else
        {
            float paso = velocidad * Time.deltaTime;
            // MovePosition es mejor para Rigidbodies Kinematic
            rb.MovePosition(rb.position + direccion.normalized * Mathf.Min(paso, distancia));
        }
    }

    private void LlegarAlFinal()
    {
        if (!estaVivo) return;

        listoParaMover = false;

        // Disparamos el evento para que el Spawner reste vida
        if (OnLlegadaFinal != null)
        {
            OnLlegadaFinal.Invoke();
        }

        // El enemigo muere sin dar dinero (muerte por llegar a la meta)
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
            // Solo da dinero si el jugador lo mata
            if (GameManager.instancia != null)
                GameManager.instancia.GanarDinero(5);
            
            Morir(true);
        }
    }

    public void Morir(bool muertoPorJugador)
    {
        if (!estaVivo) return;

        estaVivo = false;
        listoParaMover = false;

        if (animator != null)
            animator.SetTrigger("Morir");

        // Destruimos el objeto
        Destroy(gameObject);
    }
}