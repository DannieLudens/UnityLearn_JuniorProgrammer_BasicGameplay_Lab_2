# 08 · Input Actions Asset

[← Índice](README.md)

Pasar de **acciones dentro de cada script** a **un asset central** (`InputSystem_Actions`), como se hace en el pathway desde las unidades 3 y 4.

---

## Antes vs ahora

**La idea:** antes, cada script **era dueño** de sus acciones: los bindings (Space, A/D...) se guardaban **en la escena**, dentro del componente. Ahora todas las acciones viven en **un solo asset**, y los scripts **le piden** la acción por su nombre.

```
BEFORE (inline)                          NOW (project-wide asset)

PlayerController                         InputSystem_Actions.inputactions
 ├─ moveAction  [A/D, arrows]             └─ Player (action map)
 └─ jumpAction  [Space]                       ├─ Move    [WASD, arrows, left stick]
                                              ├─ Jump    [Space, gamepad South]
Another script? → its own copy                ├─ Sprint, Attack, Interact...
                                              
                                         PlayerController ──"FindAction("Move")"──► asset
                                         PauseMenu        ──"FindAction("Jump")"──► same asset
```

**Por qué es mejor a largo plazo:**
1. **Un solo lugar** para cambiar controles: si Jump pasa de Space a W, se cambia en el asset y todos los scripts se enteran.
2. **El gamepad ya viene configurado** en el asset del template.
3. **Action Maps** (`Player`, `UI`): más adelante se puede desactivar todo el mapa `Player` al pausar, de un solo golpe.
4. **Rebinding** (que el jugador cambie sus teclas en un menú) solo es posible con un asset central.

---

## Project-wide actions

**La idea:** Unity 6 tiene un asset de acciones **para todo el proyecto**. Se asigna en **Edit → Project Settings → Input System Package → Project-wide Actions**. En este proyecto ya está asignado `Assets/Input/InputSystem_Actions`.

Desde código se accede con `InputSystem.actions`. Unity **lo activa solo** al dar Play: ya no hace falta `Enable()`.

---

## El código

```csharp
private InputAction _moveAction;                         // From the project-wide actions asset
private InputAction _jumpAction;                         // From the project-wide actions asset

// Runs once before the first frame: setup
private void Start()
{
    _playerRb = GetComponent<Rigidbody>();               // Cache once, not every frame
    _moveAction = InputSystem.actions.FindAction("Player/Move"); // Look up by "Map/Action" name
    _jumpAction = InputSystem.actions.FindAction("Player/Jump"); // Already enabled by Unity
}
```

**Lo que NO cambia:** `ReadValue<Vector2>()`, `WasPressedThisFrame()` e `IsPressed()` funcionan igual, porque **sigue siendo una `InputAction`**. Solo cambia **de dónde viene**.

**Por qué ahora llevan `_`:** ya no salen en el Inspector (no son `[SerializeField]`), así que según la convención del proyecto son privados con `_` (ver [01 · Movement](01-movement.md#convenciones-de-nombres)). Renombrar con **Ctrl + R, R** en Rider.

**Por qué `"Player/Move"` y no solo `"Move"`:** `"Move"` busca en **todos** los mapas y devuelve el primero que encuentre. `"Player/Move"` dice exactamente cuál.

**Por qué buscarla una vez en `Start`:** `FindAction` recorre el asset comparando textos. Hacerlo en cada `Update` sería trabajo repetido; se guarda la referencia una vez (igual que `_playerRb`).

---

## ⚠️ Trampas

- **Un nombre mal escrito no da error al compilar.** `FindAction("Player/Jmup")` devuelve `null`, y el error aparece **después**, en `Update`: `NullReferenceException`. Si ves ese error en la línea del input, revisa el texto.
- **El asset es compartido.** Cambiar un binding ahí afecta **a todos** los scripts que usan esa acción.
- **Guardar el asset:** en la ventana del asset (doble clic), si **Auto-Save** está apagado hay que pulsar **Save Asset**, o los cambios no se aplican.
- **Move incluye W/S y arriba/abajo:** no afecta, porque el código solo usa `_moveInput.x`.

---

## Caso real: quitar `[SerializeField]` al cambiar de enfoque

**Qué pasó:** renombré `moveAction` → `_moveAction` pero dejé el `[SerializeField]`. El Inspector seguía mostrando los campos para poner bindings, **vacíos**, y aun así el juego funcionaba.

**Por qué funcionaba:** el orden de los hechos al dar Play:

```
1. Unity loads the scene    → _moveAction = empty action saved in the scene (no bindings)
2. Start() runs             → _moveAction = FindAction("Player/Move")  ← overwrites it
3. Update() reads input     → uses the asset's action (WASD, arrows, stick)
```

El campo del Inspector se **sobrescribe** en `Start`, así que lo que tenga no importa. Pero confunde: parece configurable y no lo es.

**La solución:** quitar `[SerializeField]`. Si el valor sale de código, **no debe estar en el Inspector**.

```csharp
private InputAction _moveAction;                         // From the project-wide actions asset
```

> **Regla:** `[SerializeField]` = "este valor lo pongo yo en el Inspector". Si lo asigna el código (`GetComponent`, `FindAction`), es un `private` normal.

---

## Renombrar un campo serializado: `[FormerlySerializedAs]`

**La idea:** Unity guarda los valores del Inspector en la escena **por el nombre del campo**:

```yaml
PlayerController:
  speed: 5
  moveAction:          # ← saved under this name
    m_SingletonActionBindings: [A, D, arrows...]
```

Si renombras `moveAction` → `_moveAction`, Unity busca `_moveAction`, no lo encuentra y crea uno **vacío**. **El valor se pierde** (eso pasó con los bindings).

Rider lo detecta y ofrece agregar:

```csharp
using UnityEngine.Serialization;                         // Needed for FormerlySerializedAs

[FormerlySerializedAs("moveAction")]                     // Old name: keep the saved value
[SerializeField] private InputAction _moveAction;        // New name
```

Le dice a Unity: "si en la escena encuentras `moveAction`, cárgalo aquí".

**Cuándo usarlo:** al renombrar un campo que **sigue** siendo serializado y cuyo valor quieres conservar (por ejemplo, renombrar `speed` → `walkSpeed` sin perder el 5 de todas las escenas y prefabs). Después de abrir y guardar todas las escenas, se puede quitar.

**Cuándo NO:** cuando el campo **deja** de ser serializado (este caso). Ahí la sugerencia no aplica: se quita el `[SerializeField]` y listo.

**⚠️ Trampa:** renombrar un campo serializado **sin** Rider (o ignorando su sugerencia) borra en silencio el valor configurado en todos los objetos que usan el script.

---

## Las otras formas (para cuando el proyecto crezca)

| Forma | Cómo se usa | Ideal para |
|---|---|---|
| `InputAction` inline (lo de antes) | Bindings en cada componente | Prototipos, scripts aislados |
| **Project-wide + `FindAction`** ✅ | Buscar la acción por nombre | Aprender, proyectos pequeños/medianos (**la del pathway**) |
| `InputActionReference` | Arrastrar la acción del asset a un campo del Inspector | Evitar los textos (no hay typos) |
| Clase C# generada | *Generate C# Class* → `_controls.Player.Jump` | Proyectos grandes: autocompletado y errores al compilar |
| Componente `PlayerInput` | Unity llama métodos tuyos (`OnJump`) | Multijugador local (varios mandos) |

### `FindAction` vs clase C# generada

Al marcar **Generate C# Class** en el Inspector del asset (y **Apply**), Unity escribe un archivo `InputSystem_Actions.cs` con una clase que tiene **una propiedad por cada mapa y acción**. Se regenera sola cuando cambias el asset.

```csharp
private InputSystem_Actions _controls;                   // Generated class (one copy of all actions)

// Create our own copy of the actions
private void Awake()
{
    _controls = new InputSystem_Actions();               // New instance, NOT the project-wide one
}

// Turn the Player map on with the component
private void OnEnable()
{
    _controls.Player.Enable();                           // Generated copies start disabled
}

// Turn it off when the component/object is disabled
private void OnDisable()
{
    _controls.Player.Disable();                          // Stop listening
}

// Usage: no strings, autocomplete
// _controls.Player.Jump.WasPressedThisFrame()
```

| | `FindAction("Player/Jump")` | Clase generada `_controls.Player.Jump` |
|---|---|---|
| Nombre mal escrito | `null` → error **en Play** | Error **al compilar** (rojo en Rider) |
| Autocompletado | ❌ | ✅ |
| Renombrar una acción en el asset | Rompe en silencio | El compilador te muestra cada uso |
| ¿Qué instancia usa? | **La de proyecto** (compartida) | **Una copia nueva** por cada `new` |
| `Enable()` / `Disable()` | Automático | Manual (`OnEnable` / `OnDisable`) |
| Archivos extra | Ninguno | `InputSystem_Actions.cs` |

**⚠️ Trampa de la clase generada:** cada `new InputSystem_Actions()` es una **copia independiente**. Si el jugador cambia sus teclas en un menú (rebinding) sobre otra copia, tu `PlayerController` no se entera. Hay que compartir una sola instancia.

**Cuándo pasarse a la clase generada:** cuando varios scripts lean input y los errores de texto empiecen a costar tiempo. Mientras tanto, `FindAction` es más simple y es lo que usa el pathway.

**En el proyecto:** `PlayerController.cs` → campos `_moveAction` / `_jumpAction` y `Start()`. El asset está en `Assets/Input/InputSystem_Actions.inputactions`.

---

[← 07 · Git & Workflow](07-git-and-workflow.md) · [Índice](README.md)
