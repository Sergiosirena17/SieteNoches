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
        "1. Select a pod.",
        "2. Choose a seed. The book on the right shows its required humidity and light.",
        "3. Adjust humidity with − and +, then turn on the required lights.",
        "4. Pods 2 and 3 have toxic soil (ClO₄⁻): clean them first with Dechloromonas before planting food.",
        "Every light uses battery. Plants grow when the night ends at 6:00 AM.",
        } },
        { Estaciones.SoporteVital, new[]
        {
        "1. Hold SPACE to heat the MOXIE reactor. Release it and it cools down.",
        "2. It produces oxygen in the green band. In the red zone, it burns out.",
        "3. If filters fill with soot, press Purge (P).",
        "4. On the right, choose what stays on at night. Everything uses battery: spend more than you have and there will be a blackout.",
        } },
        { Estaciones.Biomedica, new[]
        {
        "1. Choose a crew member. You can talk to only 2 each night.",
        "2. Check the monitor: pulse and CO₂.",
        "3. If CO₂ is red, the problem is the air: choose A. Otherwise, calm them with B or give them food.",
        "Green: helps you.   Red: may sabotage something at night.",
        } },
        { Estaciones.Antena, new[]
        {
        "1. Choose which message to send. SOS brings rescue closer.",
        "2. Move your wave (bright) until it overlaps the satellite wave (faint).",
        "   A / D: wider or narrower.   W / S: higher or lower.",
        "3. Keep it aligned until the bar fills before time runs out.",
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
        Texto("Titulo", caja.transform, "How it works: " + titulo, 15, new Vector2(0, 100), new Vector2(440, 24), new Color(0.95f, 0.76f, 0.31f), TextAnchor.MiddleCenter);
        Texto("Pasos", caja.transform, string.Join("\n", lineas), 12, new Vector2(0, 0), new Vector2(430, 160), Color.white, TextAnchor.UpperLeft).lineSpacing = 1.15f;
        var boton = Caja("Boton Entendido", caja.transform, new Vector2(0, -102), new Vector2(150, 26), new Color(0.18f, 0.29f, 0.49f));
        boton.gameObject.AddComponent<Outline>().effectColor = new Color(0.95f, 0.76f, 0.31f);
        Texto("Texto", boton.transform, "Got it", 13, Vector2.zero, new Vector2(150, 26), Color.white, TextAnchor.MiddleCenter);

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
