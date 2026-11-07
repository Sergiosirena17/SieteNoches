using UnityEngine;

/// <summary>Movimiento con vista superior en 8 direcciones (el dibujo usa 4). WASD o flechas.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class JugadorMovimiento : MonoBehaviour
{
    [Tooltip("Velocidad en unidades por segundo (1 unidad = 1 bloque de 32 px)")]
    public float velocidad = 4f;
    public AnimadorHojaSprites animador;

    /// <summary>Lo pone en true el mapa durante textos o la noche.</summary>
    public static bool Bloqueado;

    Rigidbody2D cuerpo;
    Vector2 entrada;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        cuerpo.gravityScale = 0f;
        cuerpo.freezeRotation = true;
    }

    void Update()
    {
        entrada = Bloqueado ? Vector2.zero : Teclas.Movimiento();
        if (animador != null) animador.Actualizar(entrada);
    }

    void FixedUpdate()
    {
        cuerpo.linearVelocity = entrada * velocidad;
    }
}
