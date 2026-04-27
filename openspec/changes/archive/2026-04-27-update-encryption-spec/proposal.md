## Why

Actualizar el esquema de encriptación del sistema para reflejar la implementación actual (commit 57d0b03) y cumplir con los requisitos de la Carpeta Modelo. El cambio es necesario para asegurar que los DNIs se almacenen de forma segura mediante encriptación simétrica (AES) permitiendo su recuperación, mientras que las contraseñas se mantienen protegidas mediante hashing irreversible (SHA256). Esto permite sincronizar la documentación técnica oficial del proyecto.

## What Changes

- **Restauración de AES**: Se re-introduce el soporte para encriptación simétrica AES para datos que requieren reversibilidad (DNI).
- **Persistencia de Hashing**: Se mantiene SHA256 para el resguardo de contraseñas.
- **Actualización de Entidades**: Modificación de la entidad `Persona` y la interfaz `IEntidadBase` para tratar el DNI como `string` en lugar de `int`, permitiendo el manejo de datos cifrados.
- **Sincronización de Documentación**: Actualización de `Carpeta_Documentacion.md` (Sección T03) para alinearse con la implementación técnica y los estándares de `carpeta_modelo.md`.

## Capabilities

### New Capabilities
- No se introducen capacidades completamente nuevas, sino que se restauran y refinan las existentes.

### Modified Capabilities
- `encryption-services`: Se modifican los requerimientos para incluir encriptación simétrica AES para DNIs y hashing para contraseñas, revirtiendo la decisión previa de usar solo hashing.

## Impact

- **BE**: `IEntidadBase` y `Persona` (cambio de tipo de `Dni` a `string`).
- **Servicios**: `Encriptador.cs` (nuevos métodos `EncriptarAES` y `DesencriptarAES`).
- **BLL**: `UsuarioBLL.cs` (lógica de cifrado al guardar/insertar).
- **MPP**: `UsuarioMPP.cs` (lógica de descifrado al mapear desde la BD).
- **DAL**: `UsuarioDAL.cs` (actualización de parámetros para DNI como string).
- **UI**: `UC_Registro.cs` (validación de DNI como texto numérico).
- **Documentación**: `Carpeta_Documentacion.md` requiere una actualización completa de la sección T03.
