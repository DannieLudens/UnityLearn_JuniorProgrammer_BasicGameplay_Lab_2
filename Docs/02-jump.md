# 02 · Jump

[← Índice](README.md)

Un salto necesita **cuatro piezas**: el input (qué tecla), la detección (cuándo), el impulso (cómo sube) y el chequeo de suelo (si puede saltar).

---

## Pieza 1 · El input: Button vs Value

**La idea:** el salto tiene su propia `InputAction`, de tipo **Button**.

```csharp
[SerializeField] private InputAction jumpAction;         // Jump input binding (set in Inspector)
[SerializeField] private float jumpForce = 7f;           // Impulse (N·s); with mass 1 = 7 m/s upward
```

```csharp
jumpAction.Enable();                                     // Input actions start disabled
```

**En el Inspector:** ⚙️ → **Action Type = Button** → **+ Add Binding** → `Space [Keyboard]`.

> **Actualización:** después el proyecto pasó al asset central `InputSystem_Actions` (la acción `Player/Jump` ya es Button). Ver [08 · Input Actions Asset](08-input-actions-asset.md). El concepto Button vs Value sigue igual.

**Por qué:**
- **Value** da un número continuo, como el movimiento: de -1 a 1.
- **Button** tiene dos estados, presionado o no, que es justo lo que es un salto.

---

## Pieza 2 · Evento vs estado (concepto clave)

| Método | Qué pregunta | Tipo | ¿Seguro en `FixedUpdate`? |
|---|---|---|---|
| `WasPressedThisFrame()` | "¿Se presionó **en este frame exacto**?" | **Evento**: dura 1 frame | ❌ Se puede perder |
| `IsPressed()` | "¿Está presionada **ahora mismo**?" | **Estado**: dura mientras la mantengas | ✅ |

**Por qué se pierde un evento:** a 144 FPS hay varios `Update` por cada `FixedUpdate`:

```
Update:       |  |  |  |  |  |  |  |  |     (144 per second)
FixedUpdate:  |        |        |           (50 per second)
                  ↑
           key pressed here
```

Cuando llega el siguiente `FixedUpdate`, `WasPressedThisFrame()` ya volvió a `false`. **El salto se pierde**, y el jugador siente que "a veces no salta".

**Regla general:** los **eventos** necesitan un intermediario (la "nota" de abajo); los **estados** se pueden leer directamente donde los necesites.

---

## El patrón de la "nota": `_jumpRequested`

**La idea:** `Update` **escribe** una nota ("quiero saltar"); `FixedUpdate` **la lee, salta y la borra**.

```csharp
private bool _jumpRequested;                             // Set in Update, consumed in FixedUpdate
```

```csharp
// In Update
if (jumpAction.WasPressedThisFrame())                    // Key went down this frame, not held
{
    _jumpRequested = true;                               // Set a flag to jump in FixedUpdate
}
```

```csharp
// Jump once if requested and grounded, then clear the request
private void HandleJump()
{
    if (_jumpRequested && IsGrounded())                  // Jump requested in Update?
    {
        _playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Instant upward force
    }
    _jumpRequested = false;                              // Always clear: midair press is discarded
}
```

**⚠️ Trampa 1: escribirlo en una línea.**

```csharp
_jumpRequested = jumpAction.WasPressedThisFrame();       // ❌ BUG
```

En el siguiente `Update`, que llega **antes** del `FixedUpdate`, esto vale `false` y **borra la nota**. Regla: **Update solo pone la nota en `true`. Solo FixedUpdate la pone en `false`.**

**⚠️ Trampa 2: borrar la nota dentro del `if`.** Si presionas en el aire, la nota nunca se borra, y al aterrizar **saltas solo**. Por eso `_jumpRequested = false` va **fuera** del `if`.

**En el proyecto:** `PlayerController.cs` → `Update()` y `HandleJump()`.

---

## Pieza 3 · El impulso

```csharp
_playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Instant upward force
```

- `Vector3.up` es `(0, 1, 0)`.
- **Impulse** aplica toda la fuerza **de una vez**, en un instante.
- **No choca con `MoveHorizontal`**, porque el movimiento solo toca X (ver [01 · Movement](01-movement.md)).

**¿En qué unidad está `jumpForce`?** Es un impulso (N·s): **cambia la velocidad en `jumpForce / masa`**.
- Masa 1 y `jumpForce = 7`: el jugador sale hacia arriba a **7 m/s**.
- Altura máxima = v² / (2·g) = 49 / 19.62 ≈ **2.5 m**.

**⚠️ Trampa:** el salto **depende de la masa**. Si cambias la masa del Rigidbody a 2, saltas a la mitad de altura. Si quieres que la masa no importe, usa `ForceMode.VelocityChange`.

---

## Los cuatro ForceMode

| ForceMode | ¿Instantáneo o continuo? | ¿Depende de la masa? | Uso típico |
|---|---|---|---|
| `Force` | Continuo | Sí | Empujar (la bola del tutorial) |
| `Impulse` | Instantáneo | Sí | **Mi salto** |
| `VelocityChange` | Instantáneo | No | Salto que ignora la masa |
| `Acceleration` | Continuo | No | **Gravedad extra** |

---

## Pieza 4 · ¿Estoy en el suelo? Raycast hacia abajo

**La idea:** lanzar un **rayo invisible** desde el centro del jugador hacia abajo. Si choca con suelo a poca distancia, estoy parado.

```
      🧍  ← player (pivot at center)
      |
      |   ← ray, length groundCheckDistance
      ↓
  ▬▬▬▬▬▬▬▬  ← platform (layer "Ground")
```

```csharp
[SerializeField] private float groundCheckDistance = 1.1f; // Ray length: half collider height + 0.1
[SerializeField] private LayerMask groundLayer;          // Only these layers count as ground
```

```csharp
// True if standing on ground, false if in the air
private bool IsGrounded()
{
    return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer); // From here, down, this far, ground layer only
}
```

Se lee así: **"desde aquí, hacia abajo, hasta esta distancia, solo contra estas capas"**.

**Cómo calcular la distancia:** **mitad de la altura del collider + un margen pequeño**. Mi cápsula mide 1.4 de alto, así que 0.7 + margen = **0.74 a 0.8**.
- **Muy corta:** el rayo nunca sale del jugador, así que **nunca puedes saltar**.
- **Muy larga:** detecta el suelo estando en el aire, así que **saltas justo antes de aterrizar**.

**Por qué raycast y no `OnCollisionEnter`:** con colisiones, tocar el **costado** de una plataforma también cuenta como suelo, y eso da un salto de pared accidental. El rayo solo mira hacia abajo.

**⚠️ Trampa:** si el **Ground Layer** queda en **Nothing** (el valor por defecto), el rayo no ve nada y **nunca saltas**. Es lo primero que hay que revisar.

---

## Tag vs Layer

| Pregunta | Usa |
|---|---|
| ¿Necesita **el motor** filtrar esto (raycasts, colisiones, cámaras, luces)? | **Layer** |
| ¿Necesita **mi script** reconocer qué es esto cuando lo toca? | **Tag** |

| | Tag | Layer |
|---|---|---|
| ¿Quién lo usa? | Mis scripts | El motor (física, render) |
| ¿Cuántos? | Ilimitados | Máximo 32 |
| Ejemplos | `Player`, `PowerUp`, `Enemy` | `Ground` |

**Por qué el suelo es una Layer:** `Physics.Raycast` **no sabe filtrar por tags**. Con tags tendrías que lanzar el rayo contra todo y luego preguntar qué golpeó, y el rayo se detiene en **lo primero** que toca:

```
      🧍
      |
      ● ← coin (trigger): ray stops HERE
      |
  ▬▬▬▬▬▬▬▬  ← ground: never reached
```

Con un `LayerMask`, el motor **ignora todo lo que no sea Ground antes de lanzar el rayo**, así que la moneda es invisible para él.

Un objeto puede tener **los dos**: por ejemplo, una plataforma móvil con Layer `Ground` y Tag `MovingPlatform`.

**Consejo con tags:**

```csharp
other.CompareTag("PowerUp")                              // ✅ fast, no garbage, errors if tag doesn't exist
other.tag == "PowerUp"                                   // ❌ creates a string each call, fails silently
```

---

## Gravedad asimétrica: caer más rápido de lo que subes

**La idea:** con gravedad realista (9.81), mi salto dura **≈1.4 s** en el aire (2·v/g = 14/9.81) y se siente como saltar en la Luna. Los plataformeros **suben normal y caen más rápido**:

```
       ⬆ rising: normal gravity  → jump feels high
      /‾‾\
     /    \
    /      |  ⬇ falling: gravity x2.5 → landing feels solid
   🧍       🧍
```

```csharp
[SerializeField] private float fallMultiplier = 2.5f;    // Gravity multiplier while falling
[SerializeField] private float lowJumpMultiplier = 2f;   // Gravity multiplier when jump released early
```

```csharp
// Faster fall, and shorter jump if the key is released early
private void ApplyJumpGravity()
{
    if (_playerRb.linearVelocity.y < 0f)                 // Moving down = falling
    {
        _playerRb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration); // Extra gravity while falling
    }
    else if (_playerRb.linearVelocity.y > 0f && !jumpAction.IsPressed()) // Rising but jump released
    {
        _playerRb.AddForce(Physics.gravity * (lowJumpMultiplier - 1f), ForceMode.Acceleration); // Cut the jump short
    }
}
```

**Por qué el `- 1f`:** Unity **ya aplica 1x** de gravedad (checkbox *Use Gravity*). Para un total de 2.5x solo hay que **sumar 1.5x**.

| fallMultiplier | Unity aplica | Yo sumo | Total |
|---|---|---|---|
| 1 | 1x | 0x | 1x (sin cambio) |
| 2.5 | 1x | 1.5x | **2.5x** |

**Por qué `ForceMode.Acceleration`:** la gravedad real acelera igual a todo, sin importar la masa. `Acceleration` también ignora la masa.

**Salto variable:** si suelto la tecla mientras subo, aplico gravedad extra, y el salto se corta. Así hay saltos cortos y largos. Aquí uso `IsPressed()` **dentro de FixedUpdate**, y es seguro porque es un **estado**, no un evento.

**Calibrar:** los campos son `[SerializeField]`, así que se pueden ajustar en el Inspector **durante Play Mode**. ⚠️ Esos cambios **se pierden al salir**: hay que anotar los valores.

**Alternativa simple (peor):** subir la gravedad global en *Project Settings → Physics*. Afecta a **todo** (objetos, enemigos) y no da la asimetría subida/caída.

**En el proyecto:** `PlayerController.cs` → `ApplyJumpGravity()`.

---

[← 01 · Movement](01-movement.md) · [Siguiente: 03 · Physics & 2.5D Scene →](03-physics-and-2.5d-scene.md)
