using UnityEngine;

public class GestorTorres : MonoBehaviour
{
    public GameObject prefabArquero; // Prefab del arquero que queremos colocar
    private GameObject torreTemporal; // Torre que sigue el mouse
    private bool modoColocacion = false;

    void Update()
    {
        if (!modoColocacion) return;

        // Seguir la posición del mouse en el mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // 2D

        if (torreTemporal != null)
            torreTemporal.transform.position = mousePos;

        // Colocar torre con clic izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            ColocarTorre(mousePos);
        }

        // Cancelar colocación con clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            CancelarColocacion();
        }
    }

    // Entrar en modo colocación
    public void EmpezarColocacion()
    {
        if (modoColocacion) return;

        modoColocacion = true;
        torreTemporal = Instantiate(prefabArquero);
        // Opcional: cambiar color o hacer semi-transparente
        SetAlpha(torreTemporal, 0.5f);
    }

    void ColocarTorre(Vector3 posicion)
    {
        if (torreTemporal == null) return;

        // Restaurar apariencia
        SetAlpha(torreTemporal, 1f);

        // Torre ya colocada → desconectarla de la temporal
        torreTemporal = null;
        modoColocacion = false;
    }

    void CancelarColocacion()
    {
        if (torreTemporal != null)
            Destroy(torreTemporal);

        torreTemporal = null;
        modoColocacion = false;
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
