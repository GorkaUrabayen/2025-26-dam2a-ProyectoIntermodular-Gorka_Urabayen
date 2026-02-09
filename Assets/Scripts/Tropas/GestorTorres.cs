using UnityEngine;

public class GestorTorres : MonoBehaviour
{
    public GameObject prefabArquero;
    public GenerarMapa generadorMapa;

    private GameObject torreTemporal;
    private bool modoColocacion = false;

    void Update()
    {
        if (!modoColocacion) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        if (torreTemporal != null)
            torreTemporal.transform.position = mousePos;

        if (Input.GetMouseButtonDown(0))
            ColocarTorre(mousePos);

        if (Input.GetMouseButtonDown(1))
            CancelarColocacion();
    }

    public void EmpezarColocacion()
    {
        if (modoColocacion) return;

        modoColocacion = true;

        torreTemporal = Instantiate(prefabArquero);

        Torre t = torreTemporal.GetComponent<Torre>();
        if (t != null)
            t.estaColocada = false;

        SetAlpha(torreTemporal, 0.5f);
    }

   void ColocarTorre(Vector3 posicion)
{
    if (torreTemporal == null) return;

    // Convertir posición mundo a celda
    Vector3Int celda = generadorMapa.tilemap.WorldToCell(posicion);

    int x = celda.x;
    int y = celda.y;

    // Fuera del mapa
    if (x < 0 || y < 0 || x >= generadorMapa.anchoMapa || y >= generadorMapa.altoMapa)
    {
        Debug.Log("Fuera del mapa");
        return;
    }

    int valor = generadorMapa.mapa[x, y];

    // 1 = camino
    // 2 = borde
    if (valor == 1 || valor == 2)
    {
        Debug.Log("NO puedes colocar torres aquí");
        return;
    }

    SetAlpha(torreTemporal, 1f);

    Torre t = torreTemporal.GetComponent<Torre>();
    if (t != null)
        t.estaColocada = true;

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
