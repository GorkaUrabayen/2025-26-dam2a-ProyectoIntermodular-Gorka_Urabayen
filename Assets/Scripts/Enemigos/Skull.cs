using UnityEngine;

public class Skull : Enemigo
{
    protected override void Awake()
    {
        base.Awake();
        velocidad = 10f;
        vida = 5;
    }
}
