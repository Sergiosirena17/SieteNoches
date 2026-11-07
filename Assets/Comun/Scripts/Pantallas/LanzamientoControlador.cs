using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Noche 7, 05:59 AM en el Puente: despegar o sellar el domo. Luego el final.</summary>
public class LanzamientoControlador : MonoBehaviour
{
    public Text textoEstado;

    void Start()
    {
        if (textoEstado == null) return;
        var sb = new StringBuilder();
        sb.Append("Rescate: ").Append(GameState.ProgresoRescate).Append("/4 (Houston necesita 3 para calcular la ventana)\n");
        sb.Append("Autosuficiencia del domo: ").Append(GameState.AutosuficienciaDomo).Append("/4\n");
        sb.Append("O₂: ").Append(GameState.Oxigeno).Append(" %   Batería de despegue: ").Append(GameState.Bateria).Append(" %\n\n");
        foreach (var t in GameState.Tripulacion)
            sb.Append(t.Nombre).Append(": ").Append(Mathf.RoundToInt(t.Bpm)).Append(" BPM (").Append(t.Estado.ToString().ToLower()).Append(")\n");
        textoEstado.text = sb.ToString();
    }

    public void Despegar()
    {
        GameState.DecisionFinal = "despegar";
        SceneManager.LoadScene(Escenas.Final);
    }

    public void SellarDomo()
    {
        GameState.DecisionFinal = "sellar";
        SceneManager.LoadScene(Escenas.Final);
    }
}
