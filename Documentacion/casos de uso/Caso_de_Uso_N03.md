# Caso de uso principal N03

La especificación CU018 se conserva como un único caso principal en Google Docs. Sus cuatro responsabilidades se describen por separado en las notas de los casos de uso de EA.

## CU018 Evaluación y selección de proveedores por producto

| Campo | Descripción |
|---|---|
| **Objetivo** | Comparar proveedores para un producto y una cantidad, cruzando propuestas comerciales, stock, ventas y compras pendientes, para fundamentar la elección y, si se decide comprar, iniciar un borrador. |
| **Precondiciones** | El actor posee permiso de Compras/Evaluación y el producto está activo. Las propuestas disponibles pueden consultarse para determinar si permiten recomendar un proveedor. |
| **Poscondiciones** | Se conserva una comparación fechada. Si Compras confirma una oferta elegible, queda registrada la decisión fundamentada; iniciar un borrador vinculado es opcional. |
| **Eventos Disparadores** | El Dueño o responsable de Compras necesita decidir de quién abastecerse para un producto y una cantidad. |
| **Flujo Normal** | 1. El Dueño / Departamento de Compras indica el producto, la cantidad y los criterios que desea considerar y solicita comparar proveedores.<br>2. El sistema presenta el ranking, margen, plazos y antecedentes, junto con stock utilizable, ventas, compras pendientes y reposición sugerida; identifica las ofertas excluidas y sus causas.<br>3. Compras revisa los resultados y confirma la selección del proveedor prioritario, indicando el fundamento de la decisión.<br>4. El sistema valida la vigencia de la evaluación y registra la decisión fundamentada.<br>5. Compras solicita, si decide realizar la compra, una orden en borrador basada en la decisión confirmada.<br>6. El sistema informa el número del borrador vinculado y permite abrirlo para continuar por el circuito de compras y revisión contable de N01. |
| **Excepciones** | E1: Datos inválidos, modificados o de otra fecha. El sistema solicita corregir los datos o comparar nuevamente antes de confirmar una decisión o crear una compra; la evidencia anterior se conserva.<br>E2: Falta de fundamento o motivo. El sistema requiere especificar el motivo de la selección; Compras completa la información requerida.<br>E3: Sin ofertas comparables o elegibles. El sistema conserva la comparación sin recomendación y explica la causa; Compras puede solicitar nuevas cotizaciones. Si cambia la cantidad, incluso para adoptar la reposición sugerida, debe comparar nuevamente antes de confirmar.<br>E4: Sin permiso. El sistema informa que el actor no está autorizado para realizar la evaluación. |

## CU018.1 Comparar proveedores para un producto y cantidad

| Campo | Descripción |
|---|---|
| **Objetivo** | Comparar ofertas mediante información comercial, inventario, ventas y compras pendientes. |
| **Precondiciones** | Compras posee permiso de evaluación y el producto está activo. |
| **Poscondiciones** | Se conserva una comparación fechada con sus fuentes y resultados, sin movimientos de stock. |
| **Eventos Disparadores** | Compras necesita decidir de quién abastecerse. |
| **Flujo Normal** | 1. Compras indica producto, cantidad y criterios y solicita comparar.<br>2. El sistema presenta puntajes, margen, plazos y antecedentes junto con la reposición sugerida.<br>3. Compras consulta las alternativas y, si lo necesita, adopta otra cantidad.<br>4. El sistema vuelve a comparar para la nueva cantidad y conserva los resultados para su consulta. |
| **Excepciones** | E1: Producto, cantidad o criterios inválidos: el sistema solicita corregirlos.<br>E2: Sin ofertas comparables o elegibles: se conserva la comparación sin recomendación y se explica la causa. |

## CU018.2 Confirmar proveedor con fundamento

| Campo | Descripción |
|---|---|
| **Objetivo** | Conservar la elección de una oferta elegible y su justificación. |
| **Precondiciones** | Existe una comparación vigente con una oferta elegible y el actor tiene permiso de Compras. |
| **Poscondiciones** | Quedan registrados proveedor, fundamento, responsable y fecha; no se crea una compra. |
| **Eventos Disparadores** | Compras decide qué oferta conviene. |
| **Flujo Normal** | 1. Compras selecciona una oferta.<br>2. El sistema muestra sus fundamentos y advertencias.<br>3. Compras indica el motivo y solicita confirmar.<br>4. El sistema confirma la decisión y permite iniciar opcionalmente una compra. |
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
| **Excepciones** | E1: Sin comparaciones: el sistema informa que todavía no hay historial para el producto.<br>E2: Se necesita decidir con datos actuales: Compras solicita una nueva comparación; la anterior permanece disponible. |
