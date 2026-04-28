## 1. Verificación de Implementación Técnica

- [x] 1.1 Validar que `Servicios/Encriptador.cs` contiene los métodos `EncriptarAES` y `DesencriptarAES` con la clave y IV correctos.
- [x] 1.2 Confirmar que `BE/Interfaces/IEntidadBase.cs` y `BE/Persona.cs` utilizan `string` para la propiedad `Dni`.
- [x] 1.3 Verificar que `BLL/UsuarioBLL.cs` aplica `EncriptarAES` en los métodos `Guardar` e `Insertar`.
- [x] 1.4 Verificar que `MPP/UsuarioMPP.cs` aplica `DesencriptarAES` al mapear el usuario desde la base de datos.

## 2. Actualización de Especificaciones (OpenSpec)

- [x] 2.1 Actualizar el archivo maestro `openspec/specs/encryption-services/spec.md` integrando los nuevos requerimientos de AES y eliminando la nota de simplificación previa.

## 3. Sincronización de Documentación Oficial

- [x] 3.1 Actualizar la sección `T03. Gestión de Encriptado` en `Documentacion/Carpeta_Documentacion.md` siguiendo estrictamente la estructura de `carpeta_modelo.md`:
    - [x] 3.1.1 Incluir Objetivo detallado (AES para DNI, SHA256 para contraseñas).
    - [x] 3.1.2 Incluir Descripción detallada de funcionamiento de ambos algoritmos.
    - [x] 3.1.3 Actualizar Diagrama de clases (Mermaid) con métodos AES y SHA256.
    - [x] 3.1.4 Incluir DER (Mermaid) mostrando el campo DNI en la tabla Usuario.
    - [x] 3.1.5 Actualizar Diagramas de Secuencia (Mermaid) para los flujos de AES y Hash.
    - [x] 3.1.6 Incluir el Diseño de los Algoritmos (Pseudocódigo/Lógica) para AES y SHA256.
- [x] 3.2 Asegurar que el lenguaje y formato cumplan con la consigna: "Análisis, diseño e implementación".
- [x] 3.3 Revisar y actualizar el Diagrama de Clases global si el tipo de dato de DNI no está actualizado a string.
