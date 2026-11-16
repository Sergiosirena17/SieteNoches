using System.Collections.Generic;
using UnityEngine;

/// <summary>Chooses the ending at dawn on Night 7.</summary>
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
        if (GameState.ProgresoRescate < 3) return Hipoxia;
        if (GameState.Bateria < 20 || GameState.NocheMoxieDanado == 6) return Sacrificio;
        if (GameState.NikolasDiscusiones >= 3 && GameState.NikolasCordiales == 0) return Nikolas;
        if (GameState.ProgresoRescate == 4 && GameState.Oxigeno > 60 && rojos == 0 && GameState.CosechaLimpia) return Heroe;
        if (GameState.ProgresoRescate == 4 && rojos >= 2) return CajaNegra;
        return AMedias;
    }

    public static readonly Dictionary<string, Final> Finales = new Dictionary<string, Final>
    {
        { Heroe, new Final { Titulo = "Heroic Return", Tipo = "Best ending", Texto = "You launch in the precise orbital window with the whole crew healthy and the biological samples alive." } },
        { RaicesRojas, new Final { Titulo = "Red Roots", Tipo = "Secret / Good", Texto = "You ignored Houston's rescue plan, but closed the water, O₂, and food cycle without perchlorates. When the sun rises, the crew accepts they no longer need to return: they found the first permanent colony on Mars." } },
        { Nikolas, new Final { Titulo = "Nikolas Stays Behind", Tipo = "Secret / Comedy", Texto = "During ascent, Mission Control realizes—12 minutes late—that Nikolas had been asleep in the auxiliary module you left on the ground." } },
        { Sacrificio, new Final { Titulo = "Sacrifice Orbit", Tipo = "Dramatic", Texto = "There is only enough energy and life support for the capsule to carry your crewmates up. You stay below in the dark, operating the valves and antenna by hand as you watch their trail cross the Martian sky." } },
        { CajaNegra, new Final { Titulo = "Black Box", Tipo = "Mixed / Dark", Texto = "The rescue ship collects you at dawn and the data is saved, but hunger, radiation, and paranoia have broken the crew forever." } },
        { Motin, new Final { Titulo = "Mutiny in the Dome", Tipo = "Bad (social)", Texto = "In the middle of the night, panic and hypercapnia make the crew take the base and lock you out of command to ration the air." } },
        { Hipoxia, new Final { Titulo = "Drift or Hypoxia Failure", Tipo = "Bad (technical)", Texto = "The -125 °C cold or accumulating CO₂ shuts down the habitat systems before sunrise." } },
        { AMedias, new Final { Titulo = "A Partial Return", Tipo = "Good", Texto = "You launch within the window. Not everything went right, but everyone returns. Houston asks what happened during those seven nights; you have the report." } },
    };

    public static string MotivoHipoxia()
    {
        if (GameState.Oxigeno <= 0) return "Oxygen reached 0%.";
        if (GameState.TiempoAgotado) return "The 60 seconds ran out before you reached the Bridge.";
        if (GameState.DecisionFinal == "sellar") return "You sealed the dome before it was self-sufficient.";
        return "You launched without enough SOS packages: Houston could not calculate the window (you need rescue progress 3 of 4).";
    }

    public static List<string> InformeCientifico()
    {
        var l = new List<string>();
        l.Add("MOXIE produced oxygen on " + GameState.NochesMoxie + " of 6 nights: it splits CO₂ (95% of Martian air) at 800 °C.");
        if (GameState.NochesApagon > 0) l.Add(GameState.NochesApagon + " blackout(s): at night, solar panels provide 0 W, so only the battery matters.");
        if (GameState.NochesSinVentilacion > 0) l.Add(GameState.NochesSinVentilacion + " night(s) without ventilation: at 0.38 g there is almost no convection, and exhaled CO₂ forms bubbles. Above 5 mmHg it causes hypercapnia.");
        l.Add("Harvests: " + GameState.Cosechas + ". Dead plants: " + GameState.PlantasMuertas + ". In low gravity, excess water drowns roots.");
        if (GameState.BiorremediacionHecha) l.Add("You removed perchlorates with Dechloromonas: regolith contains 0.5% to 1% ClO₄⁻, which is toxic to the thyroid.");
        else if (GameState.CosechasToxicas > 0) l.Add(GameState.CosechasToxicas + " toxic harvest(s): you grew food in perchlorate soil without cleaning it.");
        l.Add("SOS packages received in Houston: " + GameState.SosEnviados + ". Each message took about 12 minutes to cross space.");
        l.Add("Broken promises: " + GameState.PromesasRotas + ". Crew members in red at the end: " + GameState.ContarEnRojo() + ".");
        l.Add("Rescue " + GameState.ProgresoRescate + "/4 · Dome " + GameState.AutosuficienciaDomo + "/4 · O₂ " + GameState.Oxigeno + "% · Battery " + GameState.Bateria + "%.");
        return l;
    }
}
