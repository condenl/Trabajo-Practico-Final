# Casos de Uso Principales N01 y N02

## CU-N01-01 Preparar y enviar orden de compra a revisión

| Campo | Descripción |
|---|---|
| **Objetivo** | Registrar cotizaciones comparables, crear una orden preliminar y remitirla a Contabilidad. |
| **Precondiciones** | El usuario posee permiso de aprovisionamiento. El proveedor y los productos están activos. |
| **Poscondiciones** | La orden queda pendiente de aprobación contable y la operación queda registrada en bitácora. |
| **Eventos Disparadores** | El Dueño o responsable de Compras decide reponer uno o más productos. |
| **Flujo Normal** | 1. El Dueño / Departamento de Compras solicita preparar una compra.<br>2. El sistema presenta las opciones para registrar cotizaciones y preparar la orden.<br>3. Compras informa las cotizaciones recibidas y selecciona proveedor, productos, cantidades y precios.<br>4. El sistema muestra la orden en borrador con su importe total y señala los datos que requieren corrección.<br>5. Compras revisa el borrador y solicita enviarlo a Contabilidad.<br>6. El sistema informa si la comparación de ofertas permite presentar la compra o qué cotizaciones faltan.<br>7. Compras consulta el resultado y el sistema muestra la orden Pendiente de aprobación para su seguimiento. |
| **Excepciones** | E1: Sin permiso. El sistema informa que el actor no puede preparar la compra y no acepta la solicitud.<br>E2: Cotización inválida. El sistema indica los datos inválidos o vencidos; Compras los corrige y vuelve a presentarlos.<br>E3: Datos inactivos. El sistema identifica al proveedor o producto no disponible; Compras modifica la selección.<br>E4: Comparativa insuficiente. El sistema informa qué productos no tienen dos cotizaciones vigentes de proveedores distintos; Compras completa la comparación y reintenta el envío. La orden permanece en Borrador. |

## CU-N01-02 Revisar orden de compra

| Campo | Descripción |
|---|---|
| **Objetivo** | Permitir que Contabilidad apruebe o rechace una compra antes de emitirla al proveedor. |
| **Precondiciones** | El usuario posee permiso contable y la orden está Pendiente de aprobación. |
| **Poscondiciones** | La orden queda Aprobada o Rechazada y se informa el resultado. Compras realiza su emisión posterior. |
| **Eventos Disparadores** | Contabilidad recibe una orden pendiente para controlar su conveniencia económica. |
| **Flujo Normal** | 1. El Equipo de Contabilidad solicita consultar las compras pendientes de revisión.<br>2. El sistema presenta las órdenes pendientes con proveedor, productos e importes.<br>3. Contabilidad selecciona una orden para examinarla.<br>4. El sistema muestra el detalle y la información disponible para decidir.<br>5. Contabilidad evalúa la compra y solicita aprobarla.<br>6. El sistema confirma la aprobación y muestra el nuevo estado de la orden. |
| **Excepciones** | E1: Rechazo. Contabilidad solicita rechazar la compra e informa el motivo; el sistema confirma el rechazo. Si falta el motivo, solicita completarlo.<br>E2: Estado inválido. El sistema informa que la orden no está pendiente y no admite una nueva decisión; Contabilidad consulta su estado actual.<br>E3: Cambio concurrente. El sistema informa que la orden cambió; Contabilidad actualiza la consulta antes de continuar.<br>E4: Sin permiso contable. El sistema informa que el actor no está autorizado y no acepta la decisión. |

## CU-N01-03 Recepcionar mercadería y registrar trazabilidad

| Campo | Descripción |
|---|---|
| **Objetivo** | Registrar la recepción total o parcial de una orden y crear lotes trazables con su ingreso de stock. |
| **Precondiciones** | El usuario posee permiso de recepción. La orden está Emitida o Recibida parcialmente. |
| **Poscondiciones** | Se crean los lotes y movimientos de ingreso. La orden queda Recibida o Recibida parcialmente. |
| **Eventos Disparadores** | El proveedor entrega mercadería correspondiente a una orden emitida. |
| **Flujo Normal** | 1. Recepción / Depósito solicita registrar una entrega y selecciona la orden correspondiente.<br>2. El sistema muestra el proveedor y, por producto, las cantidades pedidas, recibidas y pendientes de recibir.<br>3. Recepción informa los productos entregados, cantidades y datos identificatorios de sus lotes.<br>4. El sistema informa si la entrega es admisible o identifica los datos y diferencias que deben corregirse.<br>5. Recepción revisa los datos informados y confirma la recepción.<br>6. El sistema confirma el ingreso e informa si la orden quedó recibida total o parcialmente.<br>7. Recepción solicita consultar las existencias y el origen de la mercadería ingresada.<br>8. El sistema muestra el stock actualizado y su trazabilidad hacia el proveedor y la compra de origen. |
| **Excepciones** | E1: Sin lotes. El sistema solicita informar al menos un lote; Recepción completa la entrega antes de reintentar.<br>E2: Lote inválido. El sistema identifica los datos faltantes, la cantidad no positiva o el vencimiento inválido; Recepción los corrige.<br>E3: Diferencia inválida. El sistema informa el producto ajeno a la orden o la cantidad que supera lo pendiente; Recepción revisa la entrega y vuelve a presentarla.<br>E4: Estado o permiso inválido. El sistema informa por qué la orden no puede recibirse o el actor no está autorizado; la recepción no se confirma. |

## CU-N02-01 Adherir cliente al programa de membresía

| Campo | Descripción |
|---|---|
| **Objetivo** | Registrar al cliente que acepta participar del programa de fidelización y asignarle su beneficio. |
| **Precondiciones** | El vendedor posee permiso de ventas y el cliente presta conformidad para asociarse. |
| **Poscondiciones** | El cliente queda registrado y, si corresponde, obtiene una membresía activa con su porcentaje de descuento. |
| **Eventos Disparadores** | Durante la atención, el cliente acepta asociarse a la perfumería. |
| **Flujo Normal** | 1. El vendedor solicita registrar al cliente que aceptó adherirse al programa de membresía.<br>2. El sistema presenta los datos y opciones de adhesión disponibles.<br>3. El vendedor informa los datos del cliente, su consentimiento de marketing y el beneficio acordado.<br>4. El sistema señala datos faltantes, un correo ya registrado o un beneficio fuera del rango permitido.<br>5. El vendedor completa o corrige la información y confirma el alta.<br>6. El sistema confirma el registro, muestra el resultado de la adhesión y selecciona al cliente para la venta en preparación.<br>7. El vendedor continúa la venta; el sistema considera la membresía activa al presentar los importes que corresponden. |
| **Excepciones** | E1: Cliente duplicado. El sistema informa que el correo ya está registrado; el vendedor busca al cliente existente y evita un alta duplicada.<br>E2: Datos inválidos. El sistema identifica los datos obligatorios faltantes o el correo inválido; el vendedor los corrige y reintenta.<br>E3: Descuento inválido. El sistema informa que el beneficio debe estar entre 0 % y 30 %; el vendedor lo corrige. Si el cliente ya quedó registrado, consulta ese registro antes de reintentar la adhesión.<br>E4: Sin permiso de ventas. El sistema informa que el actor no está autorizado y no acepta la operación. |

## CU-N02-02 Registrar venta con beneficio de membresía

| Campo | Descripción |
|---|---|
| **Objetivo** | Concretar una venta e informar el importe y el beneficio de membresía aplicable. |
| **Precondiciones** | El vendedor posee permiso de ventas. Los productos están activos y tienen stock disponible. |
| **Poscondiciones** | La venta y sus detalles quedan registrados, el stock se descuenta por lote y la operación queda auditada. |
| **Eventos Disparadores** | El cliente confirma los productos que desea comprar. |
| **Flujo Normal** | 1. El vendedor prepara una venta e indica productos, cantidades y un cliente registrado o la condición de venta ocasional.<br>2. El sistema muestra los productos elegidos y la identificación del cliente o la condición de cliente ocasional.<br>3. El vendedor revisa la selección y ajusta los productos, cantidades o cliente que correspondan.<br>4. El sistema presenta el subtotal, el descuento de membresía y el total antes de confirmar, o informa el impedimento para concretar la venta.<br>5. El vendedor revisa los importes informados y solicita confirmar la venta.<br>6. El sistema comprueba que la venta siga siendo admisible y confirma su registro por los importes revisados.<br>7. El vendedor verifica la información de la venta finalizada.<br>8. El sistema informa el número y el total registrado y prepara una nueva venta sin cliente seleccionado. |
| **Excepciones** | E1: Detalle inválido. El sistema solicita al vendedor agregar productos y cantidades positivas; el vendedor corrige la selección.<br>E2: Producto inválido. El sistema identifica el producto inexistente o inactivo; el vendedor lo retira o reemplaza.<br>E3: Cambio en stock o importes. El sistema informa la falta de existencias o presenta los importes actualizados sin confirmar la venta; el vendedor revisa la selección y los importes antes de reintentar.<br>E4: Sin permiso de ventas. El sistema informa que el actor no está autorizado y no acepta la operación. |
