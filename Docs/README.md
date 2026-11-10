# Study Guide — 2.5D Side-View Platformer

Guía de estudio de lo aprendido construyendo mi plataformero 2.5D (Unity Learn – Junior Programmer). Las explicaciones están en español; el código, los comentarios y los nombres de archivo, en inglés.

**Proyecto:** Unity 6 (6000.3) · URP · Input System · Cinemachine 3

## Cómo está organizada cada sección

1. **La idea**: el concepto en pocas líneas.
2. **El código**: el fragmento como está en el proyecto.
3. **Por qué**: la razón detrás de la decisión.
4. **⚠️ Trampa**: el error común y cómo se ve cuando ocurre.
5. **En el proyecto**: dónde vive eso en mis archivos.

## Índice (orden de lectura sugerido)

| # | Tema | Contenido |
|---|---|---|
| 01 | [Movement](01-movement.md) | `Update` vs `FixedUpdate`, `AddForce` vs `linearVelocity`, `[SerializeField] private`, nombres |
| 02 | [Jump](02-jump.md) | Input de salto, evento vs estado, raycast al suelo, Tag vs Layer, `ForceMode`, gravedad asimétrica |
| 03 | [Physics & 2.5D Scene](03-physics-and-2.5d-scene.md) | Ejes, constraints, Rigidbody 3D, Interpolate, Physics Material, Inspector vs código |
| 04 | [Camera](04-camera.md) | `LateUpdate`, `SmoothDamp`, Cinemachine, intro con Coroutines |
| 05 | [Code Organization](05-code-organization.md) | Extract Method, nombres honestos, encapsulamiento, guard clauses, advertencias del compilador, gizmos |
| 06 | [Project Organization](06-project-organization.md) | Carpetas, prefijos, mover assets sin romper referencias |
| 07 | [Git & Workflow](07-git-and-workflow.md) | Ramas, merges, retomar conversaciones con Claude Code |
| 08 | [Input Actions Asset](08-input-actions-asset.md) | Acciones inline vs asset central, project-wide actions, `FindAction`, `[FormerlySerializedAs]`, clase C# generada |

## Próximos temas

Esta es una guía viva: cada tema nuevo se agrega aquí arriba y sale de esta lista.

- [ ] **Pickups**: triggers, `OnTriggerEnter`, `CompareTag("Player")`, destruir objetos
- [ ] **Power-up de velocidad**: encapsulamiento, properties (`{ get; private set; }`) y Coroutines para la duración
- [ ] **Movimiento de objetos** (Lab 4): `Translate` + `Time.deltaTime`, `Space.World`, Transform vs Rigidbody kinematic
- [ ] **Action Maps**: desactivar el mapa `Player` al pausar (con project-wide actions ya no hace falta `Enable()` manual)
- [ ] **Ordenar los scripts** en `Scripts/Player/` y `Scripts/Camera/`
