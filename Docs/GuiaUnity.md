# Guía de Unity para el equipo (desde cero)

Esta guía es para quien nunca abrió Unity. Sigue los pasos en orden.

## 1. Instalar (una sola vez)

1. Descarga **Unity Hub** desde unity.com/download e instálalo. El Hub es el "lanzador": desde ahí abres proyectos y eliges la versión de Unity.
2. Abre Unity Hub e inicia sesión con una cuenta Unity gratuita. Acepta la licencia **Personal**.
3. En **Installs > Install Editor** busca la versión **6000.6.8f1**. Tiene que ser exactamente esa. Si no aparece en la lista, búscala en el archivo de versiones de unity.com ("Unity download archive") y pulsa el botón que la abre en el Hub.
4. En los módulos, marca **Web Build Support** y **Windows Build Support**. Si no tienes un editor de código, marca también **Visual Studio Community**.

## 2. Abrir el proyecto

1. Clona el repositorio con GitHub Desktop (**File > Clone repository**) y cambia a tu rama en **Current branch**.
2. En Unity Hub ve a **Projects > Add > Add project from disk** y elige la carpeta del repositorio, la que contiene `Assets` y `ProjectSettings`.
3. Haz clic en el proyecto. La primera vez tarda varios minutos porque Unity crea la carpeta `Library`. Es normal, y esa carpeta no se sube a GitHub.

## 3. Las ventanas de Unity

| Ventana | Para qué sirve |
| --- | --- |
| **Project** (abajo) | Los archivos del proyecto, igual que el Explorador de Windows. Aquí están `Assets/Escenas`, `Assets/Estaciones`, etc. |
| **Hierarchy** (izquierda) | Los objetos de la escena abierta: cámara, jugador, textos, estaciones. |
| **Scene** (centro) | Editor visual de la escena. Puedes mover objetos con el mouse. |
| **Game** (pestaña junto a Scene) | Lo que ve el jugador. Al presionar Play se muestra aquí. |
| **Inspector** (derecha) | Las propiedades del objeto seleccionado: posición, sprite, textos y los números de los scripts. |
| **Console** (pestaña junto a Project) | Mensajes y **errores en rojo**. Mírala siempre que algo no funcione. |

Si cerraste alguna ventana, vuelve a abrirla desde **Window > General**. Para volver a la distribución original: **Window > Layouts > Default**.

### Moverte en la ventana Scene

- **Rueda del mouse:** acercar o alejar.
- **Clic central o clic derecho y arrastrar:** mover la vista.
- **Doble clic en un objeto de Hierarchy:** centra la vista en él.
- **Tecla W y arrastrar las flechas:** mover el objeto seleccionado.

## 4. Jugar y probar

1. En **Project**, abre `Assets/Escenas/Inicio.unity` con doble clic.
2. Presiona el botón **▶ Play**, arriba al centro. Para jugar, haz clic dentro de la ventana Game.
3. Vuelve a presionar **▶** para salir.

> **Importante:** todo lo que cambies mientras Play está activo **se pierde** al salir. Si el botón Play está azul, sal primero y después edita.

Para probar una sola estación, abre su escena (por ejemplo `Assets/Estaciones/Invernadero/Est_Invernadero.unity`) y presiona Play. Al salir te lleva al mapa.

### Controles

- **Mapa:** WASD o flechas para moverte; E para entrar a una estación.
- **Textos:** Espacio o clic para avanzar.
- **Soporte Vital:** mantén Espacio para calentar el MOXIE y pulsa P para purgar el hollín.
- **Antena:** A/D para la frecuencia y W/S para la amplitud.
- **Invernadero y Biomédica:** con el mouse.

## 5. Menú SieteNoches (nuestras herramientas)

En la barra de arriba de Unity aparece el menú **SieteNoches**:

- **Correr pruebas rápidas:** revisa las reglas en segundos. Mira el resultado en la Console.
- **Probar partida completa:** entra a Play y juega sola de Inicio a Final. Al terminar, la Console dice "partida completa sin errores" o lista lo que falló.
- **Construir escenas que falten:** crea solo las escenas que no existen y nunca borra nada.
- **Capturar escenas:** guarda una imagen de cada escena en `Docs/Capturas`.

Antes de hacer un Pull Request a `main`, corre las dos pruebas.

## 6. Editar cosas comunes

- **Cambiar un número de una estación:** selecciona en Hierarchy el objeto que tiene el script `...Controlador`. En el Inspector verás sus campos; cámbialos y guarda con **Ctrl+S**.
- **Cambiar un texto:** selecciona el texto en Hierarchy y edita el campo **Text** en el Inspector.
- **Cambiar un dibujo:** reemplaza el PNG en la carpeta de Windows con el mismo nombre y medida. Unity lo detecta solo (lee el README).
- **Editar código:** doble clic en un archivo `.cs` en Project; se abre en Visual Studio o VS Code. Guarda, vuelve a Unity y espera a que termine de compilar (rueda girando abajo a la derecha). Si hay errores, salen en rojo en la Console y Play no funciona hasta que los arregles.
- **Pintar el mapa:** abre **Window > 2D > Tile Palette**, elige un bloque de `Assets/Comun/Art/Bloques/Tiles` y pinta sobre el Tilemap "Suelo" o "Paredes y rocas" de la escena Mapa. (El Mapa es compartido: avisa al grupo antes.)

## 7. Guardar y subir

1. **Ctrl+S** guarda la escena. Un asterisco `*` en el nombre de la pestaña significa que hay cambios sin guardar.
2. En GitHub Desktop revisa los archivos cambiados. Cada archivo nuevo tiene su **`.meta`**: súbelos juntos, siempre.
3. Escribe un mensaje claro, pulsa **Commit to tu-rama** y luego **Push origin**.
4. **No subas** `Library`, `Temp`, `Logs` ni `UserSettings`. El `.gitignore` ya los ignora.

## 8. Exportar el juego (cuando esté listo)

**File > Build Profiles**, elige **Web** (para jugar en el navegador) o **Windows**, revisa que estén las 11 escenas en la lista y pulsa **Build**. La primera build web tarda bastante.

## 9. Si algo falla

| Problema | Solución |
| --- | --- |
| Play no hace nada o sale error rojo | Lee el primer error de la Console y haz doble clic: te lleva a la línea del código. |
| "Script missing" en un objeto | Falta un `.cs` o su `.meta`. Haz pull de nuevo o pide el archivo a su dueño. |
| La escena aparece vacía | No abriste ninguna escena: doble clic en una de `Assets/Escenas`. |
| Unity pregunta por "Safe Mode" | Hay errores de código. Entra, revisa la Console y arréglalos. |
| Conflicto en GitHub Desktop sobre una escena | No lo resuelvas a ciegas: avisa al dueño de la carpeta. |
