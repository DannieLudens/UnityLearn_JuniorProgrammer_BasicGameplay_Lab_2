# 06 · Project Organization

[← Índice](README.md)

---

## Estructura de carpetas

```
Assets/
├── Art/
│   ├── Materials/          Mat_...
│   └── Sprites/            2D characters and items
├── Audio/                  (create when needed)
├── Documents/              GDD-Link.txt
├── Input/                  InputSystem_Actions.inputactions
├── Physics/                PM_... (physics materials)
├── Prefabs/                (create when needed)
│   ├── Player/
│   ├── Enemies/
│   ├── PickUps/
│   └── Environment/
├── Scenes/
├── Scripts/
│   ├── Player/             PlayerController.cs
│   ├── Camera/             IntroCamera.cs
│   ├── PickUps/
│   └── Enemies/
└── Settings/               URP configuration: DON'T touch
```

**Las reglas detrás:**
1. **Primer nivel por tipo de asset** (Art, Audio, Scripts, Prefabs...): es el estándar en Unity.
2. **Dentro de cada tipo, por "qué es en el juego"** (Player, Enemies, PickUps): con 15 scripts, el del power-up se encuentra en segundos.
3. **Crear carpetas solo cuando se necesitan**: las carpetas vacías estorban.

`TutorialInfo/` y `Readme.asset` vienen del template de URP y el juego no los usa, así que se pueden borrar.

`Docs/` (esta guía) va **en la raíz del repo, fuera de `Assets/`**, para que Unity no la importe.

---

## Prefijos de nombres

| Prefijo | Tipo | Ejemplo |
|---|---|---|
| `Mat_` | Material | `Mat_Gray_Platform` |
| `PM_` | Physics Material | `PM_Player_NoFriction` |
| `CM_` | Cinemachine Camera | `CM_Player`, `CM_Overview` |
| (nombre del objeto) | Prefab / GameObject | `Player`, `Enemy_Guard`, `PickUp_Juice` |

**Por qué:** en la búsqueda del Project, escribir `PM_` muestra todos los physics materials. Además, el nombre dice qué es el asset sin abrirlo.

---

## Mover assets sin romper nada

**La idea:** cada asset tiene un archivo `.meta` con un **ID único (GUID)**. Así sabe Unity que el Player usa `Mat_Blue`, aunque el archivo cambie de carpeta.

**Regla:** **mover siempre desde la ventana Project de Unity** (arrastrando), **nunca desde el Explorador de Windows**. Lo mismo para borrar: clic derecho → Delete en Unity.

**⚠️ Trampa:** si mueves desde Windows y el `.meta` no viaja con el archivo, Unity genera un GUID nuevo y **se pierden las referencias**: objetos en rosado (sin material) o scripts faltantes (*Missing Script*).

---

[← 05 · Code Organization](05-code-organization.md) · [Siguiente: 07 · Git & Workflow →](07-git-and-workflow.md)
