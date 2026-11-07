using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Recorre una partida en modo Play sin intervencion: Inicio, Intro, Noche 1 (Invernadero, Soporte Vital,
/// Biomédica, Camarotes), las 06:00, Noche 2 (Antena), la carrera de la Noche 7, el Puente y el Final.
/// Falla si aparece cualquier error en la Consola.
/// Menu: SieteNoches > Probar partida completa (la prueba sale sola del modo Play al terminar).
/// </summary>
[InitializeOnLoad]
public static class PruebaPartida
{
    const string ClavePaso = "SieteNoches.PruebaPartida.Paso";
    const string ClaveErrores = "SieteNoches.PruebaPartida.Errores";
    const string ClaveConsola = "SieteNoches.PruebaPartida.Consola";
    const int FramesPorPaso = 45;
    static int frames;

    static PruebaPartida()
    {
        if (SessionState.GetInt(ClavePaso, -1) < 0) return;
        EditorApplication.update += Paso;
        Application.logMessageReceived += AlRegistrar;
    }

    [MenuItem("SieteNoches/Probar partida completa")]
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
        if (pila != null && pila.Contains("PruebaPartida:Foto")) return; // URP en modo consola al sacar la foto, no es del juego
        SessionState.SetString(ClaveErrores, SessionState.GetString(ClaveErrores, "") + "\n- " + mensaje);
    }

    static void Revisar(bool ok, string que)
    {
        Debug.Log("[PruebaPartida] " + (ok ? "OK    " : "FALLA ") + que);
        if (!ok) SessionState.SetString(ClaveErrores, SessionState.GetString(ClaveErrores, "") + "\n- FALLA " + que);
    }

    static T Uno<T>() where T : Object { return Object.FindAnyObjectByType<T>(); }

    static EstacionMapa Estacion(string id)
    {
        foreach (var e in Object.FindObjectsByType<EstacionMapa>()) if (e.idEstacion == id) return e;
        return null;
    }

    static MapaControlador Mapa => MapaControlador.Instancia;

    /// <summary>Guarda lo que ve la camara en Docs/Capturas/Juego_*.png para revisar las pantallas con datos.</summary>
    static void Foto(string nombre)
    {
        var cam = Camera.main;
        if (cam == null) return;
        string carpeta = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Capturas"));
        Directory.CreateDirectory(carpeta);
        var rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(carpeta, "Juego_" + nombre + ".png"), tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
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
                Revisar(Uno<IntroControlador>().vinetas.Length == 8, "La intro tiene 8 viñetas");
                SceneManager.LoadScene(Escenas.Mapa);
                break;
            case 2:
                Revisar(escena == Escenas.Mapa, "Llega al Mapa");
                var anim = Uno<AnimadorHojaSprites>();
                Revisar(anim != null && anim.GetComponent<SpriteRenderer>().sprite != null, "El jugador tiene sprite cortado de la hoja");
                Revisar(Mapa.PantallaAbierta && Mapa.textoPantallaTitulo.text.StartsWith("Noche 1"), "Aparece la pantalla de las 18:30 de la Noche 1");
                Revisar(!Estacion(Estaciones.Antena).Disponible, "Noche 1: la antena todavía no funciona");
                Foto("01_Noche1_1830");
                Mapa.ContinuarPantalla();
                break;
            case 3:
                Revisar(!Mapa.PantallaAbierta, "Continuar cierra la pantalla");
                Foto("02_Mapa");
                Mapa.Entrar(Estacion(Estaciones.Invernadero));
                break;
            case 4:
            {
                Revisar(escena == "Est_Invernadero", "Entra al Invernadero");
                var inv = Uno<InvernaderoControlador>();
                inv.Seleccionar(0);
                inv.Sembrar((int)Especie.Espirulina);
                for (int i = 0; i < 8; i++) inv.CambiarHumedad(5);
                inv.AlternarLed(0); inv.AlternarLed(1);
                var c = GameState.Capsulas[0];
                Revisar(c.Especie == Especie.Espirulina && c.ConfiguracionCorrecta(false, out _), "Siembra Espirulina con 90 % y LED rojo + azul");
                break;
            }
            case 5:
            {
                var inv = Uno<InvernaderoControlador>();
                Revisar(inv.textoManual.text.Contains("Espirulina") && inv.plantas[0].enabled, "Se ve la planta y el manual en la página 1");
                Foto("03_Invernadero");
                inv.Salir();
                Revisar(inv.panelTarjeta.activeSelf && inv.textoDato.text.Contains("Veggie"), "Al salir muestra la tarjeta de dato real");
                inv.Continuar();
                break;
            }
            case 6:
                Revisar(escena == Escenas.Mapa && GameState.Visitas == 1 && GameState.Hora == "21:00", "Vuelve al mapa a las 21:00");
                Mapa.Entrar(Estacion(Estaciones.SoporteVital));
                break;
            case 7:
            {
                Revisar(escena == "Est_SoporteVital", "Entra a Soporte Vital");
                var sv = Uno<SoporteVitalControlador>();
                Revisar(sv.Logica != null, "El MOXIE está listo para encenderse");
                sv.AlternarSector(2);
                Revisar(!GameState.Ventiladores, "Se pueden apagar los ventiladores");
                sv.AlternarSector(2);
                sv.Logica.Temperatura = 805f; sv.Logica.Produccion = 0.4f; sv.Logica.Hollin = 0.3f;
                break;
            }
            case 8:
            {
                var sv = Uno<SoporteVitalControlador>();
                Foto("04_SoporteVital");
                sv.ForzarProduccion(1f);
                Revisar(GameState.MoxieUsado && Mathf.Approximately(GameState.MoxieProduccion, 1f), "El MOXIE guarda su producción");
                sv.Salir(); sv.Continuar();
                break;
            }
            case 9:
                Revisar(escena == Escenas.Mapa, "Soporte Vital vuelve al mapa");
                Mapa.Entrar(Estacion(Estaciones.Biomedica));
                break;
            case 10:
            {
                Revisar(escena == "Est_Biomedica", "Entra a la Terminal Biomédica");
                var bio = Uno<BiomedicaControlador>();
                bio.Elegir((int)TripId.Jonas);
                bio.CompletarTexto();
                break;
            }
            case 11:
            {
                var bio = Uno<BiomedicaControlador>();
                Foto("05_Biomedica");
                bio.Opcion(Dialogos.OpcionPromesa);
                bio.CompletarTexto();
                Revisar(GameState.Trip(TripId.Jonas).Promesa == Promesa.Calefaccion, "Prometerle calefacción a Jonas queda guardado");
                bio.HablarConOtro();
                bio.Elegir((int)TripId.Elena);
                bio.CompletarTexto();
                bio.Opcion(Dialogos.OpcionDiagnostico);
                bio.CompletarTexto();
                Revisar(GameState.Charlas == 2, "Habló con 2 tripulantes");
                bio.Salir(); bio.Continuar();
                break;
            }
            case 12:
                Revisar(!Estacion(Estaciones.Biomedica).Disponible, "La Biomédica se cierra tras 2 charlas");
                Mapa.Entrar(Estacion(Estaciones.Camarotes));
                break;
            case 13:
                Revisar(Mapa.PantallaAbierta && Mapa.textoPantallaTitulo.text == "6:00 AM", "Terminar la guardia lleva a las 6:00 AM");
                Revisar(GameState.Noche == 2 && GameState.Capsulas[0].Estado == EstadoPlanta.Madura, "La noche se resolvió: Noche 2 y espirulina madura");
                Foto("06_Amanecer");
                Mapa.ContinuarPantalla();
                break;
            case 14:
                Revisar(Mapa.PantallaAbierta && Mapa.textoPantallaTitulo.text.StartsWith("Noche 2"), "Aparece la pantalla de la Noche 2");
                Mapa.ContinuarPantalla();
                break;
            case 15:
                Revisar(Estacion(Estaciones.Antena).Disponible, "Noche 2: la antena ya funciona");
                Mapa.Entrar(Estacion(Estaciones.Antena));
                break;
            case 16:
            {
                Revisar(escena == "Est_Antena", "Entra a la Antena");
                var an = Uno<AntenaControlador>();
                an.ElegirPaquete(0);
                Revisar(an.grupoOscilo.activeSelf && an.Logica != null, "Elegir SOS abre el osciloscopio");
                an.Logica.Frecuencia = 0.62f;
                break;
            }
            case 17:
            {
                var an = Uno<AntenaControlador>();
                Foto("07_Antena");
                an.TerminarAlineacion(true);
                Revisar(GameState.ProgresoRescate == 1 && an.grupoMensaje.activeSelf, "El SOS llega: rescate 1/4 y responde Nikolas");
                break;
            }
            case 18:
            {
                var an = Uno<AntenaControlador>();
                Foto("08_Nikolas");
                an.Responder(1);
                an.Salir(); an.Continuar();
                break;
            }
            case 19:
                Revisar(escena == Escenas.Mapa && GameState.NikolasDiscusiones == 1, "Vuelve al mapa con 1 discusión con Nikolas");
                // Saltar a la Noche 7 con una partida buena para probar la carrera final.
                GameState.ProgresoRescate = 4; GameState.CosechaLimpia = true; GameState.Oxigeno = 80;
                foreach (var t in GameState.Tripulacion) t.Bpm = 70f;
                SimulacionNoche.EmpezarNoche(7, 20);
                GameState.PosicionJugador = null;
                SceneManager.LoadScene(Escenas.Mapa);
                break;
            case 20:
                Revisar(Mapa.PantallaAbierta && Mapa.textoPantallaTitulo.text.StartsWith("Noche 7"), "Noche 7: pantalla de la hora más oscura");
                Mapa.ContinuarPantalla();
                break;
            case 21:
                Revisar(Mapa.TiempoRestante > 0f && Mapa.TiempoRestante < MapaControlador.SegundosNoche7, "Corre la cuenta regresiva de 60 s");
                Foto("09_Noche7");
                Mapa.Entrar(Estacion(Estaciones.Puente));
                Revisar(SceneManager.GetActiveScene().name == Escenas.Mapa, "Sin cerrar las válvulas no deja despegar");
                Mapa.Entrar(Estacion(Estaciones.SoporteVital));
                Revisar(GameState.ValvulasCerradas, "Soporte Vital cierra las válvulas");
                Mapa.Entrar(Estacion(Estaciones.Puente));
                break;
            case 22:
                Revisar(escena == Escenas.Lanzamiento, "El Puente abre el Lanzamiento");
                Foto("10_Lanzamiento");
                Uno<LanzamientoControlador>().Despegar();
                break;
            case 23:
                Revisar(escena == Escenas.Final, "Despegar lleva al Final");
                var fin = Uno<FinalControlador>();
                Revisar(fin.IdFinal == EvaluadorFinal.Heroe && fin.textoTitulo.text == "Regreso héroe", "Con rescate 4/4 y todos sanos sale Regreso héroe");
                Revisar(fin.textoInforme.text.Contains("INFORME CIENTÍFICO"), "El final muestra el Informe Científico de Misión");
                Foto("11_Final");
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
