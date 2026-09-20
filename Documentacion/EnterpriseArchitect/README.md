# Diagramas de negocio en Enterprise Architect

Proyecto editable: `../../diagramas.EAP`.

Alcance: N01 y N02 de las iteraciones 1 y 2 del **Plan de Entregas TD 2026**. N03 pertenece a la iteración 3 y no se presenta como implementado. Los diagramas de procesos y secuencia representan actores y actividades organizacionales, sin un actor «Sistema».

| Requisito | N01 | N02 |
|---|---|---|
| Casos de uso principales | N01-01 | N02-01 |
| Actividad / proceso | N01-02 | N02-02 |
| Modelo conceptual | N01-03 | N02-03 |
| Clases del proceso, entidades BE | N01-04a, N01-04b | N02-04 |
| Clases de lógica de negocio BLL | N01-05 | N02-05 |
| Secuencias organizacionales | N01-06a, N01-06b | N02-06a, N02-06b |
| Modelo de datos del proceso | N01-07a, N01-07b | N02-07 |

Las secuencias de N02 complementan el modelado solicitado. Las especificaciones de los cinco casos de uso (objetivo, precondiciones, poscondiciones, disparador, flujo normal y excepciones) se incorporan en las notas de sus elementos UML; su fuente es `../casos de uso/Casos_de_Uso_N01_N02.md`.

Las propiedades de clases provienen de `BE/*.cs`; las operaciones públicas y parámetros, de `BLL/*.cs`. El modelo físico proviene de `SQL/Iteracion2_Aprovisionamiento.sql` y `SQL/Iteracion2_Ventas_Fidelizacion.sql`. `Usuarios` se muestra solo como referencia a la clave del módulo transversal. Las restricciones completas de cada tabla se conservan en sus notas. El modelo conceptual distingue el historial de membresías de la regla de una única membresía activa.

Las capturas PNG son exportaciones nativas del motor de diagramación de EA; no son imágenes Mermaid importadas. `manifest.json` relaciona cada imagen con el identificador y GUID de su diagrama.

## Reproducción

Con el proyecto `diagramas.EAP` abierto en EA:

1. `python Scripts/build_ea_model.py` genera la especificación a partir del código y las reglas.
2. Ejecutar `Scripts/import_ea_model.ps1` mediante Windows PowerShell **de 32 bits**. Crea una copia de seguridad local y un paquete nuevo; se niega a sobrescribir un paquete existente.
3. Ejecutar `Scripts/refine_ea_layout.ps1` con el mismo PowerShell para aplicar los ajustes de distribución y las firmas de operaciones.

Para exportar un modelo ya existente sin modificar elementos, ejecutar `import_ea_model.ps1 -ExportOnly`. Los scripts usan la API oficial de automatización de EA, no SQL directo sobre el archivo EAP.
