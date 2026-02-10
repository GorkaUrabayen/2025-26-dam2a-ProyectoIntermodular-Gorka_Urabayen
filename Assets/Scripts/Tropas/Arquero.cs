using UnityEngine;

public class Arquero : Torre
{
    protected void Awake()
    {
        alcance = 3f;
        coste = 30;
    }

    protected override void Atacar()
    {
        if (objetivoActual == null) return;

        Vector2 dir = (objetivoActual.transform.position - transform.position).normalized;

        if (animator != null)
        {
            // ARRIBA
            if (dir.y > 0.6f)
            {
                if (Mathf.Abs(dir.x) < 0.3f)
                    animator.Play("Shoot Up");
                else
                    animator.Play("Shoot Diagonal Up");
            }
            // ABAJO
            else if (dir.y < -0.6f)
            {
                if (Mathf.Abs(dir.x) < 0.3f)
                    animator.Play("Shoot Down");
                else
                    animator.Play("Shoot Diagonal Down");
            }
            // LATERAL
            else
            {
                animator.Play("Shoot Front");
            }
        }

        // DISPARO
        if (flechaPrefab != null && puntoDisparo != null)
        {
            GameObject flecha = Instantiate(flechaPrefab, puntoDisparo.position, Quaternion.identity);
            Flecha f = flecha.GetComponent<Flecha>();
            if (f != null)
                f.SetObjetivo(objetivoActual.transform, danio);
        }
    }
}
