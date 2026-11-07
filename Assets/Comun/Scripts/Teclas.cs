using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lectura de teclado y raton en un solo lugar (el proyecto usa el Input System nuevo).
/// Si quieren cambiar un control, se cambia solo aqui.
/// </summary>
public static class Teclas
{
    public static Vector2 Movimiento()
    {
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float x = 0f, y = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
        if (k.sKey.isPressed || k.downArrowKey.isPressed) y -= 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed) y += 1f;
        var v = new Vector2(x, y);
        return v.sqrMagnitude > 1f ? v.normalized : v;
    }

    /// <summary>E o Espacio: entrar a una mision.</summary>
    public static bool Interactuar()
    {
        var k = Keyboard.current;
        return k != null && (k.eKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame);
    }

    /// <summary>Espacio, Enter o clic: avanzar texto.</summary>
    public static bool Avanzar()
    {
        var k = Keyboard.current;
        var m = Mouse.current;
        bool teclado = k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
        bool raton = m != null && m.leftButton.wasPressedThisFrame;
        return teclado || raton;
    }

    public static bool Mantiene(Key tecla)
    {
        var k = Keyboard.current;
        return k != null && k[tecla].isPressed;
    }

    public static bool Pulsa(Key tecla)
    {
        var k = Keyboard.current;
        return k != null && k[tecla].wasPressedThisFrame;
    }
}
