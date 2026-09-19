# Casos de Uso Principales N01 y N02

## CU-N01-01 Preparar y enviar orden de compra a revisión

| Campo | Descripción |
|---|---|
| **Objetivo** | Registrar cotizaciones comparables, crear una orden preliminar y remitirla a Contabilidad. |
| **Precondiciones** | El usuario posee permiso de aprovisionamiento. El proveedor y los productos están activos. |
| **Poscondiciones** | La orden queda pendiente de aprobación contable y la operación queda registrada en bitácora. |
| **Eventos Disparadores** | El Dueño o responsable de Compras decide reponer uno o más productos. |
| **Flujo Normal** | 1. Registra las cotizaciones vigentes recibidas de proveedores.<br>2. Selecciona un proveedor y agrega los productos, cantidades y precios.<br>3. El sistema valida proveedor, productos, cantidades y precios.<br>4. El sistema calcula el total y guarda la orden en estado Borrador.<br>5. El usuario solicita enviarla a revisión.<br>6. El sistema verifica que cada producto tenga al menos dos cotizaciones vigentes de proveedores distintos.<br>7. El sistema cambia la orden a Pendiente de aprobación y registra el evento. |
| **Excepciones** | **E1: Sin permiso.** Se rechaza la operación.<br>**E2: Cotización inválida.** Si está vencida, el precio no es positivo, el plazo es negativo o hay productos duplicados, no se registra.<br>**E3: Datos inactivos.** Si el proveedor o un producto no existe o está inactivo, no se crea la orden.<br>**E4: Comparativa insuficiente.** Si faltan dos cotizaciones comparables para algún producto, la orden permanece en Borrador. |

## CU-N01-02 Revisar orden de compra

| Campo | Descripción |
|---|---|
| **Objetivo** | Permitir que Contabilidad apruebe o rechace una compra antes de emitirla al proveedor. |
| **Precondiciones** | El usuario posee permiso contable y la orden está Pendiente de aprobación. |
| **Poscondiciones** | La orden queda Aprobada o Rechazada con la identidad del aprobador o el motivo de rechazo. |
| **Eventos Disparadores** | Contabilidad recibe una orden pendiente para controlar su conveniencia económica. |
| **Flujo Normal** | 1. Contabilidad consulta la orden, su proveedor, sus productos y el total.<br>2. Verifica la documentación y la disponibilidad presupuestaria.<br>3. Selecciona Aprobar.<br>4. El sistema valida el estado actual de la orden.<br>5. El sistema registra al usuario aprobador, cambia el estado a Aprobada y deja trazabilidad en bitácora.<br>6. El responsable de Compras emite la orden aprobada al proveedor. |
| **Excepciones** | **E1: Rechazo.** Contabilidad informa un motivo obligatorio y la orden queda Rechazada.<br>**E2: Estado inválido.** Si la orden no está Pendiente de aprobación, no se permite decidir nuevamente.<br>**E3: Cambio concurrente.** Si la orden cambió de estado durante la operación, se solicita volver a consultarla.<br>**E4: Sin permiso contable.** Se rechaza la operación. |

## CU-N01-03 Recepcionar mercadería y registrar trazabilidad

| Campo | Descripción |
|---|---|
| **Objetivo** | Registrar la recepción total o parcial de una orden y crear lotes trazables con su ingreso de stock. |
| **Precondiciones** | El usuario posee permiso de recepción. La orden está Emitida o Recibida parcialmente. |
| **Poscondiciones** | Se crean los lotes y movimientos de ingreso. La orden queda Recibida o Recibida parcialmente. |
| **Eventos Disparadores** | El proveedor entrega mercadería correspondiente a una orden emitida. |
| **Flujo Normal** | 1. Recepción selecciona la orden emitida.<br>2. Informa producto, código de lote, origen, certificado, cantidad y vencimiento.<br>3. El sistema valida que el producto pertenezca a la orden y que el lote sea válido.<br>4. El sistema controla que la recepción acumulada no supere lo comprado.<br>5. Registra cada lote con el proveedor y la orden de origen.<br>6. Registra los movimientos de ingreso y actualiza el stock disponible.<br>7. Marca la orden como Recibida si se completaron todas las cantidades o como Recibida parcialmente en caso contrario.<br>8. Registra la recepción en bitácora. |
| **Excepciones** | **E1: Sin lotes.** No se registra la recepción.<br>**E2: Lote inválido.** Si falta código, origen o certificado, la cantidad no es positiva o el lote está vencido, se rechaza.<br>**E3: Diferencia inválida.** Si el producto no pertenece a la orden o la cantidad supera lo comprado, se cancela la recepción.<br>**E4: Estado o permiso inválido.** Si la orden no puede recepcionarse o el usuario carece de permiso, se rechaza la operación. |

## CU-N02-01 Adherir cliente al programa de membresía

| Campo | Descripción |
|---|---|
| **Objetivo** | Registrar al cliente que acepta participar del programa de fidelización y asignarle su beneficio. |
| **Precondiciones** | El vendedor posee permiso de ventas y el cliente presta conformidad para asociarse. |
| **Poscondiciones** | El cliente queda registrado y, si corresponde, obtiene una membresía activa con su porcentaje de descuento. |
| **Eventos Disparadores** | Durante la atención, el cliente acepta asociarse a la perfumería. |
| **Flujo Normal** | 1. El vendedor solicita nombre, apellido, correo y preferencias autorizadas.<br>2. El sistema valida los datos obligatorios y el formato del correo.<br>3. El sistema verifica que el correo no esté registrado.<br>4. El vendedor confirma la creación de la membresía y el porcentaje de descuento.<br>5. El sistema valida que el descuento esté entre 0 % y 30 %.<br>6. El sistema registra al cliente, genera su número de socio y crea la membresía.<br>7. El sistema registra el alta para auditoría. |
| **Excepciones** | **E1: Cliente duplicado.** Se informa la duplicidad y no se crea otro registro.<br>**E2: Datos inválidos.** Se solicita completar los datos obligatorios o corregir el correo.<br>**E3: Descuento inválido.** Si está fuera del rango permitido, no se crea la membresía.<br>**E4: Sin permiso de ventas.** Se rechaza la operación. |

## CU-N02-02 Registrar venta con beneficio de membresía

| Campo | Descripción |
|---|---|
| **Objetivo** | Concretar una venta, aplicar el beneficio vigente del cliente y descontar stock por lote usando FEFO. |
| **Precondiciones** | El vendedor posee permiso de ventas. Los productos están activos y tienen stock disponible. |
| **Poscondiciones** | La venta y sus detalles quedan registrados, el stock se descuenta por lote y la operación queda auditada. |
| **Eventos Disparadores** | El cliente confirma los productos que desea comprar. |
| **Flujo Normal** | 1. El vendedor agrega los productos y cantidades solicitadas.<br>2. Si el cliente se identifica, el sistema recupera su membresía activa.<br>3. El sistema verifica productos activos y stock suficiente.<br>4. El sistema asigna las unidades a los lotes disponibles con vencimiento más próximo.<br>5. Calcula subtotal, descuento de membresía y total.<br>6. El vendedor confirma la operación.<br>7. El sistema registra la venta y sus detalles, descuenta el stock y registra el evento en bitácora.<br>8. La venta finaliza con el cliente identificado o como cliente ocasional sin descuento. |
| **Excepciones** | **E1: Detalle inválido.** Si no hay productos o alguna cantidad no es positiva, no se registra.<br>**E2: Producto inválido.** Si no existe o está inactivo, se informa el producto afectado.<br>**E3: Stock insuficiente.** Se cancela la venta sin modificar existencias.<br>**E4: Sin permiso de ventas.** Se rechaza la operación. |
