# 7 Noches en Marte

Juego 2D en Unity para el NASA Space Apps Challenge 2026. La nave aterrizó con averías en Marte y tienes 7 noches de guardia para sobrevivir hasta las 06:00 AM, reparar y despegar. De noche los paneles dan 0 W: empiezas cada noche con una batería limitada y decides qué sistemas mantener encendidos.

El diseño completo está en el documento del equipo "Ideas de misiones" (Google Docs). Lo que ese documento no fijaba (números, tiempos) está en `Docs/CambiosParaDocs` en la PC de Sergio y en el código, en `SimulacionNoche.cs` y `Semillas.cs`.

## Abrir el proyecto

1. Instalar **Unity 6000.6.8f1** (exactamente esa versión) con Web Build Support y Windows Build Support.
2. Clonar este repositorio con GitHub Desktop.
3. En Unity Hub: **Add > Add project from disk** y elegir la carpeta del repositorio.
4. Abrir `Assets/Escenas/Inicio.unity` y presionar **Play**.

¿Nunca usaste Unity? Lee primero [Docs/GuiaUnity.md](Docs/GuiaUnity.md): explica cada ventana y cómo probar el juego paso a paso.

Cada estación también se puede probar sola: abrir su escena (por ejemplo `Assets/Estaciones/Invernadero/Est_Invernadero.unity`) y presionar Play.

## Cómo funciona una noche

1. **18:30:** pantalla con la telemetría real de la noche (temperatura, presión, Tau) y su incidente. En las Noches 3 y 6 hay que elegir un dilema.
2. Caminas por el domo y entras a las estaciones. **Cada visita dura 2 h 30**: caben 4 por noche. En los **Camarotes** puedes terminar la guardia antes.
3. **06:00 AM:** se resuelve todo lo que dejaste configurado (batería, O₂, plantas, CO₂, pulso, sabotajes). Lo configurado se queda para la noche siguiente: **las estaciones tienen memoria**.
4. **Noche 7, 05:00 AM:** 60 segundos con luces rojas. Cierra las válvulas en Soporte Vital y corre al Puente para despegar o sellar el domo. Luego el final y el Informe Científico de Misión.

| Estación | Escena | Qué haces | Qué cambia en las otras |
| --- | --- | --- | --- |
| Invernadero | `Est_Invernadero` | 3 cápsulas: semilla, humedad y LEDs (660/450/380 nm) según el Manual Astrobotánico | O₂, agua, raciones de papa y dosis de lechuga para la Biomédica |
| Soporte Vital | `Est_SoporteVital` | MOXIE a 800 °C (una vez por noche) y calefacción/ventilación por sector | Sin calor, las plantas húmedas se congelan; sin ventiladores, CO₂ en los camarotes |
| Terminal Biomédica | `Est_Biomedica` | Hablar con 2 tripulantes: diagnóstico, promesa o entregar comida | Tripulantes en verde dan ventajas; en rojo sabotean de noche |
| Antena Banda X | `Est_Antena` | Elegir 1 paquete (SOS, domo o ciencia) y alinear la onda con el Doppler | Rescate y domo para los finales; Nikolas responde con 12 min de retraso |

## Ramas: una por programador

| Rama | Quién | Qué toca (propuesta) |
| --- | --- | --- |
| `main` | Nadie trabaja aquí directo | Versión que funciona. Solo recibe cambios por Pull Request. |
| `sergio` | Sergio | `Assets/Estaciones/SoporteVital`, `Assets/Estaciones/Antena`, `Assets/Intro` |
| `nicolas` | Nicolás | `Assets/Estaciones/Invernadero` |
| `jean-paul` | Jean-Paul | `Assets/Estaciones/Biomedica` |

El reparto es una propuesta: cámbienlo aquí si deciden otro. La regla importante es que **cada carpeta tiene un solo dueño**. Si algo se rompe, sabemos en qué rama y carpeta buscar.

`Assets/Comun` (GameState, simulación de la noche, mapa, jugador) y `Assets/Escenas` son compartidos. Antes de cambiarlos, avisen al grupo y corran las pruebas.

### Rutina diaria (GitHub Desktop)

1. **Current branch:** tu rama (`sergio`, `nicolas` o `jean-paul`).
2. **Branch > Update from main** para traer lo último que se unió.
3. Trabaja en Unity, guarda con Ctrl+S y cierra Play antes de editar.
4. Revisa que solo cambiaste archivos de tus carpetas, **junto con sus `.meta`**.
5. **Commit** con un mensaje claro y **Push origin**.
6. Cuando tu parte funcione: **Create Pull Request** hacia `main`.

## Carpetas

```text
Assets/
  Comun/
    Scripts/        GameState, SimulacionNoche (las 06:00), Semillas, Datos (textos de cada noche), EvaluadorFinal, Teclas
      Modelo/       Capsula y Tripulante
      Estaciones/   EstacionBase (contrato que cumplen las 4 estaciones)
      Audio/        música, zumbido y crujidos generados por código (boceto)
      Jugador/      movimiento, animación por hoja de sprites, cámara
      Mapa/         estaciones del mapa y controlador (HUD, 18:30, 06:00, Noche 7)
      Pantallas/    inicio, intro, lanzamiento, final
      UI/           BotonMantener
    Art/            bloques, personaje, estaciones, retratos
    UI/             iconos de la barra y flecha
  Escenas/          Inicio, Intro, Mapa, Lanzamiento, Final
  Estaciones/<Nombre>/  escena Est_<Nombre>, Scripts/, Art/
  Intro/Vinetas/    las 8 imágenes de la cinemática
  Editor/           herramientas (menú SieteNoches) y pruebas
Docs/
  Arte/             guía de la hoja de sprites del personaje
  Capturas/         imagen de cada escena; Juego_*.png son fotos tomadas durante la prueba de partida
```

## Para artistas: cómo reemplazar los dibujos

Todo el arte actual es **boceto**. Para cambiarlo, guarden su PNG **con el mismo nombre y la misma medida** encima del archivo viejo. Unity lo configura solo: 32 píxeles por unidad, Point y sin compresión. No hace falta tocar nada en Unity.

| Archivo | Medida | Qué es |
| --- | --- | --- |
| `Comun/Art/Personajes/spr_jugador_hoja.png` | 128 × 192 | Hoja del jugador: 4 × 4 celdas de 32 × 48 (ver abajo) |
| `Comun/Art/Bloques/tile_*.png` | 32 × 32 | Piso y pared del domo, suelo y roca de Marte, puerta |
| `Comun/Art/Estaciones/spr_estacion_*.png` | 64 × 64 | invernadero, soporte, biomedica, antena, camarotes, puente, paneles |
| `Comun/Art/Retratos/spr_retrato_*.png` | 96 × 96 | elena, marco, aris, jonas, nikolas |
| `Comun/Art/Retratos/spr_retrato_*_mirada.png` | 96 × 96 | Versión "en crisis": sin animar, los ojos miran fijo a la cámara |
| `Comun/UI/ui_icono_*.png` | 32 × 32 | bateria, oxigeno, agua, co2, comida, rescate, domo |
| `Intro/Vinetas/spr_intro_01..08.png` | 640 × 240 | Viñetas de la cinemática |
| `Estaciones/Invernadero/Art/spr_planta_<especie>_<estado>.png` | 48 × 48 | especie: espirulina, papa, lechuga, arabidopsis, dechloromonas; estado: brote, crece, madura, estresada (hojas amarillas: pista de error) |
| `Estaciones/Invernadero/Art/spr_planta_{podrida,congelada,retorcida}.png` | 48 × 48 | Biomasa podrida, planta congelada y la Arabidopsis retorcida de la Noche 5 |
| `Estaciones/Invernadero/Art/spr_capsula_{hidroponica,regolito}.png` | 80 × 100 | Cápsulas de cultivo |
| `Estaciones/Invernadero/Art/spr_manual_pagina.png` | 290 × 300 | Página del Manual Astrobotánico (el texto lo pone el juego) |
| `Estaciones/*/Art/spr_*_fondo.png` | 640 × 360 | Fondo de cada estación |

### Hoja de sprites del jugador

![Guía de la hoja](Docs/Arte/guia_hoja_personaje.png)

- Cada celda mide **32 × 48 px**. La hoja completa mide 128 × 192.
- **Columnas:** frame 0 es quieto; los frames 1, 2 y 3 forman la caminata.
- **Filas, de arriba hacia abajo:** 0 abajo, 1 izquierda, 2 derecha, 3 arriba.
- Los pies van en el borde inferior de la celda, que es el punto de apoyo.
- Los números rojos del boceto solo sirven de guía; no los copien.

### Audio

El juego genera por código una música lo-fi, el zumbido de los ventiladores, los crujidos del casco, el siseo de la antena y las voces (*voice blips*). Para usar audio real, guarden el archivo en `Assets/Resources/Audio/` con uno de estos nombres: `musica`, `zumbido`, `crujido`, `siseo` o `pitido`. Reglas de tono que ya funcionan:

- Si se apagan los ventiladores **y** la calefacción de los camarotes, la música y el zumbido se cortan en seco y solo suenan crujidos.
- Con O₂ bajo 30 %, la música baja a 0.75x.
- En la antena y en la Noche 7 la música se apaga.

Inspiración: tomamos ideas de Undertale (intro con viñetas, contraste de tono), Among Us (mapa con salas), FNAF (sobrevivir hasta las 6:00 AM) y Papers, Please (interfaces diegéticas), pero **no se usa ningún sprite, música ni mapa de esos juegos**.

## Menú SieteNoches (dentro de Unity)

- **Construir escenas que falten:** crea solo las escenas que no existen. Nunca borra trabajo.
- **Correr pruebas rápidas:** revisa reglas de la noche, eventos, los 8 finales, el MOXIE, la antena, los diálogos y dos partidas simuladas de 7 noches. Córranlo antes de subir cambios a `Comun/Scripts` o a la lógica de una estación.
- **Probar partida completa:** juega sola de Inicio a Final pasando por las 4 estaciones y avisa si algo falla.
- **Capturar escenas:** guarda una imagen de cada escena en `Docs/Capturas`.

## Cambiar una estación

1. Cada estación tiene un `...Controlador` que hereda de `EstacionBase`. La lógica que se puede probar sin pantalla está en clases aparte (`MoxieLogica`, `AntenaLogica`, `Dialogos`).
2. La estación cambia `GameState` mientras se juega. Al salir, `Salir()` muestra la tarjeta de dato real y vuelve al mapa: el reloj avanza 2 h 30.
3. Los números de la noche (costos de batería, O₂, agua) están arriba de `SimulacionNoche.cs`. Los de las semillas, en `Semillas.cs`. Si cambian uno, avísenle al grupo y corran las pruebas.

## Controles

- **Mapa:** WASD o flechas para moverse. E para entrar a una estación.
- **Textos y pantallas:** Espacio o el botón Continuar.
- **Soporte Vital:** mantener Espacio para calentar el MOXIE; P para purgar el hollín.
- **Antena:** A/D o flechas izquierda/derecha para la frecuencia; W/S o arriba/abajo para la amplitud.
- **Invernadero y Biomédica:** con el mouse.
