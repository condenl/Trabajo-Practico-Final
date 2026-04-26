## ADDED Requirements

### Requirement: Hashing de datos sensibles
El sistema DEBE utilizar el algoritmo SHA256 para generar el hash de las contraseñas y otros datos sensibles, garantizando que sean almacenados de forma irreversible.

#### Scenario: Generación de hash consistente
- **WHEN** se solicita el hash de un mismo texto plano en distintas ocasiones
- **THEN** el sistema DEBE retornar siempre el mismo resultado hexadecimal de 64 caracteres.

### Requirement: Documentación del proceso de Hashing
El sistema DEBE documentar el objetivo y el diseño del algoritmo de hashing en la carpeta de documentación global.

#### Scenario: Reflejo en Carpeta Documentación
- **WHEN** se consulta la sección T03 de la documentación
- **THEN** se DEBE encontrar la descripción técnica del algoritmo SHA256 y su aplicación en el sistema.

## REMOVED Requirements

### Requirement: Encriptación simétrica reversible
**Reason**: Se ha decidido simplificar el sistema utilizando únicamente hashing unidireccional para los requisitos actuales.
**Migration**: Eliminar cualquier referencia o implementación de AES en la capa de servicios.
