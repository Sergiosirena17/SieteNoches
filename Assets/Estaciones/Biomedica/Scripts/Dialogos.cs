/// <summary>
/// Arbol de dialogos de la Terminal Biomédica (formula de 3 nodos):
/// Nodo 1 saludo con pista del problema, Nodo 2 tres opciones (A diagnostico, B promesa, C inventario),
/// Nodo 3 resolucion. Sin nada visual: se prueba solo.
/// </summary>
public static class Dialogos
{
    public const int OpcionDiagnostico = 0, OpcionPromesa = 1, OpcionPapa = 2, OpcionLechuga = 3;

    /// <summary>Voz de cada personaje (Hz del blip). En crisis no suena.</summary>
    public static float Voz(TripId id)
    {
        switch (id)
        {
            case TripId.Elena: return 520f;
            case TripId.Marco: return 330f;
            case TripId.Aris: return 620f;
            default: return 440f;
        }
    }

    /// <summary>Microfono abierto de las 3:00 AM: Marco sin ventilacion, CO2 altisimo y pulso bajo.</summary>
    public static bool MicrofonoAbierto(Tripulante t) => t.Id == TripId.Marco && t.Co2 >= 6.5f;

    public static string Saludo(Tripulante t, int noche)
    {
        if (MicrofonoAbierto(t)) return "...Mom, turn off the light, I cannot breathe...";
        if (t.Id == TripId.Jonas && t.EnCrisis && noche >= 4)
        return "Hey... who is the fifth crew member standing behind you in the greenhouse?";
        if (t.Hipercapnia) return Hipercapnia[(int)t.Id];
        switch (t.Estado)
        {
            case EstadoTrip.Rojo: return Crisis[(int)t.Id];
            case EstadoTrip.Amarillo: return Nervioso[(int)t.Id];
            default: return Calma[(int)t.Id];
        }
    }

    static readonly string[] Calma =
    {
        "I calculated the window three times and got the same answer each time. That is good, I think. Potatoes for breakfast again?",
        "The catalyst purrs like a cat. An 800-degree cat, but still a cat.",
        "Leaves transpire and vapor sticks to the cold walls. I collect it. Free water, courtesy of physics.",
        "I checked the capacitors. Twice. Okay, five times. Counting things relaxes me.",
    };

    static readonly string[] Nervioso =
    {
        "I cannot sleep. The day here is 39 minutes longer and my brain does not get it. I keep staring at the radar.",
        "My module is a closet. The air down there does not move. Any potatoes? I think better with potatoes.",
        "I can still feel the cold from the EVA suit. Fifteen millisieverts to save some seeds. It was worth it... right?",
        "Did you hear that? Metal creaks when it cools. I know, I know. But it sounds like footsteps.",
    };

    static readonly string[] Crisis =
    {
        "If we miss alignment, the window closes. Twenty-six months. Do you understand? Twenty-six.",
        "I cannot think. I am hungry, I am cold, and MOXIE sounds strange. Everything sounds strange.",
        "They are dying. The plants are dying and no one is doing anything. Maybe we should not go back. Maybe this is the place.",
        "It is creaking! The hull is shrinking! We have to get out, we have to open the airlock...",
    };

    static readonly string[] Hipercapnia =
    {
        "It hurts behind my eyes... the radar numbers move on their own. Someone changed my calculations.",
        "There is pounding behind my eyes and the letters dance... I swear Elena is lowering the oxygen on purpose.",
        "I smell chlorine... or is it perchlorate? It is everywhere. In the air. In me.",
        "The air tastes like coins... everything is spinning. 0.38 g and I still fall.",
    };

    public static string TextoOpcion(Tripulante t, int opcion)
    {
        if (MicrofonoAbierto(t)) return opcion == OpcionDiagnostico ? "A) Hang on, Marco! I am going to turn on your fan." : "";
        switch (opcion)
        {
        case OpcionDiagnostico: return "A) Diagnosis: it is the air. You have CO₂ buildup (hypercapnia). I will turn on your ventilation.";
        case OpcionPromesa: return "B) It is just nerves. Tonight I will leave the crew-quarters heating at maximum.";
        case OpcionPapa: return "C) Give a potato ration (" + GameState.Raciones + ")";
        default: return "C) Give red lettuce (" + GameState.Lechugas + ")";
        }
    }

    public static bool OpcionDisponible(Tripulante t, int opcion)
    {
        if (MicrofonoAbierto(t)) return opcion == OpcionDiagnostico;
        if (opcion == OpcionPapa) return GameState.Raciones > 0;
        if (opcion == OpcionLechuga) return GameState.Lechugas > 0;
        return true;
    }

    /// <summary>Aplica la opcion elegida (Nodo 3) y devuelve lo que responde el tripulante.</summary>
    public static string Responder(Tripulante t, int opcion)
    {
        if (MicrofonoAbierto(t))
        {
            t.Promesa = Promesa.Ventilacion;
        return "(You can only hear their labored breathing.) Run to Life Support and turn on the fans!";
        }
        switch (opcion)
        {
            case OpcionDiagnostico:
                if (t.Hipercapnia)
                {
                    t.SumarBpm(-15f);
                    t.Promesa = Promesa.Ventilacion;
                    GameState.Trip(TripId.Elena).SumarBpm(-5f);
                    GameState.Trip(TripId.Aris).SumarBpm(-5f);
        return "...the air. Of course. That is why my head is pounding. Thanks for checking the monitor. (Requirement: fans on.)";
                }
                if (t.Id == TripId.Jonas)
                {
                    t.SumarBpm(15f);
        return "So the air is failing too? Everything is failing! (The CO₂ sensor read normal.)";
                }
                t.SumarBpm(10f);
        return "The air? The sensor reads normal. You are not listening to me.";
            case OpcionPromesa:
                t.SumarBpm(-20f);
                t.Promesa = Promesa.Calefaccion;
        return "All right... I trust you. (If the heating is off at dawn, they will know.)";
            case OpcionPapa:
                if (GameState.Raciones <= 0) return "";
                GameState.Raciones--;
                t.SumarBpm(-20f);
                t.ComioPapa = true;
        return "Warm, salty, and full of potassium. I can feel my hands again.";
            default:
                if (GameState.Lechugas <= 0) return "";
                GameState.Lechugas--;
                t.SumarBpm(-40f);
                t.Radioprotegido = true;
        return "Bitter. Red. It lowers my pulse... and they say anthocyanins protect against radiation.";
        }
    }
}
