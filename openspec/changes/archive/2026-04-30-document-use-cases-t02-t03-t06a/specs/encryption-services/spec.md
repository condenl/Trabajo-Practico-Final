## MODIFIED Requirements

### Requirement: Encriptación simétrica AES para DNIs
El sistema DEBE utilizar el algoritmo AES (Advanced Encryption Standard) para cifrar los DNIs de los usuarios, permitiendo su recuperación (desencriptación) para visualización en la interfaz de usuario. Esta funcionalidad DEBE estar documentada como un caso de uso técnico.

#### Scenario: Cifrado y descifrado exitoso
- **WHEN** un usuario ingresa su DNI durante el registro
- **THEN** el sistema DEBE almacenarlo cifrado en la base de datos y mostrarlo correctamente descifrado en los listados de administración.

#### Scenario: Verificación de Caso de Uso AES
- **WHEN** se revisa la documentación de encriptado
- **THEN** el caso de uso UC-T03-02 (Cifrar/Descifrar Datos AES) DEBE estar presente con su descripción de flujos.

### Requirement: Hashing irreversible para contraseñas
El sistema DEBE utilizar el algoritmo SHA256 para el resguardo de contraseñas, asegurando que no puedan ser recuperadas en texto plano. Esta funcionalidad DEBE estar documentada como un caso de uso técnico.

#### Scenario: Validación de contraseña en login
- **WHEN** un usuario intenta iniciar sesión
- **THEN** el sistema DEBE comparar el hash de la contraseña ingresada con el hash almacenado en la base de datos.

#### Scenario: Verificación de Caso de Uso Hashing
- **WHEN** se revisa la documentación de encriptado
- **THEN** el caso de uso UC-T03-01 (Generar Hash de Contraseña) DEBE estar presente con su descripción de flujos.
