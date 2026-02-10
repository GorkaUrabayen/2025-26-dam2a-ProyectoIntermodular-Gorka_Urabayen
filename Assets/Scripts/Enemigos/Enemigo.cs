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
    if (indiceWaypoint >= waypoints.Count) return;

    Vector3 objetivo = waypoints[indiceWaypoint];
    objetivo.z = 0;

    float paso = velocidad * Time.deltaTime;

    Vector2 direccion = (Vector2)objetivo - rb.position;
    float distancia = direccion.magnitude;

    // 👇 SI LLEGA AL PUNTO EXACTO
    if (distancia <= paso)
    {
        rb.MovePosition(objetivo);
        indiceWaypoint++;

        // 🏁 FINAL DEL CAMINO
        if (indiceWaypoint >= waypoints.Count)
        {
            listoParaMover = false;

            GameManager.instancia.PerderVida(1);
            Morir(false);

            return;
        }
    }
    else
    {
        rb.MovePosition(rb.position + direccion.normalized * paso);
    }
}


    public void RecibirDanio(int cantidad)
    {
        if (!estaVivo) return;

        vida -= cantidad;

        if (animator != null)
            animator.SetTrigger("RecibirDanio");

        if (vida <= 0)
        {
            // 💰 ganas dinero SOLO al matarlo
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

        Destroy(gameObject);
    }
}
