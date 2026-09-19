# Modelado de Procesos de Negocio - Perfumeria

## 1. Contexto y Alcance

- **Contexto:** Negocio de perfumeria.
- **Alcance:** Desde la gestion de stock y aprovisionamiento hasta el motor de recomendaciones y fidelizacion de clientes.
- **Enfoque:** Procesos de negocio a nivel estrictamente organizacional (actividades humanas, decisiones y flujo documental entre los roles, sin involucrar componentes de software ni implementaciones tecnicas).
- **Actores del Negocio:**
  - **Dueno de la Perfumeria (o Responsable de Compras/Administracion):** Gestiona compras estrategicas, evaluacion de proveedores y supervision comercial.
  - **Personal de Ventas / Mostrador:** Atiende al publico, consulta y afilia miembros al programa de fidelizacion y concreta cobros.
  - **Equipo de Contabilidad:** Revisa, aprueba financieramente las compras a proveedores y analiza el impacto de costos y rentabilidad.
  - **Proveedores:** Cotizan fragancias, despachan lotes con certificados de origen/remito y remiten antecedentes de mercado.
  - **Clientes:** Compran productos en salon, acceden a membresias de fidelizacion y reciben beneficios y campanas promocionales.

---

## 2. Diagramas de Casos de Uso por Proceso de Negocio

A continuación se presentan los **Diagramas de Casos de Uso divididos individualmente por Proceso de Negocio**, delimitando con precisión sus actores participantes, límites funcionales y relaciones formales de inclusión (`<<include>>`) y extensión (`<<extend>>`).

---

### 2.1 Proceso 1: Aprovisionamiento, Control de Inventario y Trazabilidad

> **Propósito:** Gestionar el abastecimiento de fragancias garantizando el control físico de stock, la trazabilidad de origen de cada lote, la previa aprobación financiera por Contabilidad y el registro de pasivos.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P1 ["Actores Intervinientes"]
        A_Dueno1["👑 Dueño / Compras"]
        A_Prov1["🏢 Proveedor de Fragancias"]
        A_Conta1["📊 Equipo de Contabilidad"]
    end

    %% Límite del Proceso
    subgraph P1 ["📦 Proceso 1: Aprovisionamiento, Control de Inventario y Trazabilidad"]
        CU01(["CU01: Solicitar Cotizaciones de Perfumes"])
        CU02(["CU02: Emitir Cotizaciones con Certificados de Origen"])
        CU03(["CU03: Evaluar y Aprobar Presupuesto de Compra"])
        CU04(["CU04: Recepcionar Mercadería y Bultos"])
        CU05(["CU05: Validar Trazabilidad (Lote, Origen y Vto)"])
        CU06(["CU06: Ingresar Perfumes al Inventario Físico"])
        CU07(["CU07: Emitir Reclamo por Lote No Conforme"])
        CU08(["CU08: Registrar Factura y Asentar Pasivo Contable"])
    end

    %% Asociaciones
    A_Dueno1 --> CU01
    A_Prov1 --> CU02
    A_Conta1 --> CU03
    A_Dueno1 --> CU04
    A_Conta1 --> CU08

    %% Relaciones UML
    CU04 -.->|<<include>>| CU05
    CU04 -.->|<<include>>| CU06
    CU07 -.->|<<extend>>| CU04
```

---

### 2.2 Proceso 2: Venta en Mostrador y Fidelización de Clientes

> **Propósito:** Concretar ventas en mostrador incentivando la adhesión a membresías, aplicando descuentos y utilizando las preferencias para campañas de marketing.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P2 ["Actores Intervinientes"]
        A_Cli2["👤 Cliente"]
        A_Ventas2["💼 Personal de Ventas / Mostrador"]
        A_Dueno2["👑 Dueño / Marketing"]
    end

    %% Límite del Proceso
    subgraph P2 ["🛍️ Proceso 2: Venta en Mostrador y Fidelización de Clientes"]
        CU09(["CU09: Solicitar Compra de Perfumes en Salón"])
        CU10(["CU10: Concretar Venta en Mostrador"])
        CU11(["CU11: Verificar Disponibilidad en Estantería"])
        CU12(["CU12: Afiliar Cliente al Programa de Membresía"])
        CU13(["CU13: Aplicar Descuento de Fidelidad / Bienvenida"])
        CU14(["CU14: Planificar, Diseñar y Difundir Campañas de Marketing"])
    end

    %% Asociaciones
    A_Cli2 --> CU09
    A_Ventas2 --> CU10
    A_Dueno2 --> CU14

    %% Relaciones UML
    CU10 -.->|<<include>>| CU11
    CU10 -.->|<<include>>| CU13
    CU12 -.->|<<extend>>| CU10
```

---

### 2.3 Proceso 3: Evaluación y Selección Estratégica de Proveedores

> **Propósito:** Calificar y categorizar formalmente a los proveedores según prestigio, precio y antigüedad, asignando proveedores estratégicos por producto para maximizar rentabilidad.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P3 ["Actores Intervinientes"]
        A_Dueno3["👑 Dueño de la Perfumería"]
        A_Prov3["🏢 Proveedor"]
        A_Conta3["📊 Equipo de Contabilidad"]
    end

    %% Límite del Proceso
    subgraph P3 ["⭐ Proceso 3: Evaluación y Selección Estratégica de Proveedores"]
        CU15(["CU15: Solicitar Catálogos y Antecedentes Comerciales"])
        CU16(["CU16: Remitir Precios, Escalas y Certificaciones"])
        CU17(["CU17: Emitir Informe de Costos, Márgenes y Rentabilidad"])
        CU18(["CU18: Evaluar Prestigio, Precio y Antigüedad en el Mercado"])
        CU19(["CU19: Asignar Proveedor Prioritario por Producto"])
        CU20(["CU20: Formalizar Acuerdo Comercial Preferencial"])
    end

    %% Asociaciones
    A_Dueno3 --> CU15
    A_Prov3 --> CU16
    A_Conta3 --> CU17
    A_Dueno3 --> CU18
    A_Dueno3 --> CU19

    %% Relaciones UML
    CU19 -.->|<<include>>| CU18
    CU20 -.->|<<extend>>| CU19
```

---

### 2.4 Módulo Transversal: Seguridad, Autenticación y Auditoría

> **Propósito:** Proveer autenticación segura, control de acceso basado en roles, cifrado criptográfico y registro de eventos críticos en bitácora.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P4 ["Actores Intervinientes"]
        A_Usuario4["👤 Usuario del Sistema (Ventas / Contabilidad / Dueño)"]
        A_Admin4["🛡️ Administrador del Sistema"]
    end

    %% Límite del Módulo
    subgraph P4 ["🔒 Módulo Transversal: Seguridad, Autenticación y Auditoría"]
        CU21(["CU21: Iniciar / Cerrar Sesión Segura"])
        CU22(["CU22: Hashear y Encriptar Credenciales (SHA-256 / AES)"])
        CU23(["CU23: Registrar Evento Crítico en Bitácora de Auditoría"])
        CU24(["CU24: Administrar Usuarios y Asignación de Roles"])
    end

    %% Asociaciones
    A_Usuario4 --> CU21
    A_Admin4 --> CU24

    %% Relaciones UML
    CU21 -.->|<<include>>| CU22
    CU21 -.->|<<include>>| CU23
    CU24 -.->|<<include>>| CU23
```
---

## 3. Diagramas de Actividad de los Procesos de Negocio

---

### Proceso 1: Aprovisionamiento de Perfumes, Control de Inventario, Trazabilidad y Aprobacion Contable

> **Proposito:** Abastecer a la perfumeria consultando a multiples proveedores, garantizando el control fisico de stock, la trazabilidad de origen de cada lote de fragancias, la previa aprobacion financiera por el equipo contable y el resguardo del historial de compras.

```mermaid
flowchart TD
    %% Swimlane 1: Dueno de la Perfumeria / Compras
    subgraph S_Dueno ["Dueno de la Perfumeria / Compras"]
        P1_Start(["Inicio: Deteccion de necesidad de stock"]) --> P1_RevisarStock["Realizar control fisico y recuento de inventario"]
        P1_RevisarStock --> P1_ArmarPedido["Confeccionar solicitudes de cotizacion de perfumes"]
        P1_ArmarPedido --> P1_EnviarCotiz["Enviar solicitudes a multiples Proveedores"]
        
        P1_RecibirPresup["Recopilar cotizaciones y confeccionar orden preliminar"]
        P1_EnviarAprob["Enviar orden preliminar a Contabilidad para revision"]
        
        P1_EvaluarRechazo{"Ajustar orden o desistir"}
        P1_Ajustar["Ajustar cantidades / renegociar con proveedor"]
        P1_Desistir(["Fin: Compra Cancelada"])
        
        P1_EnviarOC["Emitir y enviar Orden de Compra formal aprobada"]
        P1_RecepFisica["Recibir bultos y remito con documentacion de origen"]
        P1_ControlCalidad{"Mercaderia conforme y trazabilidad verificada (lote/origen)"}
        P1_Reclamo["Emitir reclamo/devolucion formal al Proveedor"]
        P1_IngresarInv["Ingresar perfumes al inventario fisico con identificacion de lote y origen"]
        P1_ArchivarCompra["Archivar registro en el historial de compras a proveedores"]
        P1_FinExito(["Fin: Stock abastecido y compra registrada"])
    end

    %% Swimlane 2: Proveedores
    subgraph S_Prov ["Proveedores"]
        P1_RecibeSolicitud["Recibir solicitud de cotizacion"]
        P1_EmitirCotiz["Elaborar presupuesto con precios, plazos y condiciones"]
        P1_RecibirOC["Recibir Orden de Compra formal aprobada"]
        P1_PrepararLote["Preparar lote de perfumes con remito y certificados de origen"]
        P1_Despachar["Despachar pedido hacia la perfumeria"]
        P1_RecibirReclamo["Gestionar reposicion / nota de credito"]
    end

    %% Swimlane 3: Equipo de Contabilidad
    subgraph S_Conta ["Equipo de Contabilidad"]
        P1_RecibirParaAprob["Recibir orden preliminar y presupuesto de compra"]
        P1_RevisarPresup["Evaluar disponibilidad financiera y balance de costos"]
        P1_DecAprob{"Compra aprobada presupuestariamente"}
        P1_NotifAprob["Firmar / autorizar orden de compra"]
        P1_NotifRechazo["Rechazar orden e informar observaciones financieras"]
        P1_RegContable["Registrar factura y asentar pasivo para liquidacion"]
    end

    %% Conexiones entre carriles
    P1_EnviarCotiz --> P1_RecibeSolicitud
    P1_RecibeSolicitud --> P1_EmitirCotiz
    P1_EmitirCotiz --> P1_RecibirPresup
    P1_RecibirPresup --> P1_EnviarAprob
    P1_EnviarAprob --> P1_RecibirParaAprob
    P1_RecibirParaAprob --> P1_RevisarPresup
    P1_RevisarPresup --> P1_DecAprob

    P1_DecAprob -->|No| P1_NotifRechazo
    P1_NotifRechazo --> P1_EvaluarRechazo
    P1_EvaluarRechazo -->|Ajustar| P1_Ajustar
    P1_Ajustar --> P1_ArmarPedido
    P1_EvaluarRechazo -->|Desistir| P1_Desistir

    P1_DecAprob -->|Si| P1_NotifAprob
    P1_NotifAprob --> P1_EnviarOC
    P1_EnviarOC --> P1_RecibirOC
    P1_RecibirOC --> P1_PrepararLote
    P1_PrepararLote --> P1_Despachar
    P1_Despachar --> P1_RecepFisica

    P1_RecepFisica --> P1_ControlCalidad
    P1_ControlCalidad -->|No| P1_Reclamo
    P1_Reclamo --> P1_RecibirReclamo
    P1_ControlCalidad -->|Si| P1_IngresarInv
    P1_IngresarInv --> P1_ArchivarCompra
    P1_ArchivarCompra --> P1_RegContable
    P1_RegContable --> P1_FinExito
```

---

### Proceso 2: Venta y Fidelizacion de Clientes (Membresia, Descuentos y Campanas)

> **Proposito:** Concretar la venta de fragancias promoviendo la afiliacion del cliente al programa de membresia de la perfumeria, aplicando beneficios/descuentos inmediatos y utilizando los datos y preferencias registradas para la planificacion y ejecucion de campanas de marketing.

```mermaid
flowchart TD
    %% Swimlane 1: Cliente
    subgraph S_Cliente ["Cliente"]
        P2_Start(["Inicio: Interes en compra"]) --> P2_Selecc["Seleccionar perfumes y solicitar compra en mostrador"]
        P2_RespondeMembresia{"Es miembro registrado"}
        P2_PresentarDatos["Indicar datos de identificacion / credencial de miembro"]
        P2_DecAceptarReg{"Acepta registrarse como miembro"}
        P2_BrindarDatos["Proporcionar datos de contacto y preferencias olfativas"]
        P2_Pagar["Efectuar pago de la compra"]
        P2_RecibirProducto["Recibir perfume empaquetado, comprobante y bienvenida"]
        P2_RecibirCampana["Recibir comunicaciones y promociones de marketing personalizadas"]
        P2_FinCli(["Fin: Compra y fidelizacion completadas"])
    end

    %% Swimlane 2: Personal de Ventas / Mostrador
    subgraph S_Vendedor ["Personal de Ventas / Mostrador"]
        P2_Atender["Atender cliente y verificar stock en estanteria"]
        P2_ConsultarMembresia["Consultar si ya es miembro del programa de fidelizacion"]
        
        P2_ValidarMiembro["Verificar cliente en padron de miembros"]
        P2_AplicarDescMiembro["Aplicar descuento por membresia vigente"]
        
        P2_ExplicarBeneficios["Explicar beneficios del programa de fidelidad y ofrecer registro"]
        P2_TomarDatos["Completar ficha de adhesion con datos y preferencias"]
        P2_AplicarDescBienvenida["Aplicar descuento promocional de bienvenida"]
        P2_CalcularTotalEstandar["Calcular importe a tarifa estandar sin descuento"]
        
        P2_Cobrar["Cobrar importe y emitir comprobante de venta"]
        P2_Entregar["Empaquetar y entregar mercaderia junto con el ticket"]
        P2_EnviarFichas["Remitir fichas de nuevos miembros y registro de ventas a Administracion/Marketing"]
    end

    %% Swimlane 3: Dueno de la Perfumeria / Responsable de Marketing
    subgraph S_Marketing ["Dueno de la Perfumeria / Responsable de Marketing"]
        P2_RecibirFichas["Recibir planillas de ventas y fichas de adhesion"]
        P2_ActualizarPadron["Consolidar padron general de miembros fidelizados"]
        P2_Segmentar["Segmentar clientes por historial de compra y notas olfativas preferidas"]
        P2_DisenarCampana["Disenar campanas de marketing directo y promociones exclusivas"]
        P2_LanzarCampana["Difundir campana de marketing por canales de contacto"]
        P2_FinMkt(["Fin: Campana de fidelizacion activa"])
    end

    %% Conexiones entre carriles
    P2_Selecc --> P2_Atender
    P2_Atender --> P2_ConsultarMembresia
    P2_ConsultarMembresia --> P2_RespondeMembresia

    %% Camino Miembro Existente
    P2_RespondeMembresia -->|Si| P2_PresentarDatos
    P2_PresentarDatos --> P2_ValidarMiembro
    P2_ValidarMiembro --> P2_AplicarDescMiembro
    P2_AplicarDescMiembro --> P2_Cobrar

    %% Camino No Miembro
    P2_RespondeMembresia -->|No| P2_ExplicarBeneficios
    P2_ExplicarBeneficios --> P2_DecAceptarReg

    P2_DecAceptarReg -->|Si| P2_BrindarDatos
    P2_BrindarDatos --> P2_TomarDatos
    P2_TomarDatos --> P2_AplicarDescBienvenida
    P2_AplicarDescBienvenida --> P2_Cobrar

    P2_DecAceptarReg -->|No| P2_CalcularTotalEstandar
    P2_CalcularTotalEstandar --> P2_Cobrar

    %% Cierre de Venta en mostrador
    P2_Cobrar --> P2_Pagar
    P2_Pagar --> P2_Entregar
    P2_Entregar --> P2_RecibirProducto
    P2_RecibirProducto --> P2_FinCli

    %% Traspaso al area de Marketing / Dueno
    P2_TomarDatos --> P2_EnviarFichas
    P2_ValidarMiembro --> P2_EnviarFichas
    P2_EnviarFichas --> P2_RecibirFichas
    P2_RecibirFichas --> P2_ActualizarPadron
    P2_ActualizarPadron --> P2_Segmentar
    P2_Segmentar --> P2_DisenarCampana
    P2_DisenarCampana --> P2_LanzarCampana
    P2_LanzarCampana --> P2_RecibirCampana
    P2_LanzarCampana --> P2_FinMkt
```

---

### Proceso 3: Valor Agregado - Evaluacion y Seleccion Estrategica de Proveedores por Producto

> **Proposito:** Evaluar y clasificar formalmente a los proveedores en funcion de **Prestigio**, **Precio** y **Antiguedad en el mercado**, asignando proveedores preferenciales por perfume o linea de productos para evitar perdidas y maximizar la rentabilidad del negocio.

```mermaid
flowchart TD
    %% Swimlane 1: Dueno de la Perfumeria
    subgraph S_Dueno3 ["Dueno de la Perfumeria"]
        P3_Start(["Inicio: Deteccion de lineas/productos a optimizar"]) --> P3_DefinirProd["Identificar catalogo de perfumes y lineas con riesgo de perdidas o baja rotacion"]
        P3_DefinirProd --> P3_SolicitarInfo["Solicitar catalogos, listas de precios y antecedentes a multiples Proveedores"]
        
        P3_ConsolidarProp["Reunir propuestas comerciales y legajos de proveedores"]
        P3_AnalizarPrestigio["Evaluar Prestigio (reputacion de marca, autenticidad, certificaciones, calidad)"]
        P3_AnalizarAntiguedad["Evaluar Antiguedad en el mercado (trayectoria comercial, solvencia historica)"]
        P3_ConsolidarAnalisis["Integrar informe cualitativo y cuantitativo en matriz de decision"]
        
        P3_Deliberar{"Supera umbrales de prestigio, precio y antiguedad"}
        P3_Descartar["Descartar proveedor para ese producto / convocar nuevas alternativas"]
        P3_RevisarNuevos(["Fin: Reevaluacion diferida"])
        
        P3_Seleccionar["Seleccionar el Proveedor mas conveniente para cada perfume"]
        P3_AsignarPrioridad["Asignar categoria de Proveedor Prioritario en el directorio de compras"]
        P3_Acuerdo["Formalizar acuerdo comercial preferencial de suministro"]
        P3_FinExito(["Fin: Proveedores estrategicos asignados por producto"])
    end

    %% Swimlane 2: Proveedores
    subgraph S_Prov3 ["Proveedores"]
        P3_RecibirSolicitud["Recibir solicitud de antecedentes y lista de precios"]
        P3_EnviarDoc["Remitir listas de precios con escalas, trayectoria de mercado y certificados de autenticidad"]
    end

    %% Swimlane 3: Equipo de Contabilidad
    subgraph S_Conta3 ["Equipo de Contabilidad"]
        P3_RecibirPrecios["Recibir propuestas de precios y condiciones de pago"]
        P3_AnalizarCostos["Analizar margenes de ganancia, costos logisticos, descuentos por volumen e impacto financiero"]
        P3_EmitirDictamen["Emitir informe de viabilidad financiera y comparativa de costos"]
    end

    %% Conexiones entre carriles
    P3_SolicitarInfo --> P3_RecibirSolicitud
    P3_RecibirSolicitud --> P3_EnviarDoc
    P3_EnviarDoc --> P3_ConsolidarProp
    
    P3_ConsolidarProp --> P3_AnalizarPrestigio
    P3_ConsolidarProp --> P3_AnalizarAntiguedad
    P3_ConsolidarProp --> P3_RecibirPrecios
    
    P3_RecibirPrecios --> P3_AnalizarCostos
    P3_AnalizarCostos --> P3_EmitirDictamen
    
    P3_AnalizarPrestigio --> P3_ConsolidarAnalisis
    P3_AnalizarAntiguedad --> P3_ConsolidarAnalisis
    P3_EmitirDictamen --> P3_ConsolidarAnalisis
    
    P3_ConsolidarAnalisis --> P3_Deliberar
    
    P3_Deliberar -->|No| P3_Descartar
    P3_Descartar --> P3_RevisarNuevos
    
    P3_Deliberar -->|Si| P3_Seleccionar
    P3_Seleccionar --> P3_AsignarPrioridad
    P3_AsignarPrioridad --> P3_Acuerdo
    P3_Acuerdo --> P3_FinExito
```

---

## 4. Diagramas de Secuencia de Negocio

Los participantes representan personas, areas y terceros de la organizacion. Los diagramas describen el intercambio de documentos, decisiones y mercaderia sin introducir componentes del sistema.

### 4.1 Aprovisionamiento, aprobacion contable y trazabilidad

```mermaid
sequenceDiagram
    actor Compras as Dueno / Compras
    participant Inventario as Responsable de Inventario
    participant Proveedores as Proveedores
    participant Contabilidad as Equipo de Contabilidad
    participant Recepcion as Recepcion / Deposito

    Compras->>Inventario: Solicitar relevamiento de existencias y faltantes
    Inventario-->>Compras: Entregar informe de necesidades de reposicion

    loop Multiples proveedores
        Compras->>Proveedores: Solicitar cotizacion, origen y plazo de entrega
        Proveedores-->>Compras: Remitir cotizacion y documentacion comercial
    end

    Compras->>Contabilidad: Presentar comparativa y orden preliminar
    alt Compra rechazada
        Contabilidad-->>Compras: Informar rechazo y observaciones financieras
        Compras->>Proveedores: Renegociar cantidades, precio o condiciones
    else Compra aprobada
        Contabilidad-->>Compras: Autorizar presupuesto y orden de compra
        Compras->>Proveedores: Emitir orden de compra aprobada
        Proveedores->>Recepcion: Despachar mercaderia, remito y certificados de origen
        Recepcion->>Inventario: Entregar lotes y documentacion para control
        Inventario->>Inventario: Verificar cantidad, lote, origen y vencimiento
        alt Mercaderia conforme
            Inventario-->>Compras: Confirmar ingreso y actualizar historial de compra
            Recepcion->>Contabilidad: Remitir factura y conformidad de recepcion
            Contabilidad->>Contabilidad: Registrar pasivo para liquidacion
        else Lote no conforme
            Inventario->>Proveedores: Emitir reclamo o devolucion
            Proveedores-->>Recepcion: Coordinar reposicion o nota de credito
        end
    end
```

### 4.2 Venta y fidelizacion de clientes

```mermaid
sequenceDiagram
    actor Cliente
    participant Ventas as Personal de Ventas
    participant Caja
    participant Marketing as Dueno / Marketing

    Cliente->>Ventas: Solicitar perfumes seleccionados
    Ventas->>Ventas: Verificar disponibilidad fisica
    Ventas->>Cliente: Consultar pertenencia al programa de membresia

    alt Cliente ya afiliado
        Cliente-->>Ventas: Presentar identificacion de miembro
        Ventas->>Ventas: Verificar vigencia y calcular beneficio
    else Cliente no afiliado
        Ventas->>Cliente: Explicar beneficios y ofrecer adhesion
        alt Cliente acepta
            Cliente-->>Ventas: Entregar datos de contacto y preferencias olfativas
            Ventas->>Ventas: Completar ficha y aplicar beneficio de bienvenida
        else Cliente no acepta
            Ventas->>Ventas: Mantener tarifa general
        end
    end

    Ventas->>Caja: Remitir detalle, descuento e importe final
    Caja->>Cliente: Cobrar y emitir comprobante
    Cliente-->>Caja: Efectuar pago
    Caja-->>Ventas: Confirmar cobro
    Ventas-->>Cliente: Entregar perfumes y comprobante
    Ventas->>Marketing: Remitir ficha de adhesion e historial de compra
    Marketing->>Marketing: Segmentar por historial y preferencias
    Marketing-->>Cliente: Difundir promociones por canales autorizados
```

### 4.3 Evaluacion estrategica de proveedores

```mermaid
sequenceDiagram
    actor Dueno as Dueno de la Perfumeria
    participant Proveedores
    participant Contabilidad as Equipo de Contabilidad

    Dueno->>Dueno: Identificar productos con riesgo de perdida o baja rotacion
    loop Proveedores candidatos
        Dueno->>Proveedores: Solicitar precios, antecedentes y certificaciones
        Proveedores-->>Dueno: Remitir propuesta comercial y trayectoria
    end

    Dueno->>Contabilidad: Remitir propuestas y condiciones de pago
    Contabilidad->>Contabilidad: Analizar costos, margenes e impacto financiero
    Contabilidad-->>Dueno: Emitir comparativa de costos y viabilidad
    Dueno->>Dueno: Ponderar prestigio, precio y antiguedad

    alt No supera los umbrales
        Dueno-->>Proveedores: Informar que la propuesta no fue seleccionada
        Dueno->>Dueno: Programar una nueva evaluacion
    else Supera los umbrales
        Dueno->>Proveedores: Proponer acuerdo preferencial por producto
        Proveedores-->>Dueno: Aceptar o renegociar condiciones
        Dueno->>Dueno: Designar proveedor prioritario
    end
```

---

## 5. Correspondencia con el plan y la implementacion

La correspondencia se documenta fuera de los diagramas para mantenerlos centrados en el negocio.

| Codigo del plan | Proceso | Iteracion |
| :--- | :--- | :--- |
| N01 | Aprovisionamiento, aprobacion contable, inventario y trazabilidad | Diseno en Iteracion 1; implementacion en Iteracion 2 |
| N02 | Venta en mostrador y fidelizacion | Diseno e implementacion en Iteracion 2 |
| N03 | Evaluacion y recomendacion de proveedores | Implementacion planificada para Iteracion 3 |

| Actividad organizacional | Operacion de negocio implementada |
| :--- | :--- |
| Registrar propuestas de multiples proveedores | `AprovisionamientoBLL.RegistrarCotizacion` |
| Confeccionar una orden preliminar | `AprovisionamientoBLL.CrearOrdenPreliminar` |
| Presentar la comparativa a Contabilidad | `AprovisionamientoBLL.EnviarOrdenARevisionContable` |
| Autorizar o rechazar la compra | `AprovisionamientoBLL.AprobarOrdenCompra` / `RechazarOrdenCompra` |
| Emitir la orden aprobada al proveedor | `AprovisionamientoBLL.EmitirOrdenAprobada` |
| Recepcionar mercaderia con origen y lote | `AprovisionamientoBLL.RecepcionarMercaderia` |
| Consultar stock y trazabilidad | `InventarioBLL.VerificarStock` / `ObtenerTrazabilidad` |
| Consultar compras por proveedor | `AprovisionamientoBLL.ConsultarHistorialCompras` |
| Registrar cliente, preferencias y consentimiento | `VentaBLL.RegistrarCliente` |
| Incorporar al cliente al programa de fidelidad | `VentaBLL.ActivarMembresia` |
| Registrar la venta y aplicar el descuento | `VentaBLL.RegistrarVenta` |
| Reservar existencias por vencimiento mas proximo | `VentaBLL.RegistrarVenta` y `IVentaRepository.GuardarVenta` |
| Evaluar y recomendar proveedores | N03: pendiente de la Iteracion 3 |

---

## 6. Resumen de Reglas de Negocio Organizacionales

| Proceso de Negocio | Actores Clave (Minimo 3 Swimlanes) | Regla / Politica Organizacional |
| :--- | :--- | :--- |
| **1. Aprovisionamiento y Trazabilidad** | 1. Dueno / Compras<br>2. Proveedores<br>3. Equipo de Contabilidad | Toda compra requiere cotizaciones comparativas y autorizacion previa de Contabilidad. Es obligatorio verificar y registrar el lote y origen de cada producto al recepcionarlo fisicamente. |
| **2. Fidelizacion de Clientes** | 1. Cliente<br>2. Personal de Ventas<br>3. Dueno / Marketing | En cada venta en mostrador se ofrece membresia. El alta otorga descuentos inmediatos y alimenta la base de datos para la segmentacion y ejecucion de campanas de marketing. |
| **3. Valor Agregado (Seleccion de Proveedores)** | 1. Dueno de la Perfumeria<br>2. Proveedores<br>3. Equipo de Contabilidad | La eleccion del proveedor optimo por producto combina **prestigio**, **antiguedad** y **analisis financiero de costos/margenes** para mitigar perdidas y maximizar la rentabilidad. |
