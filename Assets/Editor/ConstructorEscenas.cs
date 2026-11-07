using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Crea las escenas base del juego por codigo. Menu: SieteSoles > Construir escenas que falten.
/// IMPORTANTE: solo crea las escenas que NO existen. Nunca borra el trabajo de nadie.
/// Si quieres regenerar una escena, borrala primero a mano.
/// </summary>
public static class ConstructorEscenas
{
    static readonly Color Noche = Hex("#121A2E"), Texto = Hex("#F4F1EA"), Energia = Hex("#F2C14E"),
        Alerta = Hex("#E5484D"), Correcto = Hex("#4CAF7A"), Luz = Hex("#2F4B7C"), Metal = Hex("#4A5563");

    struct Plan { public string Ruta; public Action Crear; }

    static List<Plan> Planes()
    {
        var p = new List<Plan>
        {
            new Plan { Ruta = "Assets/Escenas/Inicio.unity", Crear = CrearInicio },
            new Plan { Ruta = "Assets/Escenas/Intro.unity", Crear = CrearIntro },
            new Plan { Ruta = "Assets/Escenas/Mapa.unity", Crear = CrearMapa },
            new Plan { Ruta = "Assets/Escenas/Lanzamiento.unity", Crear = CrearLanzamiento },
            new Plan { Ruta = "Assets/Escenas/Final.unity", Crear = CrearFinal },
            new Plan { Ruta = "Assets/Minijuegos/Respira/Mj_Respira.unity", Crear = CrearRespira },
            new Plan { Ruta = "Assets/Minijuegos/Tormenta/Mj_Tormenta.unity", Crear = CrearTormenta },
        };
        foreach (var (id, carpeta) in new[] { (Misiones.Huerto, "Huerto"), (Misiones.Refugio, "Refugio"), (Misiones.Insulta, "Insulta"), (Misiones.Roca, "Roca") })
        {
            string idLocal = id;
            p.Add(new Plan { Ruta = "Assets/Minijuegos/" + carpeta + "/Mj_" + carpeta + ".unity", Crear = () => CrearPendiente(idLocal) });
        }
        return p;
    }

    [MenuItem("SieteSoles/Construir escenas que falten")]
    public static void ConstruirFaltantes() { Construir(false); }

    /// <summary>Para linea de comandos (-executeMethod). Tambien solo crea las que faltan.</summary>
    public static void ConstruirDesdeConsola() { Construir(false); }

    static void Construir(bool sobrescribir)
    {
        AssetDatabase.Refresh();
        var escenasBuild = new List<EditorBuildSettingsScene>();
        foreach (var plan in Planes())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(plan.Ruta));
            if (sobrescribir || !File.Exists(plan.Ruta))
            {
                var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                plan.Crear();
                EditorSceneManager.SaveScene(escena, plan.Ruta);
                Debug.Log("[SieteSoles] Escena creada: " + plan.Ruta);
            }
            escenasBuild.Add(new EditorBuildSettingsScene(plan.Ruta, true));
        }
        EditorBuildSettings.scenes = escenasBuild.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("[SieteSoles] Lista de escenas actualizada (" + escenasBuild.Count + ").");
    }

    // ------------------------------------------------------------------ Escenas

    static void CrearInicio()
    {
        var cam = Camara(Noche);
        Luz2D();
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Inicio").AddComponent<InicioControlador>();
        Txt("Titulo", ui, "7 Soles en Marte", 36, new Vector2(0, 80), new Vector2(600, 50), TextAnchor.MiddleCenter, Energia);
        Txt("Subtitulo", ui, "(nombre provisional)", 12, new Vector2(0, 45), new Vector2(400, 20));
        var b = Btn("Boton Jugar", ui, "Jugar", new Vector2(0, -20), new Vector2(160, 36), out _);
        Click(b, ctrl.Jugar);
        Txt("Controles", ui, "WASD o flechas: moverse     E o Espacio: interactuar     Espacio o clic: avanzar texto", 10, new Vector2(0, -150), new Vector2(620, 20));
    }

    static void CrearIntro()
    {
        var cam = Camara(Color.black);
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Intro").AddComponent<IntroControlador>();
        ctrl.imagen = Img("Vineta (640x240)", ui, new Vector2(0, 60), new Vector2(640, 240), Color.white);
        ctrl.imagen.preserveAspect = true;
        ctrl.texto = Txt("Texto de la vineta", ui, "", 16, new Vector2(0, -115), new Vector2(600, 90), TextAnchor.UpperLeft);
        ctrl.textoAyuda = Txt("Ayuda", ui, "", 10, new Vector2(220, -168), new Vector2(180, 16), TextAnchor.MiddleRight, new Color(1, 1, 1, 0.6f));
        string[] textos =
        {
            "Durante siglos, Marte fue un punto rojo en el cielo.",
            "Hasta que la nave Misti salió a buscar respuestas, contigo a bordo.",
            "El viaje fue largo. Solo hablabas con Wayra, la IA de la nave, y con Nikolas, por radio y siempre tarde.",
            "Entonces falló todo al mismo tiempo.",
            "Lo llamaron aterrizaje de emergencia. Fue más emergencia que aterrizaje.",
            "Wayra: Buenas noticias, respiras. Malas noticias, por ahora.",
            "La ventana de regreso se cierra en 7 soles. Si la pierdes, la siguiente tarda 26 meses.",
            "Antes de salir, Wayra detecta algo en el suelo. Una huella. No es tuya.",
        };
        ctrl.vinetas = new IntroControlador.Vineta[textos.Length];
        for (int i = 0; i < textos.Length; i++)
            ctrl.vinetas[i] = new IntroControlador.Vineta { imagen = Spr("Assets/Intro/Vinetas/spr_intro_" + (i + 1).ToString("00") + ".png"), texto = textos[i] };
        ctrl.imagen.sprite = ctrl.vinetas[0].imagen;
        ctrl.texto.text = textos[0];
    }

    static void CrearMapa()
    {
        var cam = Camara(Hex("#0B0F1A"));
        var luz = Luz2D();
        var ui = Lienzo(cam);
        SistemaEventos();

        // ---------- Tilemaps (los bloques se pueden repintar con la Tile Palette) ----------
        var grid = new GameObject("Mapa (Grid)").AddComponent<Grid>();
        var suelo = NuevoTilemap(grid.transform, "Suelo", 0, false);
        var paredes = NuevoTilemap(grid.transform, "Paredes y rocas", 1, true);
        var tPiso = Bloque("tile_nave_piso"); var tPared = Bloque("tile_nave_pared");
        var tMarte = Bloque("tile_marte_suelo"); var tRoca = Bloque("tile_marte_roca"); var tPuerta = Bloque("tile_nave_puerta");

        // Nave: x 0..19, y 0..19. Exterior (superficie de Marte): x 20..39.
        for (int x = 0; x < 20; x++)
            for (int y = 0; y < 20; y++)
            {
                bool borde = x == 0 || y == 0 || x == 19 || y == 19;
                bool muroVertical = (x == 8 || x == 11) && !(y == 3 || y == 4 || y == 9 || y == 10 || y == 15 || y == 16);
                bool muroHorizontal = (y == 6 || y == 12) && (x < 8 || x > 11);
                if (borde || muroVertical || muroHorizontal) paredes.SetTile(new Vector3Int(x, y, 0), tPared);
                else suelo.SetTile(new Vector3Int(x, y, 0), tPiso);
            }
        foreach (var y in new[] { 3, 4 })
        {
            paredes.SetTile(new Vector3Int(19, y, 0), null);
            suelo.SetTile(new Vector3Int(19, y, 0), tPuerta); // esclusa: salida al exterior
        }
        for (int x = 20; x < 40; x++)
            for (int y = 0; y < 20; y++)
            {
                bool borde = y == 0 || y == 19 || x == 39;
                if (borde) paredes.SetTile(new Vector3Int(x, y, 0), tRoca);
                else suelo.SetTile(new Vector3Int(x, y, 0), tMarte);
            }
        foreach (var r in new[] { new Vector3Int(24, 9, 0), new Vector3Int(33, 2, 0), new Vector3Int(37, 12, 0), new Vector3Int(28, 17, 0), new Vector3Int(22, 15, 0) })
            paredes.SetTile(r, tRoca);

        // ---------- Estaciones ----------
        var flecha = Spr("Assets/Comun/UI/ui_flecha_mision.png");
        var estaciones = new GameObject("Estaciones").transform;
        Estacion(estaciones, "Puente", Misiones.Insulta, "puente", new Vector2(4.5f, 16f), flecha, true);
        Estacion(estaciones, "Soporte vital", Misiones.Respira, "soporte", new Vector2(15.5f, 16f), flecha, false);
        Estacion(estaciones, "Invernadero", Misiones.Huerto, "invernadero", new Vector2(4.5f, 9.5f), flecha, false);
        Estacion(estaciones, "Laboratorio", Misiones.Roca, "laboratorio", new Vector2(15.5f, 9.5f), flecha, false);
        Estacion(estaciones, "Refugio", Misiones.Refugio, "refugio", new Vector2(4.5f, 3.5f), flecha, false);
        Estacion(estaciones, "Paneles solares", Misiones.Tormenta, "paneles", new Vector2(27f, 14f), flecha, false);
        Estacion(estaciones, "Zona de hielo", "", "laboratorio", new Vector2(34f, 15f), flecha, false);
        Estacion(estaciones, "Pista de dron", "", "puente", new Vector2(26f, 5f), flecha, false);
        Estacion(estaciones, "Sonda antigua", "", "refugio", new Vector2(34f, 5f), flecha, false);
        Estacion(estaciones, "Sismógrafo", "", "soporte", new Vector2(31f, 9.5f), flecha, false);

        // ---------- Jugador ----------
        var jugador = new GameObject("Jugador");
        jugador.transform.position = new Vector3(10f, 8.5f, 0f);
        var sr = jugador.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        var anim = jugador.AddComponent<AnimadorHojaSprites>();
        anim.hoja = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Comun/Art/Personajes/spr_jugador_hoja.png");
        var rb = jugador.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; rb.freezeRotation = true; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = jugador.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.35f); col.offset = new Vector2(0f, 0.18f);
        var mov = jugador.AddComponent<JugadorMovimiento>();
        mov.animador = anim;

        var seguir = cam.gameObject.AddComponent<CamaraSeguir>();
        seguir.objetivo = jugador.transform;
        seguir.limites = new Vector4(0f, 0f, 40f, 20f);
        cam.transform.position = new Vector3(10f, 9.25f, -10f);

        // ---------- Interfaz ----------
        var ctrl = new GameObject("Mapa (controlador)").AddComponent<MapaControlador>();
        ctrl.jugador = jugador.transform;
        ctrl.luzGlobal = luz;
        Img("Barra superior", ui, new Vector2(0, 160), new Vector2(624, 34), new Color(0.07f, 0.1f, 0.18f, 0.85f));
        ctrl.textoSol = Txt("Sol", ui, "Sol 1 de 7", 14, new Vector2(-235, 167), new Vector2(140, 18), TextAnchor.MiddleLeft, Energia);
        ctrl.textoMisiones = Txt("Misiones", ui, "Misiones: 0 de 2", 10, new Vector2(-235, 152), new Vector2(140, 14), TextAnchor.MiddleLeft);
        string[] nombres = { "oxigeno", "agua", "comida", "energia", "nave", "moral" };
        var textos = new Text[nombres.Length];
        for (int i = 0; i < nombres.Length; i++)
        {
            float x = -110 + i * 62;
            Img("Icono " + nombres[i], ui, new Vector2(x, 160), new Vector2(22, 22), Color.white, Spr("Assets/Comun/UI/ui_icono_" + nombres[i] + ".png"));
            textos[i] = Txt("Valor " + nombres[i], ui, "70", 13, new Vector2(x + 26, 160), new Vector2(30, 18), TextAnchor.MiddleLeft);
        }
        ctrl.textoOxigeno = textos[0]; ctrl.textoAgua = textos[1]; ctrl.textoComida = textos[2];
        ctrl.textoEnergia = textos[3]; ctrl.textoNave = textos[4]; ctrl.textoMoral = textos[5];
        ctrl.textoAviso = Txt("Aviso", ui, "E: Respira, Marte", 14, new Vector2(0, -150), new Vector2(520, 22), TextAnchor.MiddleCenter, Energia);
        ctrl.textoAviso.gameObject.AddComponent<Outline>().effectColor = Color.black;

        var panel = Img("Pantalla de sol y noche", ui, Vector2.zero, new Vector2(640, 360), new Color(0.03f, 0.05f, 0.1f, 0.92f));
        ctrl.panelPantalla = panel.gameObject;
        ctrl.textoPantallaTitulo = Txt("Titulo", panel.transform, "Sol 1", 28, new Vector2(0, 60), new Vector2(560, 40), TextAnchor.MiddleCenter, Energia);
        ctrl.textoPantallaCuerpo = Txt("Cuerpo", panel.transform, "", 14, new Vector2(0, -30), new Vector2(520, 140), TextAnchor.UpperCenter);
        panel.gameObject.SetActive(false);
    }

    static void CrearLanzamiento()
    {
        var cam = Camara(Noche);
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Lanzamiento").AddComponent<LanzamientoControlador>();
        Txt("Titulo", ui, "Sol 7: lanzamiento", 28, new Vector2(0, 110), new Vector2(560, 40), TextAnchor.MiddleCenter, Energia);
        ctrl.textoEstado = Txt("Estado", ui, "Nave: 70 / 70\nEnergía: 30 / 30", 16, new Vector2(0, 10), new Vector2(520, 140), TextAnchor.MiddleCenter);
        var b = Btn("Boton Despegar", ui, "Despegar", new Vector2(0, -120), new Vector2(180, 36), out _);
        Click(b, ctrl.Despegar);
    }

    static void CrearFinal()
    {
        var cam = Camara(Color.black);
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Final").AddComponent<FinalControlador>();
        Img("Imagen del final (reemplazar)", ui, new Vector2(-170, 30), new Vector2(240, 180), Metal);
        ctrl.textoTitulo = Txt("Titulo", ui, "Título del final", 26, new Vector2(110, 140), new Vector2(380, 36), TextAnchor.MiddleLeft, Energia);
        ctrl.textoTipo = Txt("Tipo", ui, "Final", 12, new Vector2(110, 115), new Vector2(380, 18), TextAnchor.MiddleLeft);
        ctrl.textoCuerpo = Txt("Texto", ui, "", 14, new Vector2(110, 55), new Vector2(380, 90), TextAnchor.UpperLeft);
        ctrl.textoResumen = Txt("Resumen", ui, "Tus decisiones:", 10, new Vector2(110, -55), new Vector2(380, 120), TextAnchor.UpperLeft);
        var b = Btn("Boton Jugar de nuevo", ui, "Jugar de nuevo", new Vector2(0, -150), new Vector2(180, 32), out _);
        Click(b, ctrl.JugarDeNuevo);
    }

    static void CrearRespira()
    {
        var cam = Camara(Hex("#1A2236"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Respira (controlador)").AddComponent<RespiraControlador>();
        ctrl.idMision = Misiones.Respira;
        Llenar(Img("Fondo", ui, Vector2.zero, Vector2.zero, Color.white, Spr("Assets/Minijuegos/Respira/Art/spr_respira_fondo.png")));
        Txt("Titulo", ui, "Respira, Marte", 20, new Vector2(0, 160), new Vector2(400, 28), TextAnchor.MiddleCenter, Energia);
        Txt("Instrucciones", ui, "Mantén ESPACIO para calentar y pulsa E para bombear. Las dos barras deben quedar en la zona verde.", 11, new Vector2(0, 138), new Vector2(600, 18));
        ctrl.textoTiempo = Txt("Tiempo", ui, "60 s", 16, new Vector2(270, 160), new Vector2(80, 24), TextAnchor.MiddleRight);
        ctrl.textoEstado = Txt("Estado", ui, "", 14, new Vector2(0, 40), new Vector2(240, 60), TextAnchor.MiddleCenter, Texto);
        ctrl.textoEstado.gameObject.AddComponent<Outline>().effectColor = Color.black;

        Medidor(ui, "Temperatura", new Vector2(-170, 10), Hex("#E8833A"), out ctrl.rellenoTemperatura, out ctrl.zonaTemperatura);
        Medidor(ui, "Presión", new Vector2(170, 10), Hex("#5AA9E6"), out ctrl.rellenoPresion, out ctrl.zonaPresion);

        Txt("Etiqueta oxigeno", ui, "Oxígeno producido", 11, new Vector2(0, -100), new Vector2(300, 16));
        var marco = Img("Barra oxigeno", ui, new Vector2(0, -118), new Vector2(300, 16), Hex("#0E1424"));
        var rellenoO2 = Img("Relleno", marco.transform, Vector2.zero, Vector2.zero, Hex("#7EC8E3"));
        ctrl.rellenoOxigeno = rellenoO2.rectTransform;
        ctrl.rellenoOxigeno.anchorMin = Vector2.zero; ctrl.rellenoOxigeno.anchorMax = new Vector2(0f, 1f);
        ctrl.rellenoOxigeno.offsetMin = ctrl.rellenoOxigeno.offsetMax = Vector2.zero;

        var calentar = Btn("Boton Calentar (mantener)", ui, "Calentar (ESPACIO)", new Vector2(-170, -150), new Vector2(150, 28), out _);
        ctrl.botonCalentar = calentar.gameObject.AddComponent<BotonMantener>();
        UnityEngine.Object.DestroyImmediate(calentar); // se mantiene presionado, no es un clic
        var bombear = Btn("Boton Bombear", ui, "Bombear (E)", new Vector2(170, -150), new Vector2(150, 28), out _);
        Click(bombear, ctrl.BotonBombear);
        Tarjeta(ui, ctrl);
    }

    static void CrearTormenta()
    {
        var cam = Camara(Hex("#D9A06F"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Tormenta (controlador)").AddComponent<TormentaControlador>();
        ctrl.idMision = Misiones.Tormenta;
        Llenar(Img("Fondo", ui, Vector2.zero, Vector2.zero, Color.white, Spr("Assets/Minijuegos/Tormenta/Art/spr_tormenta_fondo.png")));
        Txt("Titulo", ui, "Tormenta de polvo", 20, new Vector2(0, 160), new Vector2(400, 28), TextAnchor.MiddleCenter, Noche);
        Txt("Instrucciones", ui, "Haz clic en los paneles (o teclas 1 a 6) para limpiarlos antes de que se vaya la luz.", 11, new Vector2(0, 138), new Vector2(600, 18), TextAnchor.MiddleCenter, Noche);
        ctrl.textoTiempo = Txt("Tiempo", ui, "75 s", 16, new Vector2(270, 160), new Vector2(80, 24), TextAnchor.MiddleRight, Noche);
        ctrl.textoEstado = Txt("Estado", ui, "", 14, new Vector2(0, 108), new Vector2(400, 20), TextAnchor.MiddleCenter, Texto);
        ctrl.textoEstado.gameObject.AddComponent<Outline>().effectColor = Color.black;

        var spPanel = Spr("Assets/Minijuegos/Tormenta/Art/spr_tormenta_panel.png");
        var spPolvo = Spr("Assets/Minijuegos/Tormenta/Art/spr_tormenta_polvo.png");
        ctrl.capasPolvo = new Image[TormentaLogica.CantidadPaneles];
        for (int i = 0; i < TormentaLogica.CantidadPaneles; i++)
        {
            var pos = new Vector2(-140 + (i % 3) * 140, i < 3 ? 45 : -55);
            var img = Img("Panel " + (i + 1), ui, pos, new Vector2(120, 80), Color.white, spPanel);
            var boton = img.gameObject.AddComponent<Button>();
            UnityEventTools.AddIntPersistentListener(boton.onClick, ctrl.LimpiarPanel, i);
            var polvo = Img("Polvo", img.transform, Vector2.zero, new Vector2(120, 80), new Color(1, 1, 1, 0.4f), spPolvo);
            polvo.raycastTarget = false;
            ctrl.capasPolvo[i] = polvo;
            Txt("Numero", img.transform, (i + 1).ToString(), 12, new Vector2(-48, 28), new Vector2(20, 18), TextAnchor.MiddleCenter, Energia);
        }
        Txt("Etiqueta energia", ui, "Energía guardada", 11, new Vector2(0, -112), new Vector2(300, 16), TextAnchor.MiddleCenter, Noche);
        var marco = Img("Barra energia", ui, new Vector2(0, -130), new Vector2(300, 16), Hex("#0E1424"));
        var relleno = Img("Relleno", marco.transform, Vector2.zero, Vector2.zero, Energia);
        ctrl.rellenoEnergia = relleno.rectTransform;
        ctrl.rellenoEnergia.anchorMin = Vector2.zero; ctrl.rellenoEnergia.anchorMax = new Vector2(0f, 1f);
        ctrl.rellenoEnergia.offsetMin = ctrl.rellenoEnergia.offsetMax = Vector2.zero;
        ctrl.oscuridad = Img("Oscuridad de la tormenta", ui, Vector2.zero, Vector2.zero, new Color(0.05f, 0.03f, 0.02f, 0f));
        ctrl.oscuridad.raycastTarget = false;
        Llenar(ctrl.oscuridad);
        Tarjeta(ui, ctrl);
    }

    static void CrearPendiente(string id)
    {
        var cam = Camara(Noche);
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Minijuego pendiente (reemplazar)").AddComponent<MinijuegoPendiente>();
        ctrl.idMision = id;
        ctrl.textoTitulo = Txt("Titulo", ui, Misiones.Nombre[id] + "\n(en construcción)", 22, new Vector2(0, 110), new Vector2(560, 70), TextAnchor.MiddleCenter, Energia);
        Txt("Nota", ui, "Escena temporal para probar la partida. Quien programe este minijuego reemplaza este objeto por su propio controlador.", 11, new Vector2(0, 55), new Vector2(560, 34));
        var b1 = Btn("Boton 1", ui, "Simular: ganar", new Vector2(0, 0), new Vector2(220, 30), out ctrl.textoBoton1);
        var b2 = Btn("Boton 2", ui, "Simular: a medias", new Vector2(0, -45), new Vector2(220, 30), out ctrl.textoBoton2);
        var b3 = Btn("Boton 3", ui, "Simular: perder", new Vector2(0, -90), new Vector2(220, 30), out ctrl.textoBoton3);
        Click(b1, ctrl.Boton1); Click(b2, ctrl.Boton2); Click(b3, ctrl.Boton3);
        Tarjeta(ui, ctrl);
    }

    // ------------------------------------------------------------------ Piezas

    static void Medidor(Transform ui, string nombre, Vector2 pos, Color color, out RectTransform relleno, out RectTransform zona)
    {
        var marco = Img("Medidor " + nombre, ui, pos, new Vector2(36, 200), Hex("#0E1424"));
        var r = Img("Relleno", marco.transform, Vector2.zero, Vector2.zero, color);
        relleno = r.rectTransform;
        relleno.anchorMin = Vector2.zero; relleno.anchorMax = new Vector2(1f, 0.4f);
        relleno.offsetMin = relleno.offsetMax = Vector2.zero;
        var z = Img("Zona segura", marco.transform, Vector2.zero, Vector2.zero, new Color(Correcto.r, Correcto.g, Correcto.b, 0.45f));
        zona = z.rectTransform;
        zona.anchorMin = new Vector2(0f, 0.55f); zona.anchorMax = new Vector2(1f, 0.8f);
        zona.offsetMin = zona.offsetMax = Vector2.zero;
        Txt("Etiqueta", marco.transform, nombre, 11, new Vector2(0, -112), new Vector2(120, 16));
    }

    static void Tarjeta(Transform ui, MinijuegoBase mj)
    {
        var panel = Img("Tarjeta de dato real", ui, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.85f));
        Llenar(panel);
        var caja = Img("Caja", panel.transform, Vector2.zero, new Vector2(480, 230), Hex("#1E2A44"));
        caja.gameObject.AddComponent<Outline>().effectColor = Energia;
        mj.textoResultado = Txt("Resultado", caja.transform, "¡Lo lograste!", 18, new Vector2(0, 75), new Vector2(440, 50), TextAnchor.MiddleCenter, Energia);
        mj.textoDato = Txt("Dato real", caja.transform, "Dato real: ...", 13, new Vector2(0, 5), new Vector2(440, 70), TextAnchor.MiddleCenter);
        mj.textoFuente = Txt("Fuente", caja.transform, "Fuente: ...", 10, new Vector2(0, -45), new Vector2(440, 16), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.7f));
        var b = Btn("Boton Continuar", caja.transform, "Continuar", new Vector2(0, -85), new Vector2(150, 30), out _);
        Click(b, mj.Continuar);
        mj.panelTarjeta = panel.gameObject;
        panel.gameObject.SetActive(false);
    }

    static void Estacion(Transform padre, string nombre, string id, string sprite, Vector2 pos, Sprite flecha, bool esPuente)
    {
        var go = new GameObject("Estacion " + nombre);
        go.transform.SetParent(padre);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Spr("Assets/Comun/Art/Estaciones/spr_estacion_" + sprite + ".png");
        sr.sortingOrder = 2;
        if (string.IsNullOrEmpty(id)) sr.color = new Color(1f, 1f, 1f, 0.55f);
        var solido = go.AddComponent<BoxCollider2D>();
        solido.size = new Vector2(1.8f, 1.2f); solido.offset = new Vector2(0f, -0.3f);
        var zona = go.AddComponent<CircleCollider2D>();
        zona.isTrigger = true; zona.radius = 1.6f;
        var e = go.AddComponent<EstacionMision>();
        e.idMision = id; e.nombreVisible = nombre; e.esPuente = esPuente;
        var ind = new GameObject("Indicador de mision");
        ind.transform.SetParent(go.transform, false);
        ind.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        var isr = ind.AddComponent<SpriteRenderer>();
        isr.sprite = flecha; isr.sortingOrder = 5;
        e.indicador = isr;
    }

    static Tilemap NuevoTilemap(Transform grid, string nombre, int orden, bool solido)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(grid, false);
        var tm = go.AddComponent<Tilemap>();
        go.AddComponent<TilemapRenderer>().sortingOrder = orden;
        if (solido) go.AddComponent<TilemapCollider2D>();
        return tm;
    }

    static Tile Bloque(string nombre)
    {
        string ruta = "Assets/Comun/Art/Bloques/Tiles/" + nombre + ".asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
        if (t != null) return t;
        Directory.CreateDirectory("Assets/Comun/Art/Bloques/Tiles");
        t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = Spr("Assets/Comun/Art/Bloques/" + nombre + ".png");
        t.colliderType = Tile.ColliderType.Grid;
        AssetDatabase.CreateAsset(t, ruta);
        return t;
    }

    static Camera Camara(Color fondo)
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        var c = go.AddComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = 360f / 2f / ImportadorPixelArt.PixelesPorUnidad; // 640x360 a 32 px por unidad
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = fondo;
        go.transform.position = new Vector3(0f, 0f, -10f);
        go.AddComponent<UniversalAdditionalCameraData>();
        go.AddComponent<AudioListener>();
        return c;
    }

    static Light2D Luz2D()
    {
        var go = new GameObject("Luz global 2D");
        var l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Global;
        l.intensity = 1f;
        l.color = Color.white;
        var ids = new int[SortingLayer.layers.Length];
        for (int i = 0; i < ids.Length; i++) ids[i] = SortingLayer.layers[i].id;
        l.targetSortingLayers = ids;
        return l;
    }

    static Transform Lienzo(Camera cam)
    {
        var go = new GameObject("UI");
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceCamera;
        c.worldCamera = cam;
        c.planeDistance = 1f;
        c.sortingOrder = 100;
        var s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(640f, 360f);
        s.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go.transform;
    }

    static void SistemaEventos()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    static RectTransform Rect(string nombre, Transform padre, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var r = (RectTransform)go.transform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = tam;
        return r;
    }

    static void Llenar(Graphic g)
    {
        var r = g.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static Image Img(string nombre, Transform padre, Vector2 pos, Vector2 tam, Color color, Sprite sprite = null)
    {
        var i = Rect(nombre, padre, pos, tam).gameObject.AddComponent<Image>();
        i.color = color;
        i.sprite = sprite;
        return i;
    }

    static Text Txt(string nombre, Transform padre, string texto, int tam, Vector2 pos, Vector2 caja,
                    TextAnchor alineacion = TextAnchor.MiddleCenter, Color? color = null)
    {
        var t = Rect(nombre, padre, pos, caja).gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = tam;
        t.text = texto;
        t.alignment = alineacion;
        t.color = color ?? Texto;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Button Btn(string nombre, Transform padre, string etiqueta, Vector2 pos, Vector2 tam, out Text texto)
    {
        var img = Img(nombre, padre, pos, tam, Luz);
        img.gameObject.AddComponent<Outline>().effectColor = Energia;
        var b = img.gameObject.AddComponent<Button>();
        texto = Txt("Texto", img.transform, etiqueta, 13, Vector2.zero, tam);
        return b;
    }

    static void Click(Button b, UnityAction accion)
    {
        UnityEventTools.AddPersistentListener(b.onClick, accion);
    }

    static Sprite Spr(string ruta)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (s == null) Debug.LogWarning("[SieteSoles] No encontré el sprite: " + ruta);
        return s;
    }

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out var c);
        return c;
    }
}
