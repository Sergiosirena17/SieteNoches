using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Pantalla de titulo: el boton Jugar reinicia la partida y pasa a la intro.</summary>
public class InicioControlador : MonoBehaviour
{
    void Start()
    {
        AmbienteAudio.PrepararInicio();
    }

    public void Jugar()
    {
        GameState.Reiniciar();
        AmbienteAudio.NarrarInicio();
        SceneManager.LoadScene(Escenas.Intro);
    }
}
