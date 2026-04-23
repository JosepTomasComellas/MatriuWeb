# RELEASE.md — MatriuWeb

## Objectiu

Definir el procés real de versionat, publicació i desplegament del projecte MatriuWeb segons el teu flux de treball.

---

## Entorn considerat

### Desenvolupament
- Windows
- PowerShell
- repositori local a `D:\Claude\MatriuWeb`

### Publicació
- Git
- GitHub

### Desplegament
- LXC Proxmox
- Ubuntu Server 24
- ruta `/docker/MatriuWeb`

---

## Model de release

Una release completa té dues fases:

### Fase 1 — Preparació local
Es fa al PC Windows:
- revisar canvis
- validar
- actualitzar documentació
- commit
- push a GitHub

### Fase 2 — Desplegament al servidor
Es fa manualment a la LXC:
- entrar al directori del projecte
- executar `git pull`
- executar `docker compose up -d --build`
- o usar un script de deploy si existeix al repositori

---

## Versionat

Format recomanat: `MAJOR.MINOR.PATCH`

### Regles
- `MAJOR`: canvis incompatibles
- `MINOR`: funcionalitats noves
- `PATCH`: correccions i ajustos

---

## Procés de release local

### 1. Revisar canvis reals
- confirmar funcionalitats implementades
- no incloure experiments o proves parcials

### 2. Assignar nova versió
- decidir la nova versió de manera coherent

### 3. Actualitzar documentació
Com a mínim revisar:
- `README.md`
- changelog
- `ARCHITECTURE.md` si s'ha alterat infraestructura
- `CLAUDE.md` si s'ha canviat el flux de treball o els criteris de l'agent
- qualsevol nota sobre la coherència visual amb AutoCo si hi ha canvis rellevants de UI

### 4. Validació local
Executar validacions compatibles amb Windows i PowerShell.

### 5. Commit
Format recomanat:

```powershell
git add .
git commit -m "release: vX.Y.Z - resum curt"
```

### 6. Push
```powershell
git push
```

---

## Procés de desplegament al servidor

Després del push:

1. connectar a la LXC
2. situar-se a `/docker/MatriuWeb`
3. executar:

```bash
git pull
docker compose up -d --build
```

Si existeix un script propi de deploy, fer servir aquest script com a font de veritat operativa.

---

## Regles obligatòries

- No fer release si la documentació no reflecteix els canvis importants
- No inventar entrades al changelog
- No descriure com a automàtic un desplegament que és manual
- No trencar la compatibilitat amb LXC sense indicar-ho clarament
- No canviar l'estratègia de persistència sense revisar impacte a dades i backups
- No degradar el llenguatge visual alineat amb AutoCo sense justificar-ho

---

## Què ha de fer l'agent en mode release

Quan l'usuari demana una release, l'agent ha de:

1. identificar la nova versió
2. revisar `README.md`
3. revisar changelog
4. validar coherència general
5. preparar o executar commit i push
6. recordar que el desplegament final es fa manualment a la LXC

---

## Plantilla de changelog

```md
## vX.Y.Z

### Nova funcionalitat
- ...

### Millores
- ...

### Correccions
- ...

### Infraestructura
- ...
```

Només s'hi han d'incloure canvis reals.

---

## Criteri de finalització d'una release

Una release queda preparada quan:

- el codi està actualitzat
- la documentació està alineada
- la versió està fixada
- el commit s'ha fet
- el push a GitHub s'ha completat

La release queda desplegada només després del procés manual a la LXC.
