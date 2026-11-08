# 03 · Physics & 2.5D Scene

[← Índice](README.md)

---

## Los ejes en un side-view

```
        Y (up / jump)
        ↑
        |
        |    🧍 player
        |
        +------------→ X (walk)
       /
      /
     Z  (depth: the player must NOT move here)
```

- **X:** caminar (input).
- **Y:** saltar y caer (salto + gravedad).
- **Z:** nada. Si el jugador se mueve en Z, "se sale del plano".

**⚠️ Trampa:** el jugador está fijo en **Z = 0**, así que todo lo jugable (plataformas, enemigos, pickups) **también debe estar en Z = 0**. Si no, está "en otro carril" y nunca lo vas a tocar. El fondo decorativo sí puede ir en otras Z.

---

## Constraints: intención vs garantía

**La idea:** aunque mi código nunca empuje en Z, **el motor de física sí puede** hacerlo: al chocar con una esquina, caer en un borde o recibir un golpe en diagonal.

| Capa | Qué es | Garantiza |
|---|---|---|
| Código `velocity.x` (solo X) | Mi **intención** | Que **yo** no lo muevo en Z |
| Constraint *Freeze Position Z* | Una **regla de la física** | Que **nada** lo mueve en Z |

**Configuración:** *Rigidbody → Constraints*
- ✅ Freeze Position **Z**
- ✅ Freeze Rotation **X, Y, Z**

**Por qué congelar la rotación:** la fricción hace que el personaje ruede o se vuelque. Un sprite 2D que rota se ve de canto (desaparece, porque es plano) o patas arriba.

---

## Rigidbody (3D), no Rigidbody2D

**La idea:** Unity tiene **dos motores de física separados**, 3D y 2D, que **no chocan entre sí**.

**Por qué:** el escenario es 3D, con colliders 3D. Aunque el personaje se vea como un sprite, debe usar `Rigidbody` + un collider 3D (por ejemplo `CapsuleCollider`).

**⚠️ Trampa:** con `Rigidbody2D` + `BoxCollider2D`, el jugador **atraviesa el suelo** 3D.

---

## Interpolate: adiós al jitter

**La idea:** la física corre a 50 Hz y la pantalla a 60–144 Hz. Sin interpolación, el objeto solo cambia de posición en los pasos de física, y se ve con "saltitos".

**Configuración:** *Rigidbody → Interpolate → Interpolate*. Unity dibuja posiciones intermedias entre pasos de física, y el movimiento se ve suave. Se nota sobre todo cuando la cámara sigue al jugador.

---

## Physics Material: no quedarse pegado a las paredes

**El problema:** con `linearVelocity`, si saltas contra una pared **manteniendo la dirección hacia ella**, la fricción te frena y quedas **pegado en el aire**.

**La solución:** un material de física sin fricción en el collider del jugador.

1. Clic derecho en `Assets/Physics/` → **Create → Physics Material** → `PM_Player_NoFriction`
2. **Dynamic Friction = 0**, **Static Friction = 0**
3. **Friction Combine = Minimum**
4. Arrastrarlo al campo **Material** del collider del jugador.

**Por qué Friction Combine = Minimum:** al tocarse dos objetos, Unity **combina** sus fricciones. Con *Average* (el valor por defecto) promediaría mi 0 con la fricción de la pared, y seguiría pegándome un poco. Con *Minimum* usa el valor más bajo, o sea 0, sin importar el material de la pared.

**¿No me resbalo en el suelo?** No: `MoveHorizontal` asigna `velocity.x` directamente. Al soltar la tecla, X vale 0 al instante. **Mi código frena, no la fricción.**

**En el proyecto:** `Assets/Physics/PM_Player_NoFriction`, asignado al CapsuleCollider del Player.

---

## El valor del Inspector gana sobre el del código

```csharp
[SerializeField] private float groundCheckDistance = 1.1f;
```

En la escena, el Inspector dice **0.74**. **Gana el Inspector.**

**Por qué:** el `= 1.1f` del código es solo el **valor inicial** cuando agregas el componente por primera vez. Después, Unity **guarda en la escena** lo que pongas en el Inspector.

**⚠️ Trampa:** cambiar el número en el código **no cambia nada** en una escena que ya existe. Hay que cambiarlo en el Inspector, o usar *Reset* en el menú ⋮ del componente (que resetea **todos** sus valores).

---

[← 02 · Jump](02-jump.md) · [Siguiente: 04 · Camera →](04-camera.md)
