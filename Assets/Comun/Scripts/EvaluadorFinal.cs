using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Elige el final al amanecer de la Noche 7 (tabla "Finales conectados").
/// Se revisan de arriba hacia abajo y gana la primera condicion que se cumpla.
/// </summary>
public static class EvaluadorFinal
{
    public struct Final { public string Titulo; public string Tipo; public string Texto; }

    public const string Hipoxia = "Hipoxia", Motin = "Motin", RaicesRojas = "RaicesRojas", Sacrificio = "OrbitaSacrificio",
                        Nikolas = "NikolasSeQueda", Heroe = "RegresoHeroe", CajaNegra = "CajaNegra", AMedias = "RegresoAMedias";

    public static int Crisis120()
    {
        int n = 0;
        foreach (var t in GameState.Tripulacion) if (t.Bpm > 120f) n++;
        return n;
    }

    public static string Calcular()
    {
        if (GameState.Oxigeno <= 0 || GameState.TiempoAgotado) return Hipoxia;
        if (Crisis120() >= 3 || GameState.PromesasRotas >= 3) return Motin;

        int rojos = GameState.ContarEnRojo();
        if (GameState.DecisionFinal == "sellar")
        {
            bool ok = GameState.ProgresoRescate < 2 && GameState.AutosuficienciaDomo >= 4 && GameState.InvernaderoAlCien()
                      && GameState.Oxigeno > 80 && GameState.Trip(TripId.Aris).Estado == EstadoTrip.Verde;
            return ok ? RaicesRojas : Hipoxia;
        }

        // Despegar
        if (GameState.ProgresoRescate < 3) return Hipoxia; // sin coordenadas: fuera de la ventana
        if (GameState.Bateria < 20 || GameState.NocheMoxieDanado == 6) return Sacrificio;
        if (GameState.NikolasDiscusiones >= 3 && GameState.NikolasCordiales == 0) return Nikolas;
        if (GameState.ProgresoRescate == 4 && GameState.Oxigeno > 60 && rojos == 0 && GameState.CosechaLimpia) return Heroe;
        if (GameState.ProgresoRescate == 4 && rojos >= 2) return CajaNegra;
        return AMedias;
    }

    public static readonly Dictionary<string, Final> Finales = new Dictionary<string, Final>
    {
        { Heroe, new Final { Titulo = "Regreso héroe", Tipo = "El mejor",
            Texto = "Despegas en la ventana orbital exacta con toda la tripulación sana y las muestras biológicas vivas." } },
        { RaicesRojas, new Final { Titulo = "Raíces Rojas", Tipo = "Secreto / Bueno",
            Texto = "Ignoraste el rescate de Houston, pero cerraste el ciclo de agua, O₂ y comida sin percloratos. Cuando sale el sol, la tripulación acepta que ya no necesita volver: fundan la primera colonia permanente en Marte." } },
        { Nikolas, new Final { Titulo = "Nikolas se queda", Tipo = "Secreto / Comedia",
            Texto = "En pleno ascenso, Control de Misión se da cuenta, con 12 minutos de retraso, de que Nikolas estaba durmiendo en el módulo auxiliar que dejaste en tierra." } },
        { Sacrificio, new Final { Titulo = "Órbita de sacrificio", Tipo = "Dramático",
            Texto = "Solo hay energía y soporte vital para que la cápsula suba a tus compañeros. Te quedas abajo, en la oscuridad, operando a mano las válvulas y la antena mientras ves su estela cruzar el cielo marciano." } },
        { CajaNegra, new Final { Titulo = "Caja negra", Tipo = "Mixto / Oscuro",
            Texto = "La nave de rescate los recoge al amanecer y los datos se salvan, pero el hambre, la radiación y la paranoia rompieron para siempre a la tripulación." } },
        { Motin, new Final { Titulo = "Motín en el domo", Tipo = "Malo (social)",
            Texto = "En plena madrugada, el pánico y la hipercapnia hacen que la tripulación tome la base y te deje fuera de la cabina de mando para racionar el aire." } },
        { Hipoxia, new Final { Titulo = "Deriva o fallo por hipoxia", Tipo = "Malo (técnico)",
            Texto = "El frío de -125 °C o la acumulación de CO₂ apagan los sistemas del hábitat antes de que salga el sol." } },
        { AMedias, new Final { Titulo = "Regreso a medias", Tipo = "Bueno (propuesta, no está en el documento)",
            Texto = "Despegas dentro de la ventana. No todo salió bien, pero vuelven todos. Houston pregunta qué pasó esas siete noches; tú tienes el informe." } },
    };

    /// <summary>Motivo concreto del final malo tecnico, para no dejar al jugador adivinando.</summary>
    public static string MotivoHipoxia()
    {
        if (GameState.Oxigeno <= 0) return "El oxígeno llegó a 0 %.";
        if (GameState.TiempoAgotado) return "Se acabaron los 60 segundos antes de llegar al Puente.";
        if (GameState.DecisionFinal == "sellar") return "Sellaste el domo sin que fuera autosuficiente.";
        return "Despegaste sin suficientes paquetes SOS: Houston no pudo calcular la ventana (necesitas rescate 3 de 4).";
    }

    /// <summary>Informe Científico de Misión: explica con ciencia real que decisiones llevaron al final.</summary>
    public static List<string> InformeCientifico()
    {
        var l = new List<string>();
        l.Add("MOXIE produjo oxígeno en " + GameState.NochesMoxie + " de 6 noches: rompe el CO₂ (95 % del aire marciano) a 800 °C.");
        if (GameState.NochesApagon > 0)
            l.Add(GameState.NochesApagon + " apagón(es): de noche los paneles solares dan 0 W y solo cuenta la batería.");
        if (GameState.NochesSinVentilacion > 0)
            l.Add(GameState.NochesSinVentilacion + " noche(s) sin ventilación: en 0.38 g casi no hay convección y el CO₂ exhalado forma burbujas. Sobre 5 mmHg causa hipercapnia.");
        l.Add("Cosechas: " + GameState.Cosechas + ". Plantas muertas: " + GameState.PlantasMuertas + ". En baja gravedad el exceso de agua ahoga las raíces.");
        if (GameState.BiorremediacionHecha)
            l.Add("Limpiaste percloratos con Dechloromonas: el regolito tiene de 0.5 % a 1 % de ClO₄⁻, tóxico para la tiroides.");
        else if (GameState.CosechasToxicas > 0)
            l.Add(GameState.CosechasToxicas + " cosecha(s) tóxica(s): sembraste comida en suelo con percloratos sin limpiarlo.");
        l.Add("Paquetes SOS que llegaron a Houston: " + GameState.SosEnviados + ". Cada mensaje tardó unos 12 minutos en cruzar el espacio.");
        l.Add("Promesas rotas: " + GameState.PromesasRotas + ". Tripulantes en rojo al final: " + GameState.ContarEnRojo() + ".");
        l.Add("Rescate " + GameState.ProgresoRescate + "/4 · Domo " + GameState.AutosuficienciaDomo + "/4 · O₂ " + GameState.Oxigeno + " % · Batería " + GameState.Bateria + " %.");
        return l;
    }
}
