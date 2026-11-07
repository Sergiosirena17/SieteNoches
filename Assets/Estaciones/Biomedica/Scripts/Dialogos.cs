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
        if (MicrofonoAbierto(t)) return "...mamá, apaga la luz que no puedo respirar...";
        if (t.Id == TripId.Jonas && t.EnCrisis && noche >= 4)
            return "Oye... ¿quién es el quinto tripulante que está parado detrás de ti en el invernadero?";
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
        "Calculé la ventana tres veces y da lo mismo las tres. Eso es bueno, creo. ¿Papas para el desayuno otra vez?",
        "El catalizador ronronea como un gato. Un gato de 800 grados, pero gato.",
        "Las hojas transpiran y el vapor se pega a las paredes frías. Lo junto. Agua gratis, cortesía de la física.",
        "Revisé los condensadores. Dos veces. Bueno, cinco. Me relaja contar cosas.",
    };

    static readonly string[] Nervioso =
    {
        "No duermo. El sol de aquí dura 39 minutos más y mi cabeza no se entera. Sigo mirando el radar.",
        "Mi módulo es un armario. El aire ahí abajo no se mueve. ¿Hay papas? Pienso mejor con papas.",
        "Todavía siento el frío del traje EVA. 15 milisieverts por salvar unas semillas. Valió la pena... ¿no?",
        "¿Oíste eso? El metal cruje cuando se enfría. Lo sé, lo sé. Pero suena como pasos.",
    };

    static readonly string[] Crisis =
    {
        "Si fallamos la alineación, la ventana se cierra. Veintiséis meses. ¿Entiendes? Veintiséis.",
        "No puedo pensar. Tengo hambre, tengo frío y el MOXIE suena raro. Todo suena raro.",
        "Se mueren. Las plantas se mueren y nadie hace nada. Quizás no deberíamos volver. Quizás este es el lugar.",
        "¡Está crujiendo! ¡El casco se encoge! Tenemos que salir, tenemos que abrir la esclusa...",
    };

    static readonly string[] Hipercapnia =
    {
        "Me duele detrás de los ojos... los números del radar se mueven solos. Alguien tocó mis cálculos.",
        "Tengo un martilleo detrás de los ojos y las letras me bailan... Juro que Elena está bajando el oxígeno a propósito.",
        "Huelo a cloro... ¿o es el perclorato? Está en todo. En el aire. En mí.",
        "El aire sabe a moneda... me da vueltas todo. 0.38 g y aun así me caigo.",
    };

    public static string TextoOpcion(Tripulante t, int opcion)
    {
        if (MicrofonoAbierto(t)) return opcion == OpcionDiagnostico ? "A) ¡Aguanta, Marco! Corro a encender tu ventilador." : "";
        switch (opcion)
        {
            case OpcionDiagnostico: return "A) Diagnóstico: es el aire. Tienes CO₂ acumulado (hipercapnia). Enciendo tu ventilación.";
            case OpcionPromesa: return "B) Son solo nervios. Esta noche dejo la calefacción de los camarotes al máximo.";
            case OpcionPapa: return "C) Darle una ración de papa (" + GameState.Raciones + ")";
            default: return "C) Darle lechuga roja (" + GameState.Lechugas + ")";
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
            return "(Solo se oye su respiración ahogada.)  ¡Corre a Soporte Vital y enciende los ventiladores!";
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
                    return "...el aire. Claro. Por eso me late la cabeza. Gracias por mirar el monitor. (Cumple: ventiladores encendidos.)";
                }
                if (t.Id == TripId.Jonas)
                {
                    t.SumarBpm(15f);
                    return "¿Entonces también falla el aire? ¡Todo falla! (El sensor de CO₂ marcaba normal.)";
                }
                t.SumarBpm(10f);
                return "¿El aire? El sensor marca normal. No me estás escuchando.";
            case OpcionPromesa:
                t.SumarBpm(-20f);
                t.Promesa = Promesa.Calefaccion;
                return "Está bien... confío en ti. (Si al amanecer la calefacción está apagada, lo sabrá.)";
            case OpcionPapa:
                if (GameState.Raciones <= 0) return "";
                GameState.Raciones--;
                t.SumarBpm(-20f);
                t.ComioPapa = true;
                return "Tibia, salada y con potasio. Ya siento las manos.";
            default:
                if (GameState.Lechugas <= 0) return "";
                GameState.Lechugas--;
                t.SumarBpm(-40f);
                t.Radioprotegido = true;
                return "Amarga. Roja. Me baja el pulso... y dicen que las antocianinas protegen de la radiación.";
        }
    }
}
