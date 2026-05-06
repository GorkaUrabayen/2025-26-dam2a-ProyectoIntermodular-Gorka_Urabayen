public class Skull : Enemigo
{
    protected override void Awake()
    {
        base.Awake();
        velocidad = 1.5f;
        vida = 5;
        recompensa = 5; // Dinero Skull
    }
}