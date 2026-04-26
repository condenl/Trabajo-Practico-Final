## Context

Actualmente, el sistema requiere un mecanismo de protección de datos. Se ha decidido que el **Hashing unidireccional** es suficiente y preferible por su seguridad y simplicidad para el alcance actual del proyecto (contraseñas y auditoría).

## Goals / Non-Goals

**Goals:**
- Utilizar exclusivamente **SHA256** para la protección de datos sensibles.
- Documentar el funcionamiento del algoritmo de hashing y su flujo.
- Asegurar que no se utilicen métodos de encriptación reversible innecesarios.

**Non-Goals:**
- Implementar encriptación simétrica (AES) o asimétrica (RSA), ya que no se requiere recuperación de datos originales en esta etapa.

## Decisions

- **Mecanismo Único**: Se utilizará únicamente **SHA256**. Es un estándar robusto para verificar integridad y proteger credenciales.
- **Eliminación de AES**: Se removerán los métodos `Encriptar` y `Desencriptar` previamente considerados para simplificar la superficie de ataque y el mantenimiento.

## Risks / Trade-offs

- **[Trade-off] Irreversibilidad** → Los datos hasheados no pueden ser recuperados. *Mitigación*: Esto es deseable para contraseñas. Si en el futuro se requiere encriptación reversible, se evaluará una nueva capacidad.
