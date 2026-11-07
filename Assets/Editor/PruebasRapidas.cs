using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Pruebas automaticas de reglas (sin abrir escenas). Menu: SieteNoches > Correr pruebas rapidas.
/// Revisan la noche (bateria, O2, plantas, CO2, pulso), los eventos, los 8 finales, el MOXIE, la antena,
/// los dialogos y dos partidas completas simuladas (un jugador cuidadoso y uno que no hace nada).
/// Correr SIEMPRE antes de subir cambios a GameState, SimulacionNoche, EvaluadorFinal o la logica de una estacion.
/// </summary>
public static class PruebasRapidas
{
    static int fallas;
    static readonly List<string> informe = new List<string>();

    [MenuItem("SieteNoches/Correr pruebas rapidas")]
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
        ProbarBateria();
        ProbarPlantas();
        ProbarTripulacion();
        ProbarEventos();
        ProbarFinales();
        ProbarMoxie();
        ProbarAntena();
        ProbarPartidas();
        foreach (var linea in informe) Debug.Log("[Pruebas] " + linea);
        Debug.Log("[Pruebas] RESUMEN: " + (informe.Count - fallas) + " bien, " + fallas + " mal.");
        GameState.Reiniciar();
    }

    static void Esperar(bool condicion, string nombre)
    {
        informe.Add((condicion ? "OK    " : "FALLA ") + nombre);
        if (!condicion) { fallas++; Debug.LogError("[Pruebas] FALLA " + nombre); }
    }

    static Capsula Cap(int i) => GameState.Capsulas[i];

    /// <summary>Siembra y deja la capsula como dice el manual (o mal, si correcto es false).</summary>
    static void Sembrar(int i, Especie e, bool correcto = true)
    {
        var c = Cap(i);
        c.Vaciar();
        c.Sembrar(e);
        var s = Semillas.Info[e];
        c.Humedad = correcto ? s.Humedad : (s.Humedad > 50 ? 20 : 90);
        c.LedRojo = s.Rojo; c.LedAzul = s.Azul; c.LedUv = s.Uv;
    }

    /// <summary>Deja a la tripulacion tranquila para que no haya sabotajes en la prueba.</summary>
    static void Calmar()
    {
        foreach (var t in GameState.Tripulacion) t.Bpm = 70f;
    }

    // ------------------------------------------------------------------ Estado

    static void ProbarEstado()
    {
        GameState.Reiniciar();
        Esperar(GameState.Noche == 1 && GameState.Bateria == 70 && GameState.Oxigeno == 70 && GameState.Agua == 60, "Valores iniciales: Noche 1, batería 70, O₂ 70, agua 60");
        Esperar(GameState.Capsulas.Length == 3 && !Cap(0).Regolito && Cap(1).Perclorato && Cap(2).Perclorato, "3 cápsulas: 1 hidropónica y 2 de regolito con percloratos");
        Esperar(GameState.Tripulacion.Length == 4, "4 tripulantes");
        Esperar(GameState.Hora == "18:30", "La noche empieza a las 18:30");
        GameState.Visitas = 4;
        Esperar(GameState.Hora == "04:30" && GameState.DebeAmanecer(), "Tras 4 visitas son las 04:30 y amanece");
        GameState.Visitas = 0;
        Esperar(!GameState.EstacionDisponible(Estaciones.Antena), "La antena no funciona en la Noche 1");
        Esperar(SimulacionNoche.ConsumoProyectado() == 5 + 10 + 10 + 6, "Consumo base: hábitat + 2 calefacciones + ventiladores = 31 %");
        GameState.Charlas = 2;
        Esperar(!GameState.EstacionDisponible(Estaciones.Biomedica), "Solo 2 charlas por noche");
    }

    static void ProbarBateria()
    {
        GameState.Reiniciar();
        Calmar();
        GameState.Bateria = 20;
        SimulacionNoche.Amanecer();
        Esperar(GameState.NochesApagon == 1, "Si el consumo supera la batería hay apagón");
        Esperar(GameState.Trip(TripId.Marco).Co2 > 6f, "En el apagón se apagan los ventiladores y sube el CO₂ de Marco");

        GameState.Reiniciar();
        Calmar();
        GameState.Trip(TripId.Jonas).Bpm = 60f;
        SimulacionNoche.Amanecer();
        Esperar(GameState.Noche == 2 && GameState.Bateria == 65 + 10, "Jonas en verde da +10 % de batería a la noche siguiente");
    }

    // ------------------------------------------------------------------ Plantas

    static void ProbarPlantas()
    {
        GameState.Reiniciar();
        Calmar();
        Sembrar(0, Especie.Espirulina);
        SimulacionNoche.Amanecer();
        Esperar(GameState.Oxigeno == 70 - 12 + 15, "Espirulina bien cuidada: +15 % de O₂ (y la tripulación gasta 12 %)");
        Esperar(Cap(0).Estado == EstadoPlanta.Madura, "La espirulina queda madura y sigue produciendo");

        GameState.Reiniciar();
        Calmar();
        Sembrar(0, Especie.Papa);
        for (int i = 0; i < 3; i++) { Calmar(); SimulacionNoche.Amanecer(); }
        Esperar(GameState.Raciones == 3 && Cap(0).Vacia, "Papa: 3 noches bien cuidada dan 3 raciones");

        GameState.Reiniciar();
        Sembrar(0, Especie.Papa);
        Calmar(); SimulacionNoche.Amanecer();
        Cap(0).LedRojo = false;
        Calmar(); SimulacionNoche.Amanecer();
        Esperar(Cap(0).Estado == EstadoPlanta.Creciendo, "La papa aguanta 1 noche a oscuras");
        Calmar(); SimulacionNoche.Amanecer();
        Esperar(Cap(0).Estado == EstadoPlanta.Estresada, "Pero no 2 noches a oscuras");

        GameState.Reiniciar();
        Sembrar(0, Especie.Lechuga, false);
        Calmar(); SimulacionNoche.Amanecer();
        Esperar(Cap(0).Estado == EstadoPlanta.Estresada, "1 noche mal configurada: deja de crecer");
        Calmar(); SimulacionNoche.Amanecer();
        Esperar(Cap(0).Estado == EstadoPlanta.Podrida, "2 noches seguidas: biomasa podrida");
        int o2 = GameState.Oxigeno;
        Calmar(); SimulacionNoche.Amanecer();
        Esperar(GameState.Oxigeno == o2 - 12 - 5, "La biomasa podrida resta 5 % de O₂ por noche");

        GameState.Reiniciar();
        Calmar();
        Sembrar(0, Especie.Espirulina);
        GameState.CalefInvernadero = false;
        SimulacionNoche.Amanecer();
        Esperar(Cap(0).Estado == EstadoPlanta.Podrida && Cap(0).Congelada, "Humedad 90 % sin calefacción: la planta se congela");

        GameState.Reiniciar();
        Sembrar(1, Especie.Papa);
        Esperar(Cap(1).Toxica, "Sembrar comida en regolito con percloratos la vuelve tóxica");
        for (int i = 0; i < 3; i++) { Calmar(); SimulacionNoche.Amanecer(); }
        Esperar(GameState.Raciones == 0 && GameState.CosechasToxicas == 1, "La cosecha tóxica se descarta");

        GameState.Reiniciar();
        Calmar();
        Sembrar(1, Especie.Dechloromonas);
        int agua = GameState.Agua;
        SimulacionNoche.Amanecer();
        Esperar(!Cap(1).Perclorato && GameState.BiorremediacionHecha && GameState.AutosuficienciaDomo == 1, "Dechloromonas limpia el perclorato y suma domo +1");
        Esperar(GameState.Agua == agua - 5 - 3 + 20 + 5, "Dechloromonas devuelve +20 % de agua (y Aris en verde +5 %)");
        Sembrar(1, Especie.Papa);
        for (int i = 0; i < 3; i++) { Calmar(); SimulacionNoche.Amanecer(); }
        Esperar(GameState.CosechaLimpia && GameState.Raciones == 3 && GameState.AutosuficienciaDomo == 2, "Papa en suelo descontaminado: cosecha limpia y domo +1");
    }

    // ------------------------------------------------------------------ Tripulacion y dialogos

    static void ProbarTripulacion()
    {
        GameState.Reiniciar();
        Calmar();
        GameState.Ventiladores = false;
        SimulacionNoche.Amanecer();
        var marco = GameState.Trip(TripId.Marco);
        Esperar(Mathf.Approximately(marco.Co2, 6.5f) && marco.Hipercapnia, "Sin ventiladores Marco llega a 6.5 mmHg (hipercapnia)");
        Esperar(marco.Bpm > 70f, "La hipercapnia le sube el pulso");
        Esperar(Dialogos.MicrofonoAbierto(marco), "Con más de 6.5 mmHg se activa el micrófono de las 3:00 AM");

        GameState.Reiniciar();
        Calmar();
        var elena = GameState.Trip(TripId.Elena);
        string r = Dialogos.Responder(elena, Dialogos.OpcionDiagnostico);
        Esperar(elena.Bpm == 80f && r.Contains("No me estás escuchando"), "Diagnóstico ambiental equivocado: +10 BPM");
        marco = GameState.Trip(TripId.Marco);
        marco.Co2 = 6f;
        Dialogos.Responder(marco, Dialogos.OpcionDiagnostico);
        Esperar(marco.Bpm == 55f && marco.Promesa == Promesa.Ventilacion, "Diagnóstico correcto (hipercapnia): -15 BPM y compromiso de ventilar");

        GameState.Reiniciar();
        Calmar();
        var jonas = GameState.Trip(TripId.Jonas);
        Dialogos.Responder(jonas, Dialogos.OpcionPromesa);
        GameState.CalefCamarotes = false;
        SimulacionNoche.Amanecer();
        Esperar(GameState.PromesasRotas == 1 && jonas.Bpm > 100f, "Promesa de calefacción rota: +45 BPM y cuenta como promesa rota");

        GameState.Reiniciar();
        Calmar();
        GameState.Raciones = 1;
        Esperar(Dialogos.OpcionDisponible(GameState.Trip(TripId.Aris), Dialogos.OpcionPapa), "Con raciones se puede dar papa");
        Dialogos.Responder(GameState.Trip(TripId.Aris), Dialogos.OpcionPapa);
        Esperar(GameState.Raciones == 0 && GameState.Trip(TripId.Aris).Bpm == 55f, "Dar papa: -20 BPM y gasta 1 ración");

        GameState.Reiniciar();
        Calmar();
        GameState.Trip(TripId.Marco).Bpm = 130f;
        SimulacionNoche.Amanecer();
        Esperar(GameState.Oxigeno == 70 - 12 - 15, "Marco en rojo sabotea las válvulas: -15 % de O₂");
    }

    // ------------------------------------------------------------------ Eventos

    static void ProbarEventos()
    {
        GameState.Reiniciar();
        SimulacionNoche.EmpezarNoche(3, 0);
        Esperar(GameState.TomaCongelada, "Noche 3: hielo seco en las tomas del MOXIE");
        SimulacionNoche.EmpezarNoche(4, 0);
        Esperar(GameState.Bateria == 45 && Cap(0).Perclorato && GameState.CapsulaContaminada == 0, "Noche 4: 45 % de batería y percloratos en la cápsula hidropónica");
        SimulacionNoche.EmpezarNoche(6, 0);
        Esperar(Mathf.Approximately(GameState.HollinInicial, 0.34f), "Noche 6: el MOXIE empieza con 1/3 de hollín");
        GameState.Condensadores = 30;
        SimulacionNoche.EmpezarNoche(7, 0);
        Esperar(GameState.Bateria == 40, "Noche 7: 10 % de reserva + 30 % de condensadores");
    }

    // ------------------------------------------------------------------ Finales

    static void BaseFinal()
    {
        GameState.Reiniciar();
        SimulacionNoche.EmpezarNoche(7, 20);
        Calmar();
        GameState.Oxigeno = 70;
        GameState.DecisionFinal = "despegar";
    }

    static void ProbarFinales()
    {
        BaseFinal(); GameState.Oxigeno = 0;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Hipoxia, "Final: sin O₂ → hipoxia");
        BaseFinal(); GameState.TiempoAgotado = true;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Hipoxia, "Final: se acabaron los 60 s → hipoxia");
        BaseFinal(); for (int i = 0; i < 3; i++) GameState.Tripulacion[i].Bpm = 125f;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Motin, "Final: 3 tripulantes sobre 120 BPM → motín");
        BaseFinal(); GameState.PromesasRotas = 3;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Motin, "Final: 3 promesas rotas → motín");

        BaseFinal();
        GameState.DecisionFinal = "sellar"; GameState.AutosuficienciaDomo = 4; GameState.Oxigeno = 90;
        Sembrar(0, Especie.Espirulina); Sembrar(1, Especie.Arabidopsis); Sembrar(2, Especie.Dechloromonas);
        Cap(1).Perclorato = false; Cap(2).Perclorato = false; Cap(1).Toxica = false;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.RaicesRojas, "Final: domo 4/4, invernadero al 100 % y O₂ > 80 → Raíces Rojas");
        GameState.Oxigeno = 70;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Hipoxia, "Final: sellar el domo sin ser autosuficiente → hipoxia");

        BaseFinal(); GameState.ProgresoRescate = 2;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Hipoxia, "Final: despegar con rescate 2/4 → fuera de la ventana");
        BaseFinal(); GameState.ProgresoRescate = 3; GameState.Bateria = 10;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Sacrificio, "Final: rescate 3 y batería < 20 % → órbita de sacrificio");
        BaseFinal(); GameState.ProgresoRescate = 4; GameState.NocheMoxieDanado = 6;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Sacrificio, "Final: MOXIE dañado en la Noche 6 → órbita de sacrificio");
        BaseFinal(); GameState.ProgresoRescate = 4; GameState.NikolasDiscusiones = 3;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Nikolas, "Final: solo respuestas sarcásticas a Nikolas → Nikolas se queda");
        BaseFinal(); GameState.ProgresoRescate = 4; GameState.CosechaLimpia = true;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Heroe, "Final: rescate 4, O₂ > 60, nadie en rojo y cosecha limpia → Regreso héroe");
        BaseFinal(); GameState.ProgresoRescate = 4; GameState.Tripulacion[0].Bpm = 118f; GameState.Tripulacion[1].Bpm = 118f;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.CajaNegra, "Final: rescate 4 con 2 tripulantes en rojo → Caja negra");
        BaseFinal(); GameState.ProgresoRescate = 3;
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.AMedias, "Final: despegue correcto sin otra condición → Regreso a medias");
        Esperar(EvaluadorFinal.Finales.Count == 8 && EvaluadorFinal.InformeCientifico().Count >= 5, "Hay 8 finales y el informe científico tiene contenido");
    }

    // ------------------------------------------------------------------ Estaciones

    static void ProbarMoxie()
    {
        var m = new MoxieLogica(1, false, false, false, 0f, 1);
        for (int i = 0; i < 900 && !m.Terminado; i++) m.Avanzar(0.05f, m.Temperatura < 800f);
        Esperar(m.Produccion >= 1f && !m.Fundido, "MOXIE: manteniendo 780-830 °C se llena la producción");
        Esperar(m.Tiempo < MoxieLogica.Duracion, "MOXIE: se llena antes de 45 s (" + m.Tiempo.ToString("0") + " s)");

        m = new MoxieLogica(1, false, false, false, 0f, 2);
        for (int i = 0; i < 900 && !m.Terminado; i++) m.Avanzar(0.05f, true);
        Esperar(m.Fundido, "MOXIE: calentar sin parar funde las celdas");

        m = new MoxieLogica(6, false, false, false, 0.34f, 3);
        for (int i = 0; i < 900 && !m.Terminado; i++) m.Avanzar(0.05f, m.Temperatura < 800f);
        Esperar(m.Produccion < 1f, "MOXIE Noche 6: sin purgar, el hollín tapa los filtros");

        m = new MoxieLogica(6, false, false, false, 0.34f, 4);
        for (int i = 0; i < 900 && !m.Terminado; i++)
        {
            if (m.Tapado) m.Purgar();
            m.Avanzar(0.05f, m.Temperatura < 800f);
        }
        Esperar(m.Produccion >= 1f, "MOXIE Noche 6: purgando a tiempo se llena");
    }

    static void ProbarAntena()
    {
        var a = new AntenaLogica(false, 5);
        for (int i = 0; i < 900 && !a.Terminado; i++)
        {
            float df = a.FrecuenciaObjetivo - a.Frecuencia, da = a.AmplitudObjetivo - a.Amplitud;
            a.Avanzar(0.05f, Mathf.Abs(df) > 0.01f ? (int)Mathf.Sign(df) : 0, Mathf.Abs(da) > 0.02f ? (int)Mathf.Sign(da) : 0);
        }
        Esperar(a.Exito, "Antena: siguiendo el Doppler se bloquea la señal (" + a.Tiempo.ToString("0") + " s)");

        a = new AntenaLogica(false, 6);
        for (int i = 0; i < 900 && !a.Terminado; i++) a.Avanzar(0.05f, 0, -1);
        Esperar(a.Terminado && !a.Exito, "Antena: sin alinear se pierde el orbitador a los 40 s");
    }

    // ------------------------------------------------------------------ Partidas simuladas

    /// <summary>Un jugador cuidadoso: MOXIE, plantas segun el manual, SOS y calma a la tripulacion.</summary>
    static string JugarBien()
    {
        GameState.Reiniciar();
        Sembrar(0, Especie.Espirulina);
        Sembrar(1, Especie.Dechloromonas);
        Sembrar(2, Especie.Dechloromonas);
        for (int noche = 1; noche <= 6; noche++)
        {
            GameState.CalefInvernadero = true; GameState.CalefCamarotes = true; GameState.Ventiladores = true;
            if (noche == 2) { Sembrar(1, Especie.Papa); Sembrar(2, Especie.Lechuga); }
            if (noche == 3) GameState.SectorSinCalor = Estaciones.Camarotes; // dilema: enfriar camarotes
            if (noche == 6) // dilema: sacrificar el invernadero
            {
                GameState.InvernaderoSacrificado = true; GameState.Condensadores = 30;
                foreach (var c in GameState.Capsulas) { c.Vaciar(); c.LedRojo = c.LedAzul = c.LedUv = false; }
                GameState.CalefInvernadero = false;
            }
            // Invernadero: apagar la luz de las capsulas vacias
            foreach (var c in GameState.Capsulas) if (c.Vacia) c.LedRojo = c.LedAzul = c.LedUv = false;
            // Antena: SOS cuando hay
            if (noche >= 2 && noche != 4)
            {
                GameState.AntenaUsada = true; GameState.Paquete = "sos"; GameState.SumarRescate(1); GameState.SosEnviados++;
            }
            // MOXIE al 100 %; si la bateria no alcanza, recortar en este orden
            GameState.MoxieUsado = true; GameState.MoxieProduccion = 1f;
            if (SimulacionNoche.HabraApagon) foreach (var c in GameState.Capsulas) if (c.Especie == Especie.Papa && c.NochesOscuras == 0) c.LedRojo = false;
            if (SimulacionNoche.HabraApagon) GameState.CalefCamarotes = false;
            if (SimulacionNoche.HabraApagon) { GameState.MoxieUsado = false; GameState.MoxieProduccion = 0f; }
            if (SimulacionNoche.HabraApagon) GameState.Ventiladores = false;
            // Biomedica: calmar a los dos mas nerviosos (la promesa solo si la calefaccion queda encendida)
            for (int k = 0; k < 2; k++)
            {
                Tripulante peor = null;
                foreach (var t in GameState.Tripulacion) if (!t.HabloEstaNoche && (peor == null || t.Bpm > peor.Bpm)) peor = t;
                peor.HabloEstaNoche = true;
                int op = peor.Hipercapnia ? Dialogos.OpcionDiagnostico : GameState.Lechugas > 0 ? Dialogos.OpcionLechuga
                       : GameState.Raciones > 0 ? Dialogos.OpcionPapa : SimulacionNoche.CalefCamarotesEfectiva ? Dialogos.OpcionPromesa : -1;
                if (op >= 0) Dialogos.Responder(peor, op);
            }
            SimulacionNoche.Amanecer();
            foreach (var c in GameState.Capsulas) if (c.Especie == Especie.Papa) c.LedRojo = true;
            if (GameState.Oxigeno <= 0) break;
        }
        GameState.ValvulasCerradas = true;
        GameState.DecisionFinal = "despegar";
        return EvaluadorFinal.Calcular();
    }

    static void ProbarPartidas()
    {
        string f = JugarBien();
        Debug.Log("[Pruebas] Partida cuidadosa: final " + f + " · O₂ " + GameState.Oxigeno + " · agua " + GameState.Agua + " · rescate " + GameState.ProgresoRescate +
                  " · batería N7 " + GameState.Bateria + " · rojos " + GameState.ContarEnRojo() + " · cosecha limpia " + GameState.CosechaLimpia);
        Esperar(f == EvaluadorFinal.Heroe, "Partida cuidadosa completa: se puede llegar a Regreso héroe (salió " + f + ")");

        GameState.Reiniciar();
        for (int noche = 1; noche <= 6; noche++) { Calmar(); SimulacionNoche.Amanecer(); }
        GameState.DecisionFinal = "despegar";
        Esperar(EvaluadorFinal.Calcular() == EvaluadorFinal.Hipoxia, "Partida sin hacer nada: termina en hipoxia (O₂ " + GameState.Oxigeno + " %)");
    }
}
