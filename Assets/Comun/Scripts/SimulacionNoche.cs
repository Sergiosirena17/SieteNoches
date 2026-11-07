using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resuelve la noche a las 06:00 AM con todo lo que dejaste configurado en las estaciones.
/// Sin nada visual: se prueba sola con SieteNoches > Correr pruebas rapidas.
/// Orden: sabotajes, bateria (apagon), oxigeno, plantas, agua, CO2, pulso y empezar la noche siguiente.
/// </summary>
public static class SimulacionNoche
{
    // ---------- Consumo de bateria por noche, en % ----------
    public const int CostoBase = 5, CostoLed = 3, CostoCalefaccion = 10, CostoVentiladores = 6,
                     CostoMoxie = 12, CostoAntena = 25, CostoAntenaFallida = 10;

    // ---------- Oxigeno por noche, en % ----------
    public const int O2PorTripulante = 3, O2Moxie = 25, O2Espirulina = 15, O2PlantaOscura = 2,
                     O2Dechloromonas = 8, O2Podrida = 5;

    // ---------- Agua por noche, en % ----------
    public const int AguaTripulacion = 5, AguaDechloromonas = 20, AguaAris = 5;

    public struct Carga { public string Nombre; public int Costo; }

    public static bool CalefInvernaderoEfectiva => GameState.CalefInvernadero && GameState.SectorSinCalor != Estaciones.Invernadero;
    public static bool CalefCamarotesEfectiva => GameState.CalefCamarotes && GameState.SectorSinCalor != Estaciones.Camarotes;

    /// <summary>Lo que va a gastar la noche con la configuracion actual. Lo muestran el mapa y las estaciones.</summary>
    public static List<Carga> Cargas()
    {
        var l = new List<Carga> { new Carga { Nombre = "Hábitat (base)", Costo = CostoBase } };
        int leds = 0;
        foreach (var c in GameState.Capsulas) leds += c.CanalesLed;
        if (leds > 0) l.Add(new Carga { Nombre = "LEDs del invernadero (" + leds + " canales)", Costo = leds * CostoLed });
        if (CalefInvernaderoEfectiva) l.Add(new Carga { Nombre = "Calefacción del invernadero", Costo = CostoCalefaccion });
        if (CalefCamarotesEfectiva) l.Add(new Carga { Nombre = "Calefacción de los camarotes", Costo = CostoCalefaccion });
        if (GameState.Ventiladores) l.Add(new Carga { Nombre = "Ventiladores de los camarotes", Costo = CostoVentiladores });
        if (GameState.MoxieUsado) l.Add(new Carga { Nombre = "Reactor MOXIE", Costo = CostoMoxie });
        if (GameState.AntenaUsada) l.Add(new Carga { Nombre = GameState.AntenaFallo ? "Antena (intento fallido)" : "Antena (transmisión)", Costo = GameState.AntenaFallo ? CostoAntenaFallida : CostoAntena });
        return l;
    }

    public static int ConsumoProyectado()
    {
        int total = 0;
        foreach (var c in Cargas()) total += c.Costo;
        return total;
    }

    public static bool HabraApagon => ConsumoProyectado() > GameState.Bateria;

    /// <summary>Prepara la noche n: bateria, eventos y limpia lo que solo dura una noche.</summary>
    public static void EmpezarNoche(int n, int bonusBateria)
    {
        GameState.Noche = n;
        GameState.Bateria = TextosNoches.BateriaBase[Mathf.Clamp(n, 1, GameState.UltimaNoche)] + bonusBateria
                            + (n >= GameState.UltimaNoche ? GameState.Condensadores : 0);
        GameState.Visitas = 0;
        GameState.GuardiaTerminada = false;
        GameState.PantallaInicioMostrada = false;
        GameState.MoxieUsado = false;
        GameState.MoxieProduccion = 0f;
        GameState.MoxieDanado = false;
        GameState.CodigoReactor = GameState.CodigoReactorManana;
        GameState.CodigoReactorManana = false;
        GameState.HollinInicial = n == 6 ? 0.34f : 0f;
        GameState.Paquete = "";
        GameState.AntenaUsada = false;
        GameState.AntenaFallo = false;
        GameState.Charlas = 0;
        GameState.SectorSinCalor = "";
        GameState.TomaCongelada = n == 3;
        foreach (var t in GameState.Tripulacion) t.LimpiarNoche();

        if (n == 4) ContaminarCapsula();
    }

    /// <summary>Noche 4: el polvo mete percloratos en la primera capsula limpia.</summary>
    static void ContaminarCapsula()
    {
        GameState.CapsulaContaminada = -1;
        for (int i = 0; i < GameState.Capsulas.Length; i++)
        {
            var c = GameState.Capsulas[i];
            if (c.Perclorato) continue;
            c.Perclorato = true;
            if (c.Viva && c.Info.Comestible) c.Toxica = true;
            GameState.CapsulaContaminada = i;
            return;
        }
    }

    /// <summary>Resuelve la noche, avanza a la siguiente y devuelve el reporte de las 06:00.</summary>
    public static List<string> Amanecer()
    {
        var r = new List<string>();
        int noche = GameState.Noche;
        var tri = GameState.Tripulacion;
        var elena = GameState.Trip(TripId.Elena);
        var marco = GameState.Trip(TripId.Marco);
        var aris = GameState.Trip(TripId.Aris);
        var jonas = GameState.Trip(TripId.Jonas);

        bool calInv = CalefInvernaderoEfectiva;
        bool calCam = CalefCamarotesEfectiva;
        bool vent = GameState.Ventiladores;
        bool aguaCortada = false;
        int o2 = 0, agua = 0;

        // ---------- 1. Sabotajes: los tripulantes en rojo actuan a mitad de la noche ----------
        var enRojo = new List<Tripulante>();
        foreach (var t in tri) if (t.Estado == EstadoTrip.Rojo) enRojo.Add(t);
        foreach (var t in enRojo)
        {
            switch (t.Id)
            {
                case TripId.Elena:
                    calInv = false; aguaCortada = true;
                    r.Add("SABOTAJE: Elena cortó el agua y la calefacción del invernadero para darle energía al radar.");
                    break;
                case TripId.Marco:
                    o2 -= 15;
                    r.Add("SABOTAJE: Marco, delirando, giró las válvulas de oxígeno al azar (-15 % O₂).");
                    break;
                case TripId.Aris:
                    GameState.SumarRescate(-1);
                    r.Add("SABOTAJE: Aris usó la antena para enviar datos genómicos y borró las coordenadas SOS (rescate -1).");
                    break;
                case TripId.Jonas:
                    o2 -= 10;
                    r.Add("SABOTAJE: Jonas hiperventiló toda la noche e intentó abrir la esclusa (-10 % O₂).");
                    break;
            }
        }

        // ---------- 2. Bateria ----------
        int consumo = ConsumoProyectado();
        bool apagon = consumo > GameState.Bateria;
        if (apagon)
        {
            calInv = calCam = vent = false;
            GameState.NochesApagon++;
            r.Add("APAGÓN: necesitabas " + consumo + " % de batería y tenías " + GameState.Bateria + " %. A las 03:00 se apagó todo.");
        }
        else r.Add("Batería: gastaste " + consumo + " % de " + GameState.Bateria + " %.");

        // ---------- 3. Oxigeno de la tripulacion y del MOXIE ----------
        foreach (var t in tri) o2 -= (t.Id == TripId.Jonas && enRojo.Contains(t)) ? O2PorTripulante * 2 : O2PorTripulante;
        if (GameState.MoxieDanado) r.Add("MOXIE: celdas fundidas. No produjo oxígeno.");
        else if (GameState.MoxieUsado)
        {
            int prod = Mathf.RoundToInt(O2Moxie * GameState.MoxieProduccion * (apagon ? 0.5f : 1f));
            o2 += prod;
            if (prod > 0) GameState.NochesMoxie++;
            r.Add("MOXIE: +" + prod + " % de O₂.");
        }
        else r.Add("MOXIE: nadie lo encendió.");

        // ---------- 4. Plantas ----------
        int muertes = 0;
        foreach (var c in GameState.Capsulas)
        {
            if (c.Estado == EstadoPlanta.Podrida) { o2 -= O2Podrida; continue; }
            if (!c.Viva) continue;
            var info = c.Info;
            agua -= c.Humedad / 20;
            bool ledEncendido = c.CanalesLed > 0 && !apagon;

            if (!calInv && c.Humedad >= 60)
            {
                c.Estado = EstadoPlanta.Podrida; c.Congelada = true; muertes++;
                r.Add(c.Nombre + ": sin calefacción el agua se congeló y mató a la " + info.Nombre + ".");
                continue;
            }

            if (c.Especie == Especie.Dechloromonas) o2 -= O2Dechloromonas;
            else if (!ledEncendido) o2 -= O2PlantaOscura;

            bool usoOscura = false;
            bool correcta = !aguaCortada && GameState.Agua > 0 && c.ConfiguracionCorrecta(apagon, out usoOscura);
            if (correcta)
            {
                if (usoOscura) c.NochesOscuras++;
                c.ErroresSeguidos = 0;
                c.Progreso++;
                if (c.Especie == Especie.Espirulina && ledEncendido) o2 += O2Espirulina;
                if (c.Progreso >= info.Noches) agua += Cosechar(c, r);
                else c.Estado = EstadoPlanta.Creciendo;
            }
            else
            {
                c.ErroresSeguidos++;
                if (c.ErroresSeguidos >= 2)
                {
                    c.Estado = EstadoPlanta.Podrida; muertes++;
                    r.Add(c.Nombre + ": 2 noches mal configurada. Ahora es biomasa podrida (-5 % O₂ por noche hasta limpiarla).");
                }
                else
                {
                    c.Estado = EstadoPlanta.Estresada;
                    r.Add(c.Nombre + ": configuración incorrecta. La " + info.Nombre + " dejó de crecer (mira el color de sus hojas).");
                }
            }
        }
        GameState.PlantasMuertas += muertes;

        // ---------- 5. Agua ----------
        agua -= AguaTripulacion;
        if (aris.Estado == EstadoTrip.Verde) { agua += AguaAris; r.Add("Aris recolectó vapor condensado en las paredes frías (+5 % de agua)."); }

        GameState.Sumar(oxigeno: o2, agua: agua);
        r.Add("Oxígeno " + ConSigno(o2) + " % (ahora " + GameState.Oxigeno + " %). Agua " + ConSigno(agua) + " % (ahora " + GameState.Agua + " %).");

        // ---------- 6. CO2 en los camarotes ----------
        foreach (var t in tri) t.Co2 = vent ? t.Co2Base : t.Co2 + t.Co2SinVentilacion;
        if (!vent)
        {
            GameState.NochesSinVentilacion++;
            r.Add("Sin ventilación: el CO₂ de la tripulación dormida se acumuló (Marco: " + marco.Co2.ToString("0.0") + " mmHg).");
        }

        // ---------- 7. Pulso (BPM) para la noche siguiente ----------
        foreach (var t in tri)
        {
            if (t.Bpm > 70f) t.SumarBpm(-8f); // descanso
            if (t.Hipercapnia) t.SumarBpm(20f);
            if (t.Id == TripId.Marco && t.Co2 > 6f) t.SumarBpm(10f);
            if (!calCam) t.SumarBpm(t.Id == TripId.Jonas ? 20f : 5f);
            if (apagon) t.SumarBpm(t.Id == TripId.Jonas ? 20f : 10f);
            if (GameState.Agua <= 0) t.SumarBpm(15f);
            if (noche == 5 && !t.Radioprotegido) t.SumarBpm(GameState.CentinelaActiva() ? 8f : 15f);

            if (t.Promesa == Promesa.Ventilacion)
            {
                if (vent)
                {
                    t.SumarBpm(-15f);
                    elena.SumarBpm(-5f); aris.SumarBpm(-5f);
                }
                else t.SumarBpm(20f);
            }
            else if (t.Promesa == Promesa.Calefaccion && !calCam)
            {
                t.SumarBpm(45f);
                GameState.PromesasRotas++;
                r.Add("Promesa rota: le dijiste a " + t.Nombre + " que dejarías la calefacción al máximo (+45 BPM).");
            }
        }

        if (GameState.AntenaFallo) elena.SumarBpm(15f);
        if (GameState.Paquete == "sos") elena.SumarBpm(-10f);
        else if (GameState.Paquete != "") { elena.SumarBpm(10f); aris.SumarBpm(-10f); }

        marco.NochesSinPapa = marco.ComioPapa ? 0 : marco.NochesSinPapa + 1;
        if (marco.NochesSinPapa >= 2) marco.SumarBpm(20f);

        if (muertes > 0) aris.SumarBpm(20f);
        if (!calInv) aris.SumarBpm(15f);

        if (noche == 5) { elena.SumarBpm(10f); aris.SumarBpm(10f); }

        bool otroEnRojo = false;
        foreach (var t in tri) if (t.Id != TripId.Jonas && t.Estado == EstadoTrip.Rojo) otroEnRojo = true;
        if (otroEnRojo) jonas.SumarBpm(15f);

        foreach (var t in tri)
            if (t.Estado == EstadoTrip.Rojo) r.Add(t.Nombre + " está en ROJO (" + Mathf.RoundToInt(t.Bpm) + " BPM): mañana puede sabotear algo.");

        // ---------- 8. Cierre ----------
        if (GameState.MoxieDanado && noche == 6) GameState.NocheMoxieDanado = 6;
        if (GameState.PronosticoTau && noche + 1 < GameState.UltimaNoche)
            r.Add("Pronóstico de Houston para la Noche " + (noche + 1) + ": " + TextosNoches.Telemetria[noche + 1]);
        GameState.PronosticoTau = false;

        int bonus = jonas.Estado == EstadoTrip.Verde ? 10 : 0;
        if (bonus > 0) r.Add("Jonas revisó los condensadores antes de dormir: +10 % de batería mañana.");

        GameState.UltimoReporte.Clear();
        GameState.UltimoReporte.AddRange(r);
        EmpezarNoche(noche + 1, bonus);
        return r;
    }

    /// <summary>La planta completo su ciclo. Devuelve el agua que aporta.</summary>
    static int Cosechar(Capsula c, List<string> r)
    {
        var info = c.Info;
        if (info.Continua)
        {
            if (c.Estado != EstadoPlanta.Madura)
                r.Add(c.Nombre + ": la " + info.Nombre + " ya está madura. " + info.Efecto);
            c.Estado = EstadoPlanta.Madura;
            return 0;
        }

        int agua = 0;
        switch (c.Especie)
        {
            case Especie.Dechloromonas:
                agua = AguaDechloromonas;
                GameState.BiorremediacionHecha = true;
                if (c.Perclorato && !c.Descontaminada) { GameState.SumarDomo(1); c.Descontaminada = true; }
                c.Perclorato = false;
                r.Add(c.Nombre + ": la Dechloromonas destruyó el perclorato. Suelo limpio y +20 % de agua.");
                break;
            case Especie.Papa:
            case Especie.Lechuga:
                if (c.Toxica)
                {
                    GameState.CosechasToxicas++;
                    r.Add(c.Nombre + ": la cosecha tiene percloratos. Hay que tirarla.");
                    break;
                }
                GameState.Cosechas++;
                if (c.Especie == Especie.Papa) { GameState.Raciones += 3; r.Add(c.Nombre + ": ¡cosecha! +3 raciones de papa."); }
                else { GameState.Lechugas += 1; r.Add(c.Nombre + ": ¡cosecha! +1 dosis de lechuga roja."); }
                if (c.Regolito && !GameState.CosechaLimpia)
                {
                    GameState.CosechaLimpia = true;
                    GameState.SumarDomo(1);
                    r.Add("Primera comida cultivada en suelo marciano descontaminado (domo +1).");
                }
                break;
        }
        c.Vaciar();
        return agua;
    }

    static string ConSigno(int v) => v > 0 ? "+" + v : v.ToString();
}
