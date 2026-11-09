# Caso de uso principal N03

La especificación CU018 se conserva como un único caso principal en Google Docs. Sus cuatro responsabilidades se describen por separado en las notas de los casos de uso de EA.

## CU018 Evaluación y selección de proveedores por producto

| Campo | Descripción |
|---|---|
| **Objetivo** | Comparar proveedores para un producto y una cantidad, cruzando propuestas comerciales, stock, ventas y compras pendientes, para fundamentar la elección y, si se decide comprar, iniciar un borrador. |
| **Precondiciones** | Compras está autorizado para evaluar proveedores y el producto está activo. |
| **Poscondiciones** | Se conserva la comparación y, si se confirma una oferta, la decisión fundamentada. Iniciar una compra mediante un borrador vinculado es opcional. |
| **Eventos Disparadores** | El Dueño o responsable de Compras necesita decidir de quién abastecerse para un producto y una cantidad. |
| **Flujo Normal** | 1. Compras indica producto, cantidad y criterios y solicita comparar proveedores.<br>2. El sistema presenta el ranking justificado, costos, margen y reposición sugerida según stock, ventas y compras pendientes; explica exclusiones y advertencias.<br>3. Compras revisa las alternativas, elige una oferta y fundamenta su decisión.<br>4. El sistema comprueba la vigencia de la comparación y registra la elección, distinguiéndola de la recomendación.<br>5. Compras solicita iniciar la compra, si corresponde.<br>6. El sistema informa el borrador vinculado y permite abrirlo para continuar la revisión contable de N01. |
| **Excepciones** | E1: Datos inválidos o evaluación desactualizada. El sistema solicita corregir o comparar nuevamente; conserva la evidencia y el fundamento en preparación al cambiar cantidad o criterios.<br>E2: Elección inválida o sin fundamento. El sistema solicita revisar la oferta y justificar la decisión.<br>E3: Sin ofertas comparables o elegibles. El sistema conserva la comparación sin recomendación y explica la causa.<br>E4: Decisión sin guardar. Al cambiar de producto o abrir otra evaluación, el sistema pide confirmar su descarte; cancelar conserva el trabajo.<br>E5: Sin autorización. El sistema informa que el actor no puede realizar la evaluación. |

## CU018.1 Comparar proveedores para un producto y cantidad

| Campo | Descripción |
|---|---|
| **Objetivo** | Comparar ofertas mediante información comercial, inventario, ventas y compras pendientes. |
| **Precondiciones** | Compras posee permiso de evaluación y el producto está activo. |
| **Poscondiciones** | Se conserva una comparación fechada con sus fuentes y resultados, sin movimientos de stock. |
| **Eventos Disparadores** | Compras necesita decidir de quién abastecerse. |
| **Flujo Normal** | 1. Compras indica producto, cantidad y criterios y solicita comparar.<br>2. El sistema presenta la recomendación justificada, alternativas, costos, margen y reposición sugerida.<br>3. Compras revisa las ofertas y puede ajustar cantidad o criterios.<br>4. El sistema renueva la comparación solicitada y conserva el fundamento en preparación para su revisión. |
| **Excepciones** | E1: Datos inválidos: el sistema solicita corregirlos.<br>E2: Sin ofertas comparables o elegibles: se conserva la comparación sin recomendación y se explica la causa.<br>E3: Cambio de producto con una decisión sin guardar: el sistema consulta si se desea descartarla; cancelar conserva el trabajo. |

## CU018.2 Confirmar proveedor con fundamento

| Campo | Descripción |
|---|---|
| **Objetivo** | Conservar la elección de una oferta elegible y su justificación. |
| **Precondiciones** | Existe una comparación vigente con una oferta elegible y el actor tiene permiso de Compras. |
| **Poscondiciones** | Quedan registrados proveedor, fundamento, responsable y fecha; no se crea una compra. |
| **Eventos Disparadores** | Compras decide qué oferta conviene. |
| **Flujo Normal** | 1. Compras selecciona una oferta.<br>2. El sistema muestra sus costos, margen y advertencias, junto con la recomendación justificada.<br>3. Compras fundamenta su elección y solicita confirmarla.<br>4. El sistema registra la decisión, la distingue de la recomendación y permite iniciar opcionalmente una compra. |
| **Excepciones** | E1: Motivo insuficiente u oferta excluida: el sistema solicita corregir la selección.<br>E2: Datos modificados o de otra fecha: se requiere comparar nuevamente.<br>E3: Decisión ya confirmada: se conserva y se inicia otra comparación si se desea reconsiderarla. |

## CU018.3 Iniciar una orden de compra en Borrador

| Campo | Descripción |
|---|---|
| **Objetivo** | Continuar una decisión de abastecimiento por el circuito habitual de compras. |
| **Precondiciones** | Compras confirmó una oferta elegible y decide realizar la compra. |
| **Poscondiciones** | Existe un único borrador vinculado a la evaluación, sin aprobación ni recepción. |
| **Eventos Disparadores** | Compras solicita iniciar la compra de la oferta elegida. |
| **Flujo Normal** | 1. Compras solicita crear el borrador.<br>2. El sistema informa su número y conserva proveedor, producto, cantidad y precio evaluados.<br>3. Compras solicita abrir la orden.<br>4. El sistema presenta y selecciona ese borrador para continuar la revisión contable de N01. |
| **Excepciones** | E1: Datos modificados o de otra fecha: se requiere una nueva comparación sin crear una orden parcial.<br>E2: Borrador ya vinculado: se presenta la orden existente; no se duplica por reintentos. |

## CU018.4 Consultar historial y fundamentos

| Campo | Descripción |
|---|---|
| **Objetivo** | Consultar la evidencia y las decisiones anteriores de abastecimiento. |
| **Precondiciones** | El actor tiene permiso de Compras y seleccionó un producto. |
| **Poscondiciones** | Se muestran los resultados guardados sin alterar la evidencia. |
| **Eventos Disparadores** | Compras necesita explicar o revisar una evaluación anterior. |
| **Flujo Normal** | 1. Compras solicita consultar el historial.<br>2. El sistema presenta las comparaciones fechadas del producto.<br>3. Compras elige una comparación.<br>4. El sistema muestra sus criterios, fuentes, resultados, decisión y orden vinculada, si existen. |
| **Excepciones** | E1: Sin comparaciones: el sistema informa que todavía no hay historial para el producto.<br>E2: Se necesita decidir con datos actuales: Compras solicita una nueva comparación; la anterior permanece disponible.<br>E3: Decisión actual sin guardar: el sistema solicita confirmar su descarte antes de abrir otra evaluación; cancelar conserva el trabajo. |
