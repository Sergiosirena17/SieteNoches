using UnityEngine;

/// <summary>
/// Anima al personaje cortando una HOJA DE SPRITES en tiempo de juego.
/// Para cambiar el dibujo, el artista solo reemplaza el PNG de la hoja (mismo nombre, misma rejilla):
///   - Cada celda mide anchoCelda x altoCelda (32 x 48 px).
///   - Columnas = frames de la caminata (0 = quieto, 1, 2, 3).
///   - Filas, de ARRIBA hacia abajo en la imagen: 0 abajo, 1 izquierda, 2 derecha, 3 arriba.
/// Ver Docs/Arte/guia_hoja_personaje.png
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AnimadorHojaSprites : MonoBehaviour
{
    public enum Direccion { Abajo = 0, Izquierda = 1, Derecha = 2, Arriba = 3 }

    [Tooltip("PNG con la hoja de sprites del personaje")]
    public Texture2D hoja;
    public int anchoCelda = 32;
    public int altoCelda = 48;
    public int framesPorFila = 4;
    public float pixelesPorUnidad = 32f;
    public float framesPorSegundo = 8f;

    Sprite[,] cuadros;
    SpriteRenderer render;
    float reloj;
    int frame;
    public Direccion DireccionActual { get; private set; } = Direccion.Abajo;

    void Awake()
    {
        Preparar();
    }

    /// <summary>Corta la hoja y muestra el primer cuadro. Tambien lo usan las capturas del editor.</summary>
    public void Preparar()
    {
        render = GetComponent<SpriteRenderer>();
        Cortar();
        Mostrar(Direccion.Abajo, 0);
    }

    void Cortar()
    {
        if (hoja == null) { Debug.LogError("AnimadorHojaSprites: falta asignar la hoja de sprites.", this); return; }
        int filas = hoja.height / altoCelda;
        cuadros = new Sprite[filas, framesPorFila];
        for (int fila = 0; fila < filas; fila++)
        {
            for (int f = 0; f < framesPorFila; f++)
            {
                // Unity cuenta Y desde abajo; la fila 0 del dibujo es la de arriba.
                var rect = new Rect(f * anchoCelda, hoja.height - (fila + 1) * altoCelda, anchoCelda, altoCelda);
                var s = Sprite.Create(hoja, rect, new Vector2(0.5f, 0f), pixelesPorUnidad, 0, SpriteMeshType.FullRect);
                s.name = "jugador_" + (Direccion)fila + "_" + f;
                cuadros[fila, f] = s;
            }
        }
    }

    /// <summary>Llamar cada frame con el movimiento actual (0,0 = quieto).</summary>
    public void Actualizar(Vector2 movimiento)
    {
        if (cuadros == null) return;
        if (movimiento.sqrMagnitude < 0.01f)
        {
            reloj = 0f; frame = 0;
            Mostrar(DireccionActual, 0);
            return;
        }
        if (Mathf.Abs(movimiento.x) > Mathf.Abs(movimiento.y))
            DireccionActual = movimiento.x < 0 ? Direccion.Izquierda : Direccion.Derecha;
        else
            DireccionActual = movimiento.y < 0 ? Direccion.Abajo : Direccion.Arriba;

        reloj += Time.deltaTime;
        if (reloj >= 1f / framesPorSegundo)
        {
            reloj = 0f;
            frame = frame % (framesPorFila - 1) + 1; // 1,2,3,1,2,3...
        }
        Mostrar(DireccionActual, frame);
    }

    void Mostrar(Direccion d, int f)
    {
        int fila = (int)d;
        if (cuadros == null || fila >= cuadros.GetLength(0)) return;
        render.sprite = cuadros[fila, f];
    }
}
