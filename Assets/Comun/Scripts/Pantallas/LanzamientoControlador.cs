using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Noche 7: muestra si la nave puede despegar y pasa al final.</summary>
public class LanzamientoControlador : MonoBehaviour
{
    public Text textoEstado;

    void Start()
    {
        bool nave = GameState.Nave >= 70, energia = GameState.Energia >= 30;
        if (textoEstado != null)
            textoEstado.text =
                "Nave: " + GameState.Nave + " / 70 " + (nave ? "(lista)" : "(falta reparar)") + "\n" +
                "Energía: " + GameState.Energia + " / 30 " + (energia ? "(lista)" : "(insuficiente)") + "\n\n" +
                (nave && energia ? "Todo listo. ¿Despegamos?" : "Wayra: No recomiendo despegar. Pero tú mandas.");
    }

    public void Despegar()
    {
        SceneManager.LoadScene(Escenas.Final);
    }
}
