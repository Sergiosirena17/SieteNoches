using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Boton que se puede MANTENER presionado (raton o dedo).</summary>
public class BotonMantener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool Presionado { get; private set; }
    public void OnPointerDown(PointerEventData e) { Presionado = true; }
    public void OnPointerUp(PointerEventData e) { Presionado = false; }
    public void OnPointerExit(PointerEventData e) { Presionado = false; }
}
