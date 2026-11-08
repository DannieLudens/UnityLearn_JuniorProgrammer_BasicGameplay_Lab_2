# 07 · Git & Workflow

[← Índice](README.md)

---

## Flujo de ramas del proyecto

```
main  ←── dev  ←── feature branches (Camera, Camera_Cinemachine, ...)
                    one per feature, merged into dev via Pull Request
```

---

## La rama que no tenía el refactor (caso real)

**Qué pasó:** el refactor del `PlayerController` se hizo y commiteó en `dev`. Después seguí trabajando en una rama de cámara **creada antes** de ese commit. Unity estaba ejecutando **la versión vieja** del script, porque en el disco solo estaba lo de la rama activa.

```
dev:                ... ── Refactor ── Merge
                                       /
Camera_Cinemachine: ... ── Remove CameraFollow     ← working here (no refactor!)
```

**Por qué pasa:** Unity lee los archivos que hay **en el disco**, y el disco tiene lo de **la rama en la que estás** (checked out). Cambiar de rama cambia los scripts que Unity compila.

**Cómo se arregla:**
1. Commitear los cambios pendientes de la rama actual.
2. `git merge dev` para traer lo más reciente.
3. Volver a Unity, que recompila solo.

**Cómo evitarlo:**
- Antes de crear una rama nueva, partir de `dev` actualizado (`git pull`).
- **Ramas de vida corta**: una por feature, y merge pronto.
- Si una rama vieja sigue abierta, traer `dev` antes de continuar.

**⚠️ Trampa:** si un cambio "desapareció", antes de reescribirlo revisa en qué rama estás (`git branch --show-current`) y dónde está el commit (`git log --all --oneline --graph`).

---

## Commits

- Commitear al terminar cada paso que funcione (no al final del día).
- El mensaje dice **qué** cambió, por ejemplo: *"Lock player control during intro camera blend"*.
- Un script borrado **no se pierde**: sigue en el historial (por ejemplo, `CameraFollow.cs` en el commit `51aec4a`).

---

## Claude Code: retomar y guardar conversaciones

| Acción | Comando |
|---|---|
| Retomar la última conversación de esta carpeta | `claude --continue` |
| Elegir una conversación de una lista | `claude --resume` |
| Exportar la conversación a un archivo de texto | `/export` (dentro de Claude Code) |
| Ejecutar un comando de terminal desde Claude Code | `! comando` (por ejemplo `! unity self-update`) |

- Las conversaciones se guardan **por carpeta**: hay que abrir la terminal en la carpeta del proyecto.
- ⚠️ **Se borran automáticamente** tras **30 días** sin uso (configurable con `cleanupPeriodDays`). Para guardarlas para siempre: `/export`.
- La **memoria** de Claude (preferencias y progreso del proyecto) y **esta guía** no se borran.

---

[← 06 · Project Organization](06-project-organization.md) · [Índice](README.md)
