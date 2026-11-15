# Backlog de entrega — audio y presentación

**Versión del documento:** 0.1.1  
**Fecha:** 15 de noviembre de 2026  
**Estado:** aprobado para planificación; pendiente de asignar responsables y fechas.

Este backlog convierte los requerimientos del lead en trabajo verificable. Se apoya en el [alcance](1.%20Alcance.md), el [GDD](2.%20GDD.md), la [guía visual](3.%20Gu%C3%ADa%20visual.md) y las capturas en `Docs/Capturas/`.

## Prioridad y entregables

| ID | Prioridad | Entregable | Resultado verificable |
| --- | --- | --- | --- |
| AUD-01 | Alta | Audio integrado | Archivos reales reemplazan los sonidos de boceto sin cambiar el flujo del juego. |
| PRE-01 | Alta | Presentación en inglés | Archivo editable `.pptx` de 8 diapositivas con las capturas correctas. |
| PRE-02 | Alta | PDF de presentación | Exportación fiel del `.pptx`, de 8 páginas y apta para enviar. |
| DOC-01 | Alta | Créditos de audio | Autor, licencia y enlace de cada recurso externo, o “original del equipo”. |

---

## AUD-01 — Historia de usuario: audio de la misión

### Historia

**Como** jugador de *7 Nights on Mars*,  
**quiero** oír un ambiente que refleje el estado del hábitat y de cada estación,  
**para** reconocer peligro, tensión y progreso sin depender solo de texto o indicadores visuales.

### Contexto actual

El juego ya cuenta con un sistema de audio funcional en:

- `Assets/Comun/Scripts/Audio/AmbienteAudio.cs`: crea un objeto persistente y ajusta volúmenes según el estado de la partida.
- `Assets/Comun/Scripts/Audio/SonidoProcedural.cs`: genera sonidos de boceto por código y primero busca un `AudioClip` real en `Resources/Audio`.

No se debe reemplazar esta lógica en la primera iteración. Si un archivo no existe, el generador actual debe seguir siendo el plan B. Esto protege el juego y mantiene las pruebas existentes.

### Comportamiento que debe conservarse

| Situación de juego | Resultado sonoro esperado | Implementación existente |
| --- | --- | --- |
| Estado normal | Música ambiental suave y zumbido de ventiladores muy bajo. | Música a 0.50; zumbido a 0.12. |
| Ventiladores apagados | A los 10 s suena un crujido del casco; se repite cada 10 s mientras sigan apagados. | `AmbienteAudio.Update()`. |
| Oxígeno menor de 30 % | Música más lenta y pesada. | `pitch = 0.75`. |
| Antena alineando señal | Se apaga la música y aparece un siseo de radio. | `AntenaControlador` + `AmbienteAudio.Siseo`. |
| Noche 7 en el mapa | Sin música para aumentar la tensión. | `AmbienteAudio.Update()`. |
| Amanecer / monitor cardíaco | Pitido de confirmación o monitor. | `MapaControlador` y `MonitorCardiaco`. |
| Diálogo biomédico | *Voice blip* breve cuyo tono depende del tripulante. | `BiomedicaControlador` + `Dialogos.Voz`. |

### Estructura exacta de archivos

Colocar solo los archivos de audio final aprobados en esta ruta. Unity crea los `.meta`; se versionan junto con cada audio.

```text
Assets/
  Resources/
    Audio/
      musica.*         # loop ambiental normal
      zumbido.*        # loop de ventiladores / hábitat
      crujido.*        # efecto puntual del casco
      siseo.*          # loop de interferencia de Antena
      pitido.*         # UI, amanecer y monitor cardíaco
      narracion.*      # reproducción única después de pulsar Jugar
      minijuegos.*     # loop que sustituye la música principal dentro de una estación
```

Los cinco nombres base son un contrato técnico: el código ejecuta `Resources.Load<AudioClip>("Audio/<nombre>")`, sin extensión. Por tanto, Unity carga `musica.wav`, `musica.mp3` u `musica.ogg` como `Audio/musica`. No renombrar ni mover estos archivos sin cambiar y probar `SonidoProcedural.cs`.

**Estado de archivos, 15 de noviembre de 2026:** `musica.wav`, `zumbido.mp3`, `crujido.mp3`, `siseo.mp3` y `pitido.mp3` están en la carpeta y cubren los cinco identificadores. `death.mp3` se conserva como recurso extra; todavía no tiene evento ni lógica de reproducción asignados.

**Flujo de reproducción:** al pulsar **Jugar**, `narracion.wav` se reproduce una vez mientras corren las viñetas de Intro. Al llegar al mapa y ver al personaje, se detiene la narración e inicia `musica.wav`. Al cargar cualquiera de las cuatro estaciones, `minijuegos.mp3` se activa en loop y reemplaza temporalmente la música principal; al volver al mapa, se restaura `musica.wav`. El zumbido permanece como capa de ambiente y los demás efectos conservan su comportamiento.

Los *voice blips* seguirán generándose por código en esta entrega. Su reemplazo por voces grabadas no forma parte de AUD-01; el alcance excluye voces completas y cinemáticas 3D.

### Especificación de producción e importación

| Recurso | Uso | Duración / bucle | Formato recomendado | Importación Unity |
| --- | --- | --- | --- | --- |
| `musica.ogg` | Noche normal | 30–90 s, bucle perfecto | OGG Vorbis, estéreo | Load Type: Streaming; Loop activado por código. |
| `zumbido.ogg` | Ventiladores | 10–30 s, bucle perfecto | OGG Vorbis, mono o estéreo | Load Type: Compressed In Memory; evitar clic al repetir. |
| `crujido.wav` | Casco en tensión | 0.4–1.0 s, no loop | WAV PCM, mono | Load Type: Decompress On Load; pico controlado. |
| `siseo.ogg` | Antena sin señal | 5–15 s, bucle perfecto | OGG Vorbis, mono | Load Type: Compressed In Memory. |
| `pitido.wav` | UI y monitor | 0.05–0.20 s, no loop | WAV PCM, mono | Load Type: Decompress On Load. |

Todos deben entregarse a 44.1 kHz o 48 kHz, sin clipping y con pico máximo de -1 dBFS. El volumen final se equilibra en Unity; no se debe “normalizar” cada archivo a máximo volumen. La dirección es cálida y lo-fi dentro del hábitat, fría e incómoda en crisis, coherente con la guía visual. No usar música, voces ni efectos extraídos de otros videojuegos.

### Tareas técnicas

- [ ] Crear `Assets/Resources/Audio/` y agregar los cinco clips con los nombres del contrato.
- [ ] Escuchar cada bucle durante al menos tres repeticiones: no debe haber corte ni clic.
- [ ] Verificar en Unity que cada clip se importa como `AudioClip` y revisar los ajustes de la tabla.
- [ ] Ejecutar una partida normal: confirmar música y zumbido.
- [ ] Apagar ventiladores y calefacción: confirmar silencio tenso y crujidos.
- [ ] Forzar O₂ bajo: confirmar que la música reduce su velocidad.
- [ ] Entrar a Antena y mover la señal: confirmar silencio de música y siseo.
- [ ] Probar monitor, amanecer y un diálogo biomédico: confirmar pitido y *voice blips*.
- [ ] Correr **SieteNoches > Correr pruebas rápidas** y **Probar partida completa**.
- [ ] Registrar procedencia y licencia en la tabla de créditos de la guía visual o en `Docs/Creditos.md` si se crea.

### Criterios de aceptación

1. Los cinco sonidos reales se cargan al iniciar sin errores en Console.
2. El comportamiento de la tabla se escucha en Play Mode y el estado del juego no cambia por reproducir audio.
3. Si se elimina temporalmente uno de los clips, el juego conserva el sonido procedimental correspondiente y no falla.
4. No hay clipping, silencios no intencionales ni clic perceptible al repetir los loops.
5. Las pruebas rápidas y la partida completa terminan sin errores.
6. Cada audio tiene crédito y licencia compatibles con la entrega del NASA Space Apps Challenge.

---

## PRE-01 y PRE-02 — Presentación del NASA Space Apps Challenge

**Estado actual:** primer PPTX y PDF generados el 15 de noviembre de 2026; pendiente de revisión visual del equipo y, antes de publicación externa, reemplazar el lockup vectorial por el archivo oficial aprobado si el lead lo solicita.

### Objetivo

Crear una presentación de **exactamente 8 diapositivas**, en inglés, que explique el reto, problema, solución, alcance y evidencia del juego. El archivo maestro será editable (`.pptx`) y su exportación será un PDF de 8 páginas. Debe usar capturas reales del juego, no imágenes de terceros sin atribución.

### Ubicación y convención de entrega

```text
Docs/
  Presentacion/
    7_Nights_on_Mars_NASA_Space_Apps_2026.pptx
    7_Nights_on_Mars_NASA_Space_Apps_2026.pdf
    README.md                         # fuentes, créditos y fecha de exportación
```

No crear los binarios hasta que el equipo apruebe el contenido y las capturas. Una vez creados, ambos archivos se deben abrir y revisar antes de subirlos.

### Guion aprobado: 8 diapositivas en inglés

| # | Título y texto base en inglés | Imagen / evidencia reservada | Nota de diseño |
| --- | --- | --- | --- |
| 1 | **7 Nights on Mars**<br>*A Junior Astronaut Mission Trainer for the NASA Space Apps Challenge 2026.*<br>*Can one crew survive seven Martian nights?* | `Docs/Capturas/Inicio.png` | Título muy corto; dejar aire para logo/equipo. |
| 2 | **The Challenge**<br>*How can young explorers understand the trade-offs of keeping a Mars habitat alive?*<br>*Our mission trainer turns life-support decisions into a playable seven-night survival story.* | `Docs/Capturas/Intro.png` | Mencionar el reto “Build a Junior Astronaut Mission Trainer” solo si la organización lo confirma. |
| 3 | **The Problem: Every System Is Connected**<br>*On Mars, energy, oxygen, water, food and crew health depend on one another.*<br>*A decision that protects one system can put another at risk.* | `Docs/Capturas/Juego_02_Mapa.png` | Mostrar HUD y mapa; usar flechas o etiquetas mínimas. |
| 4 | **Our Solution**<br>*7 Nights on Mars is a 2D single-player browser game.*<br>*Players manage limited battery power, visit critical stations and learn through consequences.* | `Docs/Capturas/Juego_01_Noche1_1830.png` | Mostrar la pantalla de telemetría e incidente. |
| 5 | **Mission Scope**<br>*Four connected stations: Greenhouse, Life Support, Medical Terminal and X-Band Antenna.*<br>*Each visit advances time; the systems keep their state overnight.* | Collage: `Juego_03_Invernadero.png`, `Juego_04_SoporteVital.png`, `Juego_05_Biomedica.png`, `Juego_07_Antena.png` | Cuadrícula 2 × 2 con una etiqueta por estación. |
| 6 | **Learning Through Trade-offs**<br>*Grow food, produce oxygen, control CO₂, support the crew and send rescue messages.*<br>*Real NASA-inspired data informs the rules behind each decision.* | `Docs/Capturas/Juego_06_Amanecer.png` | Separar con claridad datos reales y ficción; agregar fuentes NASA verificadas antes de entregar. |
| 7 | **The Final Challenge**<br>*On Night 7, players have 60 seconds to close the valves and reach the bridge.*<br>*Their accumulated decisions lead to one of eight endings and a scientific mission report.* | `Juego_09_Noche7.png` + `Juego_10_Lanzamiento.png` o `Juego_11_Final.png` | Alto contraste rojo; no sobrecargar de texto. |
| 8 | **Impact and Next Steps**<br>*Make complex Mars survival systems understandable, memorable and discussable.*<br>*Next: final art and audio, external playtesting, web build and public release.*<br>*Thank you.* | `Docs/Capturas/Final.png` | Espacio para QR, enlace público y nombres del equipo cuando estén listos. |

### Reglas de contenido y diseño

- Todo el texto de las diapositivas será inglés. Los nombres propios del juego se conservan como marca; las descripciones se traducen.
- Mantener una idea principal por diapositiva; usar el guion como máximo de texto, no agregar párrafos.
- Usar imágenes a 16:9, sin deformarlas ni recortarlas de forma que oculten información crítica del HUD.
- Seguir la paleta existente: noche `#121A2E`, alerta `#E5484D`, energía `#F2C14E`, texto `#F4F1EA` y acentos verdes `#4CAF7A`.
- Reservar áreas señaladas como imagen para capturas reales actuales. Si una captura cambia, actualizarla antes de exportar PDF.
- Incluir fuentes NASA verificadas en las notas del presentador o en la diapositiva 6/8 si la regla de entrega lo exige. No afirmar afiliación o aprobación de NASA.
- Reemplazar marcadores de equipo, enlace y QR solo cuando esos datos estén confirmados.

### Criterios de aceptación

1. El `.pptx` y el `.pdf` existen en `Docs/Presentacion/`, y el PDF tiene exactamente 8 páginas.
2. Las ocho diapositivas están completamente en inglés y siguen el orden del guion.
3. Cada diapositiva usa una captura o espacio reservado indicado; no hay imágenes de terceros sin crédito.
4. Los textos son legibles a pantalla completa y no se salen del área de la diapositiva.
5. La exportación PDF conserva fuentes, imágenes y contraste del archivo PPTX.
6. La presentación explica con precisión el reto, problema, solución, alcance, evidencia jugable e impacto del proyecto.

---

## Dependencias y decisiones pendientes

| Tema | Responsable | Decisión requerida antes de cerrar |
| --- | --- | --- |
| Reto oficial | Lead | Confirmar el título exacto del reto de Space Apps que se mostrará en la diapositiva 2. |
| Recursos de audio | Audio / Lead | Aprobar fuente, licencia y tono de los cinco clips. |
| Capturas finales | Arte / QA | Confirmar qué capturas representan la versión final del juego. |
| Datos y fuentes NASA | Investigación | Verificar enlaces y formulación para la diapositiva 6. |
| Equipo, QR y URL pública | Lead | Entregar nombres, logo autorizado y enlace final. |

## Definition of Done de este backlog

Se considera cerrado cuando AUD-01, PRE-01, PRE-02 y DOC-01 cumplen sus criterios de aceptación, los binarios fueron revisados en una máquina distinta si es posible, y el CHANGELOG registra la versión de entrega.
