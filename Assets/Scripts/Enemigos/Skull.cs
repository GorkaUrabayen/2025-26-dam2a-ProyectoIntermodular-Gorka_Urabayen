public class Skull : Enemigo
{
    protected override void Awake()
    {
        base.Awake();
        velocidad = 1.5f;
        vida = 6;
        recompensa = 10; // Dinero Skull
    }
}