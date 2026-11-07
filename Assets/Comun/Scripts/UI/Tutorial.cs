using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ventana corta de "cómo se juega" que aparece la PRIMERA vez que entras a cada estación.
/// Se arma por código sobre el Canvas de la escena, así no hay que tocar las escenas.
/// Para cambiar los textos: Tutorial.Textos, más abajo.
/// </summary>
public class Tutorial : MonoBehaviour
{
    /// <summary>True mientras la ventana está abierta: las estaciones no leen teclas.</summary>
    public static bool Abierto { get; private set; }

    public static readonly System.Collections.Generic.Dictionary<string, string[]> Textos =
        new System.Collections.Generic.Dictionary<string, string[]>
    {
        { Estaciones.Invernadero, new[]
        {
            "1. Toca una cápsula para elegirla.",
            "2. Elige una semilla. El libro de la derecha dice qué humedad y qué luz necesita.",
            "3. Ajusta la humedad con − y + y enciende las luces que pide.",
            "4. Las cápsulas 2 y 3 tienen suelo tóxico (ClO₄⁻): límpialas primero con Dechloromonas antes de sembrar comida.",
            "Cada luz gasta batería. Las plantas crecen cuando termina la noche, a las 6:00 AM.",
        } },
        { Estaciones.SoporteVital, new[]
        {
            "1. Mantén ESPACIO para calentar el reactor MOXIE. Si sueltas, se enfría.",
            "2. Mientras esté en la franja verde, produce oxígeno. En la zona roja se quema.",
            "3. Si los filtros se llenan de hollín, pulsa Purgar (P).",
            "4. A la derecha decides qué queda encendido de noche. Todo gasta batería: si gastas más de lo que tienes, habrá apagón.",
        } },
        { Estaciones.Biomedica, new[]
        {
            "1. Elige a un tripulante. Solo puedes hablar con 2 por noche.",
            "2. Mira el monitor: su pulso y su CO₂.",
            "3. Si el CO₂ está en rojo, el problema es el aire: elige A. Si no, calma con B o dale comida.",
            "Verde: te ayuda.   Rojo: puede sabotear algo de noche.",
        } },
        { Estaciones.Antena, new[]
        {
            "1. Elige qué mensaje enviar. El SOS acerca el rescate.",
            "2. Mueve tu onda (la brillante) hasta taparla con la del satélite (la tenue).",
            "   A / D: más ancha o más apretada.   W / S: más alta o más baja.",
            "3. Mantenla encima hasta llenar la barra antes de que se acabe el tiempo.",
        } },
    };

    string id;

    /// <summary>Muestra el tutorial de esa estación si todavía no se vio en esta partida.</summary>
    public static void MostrarSiHaceFalta(string idEstacion, string titulo)
    {
        if (GameState.TutorialesVistos.Contains(idEstacion) || !Textos.TryGetValue(idEstacion, out var lineas)) return;
        var lienzo = Object.FindAnyObjectByType<Canvas>();
        if (lienzo == null) return;

        var fondo = Caja("Tutorial (" + idEstacion + ")", lienzo.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.8f));
        var r = fondo.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        var caja = Caja("Caja", fondo.transform, Vector2.zero, new Vector2(470, 250), new Color(0.12f, 0.16f, 0.27f, 1f));
        caja.gameObject.AddComponent<Outline>().effectColor = new Color(0.95f, 0.76f, 0.31f);
        Texto("Titulo", caja.transform, "Cómo funciona: " + titulo, 15, new Vector2(0, 100), new Vector2(440, 24), new Color(0.95f, 0.76f, 0.31f), TextAnchor.MiddleCenter);
        Texto("Pasos", caja.transform, string.Join("\n", lineas), 12, new Vector2(0, 0), new Vector2(430, 160), Color.white, TextAnchor.UpperLeft).lineSpacing = 1.15f;
        var boton = Caja("Boton Entendido", caja.transform, new Vector2(0, -102), new Vector2(150, 26), new Color(0.18f, 0.29f, 0.49f));
        boton.gameObject.AddComponent<Outline>().effectColor = new Color(0.95f, 0.76f, 0.31f);
        Texto("Texto", boton.transform, "Entendido", 13, Vector2.zero, new Vector2(150, 26), Color.white, TextAnchor.MiddleCenter);

        var t = fondo.gameObject.AddComponent<Tutorial>();
        t.id = idEstacion;
        boton.gameObject.AddComponent<Button>().onClick.AddListener(t.Cerrar);
        Abierto = true;
    }

    void Update()
    {
        if (Teclas.AvanzarTeclado()) Cerrar();
    }

    public void Cerrar()
    {
        GameState.TutorialesVistos.Add(id);
        Abierto = false;
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Abierto = false;
    }

    /// <summary>Cierra el tutorial abierto, si hay (lo usa la prueba automatica).</summary>
    public static void CerrarAbierto()
    {
        var t = Object.FindAnyObjectByType<Tutorial>();
        if (t != null) t.Cerrar();
    }

    static Image Caja(string nombre, Transform padre, Vector2 pos, Vector2 tam, Color color)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var r = (RectTransform)go.transform;
        r.anchoredPosition = pos;
        r.sizeDelta = tam;
        var i = go.AddComponent<Image>();
        i.color = color;
        return i;
    }

    static Text Texto(string nombre, Transform padre, string texto, int tam, Vector2 pos, Vector2 caja, Color color, TextAnchor alineacion)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var r = (RectTransform)go.transform;
        r.anchoredPosition = pos;
        r.sizeDelta = caja;
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = tam;
        t.text = texto;
        t.color = color;
        t.alignment = alineacion;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }
}
