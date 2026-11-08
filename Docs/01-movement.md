# 01 · Movement

[← Índice](README.md)

---

## Update vs FixedUpdate: quién lee y quién mueve

**La idea:** `Update` corre una vez por frame (60, 144... por segundo, según la PC). `FixedUpdate` corre al ritmo fijo de la física (50 por segundo por defecto). **Update lee el input; FixedUpdate mueve el Rigidbody.**

```csharp
// Every frame: read input here
private void Update()
{
    _moveInput = moveAction.ReadValue<Vector2>();        // Store input for FixedUpdate
}

// Physics step: apply movement here
private void FixedUpdate()
{
    MoveHorizontal();                                    // Uses _moveInput
}
```

**Por qué:**
- Leer el input en `Update` asegura que no se pierda ninguna pulsación, porque corre en cada frame.
- Mover en `FixedUpdate` mantiene la física estable: siempre avanza en pasos del mismo tamaño.
- La variable `_moveInput` es el puente entre los dos métodos.

**⚠️ Trampa:** mover un Rigidbody en `Update` hace que el movimiento dependa de los FPS: el juego se siente distinto en cada computadora.

**En el proyecto:** `Assets/Scripts/PlayerController.cs` → `Update()` y `FixedUpdate()`.

---

## AddForce vs linearVelocity

**La idea:** `AddForce` **empuja**: cambia la velocidad poco a poco, como acelerar. `linearVelocity` **dice la velocidad exacta**.

Con `AddForce` (`speed = 5`, masa 1, un FixedUpdate cada 0.02 s):

| Tiempo con la tecla presionada | Velocidad |
|---|---|
| 0.02 s | 0.1 m/s |
| 1 s | 5 m/s |
| 2 s | 10 m/s |
| 4 s | 20 m/s… **sigue subiendo** |

- **No hay velocidad máxima**: mientras mantengas la tecla, sigue acelerando.
- **Al soltar, sigue deslizando** por inercia. Se siente como una "bola sobre hielo".
- `speed` **no es una velocidad**: es la intensidad del empujón.

Con `linearVelocity`:
- Al presionar, va a `speed` m/s **al instante**; al soltar, se detiene.
- `speed = 5` significa de verdad **5 metros por segundo**, así que duplicarlo duplica la velocidad. Eso será útil para los power-ups.

| | `AddForce` | `linearVelocity` |
|---|---|---|
| Sensación | Inercia, hielo | Control preciso |
| Velocidad máxima | No (sin drag) | Sí, `speed` |
| Qué significa `speed` | Empujón | m/s reales |
| Ideal para | Bola rodando | Caminar en un plataformero |

> **Nota de versión:** en Unity 6 se llama `linearVelocity`. En tutoriales viejos aparece como `velocity`, que en Unity 6 está obsoleta.

---

## El patrón "leer → cambiar un eje → aplicar"

**La idea:** para caminar, solo debo cambiar **X**. Y le pertenece a la gravedad y al salto.

```csharp
// Set X velocity from input, keep Y untouched
private void MoveHorizontal()
{
    float horizontalInput = _moveInput.x;                // -1 left, 0 idle, 1 right
    Vector3 velocity = _playerRb.linearVelocity;         // Read current velocity (keeps gravity on Y)
    velocity.x = horizontalInput * speed;                // Change only the X axis
    _playerRb.linearVelocity = velocity;                 // Apply the new velocity
}
```

**Por qué:** cada sistema controla su propio eje: el input controla X, y la gravedad y el salto controlan Y. Así no se pisan entre ellos.

**⚠️ Trampa (casi todos caen):**

```csharp
_playerRb.linearVelocity = new Vector3(_moveInput.x * speed, 0f, 0f); // ❌
```

Esto pone **Y = 0 en cada paso de la física**. Consecuencias:
- La gravedad se borra 50 veces por segundo, así que el jugador **flota o cae lentísimo**.
- La velocidad del salto se borra en el siguiente paso, así que **el salto no funciona**.

**En el proyecto:** `PlayerController.cs` → `MoveHorizontal()`.

---

## `[SerializeField] private`: qué significa cada palabra

**La idea:**
- **`private`** dice **quién puede tocar** la variable: solo el código de esta clase.
- **`[SerializeField]`** dice que **Unity la muestra en el Inspector**.

```csharp
[SerializeField] private float speed = 5f;               // Walk speed in m/s (editable in Inspector)
```

**Por qué no es un obstáculo para los power-ups:** `private` **no** significa "de solo lectura". La propia clase puede cambiar `speed` cuando quiera; lo que no puede hacer es **otro script** escribir directamente `player.speed = 10`. Para eso, la clase ofrece un **método público** (por ejemplo `ApplySpeedBoost(...)`), y así es el jugador quien decide **cómo** cambiar su velocidad. Ventajas:

- Puede limitar la velocidad máxima.
- Puede guardar la velocidad base y restaurarla al terminar el boost.
- Si el boost luego activa partículas o sonido, se cambia en un solo lugar.

Esto se llama **encapsulamiento** (ver [05 · Code Organization](05-code-organization.md)).

**⚠️ Trampa:** usar `public float speed` "para que sea fácil". Funciona, pero cualquier script puede romper el estado del jugador sin que te enteres.

---

## Convenciones de nombres

| Tipo | Convención | Ejemplo |
|---|---|---|
| Campo privado | `_camelCase` | `_playerRb`, `_moveInput` |
| Campo serializado | `camelCase` (Unity lo muestra como "Move Action") | `moveAction`, `jumpForce` |
| Método | `PascalCase` | `MoveHorizontal()`, `IsGrounded()` |
| Variable local | `camelCase` | `horizontalInput`, `velocity` |

**Por qué:** no existe una convención "correcta universal". Lo importante es **elegir una regla y aplicarla siempre**. En este proyecto, el `_` marca los privados que **no** salen en el Inspector.

---

[Siguiente: 02 · Jump →](02-jump.md)
