# 05 · Code Organization

[← Índice](README.md)

---

## Extract Method (extraer método)

**La idea:** mover un bloque de código a su propio método con un nombre que diga **qué hace**. Es una forma de **refactorizar**: reorganizar el código **sin cambiar su comportamiento**.

```csharp
// Physics step: apply movement here
private void FixedUpdate()
{
    MoveHorizontal();                                    // velocity.x from input
    HandleJump();                                        // Jump if requested and grounded
    ApplyJumpGravity();                                  // Extra gravity for better jump feel
}
```

**Por qué:**
1. **El método se vuelve un índice**: dice **qué** pasa sin obligarte a leer **cómo**.
2. **El nombre documenta la intención**: es un comentario que el compilador te obliga a mantener.
3. **Facilita el futuro**, por ejemplo `if (!_isStunned) MoveHorizontal();`.

**¿Cuesta rendimiento?** No. Una llamada a un método cuesta prácticamente nada, y el compilador puede incluso "pegar" su código donde se llama (*inlining*).

**Regla de oro del refactor:** **el juego debe sentirse exactamente igual antes y después.** Hay que probarlo para confirmarlo.

### Un método, una responsabilidad

**La prueba del "y":** si para describir un método necesitas decir "**y**" ("mueve **y** salta **y** aplica gravedad"), probablemente son varios métodos.

**⚠️ Trampa:** extraer **todo** a un solo `MovePlayer()` solo cambia el código de lugar: el método sigue igual de largo y su nombre miente, porque también salta.

### ¿Cuándo NO extraer?

- **Métodos de 1 línea que no aportan un nombre mejor**, como `ClearJump() { _jumpRequested = false; }`.
- **Partir algo que solo tiene sentido junto**, como las 4 líneas de "leer, cambiar X, aplicar" de `MoveHorizontal`.

**Buen ejemplo:** `IsGrounded()` es de una sola línea, pero **sí** aporta: `if (_jumpRequested && IsGrounded())` se lee casi en español.

---

## Nombres que no mienten

**La idea:** **un nombre equivocado es peor que no tener nombre**, porque te engaña.

**Caso real del proyecto:** en un refactor, los contenidos de `HandleJump` y `ApplyJumpGravity` quedaron **intercambiados**. El juego funcionaba igual, porque solo los nombres estaban mal. Pero buscar "el salto" llevaba a la gravedad.

**Cómo renombrar bien en Rider:** cursor sobre el nombre → **Ctrl + R, R** (o Shift + F6) → **Refactor → Rename**. Cambia la declaración **y todas las llamadas** a la vez. Para intercambiar dos nombres: `A → Temp`, `B → A`, `Temp → B`.

**Formato:** **Ctrl + Alt + L** (Reformat Code) arregla espacios e indentación.

---

## Encapsulamiento: métodos públicos simples y seguros

**La idea:** otros scripts **no tocan las variables** del jugador: **le piden** algo a través de un método público. El dueño decide **cómo** hacerlo.

```
IntroCamera  ──"SetControlEnabled(false)"──►  PlayerController
                                                (decides HOW)
```

```csharp
private bool _controlEnabled = true;                     // False during intro/cutscenes

// Lets other scripts turn player input on or off
public void SetControlEnabled(bool isEnabled)
{
    _controlEnabled = isEnabled;                         // Store the new state
}
```

**Por qué solo cambia un `bool`:** Unity **no garantiza el orden** de los `Start` entre scripts. Si la intro llama a este método **antes** de que corra el `Start` del jugador, igual funciona, porque no usa nada que `Start` prepare (como `_playerRb`).

**⚠️ Trampa:** si el método hiciera `_playerRb.linearVelocity = Vector3.zero`, fallaría con `NullReferenceException` cuando lo llamen temprano.

> **Regla:** los métodos públicos que otros pueden llamar temprano deben **cambiar un estado** y dejar que el dueño actúe en su propio momento (aquí, en su `Update`).

---

## Guard clauses (cláusulas de guarda)

**La idea:** revisar **al principio** si el método debe seguir; si no, **salir temprano** con `return`. Evita meter todo el método dentro de un `if` gigante.

```csharp
// Every frame: read input here
private void Update()
{
    if (!_controlEnabled)                                // Control off: ignore input
    {
        _moveInput = Vector2.zero;                       // Stop walking (no stale input)
        return;                                          // Skip the rest of Update
    }

    _moveInput = moveAction.ReadValue<Vector2>();        // Store input for FixedUpdate
    // ...
}
```

**⚠️ Trampa: input "rancio" (stale).** Si solo haces `return`, `_moveInput` se queda con su **último valor**: si ibas a la derecha, **el jugador sigue caminando solo**. Por eso se pone en cero antes de salir.

**Nota:** bloquear el **input** no congela la **física**. `FixedUpdate` sigue corriendo, así que si el jugador está en el aire, igual cae.

**En el proyecto:** `PlayerController.cs` → `Update()` y `SetControlEnabled()`.

---

## Orden de los métodos en el archivo

Convención usada en el proyecto:

```
1. Unity methods        Start, Update, FixedUpdate, LateUpdate
2. Public methods       SetControlEnabled
3. Private methods      MoveHorizontal, HandleJump, ApplyJumpGravity, IsGrounded
4. Editor tools         OnDrawGizmos
```

## Comentarios

- **Al final de la línea** para instrucciones y campos (cortos, en inglés, alineados).
- **Encima del método** para describir qué hace el método.
- Explicar el **por qué**, no repetir el **qué** (`// Cache once, not every frame` y no `// Get the Rigidbody`).

---

## Advertencias del compilador

```
Assets/Scripts/PlayerController.cs(6,42): warning CS0414: The field 'PlayerController.speed' is assigned but its value is never used
```

| Parte | Significado |
|---|---|
| `(6,42)` | Línea 6, columna 42 |
| `warning` | Amarilla: el juego compila igual (`error` sería roja) |
| `CS` | Viene del compilador de C#, no de Unity |

**Doble clic** en el mensaje de la consola abre el archivo en esa línea.

| Código | Significado |
|---|---|
| **CS0414** | Campo privado **asignado pero nunca leído** |
| **CS0169** | Campo **nunca usado** (ni asignado ni leído) |
| **CS0649** | Campo **leído pero nunca asignado** (siempre vale el default) |
| **CS0219** | Variable **local** asignada con una **constante** y nunca leída |

**Por qué es común en Unity:** el compilador **no sabe que Unity existe**. Para él, que `[SerializeField]` muestre la variable en el Inspector no cuenta como "leerla".

**Soluciones (de mejor a peor):** usarla → borrarla → silenciarla con `#pragma warning disable CS0414` (solo si es intencional y temporal).

**Variables locales sin usar:** el compilador a menudo **no avisa** (si el valor no es constante), pero **Rider las muestra en gris**.

---

## Debug.DrawRay vs OnDrawGizmos

```csharp
// Editor-only drawing, also visible outside Play mode
private void OnDrawGizmos()
{
    Gizmos.color = Color.red;                            // Ray color in Scene view
    Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance); // Same ray as IsGrounded()
}
```

| | `Debug.DrawRay` en FixedUpdate | `OnDrawGizmos` |
|---|---|---|
| ¿Cuándo se ve? | Solo en Play Mode | **También sin dar Play** |
| ¿Existe en el build? | La llamada sí (sin efecto) | No: Unity solo lo llama en el Editor |
| Ubicación | Mezclado con la lógica | En su propio método |

**Costo:** `DrawRay` es prácticamente gratis. **`Debug.Log` sí es caro**: escribe a un archivo incluso en el build y genera basura en memoria. Hay que evitarlo dentro de `Update`/`FixedUpdate`.

**⚠️ Trampa:** si no ves los gizmos, revisa que el botón **Gizmos** de la vista Scene esté activado.

---

[← 04 · Camera](04-camera.md) · [Siguiente: 06 · Project Organization →](06-project-organization.md)
