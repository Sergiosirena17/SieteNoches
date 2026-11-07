# 7 Noches en Marte

Juego 2D en Unity para el NASA Space Apps Challenge 2026. Aterrizamos de emergencia en Marte y hay 7 noches para reparar la nave y volver, cada noche juegas 2 misiones y a las 6:00 AM se gastan los recursos. Las reglas y los números están en el GDD del equipo.

## Abrir el proyecto

1. Instalar **Unity 6000.6.8f1** (exactamente esa versión) con Web Build Support y Windows Build Support.
2. Clonar este repositorio con GitHub Desktop.
3. En Unity Hub: **Add > Add project from disk** y elegir la carpeta del repositorio.
4. Abrir `Assets/Escenas/Inicio.unity` y presionar **Play**.

¿Nunca usaste Unity? Lee primero [Docs/GuiaUnity.md](Docs/GuiaUnity.md): explica cada ventana y cómo probar el juego paso a paso.

Cada minijuego también se puede probar solo: abrir su escena (por ejemplo `Assets/Minijuegos/Respira/Mj_Respira.unity`) y presionar Play.

## Ramas: una por programador

| Rama | Quién | Qué toca |
| --- | --- | --- |
| `main` | Nadie trabaja aquí directo | Versión que funciona. Solo recibe cambios por Pull Request. |
| `sergio` | Sergio | `Assets/Minijuegos/Respira`, `Assets/Minijuegos/Tormenta`, `Assets/Intro` |
| `nicolas` | Nicolás | `Assets/Minijuegos/Huerto`, `Assets/Minijuegos/Refugio` |
| `jean-paul` | Jean-Paul | `Assets/Minijuegos/Insulta`, `Assets/Minijuegos/Roca` |

El reparto de minijuegos es una propuesta: cámbienlo aquí si deciden otro. La regla importante es que **cada carpeta tiene un solo dueño**. Si algo se rompe, sabemos en qué rama y carpeta buscar.

`Assets/Comun` (GameState, mapa, jugador) y `Assets/Escenas` son compartidos. Antes de cambiarlos, avisen al grupo.

### Rutina diaria (GitHub Desktop)

1. **Current branch:** tu rama (`sergio`, `nicolas` o `jean-paul`).
2. **Branch > Update from main** para traer lo último que se unió.
3. Trabaja en Unity, guarda con Ctrl+S y cierra Play antes de editar.
4. Revisa que solo cambiaste archivos de tus carpetas, **junto con sus `.meta`**.
5. **Commit** con un mensaje claro y **Push origin**.
6. Cuando tu minijuego funcione: **Create Pull Request** hacia `main`.

## Carpetas

```text
Assets/
  Comun/
    Scripts/      GameState, Datos (efectos, textos), EvaluadorFinal, Teclas
      Jugador/    movimiento, animación por hoja de sprites, cámara
      Mapa/       estaciones y controlador del mapa
      Minijuegos/ MinijuegoBase (contrato que cumplen todos)
      Pantallas/  inicio, intro, lanzamiento, final
    Art/          bloques, personaje, estaciones, retratos
    UI/           iconos de recursos y flecha de misión
  Escenas/        Inicio, Intro, Mapa, Lanzamiento, Final
  Intro/Vinetas/  las 8 imágenes de la cinemática
  Minijuegos/<Nombre>/  escena Mj_<Nombre>, Scripts/, Art/
  Editor/         herramientas (menú SieteNoches)
Docs/
  Arte/           guía de la hoja de sprites del personaje
  Capturas/       imagen de cada escena, generada por el menú SieteNoches
```

## Para artistas: cómo reemplazar los dibujos

Todo el arte actual es **boceto**. Para cambiarlo, guarden su PNG **con el mismo nombre y la misma medida** encima del archivo viejo. Unity lo configura solo: 32 píxeles por unidad, Point y sin compresión. No hace falta tocar nada en Unity.

| Archivo | Medida | Qué es |
| --- | --- | --- |
| `Comun/Art/Personajes/spr_jugador_hoja.png` | 128 × 192 | Hoja del jugador: 4 × 4 celdas de 32 × 48 (ver abajo) |
| `Comun/Art/Bloques/tile_*.png` | 32 × 32 | Piso y pared de la nave, suelo y roca de Marte, puerta |
| `Comun/Art/Estaciones/spr_estacion_*.png` | 64 × 64 | Máquinas donde se entra a cada misión |
| `Comun/Art/Retratos/spr_retrato_*.png` | 96 × 96 | Comandante, Nikolas y Wayra |
| `Comun/UI/ui_icono_*.png` | 32 × 32 | Iconos de los 6 recursos |
| `Intro/Vinetas/spr_intro_01..08.png` | 640 × 240 | Viñetas de la cinemática |
| `Minijuegos/Respira/Art/spr_respira_fondo.png` | 640 × 360 | Fondo de Respira |
| `Minijuegos/Tormenta/Art/spr_tormenta_*.png` | 640 × 360, 96 × 64 | Fondo, panel solar y capa de polvo |

### Hoja de sprites del jugador

![Guía de la hoja](Docs/Arte/guia_hoja_personaje.png)

- Cada celda mide **32 × 48 px**. La hoja completa mide 128 × 192.
- **Columnas:** frame 0 es quieto; los frames 1, 2 y 3 forman la caminata.
- **Filas, de arriba hacia abajo:** 0 abajo, 1 izquierda, 2 derecha, 3 arriba.
- Los pies van en el borde inferior de la celda, que es el punto de apoyo.
- Los números rojos del boceto solo sirven de guía; no los copien.

Inspiración: tomamos la idea de juegos como Undertale (intro con viñetas, movimiento con vista superior) y Among Us (mapa con salas), pero **no se usa ningún sprite, música ni mapa de esos juegos**.

## Menú SieteNoches (dentro de Unity)

- **Construir escenas que falten:** crea solo las escenas que no existen. Nunca borra trabajo.
- **Correr pruebas rápidas:** revisa reglas, finales y que los minijuegos se puedan ganar y perder. Córranlo antes de subir cambios a `Comun/Scripts` o a la lógica de un minijuego.
- **Probar partida completa:** juega sola de Inicio a Final y avisa si algo falla.
- **Capturar escenas:** guarda una imagen de cada escena en `Docs/Capturas`.

## Hacer un minijuego (Huerto, Refugio, Insulta, Roca)

Su escena ya existe con un minijuego **temporal** de tres botones, para que la partida completa se pueda probar. Para hacer el de verdad:

1. Miren Respira o Tormenta como ejemplo: una clase `...Logica` con las reglas y un `...Controlador` que hereda de `MinijuegoBase`.
2. En su escena, borren el objeto "Minijuego pendiente (reemplazar)" y pongan su controlador con `idMision` correcto.
3. Al terminar, llamen a `Terminar(Resultado.Gano | AMedias | Perdio, "mensaje")`. La roca usa `TerminarRoca(...)`.
4. No cambien recursos a mano: `MinijuegoBase` aplica la tabla del GDD y muestra la tarjeta de dato real.

## Controles

- **Mapa:** WASD o flechas para moverse. E o Espacio para entrar a una misión.
- **Textos:** Espacio, Enter o clic para avanzar.
- **Respira:** mantener Espacio para calentar y pulsar E para bombear.
- **Tormenta:** clic en los paneles o teclas 1 a 6.
