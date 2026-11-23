# Instrucciones para agentes (Antigravity, Copilot, Claude u otro)

Este archivo lo lee el agente antes de trabajar en el repo. Responde y escribe documentación en
**español**; los commits van en **inglés** (ver "Commits").

## 1. Qué es este proyecto

- Demo de un **cubo virtual con retroalimentación háptica** para un experimento que compara la
  actividad cerebral (EEG/fNIRS) al tocar un cubo físico vs. uno virtual. Laboratorio de
  Tecnologías Inmersivas, Tec de Monterrey campus Santa Fe.
- Hardware: **Meta Quest 3** + guante háptico **HexR** de Microtube Technologies (Singapur),
  conectado por Bluetooth (BLE). El guante funciona con aire: el módulo de brazo tiene la bomba y
  manda presión a cada dedo.
- Repo: fork de `MicrotubeTechnologies/HexR-Developer-Tutorial-Meta-OVR`.
- **Unity 2022.3.34f1**, plataforma **Android**, Meta XR SDK 71 (**Meta OVR**, no OpenXR), IL2CPP.

## 2. La regla más importante: la mano invertida

El laboratorio tiene **dos módulos de brazo IZQUIERDOS y ningún derecho**. El módulo izquierdo se
usa puesto en la **mano derecha**. Por eso `CustomTrigger.cs` manda la háptica de la mano
derecha al handler **izquierdo** (`Left Hand Physics`) y viceversa. Hay un comentario de
Microtube que lo dice: *"wrong on purpose for mexico university having only left arm module"*.

- **No "corrijas" esta inversión.** No es un bug.
- El guante se conecta con **"Connect To HexR Left"**. "Right" nunca conecta.

## 3. Ramas

| Rama | Qué es | ¿Se toca? |
|---|---|---|
| `cube-demo` | Nuestra rama de trabajo (sale de `Reverse_hand_Sample`) | Solo por Pull Request |
| `Reverse_hand_Sample` | Base de Microtube (v1, 2 cubos) | **No** |
| `latest_version` | Otra versión de Microtube (Unity 6, SDK como paquete) | **No** |

Reglas:
- Cada tarea va en una rama nueva que sale de `cube-demo`: `agent/<tema-corto>`
  (por ejemplo `agent/cube-touch-color`).
- **Nunca** hagas push directo a `cube-demo`, `Reverse_hand_Sample` ni `latest_version`.
- **Nunca** uses `git push --force`, `git reset --hard` sobre trabajo ajeno, ni borres ramas.
- Al terminar: push de tu rama y abre un **Pull Request hacia `cube-demo`**. Una persona lo revisa
  y lo prueba con el visor antes de hacer merge.

## 4. Archivos importantes

| Ruta | Para qué |
|---|---|
| `Assets/Scenes/Custom_Hand.unity` | La escena del cubo (la única que se compila) |
| `Assets/Script/CustomTrigger.cs` | Detecta qué dedos tocan el cubo y manda presión al guante cada 0.2 s. Contiene la inversión de manos y la conexión al guante |
| `Assets/Script/RotationOnAxis.cs` | Gira el cubo en X/Y/Z |
| `Assets/Script/CubeControlPanel.cs` | Panel propio: eje y velocidad de giro |
| `Assets/Script/FingerButton.cs` | Botón 3D que se activa con la punta del índice |
| `Assets/Editor/SetupCubeDemoScene.cs` | Arma la escena (1 cubo + panel) desde código. Re-ejecutable |
| `Assets/Editor/BuildCubeDemo.cs` | Compila el APK `com.TecLTI.CubeDemo` ("Cube Demo") |
| `Assets/HexRAssets/`, `Assets/Plugins/` | SDK del guante de Microtube (`HaptGlove.dll`, etc.). **No modificar** |

Cómo se detectan los dedos: los colliders de los dedos tienen `HapticFingerTrigger` y se llaman
`R_`/`L_` + dedo (`Thumb`, `Index`, `Middle`, `Ring`, `Pinky`, `Palm`).

## 5. Cómo compilar sin abrir Unity

Con el Editor de Unity **cerrado**, desde la raíz del repo:

```bash
UNITY="/c/Program Files/Unity/Hub/Editor/2022.3.34f1/Editor/Unity.exe"
# 1. Armar la escena (solo si cambió SetupCubeDemoScene.cs)
"$UNITY" -batchmode -quit -projectPath . -buildTarget Android -executeMethod SetupCubeDemoScene.Apply -logFile ../context/logs/unity_setup.log
# 2. Compilar el APK
"$UNITY" -batchmode -quit -projectPath . -buildTarget Android -executeMethod BuildCubeDemo.BuildAndroid -buildOutput ../builds/cube-demo/cube-demo.apk -logFile ../context/logs/unity_build.log
```

- Revisa el log: busca `error CS` (errores de C#) y `Build succeeded`/`Build Finished`.
- Si Unity está abierto, el batchmode falla porque el proyecto está bloqueado: pídele a la persona
  que lo cierre.
- La primera compilación tarda varios minutos.

### Archivos que Unity cambia solo (no se suben)

Después de compilar, regresa estos archivos con `git restore` y no los incluyas en commits:
`Assets/Resources/OVRBuildConfig.asset(.meta)`, `Assets/Resources/OculusRuntimeSettings.asset(.meta)`,
`ProjectSettings/ProjectSettings.asset`,
`ProjectSettings/Packages/com.unity.testtools.codecoverage/Settings.json`.
Nunca subas `Library/`, `Temp/`, `Logs/`, `obj/` ni APKs.

### Instalar en el visor (solo si la persona lo pide y el Quest está conectado)

```bash
ADB="/c/Program Files/Unity/Hub/Editor/2022.3.34f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe"
"$ADB" devices
"$ADB" install -r ../builds/cube-demo/cube-demo.apk
"$ADB" shell pm grant com.TecLTI.CubeDemo android.permission.ACCESS_FINE_LOCATION
"$ADB" shell pm grant com.TecLTI.CubeDemo android.permission.BLUETOOTH_SCAN
"$ADB" shell pm grant com.TecLTI.CubeDemo android.permission.BLUETOOTH_CONNECT
```

Con dos visores conectados, usa `-s <serie>`: Visor 16 = `2G0YC1ZF7T05GR`, Visor 17 = `2G0YC1ZF8B0FRP`.
**No desinstales** apps del visor sin que te lo pidan. `com.Microtube.HexRTest` es la demo de
Microtube que se usa en el laboratorio: no la toques.

## 6. Reglas de código

- C# para Unity 2022.3 (no uses APIs de Unity 6).
- Con objetos de Unity **no uses** `??` ni `?.` (no respetan el null de Unity): usa `!= null`.
- Cambios pequeños y enfocados. No reformatees archivos completos ni cambies nombres sin motivo.
- No edites escenas `.unity` a mano si se puede evitar: prefiere hacerlo desde
  `SetupCubeDemoScene.cs` para que el cambio sea reproducible.
- No cambies el SDK de Microtube ni versiones de paquetes sin que te lo pidan.
- Comentarios en inglés y breves.

## 7. Commits

Estándar del equipo:
- En **inglés**, modo **imperativo**, máximo **60 caracteres**, sin punto final.
  Ejemplo: `Add touch color feedback to cube`.
- Un cambio lógico por commit. Cuerpo opcional de 1 a 3 líneas explicando el porqué.
- **Sin** líneas `Co-Authored-By`.
- Nada de mensajes genéricos (`update`, `fix`, `changes`).

## 8. Humano en el ciclo (human in the loop)

El agente **no puede probar el guante**. El comportamiento físico solo lo confirma una persona con
el visor y el guante puestos.

- No digas que algo "funciona en el visor" o que "se siente bien" si no lo probó una persona.
  Di "compila" o "no compila", y lo que falta probar.
- En cada Pull Request incluye una sección **"Cómo probarlo en el visor"** con pasos concretos:
  qué tocar, con qué mano (la derecha), y qué debería pasar en pantalla y en el guante.
- Seguridad: si el Bluetooth se cae, el guante se queda con la última presión. Si tu cambio toca
  la háptica, asegúrate de que siempre se mande presión 0 al soltar, y menciónalo en el PR.

## 9. Formato del Pull Request

```
## Qué cambia
<1-3 líneas>

## Por qué
<1-2 líneas>

## Cómo se comprobó
- [ ] Compila (log de Unity sin errores)
- [ ] Probado en el visor con el guante (lo marca la persona)

## Cómo probarlo en el visor
1. ...
```
