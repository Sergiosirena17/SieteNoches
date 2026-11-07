using System;
using UnityEngine;

public enum TripId { Elena, Marco, Aris, Jonas }
public enum EstadoTrip { Verde, Amarillo, Rojo }

/// <summary>Lo que el jugador le prometio en la Terminal Biomédica esta noche.</summary>
public enum Promesa { Ninguna, Ventilacion, Calefaccion }

/// <summary>Un tripulante con su pulso (BPM) y el CO₂ alrededor de su cabeza mientras duerme (ppCO2 en mmHg).</summary>
[Serializable]
public class Tripulante
{
    public const float BpmMin = 55f, BpmMax = 140f;
    public const float LimiteVerde = 85f, LimiteRojo = 115f;
    public const float LimiteHipercapnia = 5f;

    public TripId Id;
    public string Nombre;
    public string Rol;
    public float Bpm;
    public float Co2;
    public Promesa Promesa;
    public int NochesSinPapa;
    public bool ComioPapa;
    public bool Radioprotegido;
    public bool HabloEstaNoche;

    public Tripulante(TripId id, string nombre, string rol, float bpm, float co2)
    {
        Id = id; Nombre = nombre; Rol = rol; Bpm = bpm; Co2 = co2;
    }

    /// <summary>Marco duerme en el modulo inferior, donde el aire se estanca.</summary>
    public float Co2Base => Id == TripId.Marco ? 2.5f : 2.0f;
    public float Co2SinVentilacion => Id == TripId.Marco ? 4.0f : 2.5f;

    public EstadoTrip Estado => Bpm < LimiteVerde ? EstadoTrip.Verde : Bpm <= LimiteRojo ? EstadoTrip.Amarillo : EstadoTrip.Rojo;
    public bool Hipercapnia => Co2 > LimiteHipercapnia;
    /// <summary>Modo crisis del dialogo: sin voz, letras lentas y mirada fija.</summary>
    public bool EnCrisis => Co2 > 5.5f || Bpm > LimiteRojo;

    public void SumarBpm(float delta)
    {
        Bpm = Mathf.Clamp(Bpm + delta, BpmMin, BpmMax);
    }

    public void LimpiarNoche()
    {
        Promesa = Promesa.Ninguna;
        Radioprotegido = false;
        HabloEstaNoche = false;
        ComioPapa = false;
    }
}
