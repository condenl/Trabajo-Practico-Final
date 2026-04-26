## Contexto y Objetivo

El sistema requiere una gestión formal del ciclo de vida y la sesión de usuario. El objetivo es asegurar que el acceso al sistema esté controlado, auditado y que el estado de la sesión sea consistente en toda la aplicación mediante el patrón Singleton.

## Descripción Detallada

El sistema utiliza una arquitectura multicapa (UI, BLL, DAL, MPP, BE). La seguridad se centra en la verificación de credenciales (Email/Password) y el control de intentos fallidos.

### Política de Log-in / Log-out
- **Log-in**: El usuario debe proporcionar Email y Contraseña. Se verifica el hash de la clave. Al tener éxito, se instancia el Singleton de Sesión y se carga el perfil del usuario. Se bloquea la cuenta tras 4 intentos fallidos.
- **Log-out**: Se limpia la instancia del Singleton de Sesión, se registran los eventos pendientes y se retorna al formulario de Login, asegurando que no quede estado residual en memoria.

## Procesos del Sistema

| Proceso | Descripción | Acciones Relevantes |
| :--- | :--- | :--- |
| **Arranque (Startup)** | Inicio de la aplicación en `Program.Main`. | Carga de estilos, verificación de conectividad básica, despliegue de `Login`. |
| **Log-in** | Validación de identidad y creación de sesión. | Verificación de credenciales, auditoría (Bitácora), carga de `RolAsignado`. |
| **Apagado (Shutdown)** | Cierre total de la aplicación. | Registro de evento SHUTDOWN, liberación de recursos de red. |
| **Log-out** | Finalización de la sesión del usuario. | Limpieza de `Session.Instance`, registro de evento LOGOUT. |

## Diseño de Arquitectura

### Patrón Singleton (Session)
Se utiliza para garantizar que un solo objeto gestione la sesión activa:
```csharp
public class Session {
    private static Session _instance;
    public static Session Instance { get { ... } }
}
```

### Diagramas (Referencia)
- **Clases**: Involucra `Session`, `UsuarioBLL`, `UsuarioDAL` y `Usuario`.
- **DER**: Tablas `Usuario` (Id, Email, Password, Activo, Intentos, RolId) y `Rol`.

## Decisiones Técnicas

1. **Uso de Singleton**: Se mantiene la implementación actual en `Servicios.Session` por ser el estándar para manejo de estados globales en WinForms.
2. **Encriptación**: Se utiliza `Servicios.Encriptador` (Hash) para no almacenar claves en texto plano.
3. **Auditoría**: Se integra con `BitacoraBLL` mediante el patrón Observer para registrar eventos de seguridad automáticamente.

## Riesgos / Mitigaciones

- **[Riesgo] Sesión Huérfana** → **Mitigación**: El evento `FormClosed` del formulario principal fuerza el `Logout`.
- **[Riesgo] Bloqueo de Cuenta** → **Mitigación**: El sistema registra intentos fallidos en la DB para persistir el estado de bloqueo entre reinicios.
