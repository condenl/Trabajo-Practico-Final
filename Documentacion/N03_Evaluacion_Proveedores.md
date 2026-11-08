# N03 — Evaluación y selección de proveedores

## Objetivo de negocio

Elegir un proveedor conveniente para un perfume y una cantidad concreta. La organización cruza propuestas comerciales, reputación y trayectoria con sus ventas, inventario y compras anteriores. Conserva los fundamentos de la decisión y puede iniciar una compra que continúa por el circuito contable de N01.

El proceso se utiliza desde **Compras e inventario → Evaluación de proveedores**, con el actor Dueño/Compras. La operación cotidiana tiene tres pasos: comparar, confirmar proveedor y, si se decide comprar, crear una orden en Borrador. Los pesos se ajustan solamente si hace falta desde Criterios; no hay pasos de negociación ni contratos adicionales.

La vista principal presenta gráficos de barras del puntaje y margen bruto por proveedor, junto con stock utilizable, compras pendientes, ventas y reposición sugerida. Seleccionar una barra cambia la oferta sobre la que se toma la decisión. El listado completo queda en otra pestaña; Criterios e Historial se despliegan a demanda. Se utilizan los controles de gráficos integrados en .NET Framework 4.7.2.

Actualizar vuelve a consultar los catálogos y el historial. Si la comparación sigue en preparación, genera otra con los datos actuales y conserva la oferta elegida y el fundamento escrito; la instantánea anterior permanece disponible. Si ya existe una decisión confirmada, recarga su registro sin recalcular ni modificar la evidencia. Comparar permite iniciar una evaluación nueva.

**Usar cantidad sugerida** toma la reposición calculada y vuelve a comparar para esa cantidad; no confirma una oferta. Si la sugerencia es cero, se explica que el stock y las compras pendientes cubren el objetivo. Después de crear un borrador, el mismo control ofrece **Abrir orden #...**, que selecciona la orden existente en Compras. El fundamento todavía no confirmado se considera trabajo pendiente al abandonar la sección.

## Información cruzada y reglas

| Información | Uso en la decisión |
| --- | --- |
| Cotizaciones por producto/proveedor | Última cotización vigente de cada proveedor; se excluyen ofertas futuras, vencidas y proveedores inactivos. Dos proveedores distintos deben tener cotizaciones comparables. |
| Precio, prestigio y años en el mercado | Puntaje explicado de 0 a 100, con pesos predeterminados 50/30/20. |
| Ventas de los últimos 90 días | Ritmo de venta y precio promedio efectivamente vendido, neto de descuentos. |
| Precio de catálogo | Referencia de venta conservadora: el menor entre catálogo y promedio neto reciente; si no hay ventas, se usa catálogo. |
| Stock utilizable y compras pendientes de recepción | Reposición sugerida para 30 días y señal de compras que ya están en curso. Se excluyen lotes vencidos. |
| Compras emitidas y recibidas de los últimos 180 días | Cantidades compradas/recibidas por proveedor para interpretar los antecedentes. Borradores, rechazos y cancelaciones no cuentan como compras realizadas. |
| Plazo cotizado y ritmo de ventas | Advertencia si el stock actual podría agotarse antes del plazo informado. No presupone cuándo llegarán pedidos pendientes. |

La cotización más reciente se determina por fecha de emisión y, en empate, por ID. Una oferta antigua más barata no desplaza una nueva vigente del mismo proveedor. Si no hay oferta vigente, se muestra la más reciente con su motivo de exclusión. Los proveedores sin antecedentes no reciben una penalización inventada: se informa la falta de datos.

Fórmulas:

- Precio: `100 × menor precio vigente / precio del proveedor`.
- Prestigio: índice registrado, de 0 a 100.
- Antigüedad: `5 × min(años, 20)`.
- Total: suma ponderada de los tres puntajes; los pesos no negativos deben sumar 100.
- Margen bruto porcentual: `100 × (precio de venta de referencia − costo unitario) / precio de venta de referencia`.
- Beneficio bruto estimado: `(precio de venta de referencia − costo unitario) × cantidad`.
- Reposición sugerida: `max(0, max(stock mínimo, techo(ventas90 / 90 × 30)) − stock utilizable − compras pendientes)`, con máximo 100000 unidades.

Se excluyen ofertas sin margen bruto positivo o cuyo importe no cabe en una orden. Si no quedan ofertas elegibles, se conserva la comparación sin recomendar proveedor. Los empates de puntaje se resuelven por menor precio y luego por ID de proveedor, de manera determinista. El usuario puede confirmar otra oferta elegible y debe fundamentar siempre su decisión en 10 a 500 caracteres.

El beneficio es **bruto estimado**, sin costos de flete, impuestos ni gastos que no se registran. La recepción acumulada no es una medición de puntualidad ni de calidad: el modelo existente no conserva fechas prometidas ni incidencias suficientes para afirmar eso. Las sugerencias de demanda no sustituyen el juicio comercial del dueño.

## Especificaciones de casos de uso

### N03-A. Comparar proveedores

**Actor:** Dueño/Compras. **Precondición:** sesión iniciada y catálogo de productos disponible. **Disparador:** necesidad de decidir una compra o revisar proveedores.

Flujo normal:

1. El actor elige un producto y la cantidad que desea evaluar.
2. El sistema muestra los criterios predeterminados; el actor puede ajustarlos si lo necesita.
3. El actor solicita comparar proveedores.
4. El sistema presenta el ranking, costos, margen estimado, antecedentes y necesidades de reposición, indicando las ofertas excluidas y sus causas.
   El resumen explica la ventaja del recomendado, el costo frente a la oferta elegible más barata y el margen bruto, utilizando los mismos datos de la comparación guardada.
5. El actor consulta el detalle de las alternativas.
6. El sistema conserva una comparación fechada, con su responsable y los datos utilizados, disponible en el historial.

Excepciones: producto inactivo, cantidad inválida o pesos que no suman 100 impiden comparar. Menos de dos ofertas comparables o ausencia de margen positivo producen una comparación sin recomendación y explican la causa.

Cambiar cantidad o ponderaciones invalida los resultados para confirmar, conservando el fundamento en preparación hasta recalcular. Cambiar de perfume o abrir una comparación histórica solicita confirmar el descarte si hay una elección sin guardar; cancelar conserva producto, texto y elección. El fundamento de una decisión ya guardada no se copia a una comparación nueva. Una selección manual distinta se presenta junto a la recomendación original, sin alterar el ranking.

**Postcondición:** comparación guardada; no se emite una compra ni se mueve stock.

### N03-B. Confirmar proveedor

**Actor:** Dueño/Compras. **Precondición:** comparación con alguna oferta elegible. **Disparador:** el actor decide qué alternativa conviene.

Flujo normal:

1. El actor selecciona una alternativa del ranking.
2. El sistema presenta sus fundamentos, costos y advertencias.
3. El actor escribe el fundamento de su elección y solicita confirmarla.
4. El sistema comprueba que la comparación siga vigente y conserva proveedor, motivo, responsable y fecha de la decisión.
5. El sistema informa que puede continuar con una orden en Borrador.

Excepciones: oferta excluida o motivo insuficiente impiden confirmar. Si cambiaron los datos de origen o pasó el día de evaluación, se solicita comparar nuevamente. Una decisión confirmada no se sobrescribe: se conserva y se genera otra comparación si se desea reconsiderarla.

**Postcondición:** elección trazable, sin cambios de stock ni aprobación contable.

### N03-C. Iniciar compra desde la decisión

**Actor:** Dueño/Compras. **Precondición:** proveedor confirmado. **Disparador:** el actor decide realizar la compra evaluada.

Flujo normal:

1. El actor solicita crear una orden en Borrador.
2. El sistema comprueba la vigencia de los datos y crea el borrador con el proveedor, producto, cantidad y precio evaluados.
3. El sistema informa su número y conserva la relación con la evaluación.
4. El actor solicita abrir el borrador y el sistema lo selecciona en Compras para continuar la revisión contable de N01.

Excepciones: si cambiaron los datos, se solicita una comparación nueva sin dejar una orden parcial. Si el actor reintenta la misma solicitud, el sistema devuelve la orden ya vinculada; dos solicitudes concurrentes tampoco la duplican.

**Postcondición:** un único borrador vinculado a la evaluación; no hay aprobación, recepción ni movimiento de stock automático.

## Implementación y conservación de evidencia

BE contiene `SolicitudEvaluacion`, `DatosEvaluacion`, `OfertaEvaluable`, `ResultadoProveedor` y `EvaluacionProveedor`. `EvaluacionProveedorBLL` valida, calcula y coordina; `IEvaluacionProveedorRepository` está en DAL y `EvaluacionProveedorDAL` lee los datos y guarda las decisiones. `ArchivoEvaluacion` conserva una instantánea XML y su firma SHA-256; no es un dígito verificador de toda la base, sino una comparación de los datos usados para detectar obsolescencia.

La tabla `EvaluacionProveedor` guarda producto, responsable, fecha, instantánea, selección y orden vinculada. La lectura de datos se realiza en una transacción serializable; confirmar y crear orden bloquean la evaluación y vuelven a leer las fuentes dentro de la misma transacción. Se conserva la evidencia original aunque cambien los catálogos. El historial muestra las 100 comparaciones más recientes del producto.

`SQL/Iteracion3_Evaluacion_Proveedores.sql` es idempotente y se incluye en el inicializador y el instalador. La base local `Trabajo Practico Final 3` recibió esta migración y la precarga adicional sin reiniciar las demos anteriores.

## Demos disponibles

- **DEMO Evaluacion - valor vs precio:** tres cotizaciones. Con los pesos 50/30/20, el proveedor con trayectoria supera a la alternativa más barata. Con Precio 100, Prestigio 0 y Antigüedad 0, gana el económico. Para 10 unidades, los costos de las alternativas principales son $120000 y $100000.
- **DEMO Evaluacion - sin margen:** las tres ofertas superan el precio de venta de referencia. Se explica por qué no se recomienda comprar.
- **DEMO Reposicion con historial:** 90 unidades vendidas en tres ventas con descuento del 10 %, 10 en stock y 8 pendientes de una recepción parcial. El objetivo de 30 días es 30 unidades y la sugerencia inicial, 12. El promedio neto vendido es $16200; el proveedor recomendado cotiza $12000 (25,9 % de margen bruto). La oferta económica entrega en 14 días, frente a una cobertura de stock de 10. Las dos compras, lotes, ventas y movimientos permiten consultar los antecedentes y continuar recibiendo o vendiendo por los circuitos normales.
- **DEMO Citrico sin stock:** una sola cotización comparable; requiere conseguir otra antes de seleccionar.

Las nuevas cotizaciones se crean con vigencia de un año y no se renuevan al reintentar el seed. Las evaluaciones no se precargan: se generan desde la vista para demostrar el proceso.
