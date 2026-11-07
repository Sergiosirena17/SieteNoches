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
/// Crea las escenas base del juego por codigo. Menu: SieteNoches > Construir escenas que falten.
/// IMPORTANTE: solo crea las escenas que NO existen. Nunca borra el trabajo de nadie.
/// Si quieres regenerar una escena, borrala primero a mano.
/// </summary>
public static class ConstructorEscenas
{
    static readonly Color Noche = Hex("#121A2E"), Texto = Hex("#F4F1EA"), Energia = Hex("#F2C14E"),
        Alerta = Hex("#E5484D"), Correcto = Hex("#4CAF7A"), Luz = Hex("#2F4B7C"), Metal = Hex("#4A5563"),
        Oscuro = Hex("#0E1424"), Tinta = Hex("#2A2418");

    struct Plan { public string Ruta; public Action Crear; }

    static List<Plan> Planes()
    {
        return new List<Plan>
        {
            new Plan { Ruta = "Assets/Escenas/Inicio.unity", Crear = CrearInicio },
            new Plan { Ruta = "Assets/Escenas/Intro.unity", Crear = CrearIntro },
            new Plan { Ruta = "Assets/Escenas/Mapa.unity", Crear = CrearMapa },
            new Plan { Ruta = "Assets/Escenas/Lanzamiento.unity", Crear = CrearLanzamiento },
            new Plan { Ruta = "Assets/Escenas/Final.unity", Crear = CrearFinal },
            new Plan { Ruta = "Assets/Estaciones/Invernadero/Est_Invernadero.unity", Crear = CrearInvernadero },
            new Plan { Ruta = "Assets/Estaciones/SoporteVital/Est_SoporteVital.unity", Crear = CrearSoporteVital },
            new Plan { Ruta = "Assets/Estaciones/Biomedica/Est_Biomedica.unity", Crear = CrearBiomedica },
            new Plan { Ruta = "Assets/Estaciones/Antena/Est_Antena.unity", Crear = CrearAntena },
        };
    }

    [MenuItem("SieteNoches/Construir escenas que falten")]
    public static void ConstruirFaltantes() { Construir(); }

    /// <summary>Para linea de comandos (-executeMethod). Tambien solo crea las que faltan.</summary>
    public static void ConstruirDesdeConsola() { Construir(); }

    static void Construir()
    {
        AssetDatabase.Refresh();
        var escenasBuild = new List<EditorBuildSettingsScene>();
        foreach (var plan in Planes())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(plan.Ruta));
            if (!File.Exists(plan.Ruta))
            {
                var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                plan.Crear();
                EditorSceneManager.SaveScene(escena, plan.Ruta);
                Debug.Log("[SieteNoches] Escena creada: " + plan.Ruta);
            }
            escenasBuild.Add(new EditorBuildSettingsScene(plan.Ruta, true));
        }
        EditorBuildSettings.scenes = escenasBuild.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("[SieteNoches] Lista de escenas actualizada (" + escenasBuild.Count + ").");
    }

    // ================================================================== Pantallas

    static void CrearInicio()
    {
        var cam = Camara(Noche);
        Luz2D();
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Inicio").AddComponent<InicioControlador>();
        Txt("Titulo", ui, "7 Noches en Marte", 36, new Vector2(0, 80), new Vector2(600, 50), TextAnchor.MiddleCenter, Energia);
        Txt("Subtitulo", ui, "Guardia nocturna: sobrevive hasta las 6:00 AM. Siete veces.", 12, new Vector2(0, 45), new Vector2(500, 20));
        var b = Btn("Boton Jugar", ui, "Jugar", new Vector2(0, -20), new Vector2(160, 36), out _);
        Click(b, ctrl.Jugar);
        Txt("Controles", ui, "WASD o flechas: moverse     E: entrar a una estación     Espacio: avanzar texto", 10, new Vector2(0, -150), new Vector2(620, 20));
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
            "La nave Misti salió con cinco personas: tú, la comandante Elena Vance, Marco \"Tornillo\" Silva, la Dra. Aris Thorne y Jonas Miller.",
            "Desde Houston, Nikolas te hablaba por radio. Siempre con 12 minutos de retraso.",
            "Entonces falló todo al mismo tiempo.",
            "Aterrizaje de emergencia en Arcadia Planitia. Fue más emergencia que aterrizaje.",
            "Se pone el sol. De noche los paneles solares dan 0 W y afuera la temperatura baja de -10 °C a -110 °C.",
            "La ventana de regreso se cierra en 7 noches. Si la pierdes, la siguiente tarda 26 meses.",
            "Tu trabajo: hacer guardia cada noche hasta las 06:00 AM. Que nada se apague. Ni las plantas, ni las máquinas, ni la tripulación.",
        };
        ctrl.vinetas = new IntroControlador.Vineta[textos.Length];
        for (int i = 0; i < textos.Length; i++)
            ctrl.vinetas[i] = new IntroControlador.Vineta { imagen = Spr("Assets/Intro/Vinetas/spr_intro_" + (i + 1).ToString("00") + ".png"), texto = textos[i] };
        ctrl.imagen.sprite = ctrl.vinetas[0].imagen;
        ctrl.texto.text = textos[0];
    }

    static void CrearLanzamiento()
    {
        var cam = Camara(Hex("#2A0B0B"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Lanzamiento").AddComponent<LanzamientoControlador>();
        Txt("Titulo", ui, "05:59 AM · Puente", 26, new Vector2(0, 140), new Vector2(560, 36), TextAnchor.MiddleCenter, Energia);
        Txt("Pregunta", ui, "Sale el sol en un minuto. ¿Qué hacen?", 13, new Vector2(0, 112), new Vector2(560, 20));
        ctrl.textoEstado = Txt("Estado", ui, "Rescate: 0/4", 12, new Vector2(0, 10), new Vector2(560, 170), TextAnchor.UpperCenter);
        var b1 = Btn("Boton Despegar", ui, "Despegar en la ventana", new Vector2(-130, -140), new Vector2(220, 32), out _);
        Click(b1, ctrl.Despegar);
        var b2 = Btn("Boton Sellar domo", ui, "Sellar el domo y quedarnos", new Vector2(130, -140), new Vector2(220, 32), out _);
        Click(b2, ctrl.SellarDomo);
    }

    static void CrearFinal()
    {
        var cam = Camara(Color.black);
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Final").AddComponent<FinalControlador>();
        Img("Imagen del final (reemplazar)", ui, new Vector2(-200, 85), new Vector2(200, 150), Metal);
        ctrl.textoTitulo = Txt("Titulo", ui, "Título del final", 22, new Vector2(100, 150), new Vector2(380, 30), TextAnchor.MiddleLeft, Energia);
        ctrl.textoTipo = Txt("Tipo", ui, "Final", 11, new Vector2(100, 128), new Vector2(380, 16), TextAnchor.MiddleLeft);
        ctrl.textoCuerpo = Txt("Texto", ui, "", 12, new Vector2(100, 70), new Vector2(380, 90), TextAnchor.UpperLeft);
        ctrl.textoInforme = Txt("Informe cientifico", ui, "INFORME CIENTÍFICO DE MISIÓN", 10, new Vector2(0, -75), new Vector2(600, 140), TextAnchor.UpperLeft);
        var b = Btn("Boton Jugar de nuevo", ui, "Jugar de nuevo", new Vector2(240, -162), new Vector2(140, 26), out _);
        Click(b, ctrl.JugarDeNuevo);
    }

    // ================================================================== Mapa

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

        // Domo: x 0..19, y 0..19. Exterior (superficie de Marte): x 20..39.
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
        Estacion(estaciones, "Puente", Estaciones.Puente, "puente", new Vector2(4.5f, 16f), flecha);
        Estacion(estaciones, "Soporte Vital", Estaciones.SoporteVital, "soporte", new Vector2(15.5f, 16f), flecha);
        Estacion(estaciones, "Invernadero", Estaciones.Invernadero, "invernadero", new Vector2(4.5f, 9.5f), flecha);
        Estacion(estaciones, "Terminal Biomédica", Estaciones.Biomedica, "biomedica", new Vector2(15.5f, 9.5f), flecha);
        Estacion(estaciones, "Camarotes", Estaciones.Camarotes, "camarotes", new Vector2(4.5f, 3.5f), flecha);
        Estacion(estaciones, "Antena Banda X", Estaciones.Antena, "antena", new Vector2(27f, 14f), flecha);
        Estacion(estaciones, "Paneles solares: 0 W de noche", "", "paneles", new Vector2(31f, 9.5f), flecha);
        Estacion(estaciones, "Zona de hielo (próximamente)", "", "camarotes", new Vector2(34f, 15f), flecha);
        Estacion(estaciones, "Sonda antigua (próximamente)", "", "puente", new Vector2(34f, 5f), flecha);

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

        // ---------- Barra superior ----------
        var ctrl = new GameObject("Mapa (controlador)").AddComponent<MapaControlador>();
        ctrl.jugador = jugador.transform;
        ctrl.luzGlobal = luz;
        Img("Barra superior", ui, new Vector2(0, 158), new Vector2(632, 42), new Color(0.07f, 0.1f, 0.18f, 0.88f));
        ctrl.textoNoche = Txt("Noche y hora", ui, "Noche 1 de 7 · 18:30", 13, new Vector2(-232, 168), new Vector2(150, 18), TextAnchor.MiddleLeft, Energia);
        string[] iconos = { "bateria", "oxigeno", "agua", "co2", "comida", "rescate", "domo" };
        var valores = new Text[iconos.Length];
        for (int i = 0; i < iconos.Length; i++)
        {
            float x = -132 + i * 66;
            Img("Icono " + iconos[i], ui, new Vector2(x, 168), new Vector2(18, 18), Color.white, Spr("Assets/Comun/UI/ui_icono_" + iconos[i] + ".png"));
            valores[i] = Txt("Valor " + iconos[i], ui, "0", 11, new Vector2(x + 33, 168), new Vector2(46, 16), TextAnchor.MiddleLeft);
        }
        ctrl.textoBateria = valores[0]; ctrl.textoOxigeno = valores[1]; ctrl.textoAgua = valores[2]; ctrl.textoCo2 = valores[3];
        ctrl.textoComida = valores[4]; ctrl.textoRescate = valores[5]; ctrl.textoDomo = valores[6];
        ctrl.textoObjetivo = Txt("Objetivo", ui, "", 9, new Vector2(-168, 148), new Vector2(280, 14), TextAnchor.MiddleLeft);
        ctrl.textosTripulacion = new Text[4];
        for (int i = 0; i < 4; i++)
            ctrl.textosTripulacion[i] = Txt("Tripulante " + (i + 1), ui, "", 9, new Vector2(60 + i * 66, 148), new Vector2(64, 14), TextAnchor.MiddleLeft, Correcto);

        ctrl.textoAviso = Txt("Aviso", ui, "", 14, new Vector2(0, -150), new Vector2(560, 22), TextAnchor.MiddleCenter, Energia);
        ctrl.textoAviso.gameObject.AddComponent<Outline>().effectColor = Color.black;

        // ---------- Pantalla de noche, amanecer y dilemas ----------
        var panel = Img("Pantalla de noche y amanecer", ui, Vector2.zero, new Vector2(640, 360), new Color(0.03f, 0.05f, 0.1f, 0.97f));
        ctrl.panelPantalla = panel.gameObject;
        ctrl.textoPantallaTitulo = Txt("Titulo", panel.transform, "Noche 1 · 18:30 MTC", 24, new Vector2(0, 128), new Vector2(580, 32), TextAnchor.MiddleCenter, Energia);
        ctrl.textoPantallaCuerpo = Txt("Cuerpo", panel.transform, "", 13, new Vector2(0, -8), new Vector2(580, 230), TextAnchor.UpperCenter);
        var bc = Btn("Boton Continuar", panel.transform, "Continuar", new Vector2(0, -155), new Vector2(160, 26), out _);
        Click(bc, ctrl.ContinuarPantalla);
        ctrl.botonContinuar = bc.gameObject;
        var ba = Btn("Opcion A", panel.transform, "Opción A", new Vector2(-152, -148), new Vector2(290, 36), out ctrl.textoOpcionA);
        ctrl.textoOpcionA.fontSize = 10;
        ClickInt(ba, ctrl.Elegir, 0);
        ctrl.botonOpcionA = ba.gameObject;
        var bb = Btn("Opcion B", panel.transform, "Opción B", new Vector2(152, -148), new Vector2(290, 36), out ctrl.textoOpcionB);
        ctrl.textoOpcionB.fontSize = 10;
        ClickInt(bb, ctrl.Elegir, 1);
        ctrl.botonOpcionB = bb.gameObject;
        panel.gameObject.SetActive(false);
    }

    // ================================================================== Estaciones

    static void CrearInvernadero()
    {
        var cam = Camara(Hex("#132A22"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Invernadero (controlador)").AddComponent<InvernaderoControlador>();
        ctrl.idEstacion = Estaciones.Invernadero;
        Llenar(Img("Fondo", ui, Vector2.zero, Vector2.zero, Color.white, Spr("Assets/Estaciones/Invernadero/Art/spr_invernadero_fondo.png")));
        Cabecera(ui, ctrl, "Invernadero hidropónico nocturno");

        string arte = "Assets/Estaciones/Invernadero/Art/";
        ctrl.marcosSeleccion = new Image[3]; ctrl.plantas = new Image[3]; ctrl.etiquetasCapsula = new Text[3]; ctrl.avisosPerclorato = new Text[3];
        for (int i = 0; i < 3; i++)
        {
            var pos = new Vector2(-250 + i * 90, 85);
            ctrl.marcosSeleccion[i] = Img("Seleccion " + (i + 1), ui, pos, new Vector2(88, 108), new Color(Energia.r, Energia.g, Energia.b, 0.55f));
            var cap = Img("Capsula " + (i + 1), ui, pos, new Vector2(80, 100), Color.white, Spr(arte + (i == 0 ? "spr_capsula_hidroponica.png" : "spr_capsula_regolito.png")));
            ClickInt(cap.gameObject.AddComponent<Button>(), ctrl.Seleccionar, i);
            ctrl.plantas[i] = Img("Planta", cap.transform, new Vector2(0, 2), new Vector2(64, 64), Color.white);
            ctrl.plantas[i].raycastTarget = false;
            ctrl.avisosPerclorato[i] = Txt("Perclorato", cap.transform, "ClO₄⁻", 10, new Vector2(0, 40), new Vector2(70, 14), TextAnchor.MiddleCenter, Alerta);
            ctrl.etiquetasCapsula[i] = Txt("Etiqueta", ui, "", 9, pos + new Vector2(0, -62), new Vector2(88, 24));
        }
        ctrl.textoCapsula = Txt("Capsula elegida", ui, "", 9, new Vector2(-160, -2), new Vector2(310, 26), TextAnchor.MiddleLeft, Energia);

        var semillas = Rect("Semillas", ui, new Vector2(-160, -34), new Vector2(310, 24));
        ctrl.grupoSemillas = semillas.gameObject;
        string[] nombres = { "Espirulina", "Papa", "Lechuga", "Arabidopsis", "Dechlorom." };
        for (int i = 0; i < 5; i++)
        {
            var b = Btn("Semilla " + nombres[i], semillas, nombres[i], new Vector2(-124 + i * 62, 0), new Vector2(58, 22), out var t);
            t.fontSize = 9;
            ClickInt(b, ctrl.Sembrar, i + 1);
        }

        var ajustes = Rect("Ajustes", ui, new Vector2(-160, -90), new Vector2(310, 70));
        ctrl.grupoAjustes = ajustes.gameObject;
        ClickInt(Btn("Humedad -", ajustes, "−", new Vector2(-120, 20), new Vector2(26, 22), out _), ctrl.CambiarHumedad, -5);
        ctrl.textoHumedad = Txt("Humedad", ajustes, "Humedad 50 %", 11, new Vector2(-60, 20), new Vector2(90, 20));
        ClickInt(Btn("Humedad +", ajustes, "+", new Vector2(0, 20), new Vector2(26, 22), out _), ctrl.CambiarHumedad, 5);
        string[] leds = { "Rojo 660 nm", "Azul 450 nm", "UV-A 380 nm" };
        ctrl.botonesLed = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var b = Btn("LED " + leds[i], ajustes, leds[i], new Vector2(-100 + i * 100, -10), new Vector2(94, 22), out var t);
            t.fontSize = 10;
            ClickInt(b, ctrl.AlternarLed, i);
            ctrl.botonesLed[i] = b.GetComponent<Image>();
        }
        var accion = Btn("Boton limpiar o arrancar", ui, "Arrancar planta", new Vector2(-90, -128), new Vector2(130, 20), out ctrl.textoAccion);
        ctrl.textoAccion.fontSize = 10;
        Click(accion, ctrl.Accion);
        ctrl.botonAccion = accion.gameObject;
        ctrl.textoAviso = Txt("Aviso", ui, "", 9, new Vector2(-160, -150), new Vector2(310, 16), TextAnchor.MiddleLeft, Energia);

        var manual = Img("Manual Astrobotanico", ui, new Vector2(162, 0), new Vector2(290, 296), Color.white, Spr(arte + "spr_manual_pagina.png"));
        ctrl.textoManual = Txt("Texto del manual", manual.transform, "", 9, new Vector2(0, 12), new Vector2(264, 262), TextAnchor.UpperLeft, Tinta);
        ctrl.textoPagina = Txt("Pagina", manual.transform, "1 / 3", 10, new Vector2(0, -134), new Vector2(60, 16), TextAnchor.MiddleCenter, Tinta);
        Click(Btn("Pagina anterior", manual.transform, "<", new Vector2(-110, -134), new Vector2(30, 20), out _), ctrl.PaginaAnterior);
        Click(Btn("Pagina siguiente", manual.transform, ">", new Vector2(110, -134), new Vector2(30, 20), out _), ctrl.PaginaSiguiente);

        var lista = new List<InvernaderoControlador.SpritesEspecie>();
        foreach (var e in Semillas.Todas)
        {
            string n = e.ToString().ToLower();
            lista.Add(new InvernaderoControlador.SpritesEspecie
            {
                especie = e,
                brote = Spr(arte + "spr_planta_" + n + "_brote.png"),
                crece = Spr(arte + "spr_planta_" + n + "_crece.png"),
                madura = Spr(arte + "spr_planta_" + n + "_madura.png"),
                estresada = Spr(arte + "spr_planta_" + n + "_estresada.png"),
            });
        }
        ctrl.spritesEspecies = lista.ToArray();
        ctrl.spritePodrida = Spr(arte + "spr_planta_podrida.png");
        ctrl.spriteCongelada = Spr(arte + "spr_planta_congelada.png");
        ctrl.spriteRetorcida = Spr(arte + "spr_planta_retorcida.png");

        BotonSalir(ui, ctrl);
        Tarjeta(ui, ctrl);
    }

    static void CrearSoporteVital()
    {
        var cam = Camara(Hex("#1A2236"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Soporte Vital (controlador)").AddComponent<SoporteVitalControlador>();
        ctrl.idEstacion = Estaciones.SoporteVital;
        Llenar(Img("Fondo", ui, Vector2.zero, Vector2.zero, Color.white, Spr("Assets/Estaciones/SoporteVital/Art/spr_soporte_fondo.png")));
        Cabecera(ui, ctrl, "Soporte Vital y termodinámica");

        Txt("Titulo MOXIE", ui, "Reactor MOXIE (2CO₂ → 2CO + O₂)", 12, new Vector2(-160, 140), new Vector2(300, 18), TextAnchor.MiddleCenter, Energia);
        ctrl.textoTiempo = Txt("Tiempo", ui, "45 s", 12, new Vector2(-30, 118), new Vector2(60, 16), TextAnchor.MiddleRight);
        var marco = Img("Medidor de temperatura", ui, new Vector2(-275, 38), new Vector2(30, 150), Oscuro);
        var relleno = Img("Relleno", marco.transform, Vector2.zero, Vector2.zero, Hex("#E8833A"));
        ctrl.rellenoTemperatura = relleno.rectTransform;
        ctrl.zonaProduccion = Img("Franja verde 780-830", marco.transform, Vector2.zero, Vector2.zero, new Color(Correcto.r, Correcto.g, Correcto.b, 0.6f)).rectTransform;
        ctrl.zonaFusion = Img("Zona roja (fusion)", marco.transform, Vector2.zero, Vector2.zero, new Color(Alerta.r, Alerta.g, Alerta.b, 0.5f)).rectTransform;
        Txt("Max", marco.transform, "900 °C", 9, new Vector2(0, 82), new Vector2(60, 12));
        Txt("Min", marco.transform, "-60 °C", 9, new Vector2(0, -82), new Vector2(60, 12));
        ctrl.textoTemperatura = Txt("Temperatura", ui, "-60 °C", 24, new Vector2(-150, 92), new Vector2(180, 30));

        Txt("Etiqueta produccion", ui, "O₂ producido esta noche", 10, new Vector2(-150, 60), new Vector2(200, 14));
        var bp = Img("Barra produccion", ui, new Vector2(-150, 46), new Vector2(180, 12), Oscuro);
        ctrl.rellenoProduccion = Img("Relleno", bp.transform, Vector2.zero, Vector2.zero, Hex("#7EC8E3")).rectTransform;
        Txt("Etiqueta hollin", ui, "Hollín en los filtros", 10, new Vector2(-150, 26), new Vector2(200, 14));
        var bh = Img("Barra hollin", ui, new Vector2(-150, 12), new Vector2(180, 12), Oscuro);
        ctrl.rellenoHollin = Img("Relleno", bh.transform, Vector2.zero, Vector2.zero, Hex("#3B3B3B")).rectTransform;
        ctrl.textoEstado = Txt("Estado", ui, "", 10, new Vector2(-150, -22), new Vector2(240, 40));

        var controles = Rect("Controles del reactor", ui, new Vector2(-140, -98), new Vector2(260, 60));
        ctrl.grupoControles = controles.gameObject;
        var calentar = Btn("Boton Calentar (mantener)", controles, "Calentar (ESPACIO)", new Vector2(-65, 12), new Vector2(124, 24), out _);
        ctrl.botonCalentar = calentar.gameObject.AddComponent<BotonMantener>();
        UnityEngine.Object.DestroyImmediate(calentar); // se mantiene presionado, no es un clic
        Click(Btn("Boton Purgar", controles, "Purgar filtro (P)", new Vector2(65, 12), new Vector2(124, 24), out _), ctrl.Purgar);
        Click(Btn("Boton Apagar", controles, "Apagar reactor", new Vector2(0, -18), new Vector2(124, 22), out _), ctrl.ApagarReactor);

        Txt("Titulo sectores", ui, "Distribución de energía", 12, new Vector2(165, 140), new Vector2(280, 18), TextAnchor.MiddleCenter, Energia);
        ctrl.botonesSector = new Image[3];
        ctrl.textosSector = new Text[3];
        for (int i = 0; i < 3; i++)
        {
            var b = Btn("Sector " + (i + 1), ui, "", new Vector2(165, 110 - i * 30), new Vector2(280, 24), out ctrl.textosSector[i]);
            ctrl.textosSector[i].fontSize = 10;
            ClickInt(b, ctrl.AlternarSector, i);
            ctrl.botonesSector[i] = b.GetComponent<Image>();
        }
        ctrl.textoCargas = Txt("Consumo", ui, "", 9, new Vector2(165, -60), new Vector2(280, 140), TextAnchor.UpperLeft);

        BotonSalir(ui, ctrl);
        Tarjeta(ui, ctrl);
    }

    static void CrearBiomedica()
    {
        var cam = Camara(Hex("#10202A"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Biomedica (controlador)").AddComponent<BiomedicaControlador>();
        ctrl.idEstacion = Estaciones.Biomedica;
        Llenar(Img("Fondo", ui, Vector2.zero, Vector2.zero, Color.white, Spr("Assets/Estaciones/Biomedica/Art/spr_biomedica_fondo.png")));
        Cabecera(ui, ctrl, "Terminal Biomédica y psicología");

        string[] ids = { "elena", "marco", "aris", "jonas" };
        ctrl.retratos = new Sprite[4]; ctrl.retratosMirada = new Sprite[4];
        ctrl.retratosTarjeta = new Image[4]; ctrl.textosTarjeta = new Text[4]; ctrl.fondosTarjeta = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            ctrl.retratos[i] = Spr("Assets/Comun/Art/Retratos/spr_retrato_" + ids[i] + ".png");
            ctrl.retratosMirada[i] = Spr("Assets/Comun/Art/Retratos/spr_retrato_" + ids[i] + "_mirada.png");
            var fondo = Img("Tarjeta " + ids[i], ui, new Vector2(-237 + i * 158, 122), new Vector2(150, 50), Correcto);
            ClickInt(fondo.gameObject.AddComponent<Button>(), ctrl.Elegir, i);
            ctrl.fondosTarjeta[i] = fondo;
            ctrl.retratosTarjeta[i] = Img("Retrato", fondo.transform, new Vector2(-48, 0), new Vector2(44, 44), Color.white, ctrl.retratos[i]);
            ctrl.retratosTarjeta[i].raycastTarget = false;
            ctrl.textosTarjeta[i] = Txt("Datos", fondo.transform, "", 9, new Vector2(24, 0), new Vector2(96, 46), TextAnchor.MiddleLeft, Oscuro);
        }

        var monitor = Img("Monitor cardiaco", ui, new Vector2(-150, 56), new Vector2(320, 72), Hex("#06140E"));
        var ecg = Rect("ECG", monitor.transform, new Vector2(0, 8), new Vector2(300, 48));
        var mc = monitor.gameObject.AddComponent<MonitorCardiaco>();
        mc.altoMaximo = 40f;
        mc.barras = new RectTransform[75];
        for (int i = 0; i < mc.barras.Length; i++)
        {
            var b = Img("Barra " + i, ecg, new Vector2(-148 + i * 4, 0), new Vector2(3, 2), Correcto);
            b.raycastTarget = false;
            mc.barras[i] = b.rectTransform;
        }
        ctrl.monitor = mc;
        ctrl.textoMonitor = Txt("Lectura", monitor.transform, "", 9, new Vector2(0, -27), new Vector2(312, 14), TextAnchor.MiddleCenter, Correcto);
        Txt("Guia", ui, "Verde: menos de 85 BPM, da ventajas.\nRojo: más de 115 BPM, sabotea de noche.\nppCO₂ sobre 5.0 mmHg: hipercapnia (es el aire, no los nervios).\nB) promete calefacción: si no cumples, +45 BPM.",
            9, new Vector2(165, 56), new Vector2(280, 72), TextAnchor.MiddleLeft);

        var caja = Img("Caja de dialogo", ui, new Vector2(0, -66), new Vector2(624, 134), new Color(0.04f, 0.06f, 0.1f, 0.95f));
        caja.gameObject.AddComponent<Outline>().effectColor = Energia;
        ctrl.retratoDialogo = Img("Retrato", caja.transform, new Vector2(-255, 12), new Vector2(96, 96), Color.white);
        ctrl.textoNombre = Txt("Nombre", caja.transform, "", 9, new Vector2(-255, -52), new Vector2(110, 14), TextAnchor.MiddleCenter, Energia);
        ctrl.textoDialogo = Txt("Texto", caja.transform, "", 11, new Vector2(55, 38), new Vector2(500, 52), TextAnchor.UpperLeft);
        ctrl.botonesOpcion = new GameObject[4];
        ctrl.textosOpcion = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            var b = Btn("Opcion " + (i + 1), caja.transform, "", new Vector2(55, -2 - i * 19), new Vector2(500, 17), out ctrl.textosOpcion[i]);
            ctrl.textosOpcion[i].fontSize = 9;
            ctrl.textosOpcion[i].alignment = TextAnchor.MiddleLeft;
            ClickInt(b, ctrl.Opcion, i);
            ctrl.botonesOpcion[i] = b.gameObject;
        }
        var otro = Btn("Boton Hablar con otro", ui, "Hablar con otro", new Vector2(110, -162), new Vector2(130, 22), out _);
        Click(otro, ctrl.HablarConOtro);
        ctrl.botonOtro = otro.gameObject;

        BotonSalir(ui, ctrl);
        Tarjeta(ui, ctrl);
    }

    static void CrearAntena()
    {
        var cam = Camara(Hex("#0B0F1A"));
        var ui = Lienzo(cam);
        SistemaEventos();
        var ctrl = new GameObject("Antena (controlador)").AddComponent<AntenaControlador>();
        ctrl.idEstacion = Estaciones.Antena;
        Llenar(Img("Fondo", ui, Vector2.zero, Vector2.zero, Color.white, Spr("Assets/Estaciones/Antena/Art/spr_antena_fondo.png")));
        Cabecera(ui, ctrl, "Comunicaciones y Antena Banda X");

        // ---------- Paso 1: elegir paquete ----------
        var paquetes = Img("Paso 1 - Elegir paquete", ui, new Vector2(0, -5), new Vector2(600, 300), new Color(0.04f, 0.06f, 0.1f, 0.95f));
        ctrl.grupoPaquetes = paquetes.gameObject;
        Txt("Explicacion", paquetes.transform, "Solo hay ancho de banda y batería para 1 paquete esta noche (25 % de batería; si se pierde la señal, 10 %).\n¿Qué envías?", 12, new Vector2(0, 110), new Vector2(560, 50));
        string[] opciones =
        {
            "SOS a Nikolas en Houston  ·  Rescate +1  ·  Elena se calma",
            "Esquema de reciclaje y mejoras del domo  ·  Domo +1 y agua +10 %  ·  Aris contenta, Elena no",
            "Datos científicos  ·  Aris contenta, Elena no",
        };
        for (int i = 0; i < 3; i++)
        {
            var b = Btn("Paquete " + (i + 1), paquetes.transform, opciones[i], new Vector2(0, 50 - i * 42), new Vector2(520, 32), out var t);
            t.fontSize = 11;
            ClickInt(b, ctrl.ElegirPaquete, i);
        }
        Click(Btn("No transmitir", paquetes.transform, "No transmitir esta noche (ahorra batería)", new Vector2(0, -95), new Vector2(300, 24), out _), ctrl.NoTransmitir);

        // ---------- Paso 2: osciloscopio ----------
        var oscilo = Rect("Paso 2 - Osciloscopio", ui, Vector2.zero, new Vector2(640, 360));
        ctrl.grupoOscilo = oscilo.gameObject;
        var pantalla = Img("Pantalla", oscilo, new Vector2(-60, 45), new Vector2(440, 150), Hex("#04140A"));
        pantalla.gameObject.AddComponent<Outline>().effectColor = Correcto;
        ctrl.tamanoOscilo = new Vector2(420f, 130f);
        ctrl.puntosObjetivo = Puntos(pantalla.transform, "Onda del satelite", 84, new Color(Correcto.r, Correcto.g, Correcto.b, 0.35f), 4);
        ctrl.puntosJugador = Puntos(pantalla.transform, "Tu onda", 84, Hex("#9BFFB8"), 3);
        ctrl.textoVacio = Txt("El vacio", pantalla.transform, "", 12, Vector2.zero, new Vector2(420, 50), TextAnchor.MiddleCenter, Texto);
        ctrl.textoVacio.gameObject.AddComponent<Outline>().effectColor = Color.black;

        var regla = Img("Regla de frecuencia", oscilo, new Vector2(-60, -42), new Vector2(420, 6), Metal);
        ctrl.anchoRegla = 420f;
        ctrl.marcaObjetivo = Img("Marca Elena (Doppler exacto)", regla.transform, Vector2.zero, new Vector2(4, 16), Energia).rectTransform;
        ctrl.marcaJugador = Img("Marca tuya", regla.transform, Vector2.zero, new Vector2(4, 12), Hex("#9BFFB8")).rectTransform;
        Txt("Etiqueta bloqueo", oscilo, "Señal bloqueada", 10, new Vector2(-60, -60), new Vector2(200, 14));
        var bl = Img("Barra bloqueo", oscilo, new Vector2(-60, -74), new Vector2(300, 10), Oscuro);
        ctrl.rellenoBloqueo = Img("Relleno", bl.transform, Vector2.zero, Vector2.zero, Correcto).rectTransform;

        ctrl.botonFrecMenos = Mantener(oscilo, "◀ Frecuencia (A)", new Vector2(-230, -105));
        ctrl.botonFrecMas = Mantener(oscilo, "Frecuencia ▶ (D)", new Vector2(-130, -105));
        ctrl.botonAmpMas = Mantener(oscilo, "▲ Amplitud (W)", new Vector2(10, -105));
        ctrl.botonAmpMenos = Mantener(oscilo, "▼ Amplitud (S)", new Vector2(110, -105));
        ctrl.textoTiempo = Txt("Tiempo", oscilo, "40 s", 14, new Vector2(240, 110), new Vector2(100, 20), TextAnchor.MiddleCenter, Energia);
        ctrl.textoEstado = Txt("Estado", oscilo, "", 10, new Vector2(240, 40), new Vector2(140, 110), TextAnchor.UpperLeft);
        Txt("Ayuda Doppler", oscilo, "El orbitador cruza el cielo: su frecuencia se corre de derecha a izquierda (efecto Doppler). Si Elena está en verde, marca el punto exacto.", 9, new Vector2(240, -40), new Vector2(140, 90), TextAnchor.UpperLeft);

        // ---------- Paso 3: mensaje de Nikolas ----------
        var mensaje = Img("Paso 3 - Mensaje de Nikolas", ui, new Vector2(0, -5), new Vector2(600, 300), new Color(0.04f, 0.06f, 0.1f, 0.95f));
        ctrl.grupoMensaje = mensaje.gameObject;
        Img("Retrato Nikolas", mensaje.transform, new Vector2(-235, 70), new Vector2(96, 96), Color.white, Spr("Assets/Comun/Art/Retratos/spr_retrato_nikolas.png"));
        ctrl.textoMensaje = Txt("Mensaje", mensaje.transform, "", 11, new Vector2(55, 40), new Vector2(460, 200), TextAnchor.UpperLeft);
        ctrl.botonesRespuesta = new GameObject[2];
        var r1 = Btn("Respuesta cordial", mensaje.transform, "Responder: «Recibido, Houston. Gracias, Nikolas.»", new Vector2(55, -95), new Vector2(460, 22), out _);
        ClickInt(r1, ctrl.Responder, 0);
        var r2 = Btn("Respuesta sarcastica", mensaje.transform, "Responder: «Tú nos mandaste a aterrizar aquí, genio.»", new Vector2(55, -122), new Vector2(460, 22), out _);
        ClickInt(r2, ctrl.Responder, 1);
        ctrl.botonesRespuesta[0] = r1.gameObject; ctrl.botonesRespuesta[1] = r2.gameObject;

        oscilo.gameObject.SetActive(false);
        mensaje.gameObject.SetActive(false);
        BotonSalir(ui, ctrl);
        Tarjeta(ui, ctrl);
    }

    // ================================================================== Piezas

    static RectTransform[] Puntos(Transform padre, string nombre, int cantidad, Color color, float tam)
    {
        var grupo = Rect(nombre, padre, Vector2.zero, Vector2.zero);
        var r = new RectTransform[cantidad];
        for (int i = 0; i < cantidad; i++)
        {
            var p = Img("p" + i, grupo, Vector2.zero, new Vector2(tam, tam), color);
            p.raycastTarget = false;
            r[i] = p.rectTransform;
        }
        return r;
    }

    static BotonMantener Mantener(Transform padre, string texto, Vector2 pos)
    {
        var b = Btn("Boton " + texto, padre, texto, pos, new Vector2(94, 24), out var t);
        t.fontSize = 10;
        var m = b.gameObject.AddComponent<BotonMantener>();
        UnityEngine.Object.DestroyImmediate(b);
        return m;
    }

    static void Cabecera(Transform ui, EstacionBase est, string titulo)
    {
        Img("Barra superior", ui, new Vector2(0, 166), new Vector2(632, 24), new Color(0.07f, 0.1f, 0.18f, 0.9f));
        Txt("Titulo", ui, titulo, 13, new Vector2(-170, 166), new Vector2(290, 20), TextAnchor.MiddleLeft, Energia);
        est.textoHora = Txt("Hora", ui, "Noche 1 · 18:30", 10, new Vector2(40, 166), new Vector2(120, 16), TextAnchor.MiddleCenter);
        est.textoBateria = Txt("Bateria", ui, "Batería", 10, new Vector2(208, 166), new Vector2(200, 16), TextAnchor.MiddleRight);
    }

    static void BotonSalir(Transform ui, EstacionBase est)
    {
        var b = Btn("Boton Salir", ui, "Salir (+2 h 30)", new Vector2(250, -162), new Vector2(120, 22), out _);
        Click(b, est.Salir);
    }

    static void Tarjeta(Transform ui, EstacionBase est)
    {
        var panel = Img("Tarjeta de dato real", ui, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.85f));
        Llenar(panel);
        var caja = Img("Caja", panel.transform, Vector2.zero, new Vector2(500, 240), Hex("#1E2A44"));
        caja.gameObject.AddComponent<Outline>().effectColor = Energia;
        est.textoResultado = Txt("Resultado", caja.transform, "", 12, new Vector2(0, 75), new Vector2(460, 60), TextAnchor.MiddleCenter, Energia);
        est.textoDato = Txt("Dato real", caja.transform, "Dato real: ...", 12, new Vector2(0, 5), new Vector2(460, 70), TextAnchor.MiddleCenter);
        est.textoFuente = Txt("Fuente", caja.transform, "Fuente: ...", 10, new Vector2(0, -45), new Vector2(460, 16), TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.7f));
        var b = Btn("Boton Continuar", caja.transform, "Volver al mapa", new Vector2(0, -88), new Vector2(150, 28), out _);
        Click(b, est.Continuar);
        est.panelTarjeta = panel.gameObject;
        panel.gameObject.SetActive(false);
    }

    static void Estacion(Transform padre, string nombre, string id, string sprite, Vector2 pos, Sprite flecha)
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
        var e = go.AddComponent<EstacionMapa>();
        e.idEstacion = id; e.nombreVisible = nombre;
        var ind = new GameObject("Indicador");
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
        t.supportRichText = true;
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
        texto = Txt("Texto", img.transform, etiqueta, 13, Vector2.zero, tam - new Vector2(6, 0));
        return b;
    }

    static void Click(Button b, UnityAction accion)
    {
        UnityEventTools.AddPersistentListener(b.onClick, accion);
    }

    static void ClickInt(Button b, UnityAction<int> accion, int valor)
    {
        UnityEventTools.AddIntPersistentListener(b.onClick, accion, valor);
    }

    static Sprite Spr(string ruta)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (s == null) Debug.LogWarning("[SieteNoches] No encontré el sprite: " + ruta);
        return s;
    }

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out var c);
        return c;
    }
}
