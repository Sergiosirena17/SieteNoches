using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Pruebas automaticas de reglas (sin abrir escenas). Menu: SieteSoles > Correr pruebas rapidas.
/// Revisan el GameState, los 9 finales y que los minijuegos se puedan ganar y perder.
/// Correr SIEMPRE antes de subir cambios a GameState, Efectos, EvaluadorFinal o la logica de un minijuego.
/// </summary>
public static class PruebasRapidas
{
    static int fallas;
    static readonly List<string> informe = new List<string>();

    [MenuItem("SieteSoles/Correr pruebas rapidas")]
    public static void Correr()
    {
        Ejecutar();
        EditorUtility.DisplayDialog("Pruebas rápidas", fallas == 0 ? "Todo bien (" + informe.Count + " pruebas)." : fallas + " pruebas fallaron. Mira la Consola.", "OK");
    }

    /// <summary>Para linea de comandos: termina Unity con codigo 1 si algo falla.</summary>
    public static void CorrerDesdeConsola()
    {
        Ejecutar();
        EditorApplication.Exit(fallas == 0 ? 0 : 1);
    }

    static void Ejecutar()
    {
        fallas = 0;
        informe.Clear();
        ProbarEstado();
        ProbarFinales();
        ProbarRespira();
        ProbarTormenta();
        foreach (var linea in informe) Debug.Log("[Pruebas] " + linea);
        Debug.Log("[Pruebas] RESUMEN: " + (informe.Count - fallas) + " bien, " + fallas + " mal.");
        GameState.Reiniciar();
    }

    static void Esperar(bool condicion, string nombre)
    {
        informe.Add((condicion ? "OK    " : "FALLA ") + nombre);
        if (!condicion) { fallas++; Debug.LogError("[Pruebas] FALLA " + nombre); }
    }

    static void ProbarEstado()
    {
        GameState.Reiniciar();
        Esperar(GameState.Oxigeno == 70 && GameState.Nave == 20 && GameState.Sol == 1, "P01 valores iniciales");
        Esperar(GameState.MisionesDisponibles().Count == 3, "Sol 1 tiene 3 misiones");

        GameState.RegistrarMision(Misiones.Respira, "prueba");
        Esperar(!GameState.MisionesDisponibles().Contains(Misiones.Respira), "Una mision no se repite el mismo sol");
        Esperar(!GameState.DebeLlegarLaNoche(), "Con 1 mision todavia es de dia");
        GameState.RegistrarMision(Misiones.Tormenta, "prueba");
        Esperar(GameState.DebeLlegarLaNoche(), "P03 con 2 misiones llega la noche");

        GameState.Reiniciar();
        for (int i = 0; i < 5; i++) GameState.PasarNoche();
        Esperar(GameState.Oxigeno == 0, "P04 sin misiones de oxigeno, el oxigeno llega a 0 en la noche 5");
        Esperar(EvaluadorFinal.Calcular() == "SinAire", "P04 eso da el final Sin aire");

        GameState.Reiniciar();
        GameState.Moral = 0;
        Esperar(GameState.MisionesPermitidas() == 1, "P05 con moral 0 solo 1 mision");

        GameState.Reiniciar();
        GameState.Sol = 7;
        Esperar(GameState.MisionesDisponibles().Count == 0 && !GameState.DebeLlegarLaNoche(), "P06 el sol 7 no tiene misiones ni noche");

        GameState.Reiniciar();
        GameState.Sol = 2;
        Efectos.DecidirRoca(DecisionRoca.Dejar);
        Esperar(!GameState.MisionesDisponibles().Contains(Misiones.Roca), "Decidida la roca, ya no aparece");

        GameState.Reiniciar();
        Efectos.Aplicar(Misiones.Respira, Resultado.Gano);
        Esperar(GameState.Oxigeno == 100 && GameState.Nave == 30, "Ganar Respira: oxigeno +30 (tope 100) y nave +10");
        Efectos.Aplicar(Misiones.Tormenta, Resultado.Perdio);
        Esperar(GameState.Energia == 60, "Perder Tormenta: energia -10");

        GameState.Reiniciar();
        GameState.Marcas.Add("x");
        GameState.Reiniciar();
        Esperar(GameState.Marcas.Count == 0 && GameState.Historial.Count == 0, "P08 Reiniciar borra marcas e historial");
    }

    static void Preparar(int nave, int energia, int oxigeno = 70, int comida = 70, int moral = 70)
    {
        GameState.Reiniciar();
        GameState.Nave = nave; GameState.Energia = energia; GameState.Oxigeno = oxigeno;
        GameState.Comida = comida; GameState.Moral = moral;
    }

    static void ProbarFinales()
    {
        Preparar(70, 30, oxigeno: 0); Esperar(EvaluadorFinal.Calcular() == "SinAire", "Final 1 Sin aire");
        Preparar(70, 30); GameState.NikolasGanadas = 3; Esperar(EvaluadorFinal.Calcular() == "UltimaPalabra", "Final 2 La ultima palabra");
        Preparar(70, 30); GameState.Marcas.Add("roca_dejada"); Esperar(EvaluadorFinal.Calcular() == "PrimerContacto", "Final 3 Primer contacto");
        Preparar(70, 30, moral: 40); GameState.Marcas.Add("roca_dejada"); Esperar(EvaluadorFinal.Calcular() == "VueltaJusta", "Primer contacto exige moral 50");
        Preparar(70, 30); GameState.Marcas.Add("roca_sin_analizar"); Esperar(EvaluadorFinal.Calcular() == "Pasajero", "Final 4 Pasajero");
        Preparar(70, 30); GameState.Marcas.Add("roca_analizada"); Esperar(EvaluadorFinal.Calcular() == "RegresoHeroe", "Final 5 Regreso heroe");
        Preparar(70, 30); Esperar(EvaluadorFinal.Calcular() == "VueltaJusta", "Final 6 Vuelta justa");
        Preparar(70, 29); Esperar(EvaluadorFinal.Calcular() == "DerivaSilenciosa", "Final 7 Deriva silenciosa");
        Preparar(69, 30, oxigeno: 30, comida: 30); Esperar(EvaluadorFinal.Calcular() == "ColonosALaFuerza", "Final 8 Colonos a la fuerza");
        Preparar(69, 30, oxigeno: 29); Esperar(EvaluadorFinal.Calcular() == "FalloEnLaRampa", "Final 9 Fallo en la rampa");
        foreach (var id in new[] { "SinAire", "UltimaPalabra", "PrimerContacto", "Pasajero", "RegresoHeroe", "VueltaJusta", "DerivaSilenciosa", "ColonosALaFuerza", "FalloEnLaRampa" })
            Esperar(EvaluadorFinal.Finales.ContainsKey(id), "El final " + id + " tiene texto");
    }

    // Jugadores simulados ("bots") para comprobar que el minijuego no es imposible ni regalado.
    static Resultado JugarRespira(string estilo, int semilla, out float segundos)
    {
        var l = new RespiraLogica(0, semilla);
        float dt = 0.05f, ultimoBombeo = -1f;
        while (!l.Terminado)
        {
            bool calentar = false;
            if (estilo == "bueno")
            {
                calentar = l.Temperatura < (l.ZonaTMin + l.ZonaTMax) / 2f;
                if (l.Presion < (l.ZonaPMin + l.ZonaPMax) / 2f && l.Tiempo - ultimoBombeo > 0.3f) { l.Bombear(); ultimoBombeo = l.Tiempo; }
            }
            else if (estilo == "siempre calienta") calentar = true;
            l.Avanzar(dt, calentar);
        }
        segundos = l.Tiempo;
        return l.ResultadoFinal;
    }

    static void ProbarRespira()
    {
        for (int s = 1; s <= 5; s++)
        {
            var r = JugarRespira("bueno", s, out float t);
            Esperar(r == Resultado.Gano, "Respira: un jugador atento gana (semilla " + s + ", " + t.ToString("0") + " s)");
        }
        Esperar(JugarRespira("quieto", 1, out _) == Resultado.Perdio, "Respira: sin hacer nada se pierde");
        var l = new RespiraLogica(0, 1);
        var quemada = JugarRespira("siempre calienta", 1, out float tq);
        Esperar(quemada == Resultado.Perdio && tq < 10f, "Respira: calentar sin parar quema la maquina (" + tq.ToString("0.0") + " s)");
        var dificil = new RespiraLogica(3, 1);
        Esperar(dificil.ZonaTMax - dificil.ZonaTMin < l.ZonaTMax - l.ZonaTMin, "Respira: la zona se achica al repetir la mision");
    }

    static Resultado JugarTormenta(float clicsPorSegundo, int semilla, out float segundos)
    {
        var l = new TormentaLogica(0, semilla);
        float dt = 0.05f, acumulado = 0f;
        while (!l.Terminado)
        {
            acumulado += clicsPorSegundo * dt;
            while (acumulado >= 1f) { l.Limpiar(l.PanelMasSucio()); acumulado -= 1f; }
            l.Avanzar(dt);
        }
        segundos = l.Tiempo;
        return l.ResultadoFinal;
    }

    static void ProbarTormenta()
    {
        for (int s = 1; s <= 5; s++)
        {
            var r = JugarTormenta(3f, s, out float t);
            Esperar(r == Resultado.Gano, "Tormenta: 3 clics por segundo gana (semilla " + s + ", " + t.ToString("0") + " s)");
        }
        Esperar(JugarTormenta(0f, 1, out _) == Resultado.Perdio, "Tormenta: sin limpiar se pierde");
        var lento = JugarTormenta(1f, 1, out _);
        Esperar(lento != Resultado.Gano, "Tormenta: 1 clic por segundo no alcanza para ganar (sale " + lento + ")");
    }
}
