using UnityEngine;

public class Oso : Enemigo
{
    protected override void Awake()
    {
        base.Awake();
        velocidad = 4f;
        vida = 20;
    }
}
