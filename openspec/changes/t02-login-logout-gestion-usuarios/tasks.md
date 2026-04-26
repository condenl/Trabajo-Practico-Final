## 1. Documentación y Análisis de Referencia

- [x] 1.1 Validar que la clase `Servicios.Session` implementa correctamente el patrón Singleton (Thread-safe).
- [x] 1.2 Documentar el flujo de autenticación existente en `BLL.UsuarioBLL.Login`.
- [x] 1.3 Verificar que el mapeo de `Usuario` y `Rol` en `MPP.UsuarioMPP` incluye todos los campos necesarios para la sesión.

## 2. Ajustes Mínimos de Ciclo de Vida

- [x] 2.1 Implementar `BLL.IntegrityBLL` para verificar conectividad a la base de datos al inicio (Startup).
- [x] 2.2 Integrar el llamado a la integridad en `UI.Program.Main` antes de `Application.Run`.
- [x] 2.3 Asegurar que el evento LOGOUT registre una entrada en la Bitácora (actualmente el logout es solo limpieza de instancia).

## 3. Verificación de Funcionamiento (Referencia)

- [x] 3.1 Probar el proceso de bloqueo de cuenta tras 4 intentos fallidos usando el código actual.
- [x] 3.2 Validar que `FormPrincipal` recibe y utiliza correctamente el objeto `Usuario` de la sesión.
- [x] 3.3 Confirmar que el cierre de la aplicación libera la sesión de forma segura.
