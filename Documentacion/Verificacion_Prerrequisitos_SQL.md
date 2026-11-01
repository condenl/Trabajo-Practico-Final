# Verificación de prerrequisitos SQL del instalador

Fecha: 31/10/2026.

## Cambio

El instalador diferencia motor ausente, instancia no encontrada, servicio detenido y fallo de conexión/permisos. La conexión real sigue determinando la versión: se admite SQL Server 16.x o superior. LocalDB se trata separadamente porque no utiliza un servicio de Windows.

La página de SQL incluye **Descargar SQL Server**, que abre `https://www.microsoft.com/es-es/sql-server/sql-server-downloads`, y muestra **Abrir servicios** cuando el servicio de la instancia no está en ejecución. El enlace de Download Center utilizado anteriormente devolvió 404 al comprobarlo; la página general en español respondió correctamente. Se evita descargar un motor nuevo ante cualquier error de conexión. Se conserva la descarga e instalación automática de .NET cuando falta.

La comprobación ocurre al pulsar Siguiente y antes de copiar la aplicación. Un resultado fallido permite corregir y reintentar. No se almacena un resultado exitoso que pueda quedar obsoleto si cambia la instancia o se detiene su servicio. La comunicación de textos utiliza INI UTF-16 y los scripts usan UTF-8 con BOM para PowerShell de Windows.

## Pruebas

- `Scripts/test-sql-prerequisites.ps1`: **45 verificaciones correctas**. Usa las funciones de producción con inventarios simulados para ausencia de motor, nombre incorrecto, servicios detenidos/pausados/iniciándose, servicio activo sin conexión, instancia registrada sin servicio, inventario inaccesible, LocalDB y conexión remota fallida. Incluye procesos reales de PowerShell para SQL Server 16, LocalDB 17, instancia inexistente y entrada inválida; verifica los códigos de salida y la escritura del INI Unicode. Comprueba la prioridad de selección (base existente, Express, otros servicios, LocalDB), descarte de versiones antiguas y conexiones fallidas, deduplicación y descubrimiento real con reconexión al nombre seleccionado.
- `Scripts/test-sql-version-policy.ps1`: correctas las comprobaciones de prerrequisitos, inicialización y seed para versiones 0/15 rechazadas y 16/17/18 aceptadas.
- Comprobación adicional con PowerShell de **32 bits**: el nombre inexistente devolvió 23 y el mensaje enumeró las instancias del registro de 64 bits y LocalDB.
- Inno Setup 6 compiló correctamente el instalador actualizado.

## Revisión de interfaz

Se compiló en una carpeta temporal una copia de la definición del asistente con privilegios de usuario para inspeccionar las páginas sin pedir elevación ni instalar la aplicación. El cambio de privilegios se limitó a esa copia de prueba; el instalador distribuido conserva `PrivilegesRequired=admin`.

Se revisaron la página SQL, la legibilidad de los mensajes y el botón de descarga. Con una instancia inexistente se mostró el error y las alternativas detectadas, permaneciendo en la misma página. Se pulsó Descargar SQL Server y se abrió la página de Microsoft en Chrome. Al cambiar la instancia a `.`, la comprobación real de SQL Server 16 permitió avanzar a las tareas adicionales. Se canceló el asistente antes de instalar.

No se desinstaló ningún motor, no se detuvieron servicios reales y no se modificaron bases de la aplicación. La ausencia de motor y los servicios detenidos se verificaron mediante inventarios simulados; no se ejecutó una instalación completa en una computadora externa sin SQL Server.

La revisión visual de autoselección precargó `.` con SQL Server 16 e informó que ya contenía la base de Perfumería. Se cambió manualmente a `(localdb)\MSSQLLocalDB`, se retrocedió y se volvió a avanzar: se conservó la elección manual. La búsqueda solo se ejecuta al entrar por primera vez cuando no hay un nombre suministrado y utiliza consultas de lectura; la comprobación antes de instalar sigue siendo obligatoria.
