## ADDED Requirements

### Requirement: Encriptación simétrica AES para DNIs
El sistema DEBE utilizar el algoritmo AES (Advanced Encryption Standard) para cifrar los DNIs de los usuarios, permitiendo su recuperación (desencriptación) para visualización en la interfaz de usuario.

#### Scenario: Cifrado y descifrado exitoso
- **WHEN** un usuario ingresa su DNI durante el registro
- **THEN** el sistema DEBE almacenarlo cifrado en la base de datos y mostrarlo correctamente descifrado en los listados de administración.

### Requirement: Hashing irreversible para contraseñas
El sistema DEBE utilizar el algoritmo SHA256 para el resguardo de contraseñas, asegurando que no puedan ser recuperadas en texto plano.

#### Scenario: Validación de contraseña en login
- **WHEN** un usuario intenta iniciar sesión
- **THEN** el sistema DEBE comparar el hash de la contraseña ingresada con el hash almacenado en la base de datos.

### Requirement: Documentación Técnica T03 Sincronizada
El sistema DEBE mantener la sección T03 de la Carpeta_Documentacion.md actualizada con los algoritmos AES y SHA256, incluyendo sus objetivos y detalles de implementación.

#### Scenario: Verificación de documentación
- **WHEN** se revisa el archivo Carpeta_Documentacion.md
- **THEN** la sección T03 DEBE detallar el uso de AES para DNI y SHA256 para contraseñas, alineándose con carpeta_modelo.md.
