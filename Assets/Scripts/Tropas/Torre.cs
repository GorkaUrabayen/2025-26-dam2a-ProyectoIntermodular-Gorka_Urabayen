using UnityEngine;

public class Torre : MonoBehaviour
{
    public GameObject flechaPrefab;
    public Transform puntoDisparo;
    public float alcance = 5f;
    public float cadencia = 1f;
    public int danio = 5;

    public int coste = 20;
public bool estaColocada = false;


    protected float tiempoUltimoDisparo = 0f;
    protected Enemigo objetivoActual;

    [Header("Animaciones")]
    public Animator animator;

    void Update()
    {
        if (!estaColocada) return;

        objetivoActual = BuscarEnemigoCercano();

        if (objetivoActual != null)
        {
            ApuntarAOEnemigo();

            if (Time.time - tiempoUltimoDisparo >= 1f / cadencia)
            {
                Atacar();
                tiempoUltimoDisparo = Time.time;
            }
        }
        else
        {
            if (animator != null)
                animator.Play("Idle");
        }
    }

    Enemigo BuscarEnemigoCercano()
    {
        Enemigo[] enemigos = FindObjectsOfType<Enemigo>();
        Enemigo masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (Enemigo e in enemigos)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);

            if (dist < distanciaMin && dist <= alcance)
            {
                distanciaMin = dist;
                masCercano = e;
            }
        }

        return masCercano;
    }

    void ApuntarAOEnemigo()
    {
        if (objetivoActual == null) return;

        Vector3 dir = objetivoActual.transform.position - transform.position;

        transform.localScale =
            dir.x < 0 ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    protected virtual void Atacar()
    {
        if (flechaPrefab != null && puntoDisparo != null && objetivoActual != null)
        {
            GameObject flecha = Instantiate(flechaPrefab, puntoDisparo.position, Quaternion.identity);

            Flecha f = flecha.GetComponent<Flecha>();

            if (f != null)
                f.SetObjetivo(objetivoActual.transform, danio);
        }
    }
}
