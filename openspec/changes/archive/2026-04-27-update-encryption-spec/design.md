## Context

El sistema inicialmente propuso una simplificación hacia solo hashing, pero la implementación real (commit 57d0b03) y los requisitos de la `carpeta_modelo.md` exigen encriptación reversible para datos sensibles como el DNI. Actualmente, `Encriptador.cs` ya cuenta con los métodos `EncriptarAES` y `DesencriptarAES`, y las entidades han sido migradas para tratar el DNI como `string`.

## Goals / Non-Goals

**Goals:**
- Formalizar el uso de AES-256 para la protección de DNIs.
- Asegurar que la documentación técnica (T03) refleje fielmente la implementación de AES y SHA256.
- Garantizar la consistencia entre las capas BE, BLL, DAL y MPP respecto al manejo del DNI cifrado.

**Non-Goals:**
- No se busca implementar rotación de claves en esta etapa.
- No se modificará el algoritmo de hashing para contraseñas (se mantiene SHA256).

## Decisions

### 1. Algoritmo de Encriptación para DNI: AES-256
Se utiliza AES con una clave maestra y un IV estáticos definidos en `Servicios/Encriptador.cs`.
- **Rationale**: El DNI es un dato que requiere ser visualizado en la interfaz de usuario (ej: listado de usuarios para el administrador), por lo que el hashing no es suficiente. AES ofrece un balance óptimo entre seguridad y rendimiento para este tipo de aplicación.
- **Alternativas**: RSA (Asimétrico). Se descartó por la complejidad de manejo de claves y mayor consumo de recursos para datos simples como un DNI.

### 2. Cambio de tipo de dato para DNI: string
El DNI en `IEntidadBase` y `Persona` cambió de `int` a `string`.
- **Rationale**: Los resultados de la encriptación AES son cadenas en Base64, las cuales no pueden ser almacenadas en tipos numéricos. El tipo `string` permite almacenar tanto el texto plano (temporalmente en UI) como el cifrado (en tránsito y persistencia).

### 3. Sincronización de Carpeta_Documentacion.md
Actualizar la sección T03 para incluir diagramas y descripciones de AES y SHA256.
- **Rationale**: Cumplimiento con la normativa de la materia y asegurar que la documentación técnica sea una fuente de verdad confiable.

## Risks / Trade-offs

- **[Riesgo] Claves estáticas en código** → Las claves están hardcoded en `Encriptador.cs`.
  - **Mitigación**: Para el alcance de este proyecto académico, es aceptable, pero se documentará como una limitación de seguridad en la arquitectura.
- **[Riesgo] Inconsistencia en la BD** → Datos viejos (ints) pueden chocar con los nuevos (strings cifrados).
  - **Mitigación**: Se asume una limpieza de la base de datos o migración manual de los registros existentes durante la aplicación del cambio.
