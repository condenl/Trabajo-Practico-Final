# Descripcion Global del Producto y Arquitectura del Sistema

## G01. Proposito

El sistema de informacion para la perfumeria tiene como proposito resolver las siguientes problematicas organizacionales y operativas:

1. **Control de Inventario y Trazabilidad:** Eliminar discrepancias de stock y garantizar la trazabilidad de origen y lote de cada fragancia comercializada, mitigando riesgos de falsificacion o perdidas.
2. **Gobernanza Financiera en Compras:** Formalizar el circuito de compras a multiples proveedores, estableciendo una instancia de aprobacion obligatoria por el equipo de contabilidad previa a la emision de ordenes de compra.
3. **Fidelizacion de Clientes:** Implementar un programa de membresia en el punto de venta para captar datos de contacto y preferencias olfativas, aplicando descuentos inmediatos y habilitando campanas de marketing segmentadas.
4. **Optimizacion en la Seleccion de Proveedores:** Dotar a la administracion de una matriz multicriterio para evaluar proveedores segun prestigio, precio y antiguedad en el mercado, asignando compras prioritarias por producto para maximizar rentabilidad y evitar perdidas.
5. **Seguridad y Auditoria:** Garantizar la integridad transaccional, trazabilidad de operaciones criticas y control de accesos basado en perfiles.

---

## G02. Descripcion Funcional del Producto y Alcance

### 2.1 Gestiones Organizacionales Involucradas

1. **Gestion de Aprovisionamiento, Stock y Trazabilidad:**
   - Emision de solicitudes de cotizacion a multiples proveedores.
   - Registro de ordenes de compra preliminares sujetas a aprobacion contable.
   - Recepcion fisica de mercaderia, control de calidad y registro de lotes/certificados de origen.
   - Control de inventario y archivo historico de compras.

2. **Gestion de Ventas y Fidelizacion:**
   - Atencion y despacho de productos en mostrador.
   - Identificacion y registro de clientes en el programa de membresia.
   - Aplicacion automatica de beneficios y descuentos vigentes.
   - Consolidacion de datos y preferencias para campanas de marketing directo.

3. **Gestion de Evaluacion Estrategica de Proveedores:**
   - Relevamiento de propuestas comerciales, antecedentes y certificaciones.
   - Evaluacion ponderada de tres dimensiones: Prestigio, Precio/Impacto Contable y Antiguedad.
   - Asignacion de proveedor preferencial por producto o familia olfativa.

4. **Gestion de Seguridad, Auditoria y Soporte:**
   - Autenticacion y autorizacion mediante control de acceso basado en roles.
   - Registro inmutable de eventos en bitacora.
   - Verificacion de integridad de datos mediante digitos verificadores.

### 2.2 Limites del Alcance

- **Incluido:** Gestion integral de stock, trazabilidad por lote, compras con flujo contable, ventas con membresia, evaluacion de proveedores, auditoria y seguridad en 4 capas.
- **Excluido:** Integracion con pasarelas de pago bancarias externas en tiempo real (se asienta comprobante de cobro), comercio electronico web B2C (alcance limitado a terminal de punto de venta y gestion administrativa) y fabricacion directa de fragancias.

### 2.3 Diagramas de Casos de Uso por Proceso de Negocio

A continuación se presentan los **Diagramas de Casos de Uso divididos individualmente por Proceso de Negocio**, delimitando con precisión sus actores participantes, límites funcionales y relaciones formales de inclusión (`<<include>>`) y extensión (`<<extend>>`).

---

#### 2.3.1 Proceso 1: Aprovisionamiento, Control de Inventario y Trazabilidad

> **Propósito:** Gestionar el abastecimiento de fragancias garantizando el control físico de stock, la trazabilidad de origen de cada lote, la previa aprobación financiera por Contabilidad y el registro de pasivos.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P1 ["Actores Intervinientes"]
        A_Dueno1["👤 Dueño / Compras"]
        A_Prov1["👤 Proveedor de Fragancias"]
        A_Conta1["👤 Equipo de Contabilidad"]
    end

    %% Límite del Proceso
    subgraph P1 ["Proceso 1: Aprovisionamiento, Control de Inventario y Trazabilidad"]
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

#### 2.3.2 Proceso 2: Venta en Mostrador y Fidelización de Clientes

> **Propósito:** Concretar ventas en mostrador incentivando la adhesión a membresías, aplicando descuentos y utilizando las preferencias para campañas de marketing.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P2 ["Actores Intervinientes"]
        A_Cli2["👤 Cliente"]
        A_Ventas2["👤 Personal de Ventas / Mostrador"]
        A_Dueno2["👤 Dueño / Marketing"]
    end

    %% Límite del Proceso
    subgraph P2 ["Proceso 2: Venta en Mostrador y Fidelización de Clientes"]
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

#### 2.3.3 Proceso 3: Evaluación y Selección Estratégica de Proveedores

> **Propósito:** Calificar y categorizar formalmente a los proveedores según prestigio, precio y antigüedad, asignando proveedores estratégicos por producto para maximizar rentabilidad.

```mermaid
flowchart LR
    %% Actores
    subgraph Actores_P3 ["Actores Intervinientes"]
        A_Dueno3["👤 Dueño de la Perfumería"]
        A_Prov3["👤 Proveedor"]
        A_Conta3["👤 Equipo de Contabilidad"]
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

#### 2.3.4 Módulo Transversal: Seguridad, Autenticación y Auditoría

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

### 2.4 Diagramas de Actividad de Negocio

#### Proceso 1: Aprovisionamiento de Perfumes, Trazabilidad y Aprobacion Contable

```mermaid
flowchart TD
    subgraph S_Dueno ["Dueno de la Perfumeria / Compras"]
        P1_Start(["Inicio: Deteccion de necesidad de stock"]) --> P1_RevisarStock["Realizar control fisico y recuento de inventario"]
        P1_RevisarStock --> P1_ArmarPedido["Confeccionar solicitudes de cotizacion"]
        P1_ArmarPedido --> P1_EnviarCotiz["Enviar solicitudes a multiples Proveedores"]
        
        P1_RecibirPresup["Recopilar cotizaciones y armar orden preliminar"]
        P1_EnviarAprob["Enviar orden preliminar a Contabilidad"]
        
        P1_EvaluarRechazo{"Ajustar orden o desistir"}
        P1_Ajustar["Ajustar cantidades / renegociar con proveedor"]
        P1_Desistir(["Fin: Compra Cancelada"])
        
        P1_EnviarOC["Emitir y enviar Orden de Compra aprobada"]
        P1_RecepFisica["Recibir mercaderia y remito con documentacion de origen"]
        P1_ControlCalidad{"Mercaderia conforme y trazabilidad verificada"}
        P1_Reclamo["Emitir reclamo/devolucion formal al Proveedor"]
        P1_IngresarInv["Ingresar perfumes al inventario con identificacion de lote y origen"]
        P1_ArchivarCompra["Archivar registro en historial de compras"]
        P1_FinExito(["Fin: Stock abastecido y compra registrada"])
    end

    subgraph S_Prov ["Proveedores"]
        P1_RecibeSolicitud["Recibir solicitud de cotizacion"]
        P1_EmitirCotiz["Elaborar presupuesto con precios, plazos y condiciones"]
        P1_RecibirOC["Recibir Orden de Compra formal"]
        P1_PrepararLote["Preparar lote de perfumes con remito y certificados de origen"]
        P1_Despachar["Despachar pedido hacia la perfumeria"]
        P1_RecibirReclamo["Gestionar reposicion / nota de credito"]
    end

    subgraph S_Conta ["Equipo de Contabilidad"]
        P1_RecibirParaAprob["Recibir orden preliminar y presupuesto"]
        P1_RevisarPresup["Evaluar disponibilidad financiera y balance de costos"]
        P1_DecAprob{"Compra aprobada presupuestariamente"}
        P1_NotifAprob["Firmar y autorizar orden de compra"]
        P1_NotifRechazo["Rechazar orden e informar observaciones"]
        P1_RegContable["Registrar factura y asentar pasivo para liquidacion"]
    end

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

#### Proceso 2: Venta y Fidelizacion de Clientes (Membresia, Descuentos y Campanas)

```mermaid
flowchart TD
    subgraph S_Cliente ["Cliente"]
        P2_Start(["Inicio: Interes en compra"]) --> P2_Selecc["Seleccionar perfumes y solicitar compra en mostrador"]
        P2_RespondeMembresia{"Es miembro registrado"}
        P2_PresentarDatos["Indicar identificacion / credencial de miembro"]
        P2_DecAceptarReg{"Acepta registrarse como miembro"}
        P2_BrindarDatos["Proporcionar datos de contacto y preferencias olfativas"]
        P2_Pagar["Efectuar pago de la compra"]
        P2_RecibirProducto["Recibir perfume empaquetado y comprobante"]
        P2_RecibirCampana["Recibir promociones y campanas personalizadas"]
        P2_FinCli(["Fin: Compra completada"])
    end

    subgraph S_Vendedor ["Personal de Ventas / Mostrador"]
        P2_Atender["Atender cliente y verificar stock disponible"]
        P2_ConsultarMembresia["Consultar si es miembro del programa de fidelizacion"]
        
        P2_ValidarMiembro["Verificar cliente en padron de miembros"]
        P2_AplicarDescMiembro["Aplicar descuento por membresia vigente"]
        
        P2_ExplicarBeneficios["Explicar beneficios del programa y ofrecer registro"]
        P2_TomarDatos["Completar ficha de adhesion con datos y preferencias"]
        P2_AplicarDescBienvenida["Aplicar descuento de bienvenida"]
        P2_CalcularTotalEstandar["Calcular importe a tarifa estandar"]
        
        P2_Cobrar["Cobrar importe y emitir comprobante de venta"]
        P2_Entregar["Empaquetar y entregar mercaderia con ticket"]
        P2_EnviarFichas["Remitir fichas de miembros y registro de ventas a Marketing"]
    end

    subgraph S_Marketing ["Dueno de la Perfumeria / Marketing"]
        P2_RecibirFichas["Recibir planillas de ventas y fichas de adhesion"]
        P2_ActualizarPadron["Consolidar padron general de miembros fidelizados"]
        P2_Segmentar["Segmentar clientes por historial y preferencias"]
        P2_DisenarCampana["Disenar campanas de marketing directo y promociones"]
        P2_LanzarCampana["Difundir campana de marketing por canales habilitados"]
        P2_FinMkt(["Fin: Campana de fidelizacion activa"])
    end

    P2_Selecc --> P2_Atender
    P2_Atender --> P2_ConsultarMembresia
    P2_ConsultarMembresia --> P2_RespondeMembresia

    P2_RespondeMembresia -->|Si| P2_PresentarDatos
    P2_PresentarDatos --> P2_ValidarMiembro
    P2_ValidarMiembro --> P2_AplicarDescMiembro
    P2_AplicarDescMiembro --> P2_Cobrar

    P2_RespondeMembresia -->|No| P2_ExplicarBeneficios
    P2_ExplicarBeneficios --> P2_DecAceptarReg

    P2_DecAceptarReg -->|Si| P2_BrindarDatos
    P2_BrindarDatos --> P2_TomarDatos
    P2_TomarDatos --> P2_AplicarDescBienvenida
    P2_AplicarDescBienvenida --> P2_Cobrar

    P2_DecAceptarReg -->|No| P2_CalcularTotalEstandar
    P2_CalcularTotalEstandar --> P2_Cobrar

    P2_Cobrar --> P2_Pagar
    P2_Pagar --> P2_Entregar
    P2_Entregar --> P2_RecibirProducto
    P2_RecibirProducto --> P2_FinCli

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

#### Proceso 3: Evaluacion y Seleccion Estrategica de Proveedores por Producto

```mermaid
flowchart TD
    subgraph S_Dueno3 ["Dueno de la Perfumeria"]
        P3_Start(["Inicio: Deteccion de lineas a optimizar"]) --> P3_DefinirProd["Identificar catalogo de perfumes con riesgo de perdida o baja rotacion"]
        P3_DefinirProd --> P3_SolicitarInfo["Solicitar catalogos, listas de precios y antecedentes a Proveedores"]
        
        P3_ConsolidarProp["Reunir propuestas comerciales y legajos de proveedores"]
        P3_AnalizarPrestigio["Evaluar Prestigio (reputacion, autenticidad, certificaciones)"]
        P3_AnalizarAntiguedad["Evaluar Antiguedad en el mercado (trayectoria, solidez comercial)"]
        P3_ConsolidarAnalisis["Integrar evaluacion cualitativa y cuantitativa en matriz de decision"]
        
        P3_Deliberar{"Supera umbrales de prestigio, precio y antiguedad"}
        P3_Descartar["Descartar proveedor para ese producto / buscar alternativas"]
        P3_RevisarNuevos(["Fin: Reevaluacion diferida"])
        
        P3_Seleccionar["Seleccionar Proveedor mas conveniente por perfume"]
        P3_AsignarPrioridad["Asignar categoria de Proveedor Prioritario en directorio de compras"]
        P3_Acuerdo["Formalizar acuerdo comercial preferencial de suministro"]
        P3_FinExito(["Fin: Proveedores estrategicos asignados por producto"])
    end

    subgraph S_Prov3 ["Proveedores"]
        P3_RecibirSolicitud["Recibir solicitud de antecedentes y listas de precios"]
        P3_EnviarDoc["Remitir precios con escalas, trayectoria de mercado y certificaciones"]
    end

    subgraph S_Conta3 ["Equipo de Contabilidad"]
        P3_RecibirPrecios["Recibir propuestas de precios y condiciones de pago"]
        P3_AnalizarCostos["Analizar margenes de ganancia, costos logisticos e impacto financiero"]
        P3_EmitirDictamen["Emitir informe de viabilidad financiera y balance de costos"]
    end

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

### 2.5 Diagramas de Secuencia de Negocio

Los participantes representan personas, areas y terceros de la organizacion. Por ese motivo los diagramas describen intercambios de documentos, decisiones y mercaderia sin introducir componentes del sistema.

#### Secuencia 1: Aprovisionamiento, aprobacion contable y trazabilidad

```mermaid
sequenceDiagram
    actor Compras as Dueno / Compras
    participant Inventario as Responsable de Inventario
    participant Proveedores
    participant Contabilidad as Equipo de Contabilidad
    participant Recepcion as Recepcion / Deposito

    Compras->>Inventario: Solicitar relevamiento de existencias y faltantes
    Inventario-->>Compras: Entregar informe de necesidades de reposicion
    loop Multiples proveedores
        Compras->>Proveedores: Solicitar cotizacion, origen y plazo
        Proveedores-->>Compras: Remitir propuesta y documentacion comercial
    end
    Compras->>Contabilidad: Presentar comparativa y orden preliminar
    alt Compra rechazada
        Contabilidad-->>Compras: Informar observaciones financieras
        Compras->>Proveedores: Renegociar precio, cantidad o condiciones
    else Compra aprobada
        Contabilidad-->>Compras: Autorizar presupuesto y orden
        Compras->>Proveedores: Emitir orden aprobada
        Proveedores->>Recepcion: Despachar mercaderia, remito y certificados
        Recepcion->>Inventario: Entregar lotes y documentacion
        Inventario->>Inventario: Verificar cantidad, lote, origen y vencimiento
        alt Mercaderia conforme
            Inventario-->>Compras: Confirmar ingreso e historial de compra
            Recepcion->>Contabilidad: Remitir factura y conformidad
            Contabilidad->>Contabilidad: Registrar pasivo
        else Lote no conforme
            Inventario->>Proveedores: Emitir reclamo o devolucion
            Proveedores-->>Recepcion: Coordinar reposicion o nota de credito
        end
    end
```

#### Secuencia 2: Venta y fidelizacion de clientes

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
            Cliente-->>Ventas: Entregar contacto y preferencias olfativas
            Ventas->>Ventas: Completar ficha y aplicar bienvenida
        else Cliente no acepta
            Ventas->>Ventas: Mantener tarifa general
        end
    end
    Ventas->>Caja: Remitir detalle, descuento e importe
    Caja->>Cliente: Cobrar y emitir comprobante
    Cliente-->>Caja: Efectuar pago
    Caja-->>Ventas: Confirmar cobro
    Ventas-->>Cliente: Entregar perfumes y comprobante
    Ventas->>Marketing: Remitir ficha e historial de compra
    Marketing->>Marketing: Segmentar por historial y preferencias
    Marketing-->>Cliente: Difundir promociones autorizadas
```

#### Secuencia 3: Evaluacion estrategica de proveedores

```mermaid
sequenceDiagram
    actor Dueno as Dueno de la Perfumeria
    participant Proveedores
    participant Contabilidad as Equipo de Contabilidad

    Dueno->>Dueno: Identificar productos con riesgo de perdida
    loop Proveedores candidatos
        Dueno->>Proveedores: Solicitar precios, antecedentes y certificaciones
        Proveedores-->>Dueno: Remitir propuesta y trayectoria
    end
    Dueno->>Contabilidad: Remitir propuestas y condiciones de pago
    Contabilidad->>Contabilidad: Analizar costos, margenes e impacto financiero
    Contabilidad-->>Dueno: Emitir comparativa y dictamen de viabilidad
    Dueno->>Dueno: Ponderar prestigio, precio y antiguedad
    alt No supera los umbrales
        Dueno-->>Proveedores: Informar propuesta no seleccionada
        Dueno->>Dueno: Programar nueva evaluacion
    else Supera los umbrales
        Dueno->>Proveedores: Proponer acuerdo preferencial por producto
        Proveedores-->>Dueno: Aceptar o renegociar condiciones
        Dueno->>Dueno: Designar proveedor prioritario
    end
```

#### Correspondencia con la implementacion de las iteraciones 1 y 2

De acuerdo con el plan de entregas, N01 se disena en la Iteracion 1 y se implementa en la Iteracion 2; N02 se analiza, disena e implementa en la Iteracion 2; N03 queda para la Iteracion 3.

| Actividad de negocio | Operacion implementada |
| :--- | :--- |
| Registrar propuestas de proveedores | `AprovisionamientoBLL.RegistrarCotizacion` |
| Confeccionar la orden preliminar | `AprovisionamientoBLL.CrearOrdenPreliminar` |
| Presentar la comparativa a Contabilidad | `AprovisionamientoBLL.EnviarOrdenARevisionContable` |
| Autorizar o rechazar la compra | `AprovisionamientoBLL.AprobarOrdenCompra` / `RechazarOrdenCompra` |
| Emitir la orden aprobada | `AprovisionamientoBLL.EmitirOrdenAprobada` |
| Recepcionar lotes y actualizar existencias | `AprovisionamientoBLL.RecepcionarMercaderia` |
| Consultar stock y trazabilidad | `InventarioBLL.VerificarStock` / `ObtenerTrazabilidad` |
| Registrar cliente, preferencias y consentimiento | `VentaBLL.RegistrarCliente` |
| Activar membresia y beneficio | `VentaBLL.ActivarMembresia` |
| Registrar venta y egreso FEFO | `VentaBLL.RegistrarVenta` |
| Evaluar y recomendar proveedores | N03: implementacion prevista para la Iteracion 3 |

---

## G03. Planificacion del Proyecto

Plan de entregas reconciliado con el spreadsheet provisto:

| Semana | Iteracion | Hito / Entregable | Objetivos Especificos |
| :--- | :--- | :--- | :--- |
| Entrega 1 | Iteracion 1 | G01-G08 | Descripcion global: proposito, alcance, planificacion, glosario, participantes, requisitos generales, clases por capa y modelo de datos. |
| Entrega 1 | Iteracion 1 | N01 | Analisis y diseno del proceso de aprovisionamiento e inventario, incluidos actividad, casos principales, clases, secuencias y datos. |
| Entrega 2 | Iteracion 2 | N01 | Implementacion de los conceptos disenados en la iteracion anterior. |
| Entrega 2 | Iteracion 2 | N02 | Analisis, diseno e implementacion de venta y fidelizacion. |
| Entrega 2 | Iteracion 2 | A01 | Instalador inicial simple con creacion/restauracion de base, tablas, indices y relaciones. |
| Entrega 3 | Iteracion 3 | N03 | Proceso complejo de valor agregado: evaluacion y recomendacion de proveedores. |

---

## G04. Definiciones, Acronimos y Abreviaciones

### Definiciones

- **Perfume / Fragancia:** Articulo comercializable clasificado por marca, linea, concentracion y familia olfativa.
- **Lote:** Conjunto de unidades de un mismo perfume producidas o adquiridas bajo un codigo unico que determina fecha de elaboracion y partida de origen.
- **Trazabilidad de Origen:** Capacidad de rastrear el historico de adquisicion, proveedor emisor, remito y lote de cada producto ingresado.
- **Membresia de Fidelizacion:** Programa de afiliacion que asocia compras, descuentos acumulados y preferencias de los clientes para acciones de marketing.
- **Proveedor Prioritario:** Calificacion resultante de la ponderacion multicriterio que determina la preferencia de compra para un producto determinado.
- **Digitos Verificadores (DVH/DVV):** Mecanismos criptograficos de control que aseguran la no alteracion de registros en la base de datos.

### Acronimos

- **CUN:** Caso de Uso de Negocio.
- **DER:** Diagrama Entidad-Relacion.
- **MDI:** Multiple Document Interface (Interfaz de Multiples Documentos).
- **POO:** Programacion Orientada a Objetos.
- **RF / RNF:** Requerimiento Funcional / Requerimiento No Funcional.
- **BE:** Business Entities (Capa de Entidades de Negocio).
- **BLL:** Business Logic Layer (Capa de Logica de Negocio).
- **DAL:** Data Access Layer (Capa de Acceso a Datos).
- **MPP:** Mapper / Mapeador de Persistencia.

### Abreviaturas

- **OC:** Orden de Compra.
- **Prov:** Proveedor.
- **Cli:** Cliente.
- **Inv:** Inventario.
- **Reg:** Registro.

---

## G05. Descripcion de las Personas Participantes

### 5.1 Interesados y Participantes Organizacionales

| Nombre / Rol del Participante | Descripcion del Puesto | Responsabilidad en el Proyecto |
| :--- | :--- | :--- |
| **Dueno de la Perfumeria** | Direccion general y administracion comercial | Patrocinador, define politicas de abastecimiento, criterios de seleccion de proveedores y aprueba el alcance. |
| **Responsable de Compras** | Encargado operativo de adquisiciones e inventario | Define requerimientos de stock, recepcion de mercaderia, control de lotes y trazabilidad. |
| **Personal de Ventas / Mostrador** | Atencion directa al cliente en salon | Opera el punto de venta, registra nuevos miembros en el programa de fidelizacion y aplica beneficios. |
| **Equipo de Contabilidad** | Control financiero, costos y balances | Define requerimientos de autorizacion presupuestaria, calculo de margenes y asiento de pasivos. |
| **Analista y Desarrollador** | Equipo tecnico de ingenieria de software | Modela requerimientos, implementa la arquitectura en capas, desarrolla codigo y ejecuta pruebas. |

### 5.2 Matriz Preliminar de Accesos al Sistema

| Rol en Sistema | Modulos con Acceso Permitido | Nivel de Permiso |
| :--- | :--- | :--- |
| **Administrador / Dueno** | Todos los modulos (Compras, Ventas, Evaluacion, Seguridad, Auditoria) | Total (Lectura, Escritura, Configuracion) |
| **Contabilidad** | Aprobacion de OC, Registro de Compras, Informes Financieros, Auditoria | Lectura y Autorizacion Financiera |
| **Compras / Deposito** | Solicitudes de Cotizacion, Recepcion de Stock, Trazabilidad de Lotes | Lectura y Escritura operativa |
| **Vendedor** | Facturacion Mostrador, Alta de Membresias, Catalogo de Productos | Operativo de Venta y Fidelizacion |

---

## G06. Otros Requisitos del Producto y Documentacion

### 6.1 Estandares Aplicables
- **Notacion UML 2.5:** Para diagramas de casos de uso, actividad y clases.
- **Arquitectura en N-Capas (UI, BLL, DAL/MPP, BE, Servicios):** Separacion estricta de responsabilidades y bajo acoplamiento.
- **Normalizacion Relacional:** Modelo de datos estructurado bajo Tercera Forma Normal (3FN).
- **Estandares de Codificacion C# / .NET:** Nomenclatura PascalCase/camelCase y principios SOLID.

### 6.2 Requisitos de Sistema
- **Plataforma Operativa:** Microsoft Windows 10 / 11 o Windows Server 2019+.
- **Framework:** .NET Framework 4.8 o superior.
- **Motor de Persistencia:** Microsoft SQL Server 2017+.

### 6.3 Requisitos de Desempeno
- Tiempo de respuesta en consultas de stock y membresias inferior a 2 segundos bajo carga concurrente de hasta 20 terminales locales.
- Consistencia transaccional ACID garantizada en registros de ventas y recepciones de inventario.

### 6.4 Requisitos de Entorno
- Interfaz grafica de usuario de escritorio basada en contenedores MDI.
- Conexion cifrada contra la base de datos relacional centralizada.

### 6.5 Requisitos de Documentacion
- Manual de usuario final por rol.
- Guia de instalacion, despliegue y configuracion de base de datos.
- Diccionario de datos y documentacion tecnica de arquitectura.

---

## G07. Diagrama de Clases de todos los Modulos Implementados

### 7.1 Capa de Entidades de Negocio (BE) y Servicios

```mermaid
classDiagram
    class Usuario {
        +int IdUsuario
        +string NombreUsuario
        +string ClaveEncriptada
        +string Nombre
        +string Apellido
        +string Email
        +bool Bloqueado
        +int IntentosFallidos
        +string DVH
    }

    class Componente {
        <<abstract>>
        +int Id
        +string Nombre
        +bool EsValido(string permiso)*
    }

    class Patente {
        +string Permiso
        +bool EsValido(string permiso)
    }

    class Familia {
        +List~Componente~ Hijos
        +void AgregarHijo(Componente c)
        +void QuitarHijo(Componente c)
        +bool EsValido(string permiso)
    }

    class EventoBitacora {
        +int IdEvento
        +DateTime FechaHora
        +int IdUsuario
        +string Modulo
        +string Accion
        +string Criticidad
        +string DVH
    }

    class Producto {
        +int IdProducto
        +string Codigo
        +string Nombre
        +string Marca
        +string FamiliaOlfativa
        +decimal PrecioVenta
        +int StockMinimo
    }

    class Lote {
        +int IdLote
        +int IdProducto
        +string CodigoLote
        +string CertificadoOrigen
        +DateTime FechaIngreso
        +int CantidadInicial
        +int CantidadActual
    }

    class Proveedor {
        +int IdProveedor
        +string RazonSocial
        +string Cuit
        +string Contacto
        +int AntiguedadMercado
        +decimal IndicePrestigio
    }

    class OrdenCompra {
        +int IdOrdenCompra
        +int IdProveedor
        +DateTime FechaEmision
        +decimal Total
        +string Estado
        +int? IdUsuarioAprobadorContable
        +DateTime? FechaAprobacion
    }

    class DetalleOrdenCompra {
        +int IdDetalle
        +int IdOrdenCompra
        +int IdProducto
        +int Cantidad
        +decimal PrecioUnitario
    }

    class Cliente {
        +int IdCliente
        +string Dni
        +string Nombre
        +string Apellido
        +string Email
        +string Telefono
        +string NotasOlfativasPreferidas
    }

    class Membresia {
        +int IdMembresia
        +int IdCliente
        +string NroCredencial
        +DateTime FechaAlta
        +decimal PorcentajeDescuento
        +bool Activa
    }

    class Venta {
        +int IdVenta
        +int? IdCliente
        +DateTime FechaVenta
        +decimal Subtotal
        +decimal Descuento
        +decimal Total
        +int IdUsuarioVendedor
    }

    class DetalleVenta {
        +int IdDetalleVenta
        +int IdVenta
        +int IdProducto
        +int IdLote
        +int Cantidad
        +decimal PrecioUnitario
        +decimal Subtotal
    }

    class EvaluacionProveedor {
        +int IdEvaluacion
        +int IdProveedor
        +int IdProducto
        +decimal PuntajePrestigio
        +decimal PuntajePrecio
        +decimal PuntajeAntiguedad
        +decimal PuntajePonderadoFinal
        +bool EsPrioritario
    }

    Componente <|-- Patente
    Componente <|-- Familia
    Usuario o-- Componente
    Producto "1" *-- "0..*" Lote
    Proveedor "1" o-- "0..*" OrdenCompra
    OrdenCompra "1" *-- "1..*" DetalleOrdenCompra
    DetalleOrdenCompra --> Producto
    Cliente "1" -- "0..1" Membresia
    Cliente "1" o-- "0..*" Venta
    Venta "1" *-- "1..*" DetalleVenta
    DetalleVenta --> Producto
    DetalleVenta --> Lote
    Proveedor "1" o-- "0..*" EvaluacionProveedor
    Producto "1" o-- "0..*" EvaluacionProveedor
```

### 7.2 Capa de Logica de Negocio (BLL) y Capa de Persistencia (DAL/MPP)

```mermaid
classDiagram
    class AprovisionamientoBLL {
        +CrearOrdenPreliminar(OrdenCompra oc, List~DetalleOrdenCompra~ detalles) bool
        +AprobarOrdenCompra(int idOC, int idUsuarioContable) bool
        +RechazarOrdenCompra(int idOC, string motivo) bool
        +RecepcionarMercaderia(int idOC, List~Lote~ lotesRecibidos) bool
        +ConsultarHistorialCompras(int? idProveedor) List~OrdenCompra~
    }

    class InventarioBLL {
        +VerificarStock(int idProducto) int
        +RegistrarIngresoLote(Lote lote) bool
        +DescontarStockLote(int idLote, int cantidad) bool
        +ObtenerLotesDisponibles(int idProducto) List~Lote~
    }

    class VentaBLL {
        +RegistrarVenta(Venta venta, List~DetalleVenta~ detalles) bool
        +CalcularImporte(List~DetalleVenta~ detalles, int? idCliente) decimal
    }

    class FidelizacionBLL {
        +RegistrarMiembro(Cliente cliente, decimal porcentajeInicial) Membresia
        +ConsultarMembresia(int idCliente) Membresia
        +SegmentarMiembros(string familiaOlfativa) List~Cliente~
    }

    class EvaluacionProveedorBLL {
        +CalcularConveniencia(int idProveedor, int idProducto, decimal precio) EvaluacionProveedor
        +AsignarProveedorPrioritario(int idProveedor, int idProducto) bool
        +ObtenerMatrizConveniencia(int idProducto) List~EvaluacionProveedor~
    }

    class SeguridadBLL {
        +Autenticar(string usuario, string clave) Usuario
        +TienePermiso(Usuario usuario, string permiso) bool
        +BloquearUsuario(int idUsuario) void
    }

    class BitacoraBLL {
        +RegistrarEvento(EventoBitacora evento) void
        +ConsultarEventos(DateTime desde, DateTime hasta, string modulo) List~EventoBitacora~
    }

    class AprovisionamientoMPP {
        +InsertarOC(OrdenCompra oc) int
        +ActualizarEstadoOC(int idOC, string estado, int? idContable) void
        +InsertarLote(Lote lote) int
    }

    class VentaMPP {
        +InsertarVenta(Venta v) int
        +InsertarDetalle(DetalleVenta dv) void
        +ActualizarStock(int idLote, int cant) void
    }

    class FidelizacionMPP {
        +InsertarCliente(Cliente c) int
        +InsertarMembresia(Membresia m) int
        +ListarPorPreferencia(string pref) List~Cliente~
    }

    class EvaluacionMPP {
        +InsertarEvaluacion(EvaluacionProveedor ep) void
        +ListarEvaluaciones(int idProducto) List~EvaluacionProveedor~
    }

    AprovisionamientoBLL --> AprovisionamientoMPP
    AprovisionamientoBLL --> InventarioBLL
    VentaBLL --> VentaMPP
    VentaBLL --> InventarioBLL
    VentaBLL --> FidelizacionBLL
    FidelizacionBLL --> FidelizacionMPP
    EvaluacionProveedorBLL --> EvaluacionMPP
```

---

## G08. Modelo de Datos de todos los Modulos Implementados

### 8.1 Diagrama Entidad-Relacion (DER)

```mermaid
erDiagram
    USUARIO {
        int IdUsuario PK
        varchar NombreUsuario
        varchar ClaveEncriptada
        varchar Nombre
        varchar Apellido
        varchar Email
        bit Bloqueado
        int IntentosFallidos
        varchar DVH
    }

    EVENTO_BITACORA {
        int IdEvento PK
        datetime FechaHora
        int IdUsuario FK
        varchar Modulo
        varchar Accion
        varchar Criticidad
        varchar DVH
    }

    PROVEEDOR {
        int IdProveedor PK
        varchar RazonSocial
        varchar Cuit
        varchar Contacto
        int AntiguedadMercado
        decimal IndicePrestigio
    }

    PRODUCTO {
        int IdProducto PK
        varchar Codigo
        varchar Nombre
        varchar Marca
        varchar FamiliaOlfativa
        decimal PrecioVenta
        int StockMinimo
    }

    LOTE {
        int IdLote PK
        int IdProducto FK
        varchar CodigoLote
        varchar CertificadoOrigen
        datetime FechaIngreso
        int CantidadInicial
        int CantidadActual
    }

    ORDEN_COMPRA {
        int IdOrdenCompra PK
        int IdProveedor FK
        datetime FechaEmision
        decimal Total
        varchar Estado
        int IdUsuarioAprobadorContable FK
        datetime FechaAprobacion
    }

    DETALLE_ORDEN_COMPRA {
        int IdDetalle PK
        int IdOrdenCompra FK
        int IdProducto FK
        int Cantidad
        decimal PrecioUnitario
    }

    CLIENTE {
        int IdCliente PK
        varchar Dni
        varchar Nombre
        varchar Apellido
        varchar Email
        varchar Telefono
        varchar NotasOlfativasPreferidas
    }

    MEMBRESIA {
        int IdMembresia PK
        int IdCliente FK
        varchar NroCredencial
        datetime FechaAlta
        decimal PorcentajeDescuento
        bit Activa
    }

    VENTA {
        int IdVenta PK
        int IdCliente FK
        datetime FechaVenta
        decimal Subtotal
        decimal Descuento
        decimal Total
        int IdUsuarioVendedor FK
    }

    DETALLE_VENTA {
        int IdDetalleVenta PK
        int IdVenta FK
        int IdProducto FK
        int IdLote FK
        int Cantidad
        decimal PrecioUnitario
        decimal Subtotal
    }

    EVALUACION_PROVEEDOR {
        int IdEvaluacion PK
        int IdProveedor FK
        int IdProducto FK
        decimal PuntajePrestigio
        decimal PuntajePrecio
        decimal PuntajeAntiguedad
        decimal PuntajePonderadoFinal
        bit EsPrioritario
    }

    USUARIO ||--o{ EVENTO_BITACORA : "genera"
    USUARIO ||--o{ ORDEN_COMPRA : "aprueba"
    USUARIO ||--o{ VENTA : "registra"
    
    PROVEEDOR ||--o{ ORDEN_COMPRA : "recibe"
    ORDEN_COMPRA ||--|{ DETALLE_ORDEN_COMPRA : "contiene"
    PRODUCTO ||--o{ DETALLE_ORDEN_COMPRA : "incluido_en"
    
    PRODUCTO ||--o{ LOTE : "agrupa"
    
    CLIENTE ||--o| MEMBRESIA : "posee"
    CLIENTE ||--o{ VENTA : "realiza"
    VENTA ||--|{ DETALLE_VENTA : "contiene"
    PRODUCTO ||--o{ DETALLE_VENTA : "facturado_en"
    LOTE ||--o{ DETALLE_VENTA : "descontado_de"

    PROVEEDOR ||--o{ EVALUACION_PROVEEDOR : "evaluado_en"
    PRODUCTO ||--o{ EVALUACION_PROVEEDOR : "analizado_en"
```

### 8.2 Integridad y Normalizacion
- **Tercera Forma Normal (3FN):** Todas las tablas poseen claves primarias atomicas y atributos no clave con dependencia funcional directa y exclusiva de la PK.
- **Trazabilidad:** Cada venta descuenta de un registro exacto en `LOTE`, vinculando la salida con su `LOTE.CertificadoOrigen` e `IdProducto`.
- **Auditoria e Integridad:** Los eventos de `EVENTO_BITACORA` y registros criticos en `USUARIO` cuentan con campos `DVH` para la validacion horizontal mediante algoritmos hash, complementados por la tabla global `DVV`.
