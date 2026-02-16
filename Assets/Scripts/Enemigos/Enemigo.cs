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
    private Collider2D col; 

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.velocity = Vector2.zero;
    }

    void Update()
    {
        if (Mathf.Approximately(Time.timeScale, 0f)) return;
        if (listoParaMover && estaVivo) Mover();
    }

    public void SetWaypoints(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2) return;
        waypoints = new List<Vector3>(puntos);
        indiceWaypoint = 0;
        Vector3 inicio = waypoints[0];
        inicio.z = 0;
        transform.position = inicio;
        rb.position = inicio;
        listoParaMover = true;
        if (animator != null) animator.SetBool("Caminando", true);
    }

    protected virtual void Mover()
    {
        if (indiceWaypoint >= waypoints.Count || !estaVivo) return;

        Vector3 puntoObjetivo = waypoints[indiceWaypoint];
        Vector2 objetivo2D = new Vector2(puntoObjetivo.x, puntoObjetivo.y);
        Vector2 posicionActual2D = rb.position;
        Vector2 direccion = objetivo2D - posicionActual2D;
        float distancia = direccion.magnitude;

        if (distancia < 0.1f) // Aumentamos ligeramente el margen para evitar el "pasarse" de largo
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
            if (estaVivo)
            {
                rb.MovePosition(posicionActual2D + direccion.normalized * Mathf.Min(paso, distancia));
            }
        }
    }

    private void LlegarAlFinal()
    {
        if (!estaVivo) return;

        // --- PARADA DE EMERGENCIA ---
        estaVivo = false;
        listoParaMover = false;
        rb.velocity = Vector2.zero;
        rb.simulated = false; // Esto evita que "rebote" o se mueva hacia atrás
        if (col != null) col.enabled = false;

        if (OnLlegadaFinal != null)
            OnLlegadaFinal.Invoke();

        Morir(false);
    }

    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;
        vida -= cantidad;
        if (animator != null) animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            if (GameManager.instancia != null)
                GameManager.instancia.GanarDinero(5);
            Morir(true);
        }
    }

    public void Morir(bool muertoPorJugador)
    {
        // Si ya estaba muriendo por llegar al final, no repetimos la lógica
        if (!estaVivo && muertoPorJugador) return; 

        estaVivo = false;
        listoParaMover = false;
        rb.simulated = false; // Desactivar física totalmente
        if (col != null) col.enabled = false;

        if (GameManager.instancia != null)
        {
            GameManager.instancia.NotificarMuerteEnemigo();
        }

        if (animator != null)
        {
            animator.SetBool("Caminando", false);
            animator.SetTrigger("Morir");
        }

        // Destruir inmediatamente para que no haya restos visuales moviéndose
        Destroy(gameObject);
    }
}