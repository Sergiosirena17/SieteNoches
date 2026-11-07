using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Recorre una partida en modo Play sin intervencion: Inicio, Intro, Mapa, Respira, Tormenta, noche,
/// Roca, Lanzamiento y Final. Falla si aparece cualquier error en la Consola.
/// Menu: SieteSoles > Probar partida completa (la prueba sale sola del modo Play al terminar).
/// </summary>
[InitializeOnLoad]
public static class PruebaPartida
{
    const string ClavePaso = "SieteSoles.PruebaPartida.Paso";
    const string ClaveErrores = "SieteSoles.PruebaPartida.Errores";
    const string ClaveConsola = "SieteSoles.PruebaPartida.Consola";
    const int FramesPorPaso = 45;
    static int frames;

    static PruebaPartida()
    {
        if (SessionState.GetInt(ClavePaso, -1) < 0) return;
        EditorApplication.update += Paso;
        Application.logMessageReceived += AlRegistrar;
    }

    [MenuItem("SieteSoles/Probar partida completa")]
    public static void Correr() { Empezar(false); }

    public static void CorrerDesdeConsola() { Empezar(true); }

    static void Empezar(bool consola)
    {
        SessionState.SetInt(ClavePaso, 0);
        SessionState.SetString(ClaveErrores, "");
        SessionState.SetBool(ClaveConsola, consola);
        frames = 0;
        // El proyecto entra a Play sin recargar el codigo, asi que hay que engancharse aqui mismo.
        EditorApplication.update -= Paso;
        EditorApplication.update += Paso;
        Application.logMessageReceived -= AlRegistrar;
        Application.logMessageReceived += AlRegistrar;
        EditorSceneManager.OpenScene("Assets/Escenas/Inicio.unity");
        EditorApplication.EnterPlaymode();
    }

    static void AlRegistrar(string mensaje, string pila, LogType tipo)
    {
        if (tipo != LogType.Error && tipo != LogType.Exception && tipo != LogType.Assert) return;
        if (mensaje.Contains("[PruebaPartida]")) return;
        if (pila != null && pila.Contains("UnityEditor.Search")) return; // error interno del buscador de Unity, no es nuestro
        SessionState.SetString(ClaveErrores, SessionState.GetString(ClaveErrores, "") + "\n- " + mensaje);
    }

    static void Revisar(bool ok, string que)
    {
        Debug.Log("[PruebaPartida] " + (ok ? "OK    " : "FALLA ") + que);
        if (!ok) SessionState.SetString(ClaveErrores, SessionState.GetString(ClaveErrores, "") + "\n- FALLA " + que);
    }

    static T Uno<T>() where T : Object { return Object.FindAnyObjectByType<T>(); }

    static void Llamar(object objeto, string metodo, params object[] args)
    {
        objeto.GetType().GetMethod(metodo, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
              .Invoke(objeto, args);
    }

    static EstacionMision Estacion(string id)
    {
        foreach (var e in Object.FindObjectsByType<EstacionMision>()) if (e.idMision == id) return e;
        return null;
    }

    static void Paso()
    {
        if (!EditorApplication.isPlaying) return;
        if (++frames < FramesPorPaso) return;
        frames = 0;
        int paso = SessionState.GetInt(ClavePaso, 0);
        string escena = SceneManager.GetActiveScene().name;
        switch (paso)
        {
            case 0:
                Revisar(escena == Escenas.Inicio, "Arranca en Inicio");
                Uno<InicioControlador>().Jugar();
                break;
            case 1:
                Revisar(escena == Escenas.Intro, "Jugar lleva a la Intro");
                Revisar(Uno<IntroControlador>().vinetas.Length == 8, "La intro tiene 8 vinetas");
                SceneManager.LoadScene(Escenas.Mapa);
                break;
            case 2:
                Revisar(escena == Escenas.Mapa, "Llega al Mapa");
                var anim = Uno<AnimadorHojaSprites>();
                Revisar(anim != null && anim.GetComponent<SpriteRenderer>().sprite != null, "El jugador tiene sprite cortado de la hoja");
                Revisar(MapaControlador.Instancia.panelPantalla.activeSelf, "Aparece la pantalla del Sol 1");
                Revisar(Estacion(Misiones.Respira).Disponible && !Estacion(Misiones.Huerto).Disponible, "Sol 1: Respira disponible y Huerto no");
                MapaControlador.Instancia.Entrar(Estacion(Misiones.Respira));
                break;
            case 3:
                Revisar(escena == "Mj_Respira", "Entra a Respira");
                var respira = Uno<RespiraControlador>();
                Llamar(respira, "Terminar", Resultado.Gano, "prueba");
                Revisar(respira.panelTarjeta.activeSelf && respira.textoDato.text.Contains("MOXIE"), "Respira muestra la tarjeta de dato real");
                respira.Continuar();
                break;
            case 4:
                Revisar(escena == Escenas.Mapa, "Respira vuelve al Mapa");
                Revisar(GameState.Oxigeno == 100 && GameState.Nave == 30 && GameState.MisionesHoy == 1, "Ganar Respira suma oxígeno y nave");
                MapaControlador.Instancia.Entrar(Estacion(Misiones.Tormenta));
                break;
            case 5:
                Revisar(escena == "Mj_Tormenta", "Entra a Tormenta");
                var tormenta = Uno<TormentaControlador>();
                tormenta.LimpiarPanel(0);
                Llamar(tormenta, "Terminar", Resultado.Perdio, "prueba");
                tormenta.Continuar();
                break;
            case 6:
                Revisar(escena == Escenas.Mapa, "Tormenta vuelve al Mapa");
                Revisar(GameState.Energia == 60, "Perder Tormenta resta energía");
                Revisar(MapaControlador.Instancia.textoPantallaTitulo.text == "Noche 1", "Tras 2 misiones llega la Noche 1");
                SceneManager.LoadScene("Mj_Roca");
                break;
            case 7:
                Revisar(escena == "Mj_Roca", "Abre la escena temporal de la Roca");
                var roca = Uno<MinijuegoPendiente>();
                roca.Boton1();
                roca.Continuar();
                break;
            case 8:
                Revisar(GameState.Marcas.Contains("roca_analizada"), "Analizar la roca deja su marca");
                GameState.Sol = 7; GameState.Nave = 80; GameState.Energia = 50;
                SceneManager.LoadScene(Escenas.Lanzamiento);
                break;
            case 9:
                Revisar(escena == Escenas.Lanzamiento, "Abre Lanzamiento");
                Uno<LanzamientoControlador>().Despegar();
                break;
            case 10:
                Revisar(escena == Escenas.Final, "Despegar lleva al Final");
                Revisar(Uno<FinalControlador>().textoTitulo.text == "Regreso héroe", "Con la roca analizada sale Regreso héroe");
                Terminar();
                return;
        }
        SessionState.SetInt(ClavePaso, paso + 1);
    }

    static void Terminar()
    {
        string errores = SessionState.GetString(ClaveErrores, "");
        bool consola = SessionState.GetBool(ClaveConsola, false);
        SessionState.EraseInt(ClavePaso);
        EditorApplication.update -= Paso;
        Application.logMessageReceived -= AlRegistrar;
        if (errores.Length == 0) Debug.Log("[PruebaPartida] RESULTADO: partida completa sin errores.");
        else Debug.Log("[PruebaPartida] RESULTADO: hubo problemas:" + errores);
        EditorApplication.ExitPlaymode();
        if (consola) EditorApplication.Exit(errores.Length == 0 ? 0 : 1);
    }
}
