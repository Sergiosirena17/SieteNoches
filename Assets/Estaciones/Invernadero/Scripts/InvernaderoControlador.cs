using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Estacion 1: Invernadero hidropónico nocturno. Izquierda: 3 capsulas. Derecha: Manual Astrobotánico.
/// No hay texto de ayuda flotante: el jugador cruza los datos del manual con la configuracion.
/// Lo que configures aqui se aplica a las 06:00 (SimulacionNoche) y se queda para las noches siguientes.
/// </summary>
public class InvernaderoControlador : EstacionBase
{
    [System.Serializable]
    public class SpritesEspecie
    {
        public Especie especie;
        public Sprite brote, crece, madura, estresada;
    }

    [Header("Capsulas (3)")]
    public Image[] marcosSeleccion;
    public Image[] plantas;
    public Text[] etiquetasCapsula;
    public Text[] avisosPerclorato;

    [Header("Panel de la capsula elegida")]
    public Text textoCapsula;
    public GameObject grupoSemillas;
    public GameObject grupoAjustes;
    public Text textoHumedad;
    public Image[] botonesLed;
    public Text textoAccion;
    public GameObject botonAccion;
    public Text textoAviso;

    [Header("Manual")]
    public Text textoManual;
    public Text textoPagina;

    [Header("Dibujos de plantas")]
    public SpritesEspecie[] spritesEspecies;
    public Sprite spritePodrida, spriteCongelada, spriteRetorcida;

    public int Seleccionada { get; private set; }
    int pagina;
    float relojAlucinacion;
    bool alucinando;

    static readonly Color[] ColoresLed = { new Color(0.9f, 0.2f, 0.25f), new Color(0.25f, 0.45f, 0.95f), new Color(0.62f, 0.35f, 0.95f) };
    static readonly Color LedApagado = new Color(0.18f, 0.22f, 0.3f);

    protected override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(idEstacion)) idEstacion = Estaciones.Invernadero;
    }

    void Start()
    {
        Seleccionar(0);
        MostrarPagina(0);
        MostrarTutorial();
        if (GameState.Noche == 5 && HayArabidopsisViva())
            Avisar("La Arabidopsis se retorció y sus hojas están negras. Algo atraviesa el casco...");
    }

    protected override void Update()
    {
        base.Update();
        Dibujar();
        Alucinacion();
    }

    // ------------------------------------------------------------------ Acciones (botones)

    public void Seleccionar(int i)
    {
        Seleccionada = Mathf.Clamp(i, 0, GameState.Capsulas.Length - 1);
        Avisar("");
    }

    /// <summary>Boton de semilla: 1 Espirulina, 2 Papa, 3 Lechuga, 4 Arabidopsis, 5 Dechloromonas.</summary>
    public void Sembrar(int especie)
    {
        var c = GameState.Capsulas[Seleccionada];
        if (!c.Vacia || Saliendo) return;
        var e = (Especie)especie;
        c.Sembrar(e);
        if (c.Toxica)
        {
            GameState.Trip(TripId.Aris).SumarBpm(15f);
            Avisar("Aris: «¡Ese suelo tiene percloratos! Primero hay que limpiarlo.»");
        }
        else Avisar("Sembraste " + Semillas.Info[e].Nombre + ". Ajusta humedad y luz según el manual.");
    }

    public void CambiarHumedad(int delta)
    {
        var c = GameState.Capsulas[Seleccionada];
        c.Humedad = Mathf.Clamp(c.Humedad + delta, 0, 100);
    }

    /// <summary>0 rojo 660 nm, 1 azul 450 nm, 2 UV-A 380 nm.</summary>
    public void AlternarLed(int canal)
    {
        var c = GameState.Capsulas[Seleccionada];
        if (canal == 0) c.LedRojo = !c.LedRojo;
        else if (canal == 1) c.LedAzul = !c.LedAzul;
        else c.LedUv = !c.LedUv;
    }

    /// <summary>Limpia la biomasa podrida o arranca la planta actual.</summary>
    public void Accion()
    {
        var c = GameState.Capsulas[Seleccionada];
        if (c.Vacia) return;
        bool podrida = c.Estado == EstadoPlanta.Podrida;
        c.Vaciar();
        Avisar(podrida ? "Limpiaste la biomasa podrida." : "Arrancaste la planta.");
    }

    public void PaginaSiguiente() { MostrarPagina(pagina + 1); }
    public void PaginaAnterior() { MostrarPagina(pagina - 1); }

    // ------------------------------------------------------------------ Dibujo

    void Dibujar()
    {
        var caps = GameState.Capsulas;
        for (int i = 0; i < caps.Length; i++)
        {
            var c = caps[i];
            if (i < marcosSeleccion.Length && marcosSeleccion[i] != null) marcosSeleccion[i].enabled = i == Seleccionada;
            if (i < plantas.Length && plantas[i] != null)
            {
                var s = SpritePlanta(c);
                plantas[i].sprite = s;
                plantas[i].enabled = s != null;
            }
            if (i < etiquetasCapsula.Length && etiquetasCapsula[i] != null)
                etiquetasCapsula[i].text = (c.Regolito ? "Regolito" : "Hidropónica") + "\n" + (c.Vacia ? "vacía" : c.Info.Nombre);
            if (i < avisosPerclorato.Length && avisosPerclorato[i] != null) avisosPerclorato[i].enabled = c.Perclorato;
        }

        var sel = caps[Seleccionada];
        if (textoCapsula != null) textoCapsula.text = Descripcion(sel);
        if (grupoSemillas != null) grupoSemillas.SetActive(sel.Vacia);
        if (grupoAjustes != null) grupoAjustes.SetActive(true);
        if (textoHumedad != null) textoHumedad.text = "Humedad " + sel.Humedad + " %";
        bool[] leds = { sel.LedRojo, sel.LedAzul, sel.LedUv };
        for (int k = 0; k < botonesLed.Length && k < 3; k++)
            if (botonesLed[k] != null) botonesLed[k].color = leds[k] ? ColoresLed[k] : LedApagado;
        if (botonAccion != null) botonAccion.SetActive(!sel.Vacia);
        if (textoAccion != null) textoAccion.text = sel.Estado == EstadoPlanta.Podrida ? "Limpiar biomasa" : "Arrancar planta";
    }

    static string Descripcion(Capsula c)
    {
        string suelo = c.Regolito ? (c.Perclorato ? "Regolito con percloratos (ClO₄⁻)" : "Regolito limpio") : (c.Perclorato ? "Hidropónica contaminada (ClO₄⁻)" : "Hidropónica limpia");
        if (c.Vacia) return c.Nombre.Split('(')[0].Trim() + " · " + suelo + " · Elige una semilla:";
        string estado;
        switch (c.Estado)
        {
            case EstadoPlanta.Podrida: estado = c.Congelada ? "congelada (muerta)" : "biomasa podrida"; break;
            case EstadoPlanta.Estresada: estado = "estresada: no creció anoche"; break;
            case EstadoPlanta.Madura: estado = "madura"; break;
            default: estado = "noche " + c.Progreso + " de " + c.Info.Noches; break;
        }
        return c.Nombre.Split('(')[0].Trim() + " · " + suelo + " · " + c.Info.Nombre + ": " + estado;
    }

    Sprite SpritePlanta(Capsula c)
    {
        if (c.Vacia) return null;
        if (c.Estado == EstadoPlanta.Podrida) return c.Congelada ? spriteCongelada : spritePodrida;
        if (c.Especie == Especie.Arabidopsis && GameState.Noche == 5) return spriteRetorcida;
        SpritesEspecie set = null;
        if (spritesEspecies != null) foreach (var s in spritesEspecies) if (s.especie == c.Especie) set = s;
        if (set == null) return null;
        if (c.Estado == EstadoPlanta.Estresada) return set.estresada;
        if (c.Estado == EstadoPlanta.Madura) return set.madura;
        return c.Progreso == 0 ? set.brote : set.crece;
    }

    static bool HayArabidopsisViva()
    {
        foreach (var c in GameState.Capsulas) if (c.Especie == Especie.Arabidopsis && c.Viva) return true;
        return false;
    }

    void MostrarPagina(int p)
    {
        pagina = Mathf.Clamp(p, 0, Semillas.PaginasManual.Length - 1);
        alucinando = false;
        if (textoManual != null) textoManual.text = Semillas.PaginasManual[pagina];
        if (textoPagina != null) textoPagina.text = (pagina + 1) + " / " + Semillas.PaginasManual.Length;
    }

    /// <summary>Con oxigeno bajo, una linea del manual cambia por 1 segundo.</summary>
    void Alucinacion()
    {
        if (textoManual == null) return;
        relojAlucinacion += Time.deltaTime;
        if (alucinando && relojAlucinacion > 1f)
        {
            MostrarPagina(pagina);
            relojAlucinacion = 0f;
        }
        else if (!alucinando && GameState.Oxigeno < 30 && relojAlucinacion > 7f)
        {
            var lineas = new List<string>(Semillas.PaginasManual[pagina].Split('\n'));
            if (lineas.Count > 4) lineas[4] = Semillas.LineaAlucinacion;
            textoManual.text = string.Join("\n", lineas);
            alucinando = true;
            relojAlucinacion = 0f;
        }
    }

    void Avisar(string texto)
    {
        if (textoAviso != null) textoAviso.text = texto;
    }

    protected override string ResumenSalida()
    {
        var partes = new List<string>();
        foreach (var c in GameState.Capsulas)
        {
            if (c.Vacia) continue;
            partes.Add(c.Info.Nombre + " " + c.Humedad + " % " + LedsCortos(c));
        }
        return "Invernadero: " + (partes.Count == 0 ? "sin plantas" : string.Join(", ", partes));
    }

    static string LedsCortos(Capsula c)
    {
        if (c.CanalesLed == 0) return "(a oscuras)";
        var l = new List<string>();
        if (c.LedRojo) l.Add("R");
        if (c.LedAzul) l.Add("A");
        if (c.LedUv) l.Add("UV");
        return "(" + string.Join("+", l) + ")";
    }
}
