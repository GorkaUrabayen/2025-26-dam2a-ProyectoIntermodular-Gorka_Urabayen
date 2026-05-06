public class Lizzard : Enemigo
{
    protected override void Awake()
    {
        base.Awake(); 
        velocidad = 1f;
        vida = 1;
        recompensa = 8; // Dinero Lizzard
    }
}