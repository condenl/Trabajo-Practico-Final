## 1. Refactorización Técnica en Servicios

- [x] 1.1 Eliminar los métodos `Encriptar`, `Desencriptar` y `PadClave` de la clase `Encriptador.cs` en el proyecto `Servicios`.
- [x] 1.2 Asegurar que el método `Hash` (SHA256) sea el único método público disponible.

## 2. Actualización de Documentación Global

- [x] 2.1 Actualizar la sección "T03. Gestión de Encriptado" en `Documentacion/Carpeta_Documentacion.md` para remover menciones a AES.
- [x] 2.2 Ajustar el Objetivo y Descripción para centrarse exclusivamente en Hashing (SHA256).
- [x] 2.3 Actualizar el Diagrama de Clases (Mermaid) removiendo los métodos eliminados.
- [x] 2.4 Reemplazar el Diagrama de Secuencia de encriptación simétrica por uno que muestre el flujo de Hashing.
- [x] 2.5 Actualizar el diseño del algoritmo en la documentación para reflejar solo SHA256.

## 3. Validación

- [x] 3.1 Verificar que el código en `Servicios` esté limpio y solo contenga el método `Hash`.
- [x] 3.2 Confirmar que `Carpeta_Documentacion.md` sea consistente con este enfoque simplificado.
