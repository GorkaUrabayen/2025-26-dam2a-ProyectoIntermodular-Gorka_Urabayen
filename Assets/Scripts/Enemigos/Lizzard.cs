using UnityEngine;

public class Lizzard : Enemigo
{
    protected override void Awake()
    {
        
        base.Awake(); 

        velocidad = 20f;
        vida = 6;
    }
}