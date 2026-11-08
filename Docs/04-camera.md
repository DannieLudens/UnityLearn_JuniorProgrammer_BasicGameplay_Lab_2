# 04 · Camera

[← Índice](README.md)

Dos formas de seguir al jugador: **un script propio** (para entender los fundamentos) y **Cinemachine** (la herramienta profesional). Primero hice el script y después lo reemplacé por Cinemachine.

---

## Parte 1 · Script propio (CameraFollow)

> Este script ya no está en el proyecto: lo reemplacé por Cinemachine. Sigue en el historial de git (commit `51aec4a`). Lo dejo aquí porque los conceptos son la base para entender qué hace Cinemachine.

### La idea: el offset

```
offset = camera - player
```

Seguir al jugador es: **en cada frame, poner la cámara en `player + offset`**. Calcular el offset en `Start` respeta el encuadre armado en la escena.

```csharp
[SerializeField] private Transform target;               // What to follow
[SerializeField] private float smoothTime = 0.2f;        // Seconds to catch up (0 = instant)

private Vector3 _offset;                                 // Camera position relative to target
private Vector3 _velocity;                               // Used internally by SmoothDamp

// Remember the framing set up in the scene
private void Start()
{
    _offset = transform.position - target.position;      // Camera minus player = distance to keep
}

// Move after everything else has moved this frame
private void LateUpdate()
{
    Vector3 desired = target.position + _offset;         // Where the camera should be
    // desired.y = transform.position.y;                 // Uncomment to follow X only
    transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime); // Ease toward target
}
```

### LateUpdate: el tercer método

| Método | Cuándo corre | Para qué |
|---|---|---|
| `FixedUpdate` | Al ritmo de la física (50/s) | Mover Rigidbodies |
| `Update` | Cada frame | Input, lógica |
| **`LateUpdate`** | Cada frame, **después de todos los Update** | **Cámaras** |

**Por qué:** Unity no garantiza el orden de los `Update` entre scripts. Si la cámara se mueve antes que el jugador, va un frame atrasada y **tiembla**. `LateUpdate` garantiza que se mueve **al final**, incluyendo la posición interpolada del Rigidbody.

**⚠️ Trampa:** mover la cámara en `FixedUpdate` hace que se mueva a saltos (50 Hz contra una pantalla de 60–144 Hz).

### SmoothDamp y `ref`

`SmoothDamp` lleva la cámara hacia el objetivo en unos `smoothTime` segundos, como un resorte amortiguado.

**`ref`:** normalmente un método recibe **una copia** de la variable. Con `ref` recibe **la original** y puede modificarla. `SmoothDamp` necesita **recordar la velocidad** entre frames. Por eso `_velocity` es un **campo** (vive entre frames) y no una variable local (moriría al terminar el método).

### ¿Por qué no Lerp?

```csharp
transform.position = Vector3.Lerp(transform.position, desired, 0.1f); // ❌ framerate dependent
```

"Avanza 10% **por frame**": a 144 FPS alcanza al jugador mucho más rápido que a 30 FPS, así que **el juego se siente distinto en cada PC**. `SmoothDamp` trabaja en **segundos**.

### Seguir en Y o no

- **Solo X:** estable, pero al subir a una plataforma alta el jugador queda en el borde superior de la pantalla.
- **X e Y:** sigue los cambios de altura (necesario con niveles a distintas alturas y plataformas móviles), pero la cámara sube y baja con **cada** salto.

La solución a los dos problemas es la **dead zone**, que trae Cinemachine.

---

## Parte 2 · Cinemachine

### Cómo funciona

- **Cinemachine Camera** (cámara virtual): **no renderiza**. Es un conjunto de instrucciones: a quién seguir, cómo, con qué suavizado...
- **Cinemachine Brain** (en la Main Camera): en cada frame mira cuál Cinemachine Camera **activa tiene la mayor prioridad** y mueve la cámara real según sus instrucciones.

**⚠️ Trampa importante:** con el Brain, **ya no se edita la Main Camera**. Si la mueves, el Brain la regresa en el siguiente frame. **Todo se configura en las Cinemachine Cameras.**

**⚠️ Trampa:** no mezclar un script de cámara propio con Cinemachine en la misma cámara: los dos pelean y la cámara tiembla.

### Configuración para side-view (CM_Player)

1. **GameObject → Cinemachine → Cinemachine Camera** (agrega el Brain a la Main Camera automáticamente).
2. **Rotation** del Transform: `(10, 0, 0)`.
3. **Tracking Target** → Player.
4. **Position Control → Position Composer**: seguir **sin rotar**.
5. **Rotation Control → None**: la cámara nunca gira, y se conserva la ilusión del plano 2.5D.

### Position Composer

| Propiedad | Qué hace | Valor en el proyecto |
|---|---|---|
| **Camera Distance** | Distancia al jugador | `9` |
| **Damping** (X, Y, Z) | Suavizado **por eje**, en segundos | `(0.5, 1, 0)` |
| **Dead Zone** | Rectángulo donde el jugador se mueve sin que la cámara lo siga | `0.10 × 0.17` |
| **Lookahead** | Adelantarse hacia donde camina el jugador | Time `0.3`, Smoothing `7`, ✅ **Ignore Y** |

**Por qué el damping es distinto por eje:** con X bajo, la cámara sigue rápido al caminar; con Y alto, sigue lento los cambios de altura y suaviza los saltos.

**Cómo resuelve la dead zone el problema de Y:** un salto normal mantiene al jugador **dentro** del rectángulo, así que la cámara no se mueve. Al subir a una plataforma alta, el jugador **sale** del rectángulo y la cámara lo sigue. **Sigue los cambios de nivel e ignora los saltos.**

**Por qué Ignore Y en el lookahead:** sin esa opción, la cámara se adelanta hacia arriba al saltar y hacia abajo al caer, y eso marea.

**⚠️ Trampa:** con **Lookahead Smoothing = 0**, la cámara da tirones al cambiar de dirección rápido. Usar 5–10.

**Guías en la ventana Game** (con la Cinemachine Camera seleccionada): la zona transparente es la dead zone, la azul es la soft zone y la roja es el límite duro. Se pueden **arrastrar sus bordes** directamente, incluso en Play Mode (los cambios en Play Mode se pierden al salir).

---

## Parte 3 · Intro: vista general → jugador

### La idea: prioridades y blend

```
Start:
  CM_Overview  (priority 10)  ← the Brain shows this one
  CM_Player    (priority 0)

CM_Overview's component gets disabled:
  CM_Player    (priority 0)   ← the only active one → the Brain BLENDS to it
```

El Brain **no corta**: hace una transición usando su **Default Blend** (en el proyecto: *Ease In Out*, 2 s).

| Style | Sensación |
|---|---|
| Cut | Corte instantáneo |
| Ease In Out | Arranca lento, acelera y frena: cinematográfico |
| Linear | Velocidad constante, más mecánico |

**CM_Overview:** sin Tracking Target, sin Position Control (cámara fija), y Priority **10** (con el checkbox activado). Para encuadrarla: mover la vista Scene como queramos → seleccionar la cámara → **GameObject → Align With View** (Ctrl + Shift + F).

> La regla "rotación Y = 0" aplica a la cámara **de juego**. Una cámara de intro es un plano cinematográfico y puede tener cualquier ángulo. En el proyecto, CM_Overview usa una vista en diagonal (Y ≈ 53°), y el blend barre esa rotación hasta 0°.

### Coroutines

**La idea:** un método normal corre de principio a fin **en un frame**. Una **Coroutine** puede **pausarse y continuar en un frame futuro** sin congelar el juego.

```
Normal method:   [start ────────── end]                              (one frame)

Coroutine:       [start ── yield] ... game keeps running ... [continue ── end]
```

| Palabra | Significado |
|---|---|
| `IEnumerator` | Tipo de retorno de una Coroutine (requiere `using System.Collections;`) |
| `yield return ...` | "Pausa aquí; continúa cuando se cumpla esto" |
| `StartCoroutine(...)` | La forma de arrancarla |
| `yield return null` | Esperar exactamente **un frame** |
| `WaitForSeconds(t)` | Esperar `t` segundos |
| `WaitWhile(() => cond)` | Esperar **mientras** la condición sea verdadera (su gemela es `WaitUntil`) |

**⚠️ Trampa:** llamar `PlayIntro();` sin `StartCoroutine(...)` **no hace nada**, y tampoco da error.

### El script

```csharp
[SerializeField] private float showTime = 2f;            // Seconds to show the overview
[SerializeField] private PlayerController player;        // Player to lock during the intro
[SerializeField] private CinemachineBrain brain;         // Brain on Main Camera (to know when blend ends)

private CinemachineCamera _overviewCam;                  // This overview camera

// Start the intro countdown
private void Start()
{
    _overviewCam = GetComponent<CinemachineCamera>();    // Cache our own camera
    StartCoroutine(PlayIntro());                         // Run the sequence
}

// Lock player, show overview, blend to player camera, unlock player
private IEnumerator PlayIntro()
{
    player.SetControlEnabled(false);                     // No input during the intro
    yield return new WaitForSeconds(showTime);           // Show the overview

    _overviewCam.enabled = false;                        // Brain blends to the next camera
    yield return null;                                   // Wait exactly one frame so the blend starts
    yield return new WaitWhile(() => brain.IsBlending);  // Wait until the blend finishes

    player.SetControlEnabled(true);                      // Give control back
}
```

**Por qué `yield return null` antes del `WaitWhile`:** el Brain actualiza en `LateUpdate`. Justo después de desactivar la cámara todavía no se ha enterado, así que `IsBlending` daría `false` y el control volvería al instante.

**Por qué `WaitWhile` y no `WaitForSeconds(2f)`:** con `WaitForSeconds` habría **el mismo número en dos lugares** (el Brain y el script). Si cambio el blend y olvido el script, el control vuelve a mitad de la transición. Preguntándole al Brain, siempre está sincronizado.

**Lambda `() => brain.IsBlending`:** `WaitWhile` necesita **una pregunta que pueda repetir cada frame**, no una respuesta de una sola vez.

```
brain.IsBlending           → an answer, right now, once
() => brain.IsBlending     → a question Unity can ask again every frame
```

**⚠️ Trampa: desactivar el componente, no el GameObject.**

| Acción | ¿El Brain la ignora? | ¿La Coroutine sigue? |
|---|---|---|
| `gameObject.SetActive(false)` | ✅ | ❌ muere |
| `_overviewCam.enabled = false` | ✅ | ✅ sigue |

**Arrastrar un GameObject a un campo de componente:** si el campo es de tipo `PlayerController` o `CinemachineBrain`, al arrastrar el GameObject Unity encuentra solo ese componente.

**¿Por qué no un temporizador en Update?** Revisaría una condición cada frame, para siempre, por algo que pasa una vez. La Coroutine **se lee como una receta**.

**En el proyecto:** `Assets/Scripts/IntroCamera.cs` (en `CM_Overview`) y `PlayerController.SetControlEnabled()`. El bloqueo de control está explicado en [05 · Code Organization](05-code-organization.md).

---

[← 03 · Physics & 2.5D Scene](03-physics-and-2.5d-scene.md) · [Siguiente: 05 · Code Organization →](05-code-organization.md)
