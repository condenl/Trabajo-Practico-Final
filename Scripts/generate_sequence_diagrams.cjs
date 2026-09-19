"use strict";

const fs = require("fs");
const path = require("path");
const sharp = require("sharp");

const outputDir = path.resolve(__dirname, "..", "Documentacion", "Diagramas");
fs.mkdirSync(outputDir, { recursive: true });

const diagrams = [
  {
    file: "secuencia_aprovisionamiento",
    title: "Aprovisionamiento, aprobación contable y trazabilidad",
    participants: ["Dueño / Compras", "Responsable de Inventario", "Proveedores", "Equipo de Contabilidad", "Recepción / Depósito"],
    steps: [
      { from: 0, to: 1, label: "Solicitar relevamiento de existencias y faltantes" },
      { from: 1, to: 0, label: "Entregar informe de necesidades de reposición", dashed: true },
      { kind: "section", label: "Repetir para múltiples proveedores" },
      { from: 0, to: 2, label: "Solicitar cotización, origen y plazo de entrega" },
      { from: 2, to: 0, label: "Remitir cotización y documentación comercial", dashed: true },
      { from: 0, to: 3, label: "Presentar comparativa y orden preliminar" },
      { kind: "section", label: "Decisión presupuestaria" },
      { from: 3, to: 0, label: "Autorizar la compra o informar observaciones", dashed: true },
      { from: 0, to: 2, label: "Emitir la orden aprobada o renegociar condiciones" },
      { from: 2, to: 4, label: "Despachar mercadería, remito y certificados" },
      { from: 4, to: 1, label: "Entregar lotes y documentación para control" },
      { from: 1, to: 1, label: "Verificar cantidad, lote, origen y vencimiento" },
      { kind: "section", label: "Si la mercadería es conforme" },
      { from: 1, to: 0, label: "Confirmar ingreso y actualizar historial", dashed: true },
      { from: 4, to: 3, label: "Remitir factura y conformidad de recepción" },
      { from: 3, to: 3, label: "Registrar pasivo para liquidación" },
      { kind: "section", label: "Si el lote no es conforme" },
      { from: 1, to: 2, label: "Emitir reclamo o devolución" },
      { from: 2, to: 4, label: "Coordinar reposición o nota de crédito", dashed: true },
    ],
  },
  {
    file: "secuencia_venta_fidelizacion",
    title: "Venta y fidelización de clientes",
    participants: ["Cliente", "Personal de Ventas", "Caja", "Dueño / Marketing"],
    steps: [
      { from: 0, to: 1, label: "Solicitar los perfumes seleccionados" },
      { from: 1, to: 1, label: "Verificar disponibilidad física" },
      { from: 1, to: 0, label: "Consultar pertenencia al programa de membresía" },
      { kind: "section", label: "Cliente afiliado" },
      { from: 0, to: 1, label: "Presentar identificación de miembro", dashed: true },
      { from: 1, to: 1, label: "Verificar vigencia y calcular beneficio" },
      { kind: "section", label: "Cliente no afiliado" },
      { from: 1, to: 0, label: "Explicar beneficios y ofrecer adhesión" },
      { from: 0, to: 1, label: "Aceptar y entregar contacto y preferencias", dashed: true },
      { from: 1, to: 1, label: "Completar ficha y aplicar bienvenida" },
      { kind: "section", label: "Cierre de la venta" },
      { from: 1, to: 2, label: "Remitir detalle, descuento e importe final" },
      { from: 2, to: 0, label: "Cobrar y emitir comprobante" },
      { from: 0, to: 2, label: "Efectuar pago", dashed: true },
      { from: 2, to: 1, label: "Confirmar cobro", dashed: true },
      { from: 1, to: 0, label: "Entregar perfumes y comprobante", dashed: true },
      { from: 1, to: 3, label: "Remitir ficha e historial de compra" },
      { from: 3, to: 3, label: "Segmentar por historial y preferencias" },
      { from: 3, to: 0, label: "Difundir promociones autorizadas", dashed: true },
    ],
  },
  {
    file: "secuencia_evaluacion_proveedores",
    title: "Evaluación estratégica de proveedores",
    participants: ["Dueño de la Perfumería", "Proveedores", "Equipo de Contabilidad"],
    steps: [
      { from: 0, to: 0, label: "Identificar productos con riesgo de pérdida o baja rotación" },
      { kind: "section", label: "Repetir para cada proveedor candidato" },
      { from: 0, to: 1, label: "Solicitar precios, antecedentes y certificaciones" },
      { from: 1, to: 0, label: "Remitir propuesta comercial y trayectoria", dashed: true },
      { from: 0, to: 2, label: "Remitir propuestas y condiciones de pago" },
      { from: 2, to: 2, label: "Analizar costos, márgenes e impacto financiero" },
      { from: 2, to: 0, label: "Emitir comparativa y dictamen de viabilidad", dashed: true },
      { from: 0, to: 0, label: "Ponderar prestigio, precio y antigüedad" },
      { kind: "section", label: "Si no supera los umbrales" },
      { from: 0, to: 1, label: "Informar que la propuesta no fue seleccionada" },
      { from: 0, to: 0, label: "Programar una nueva evaluación" },
      { kind: "section", label: "Si supera los umbrales" },
      { from: 0, to: 1, label: "Proponer acuerdo preferencial por producto" },
      { from: 1, to: 0, label: "Aceptar o renegociar condiciones", dashed: true },
      { from: 0, to: 0, label: "Designar proveedor prioritario" },
    ],
  },
];

function xml(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

function wrap(value, maxChars) {
  const words = value.split(/\s+/);
  const lines = [];
  let current = "";
  for (const word of words) {
    const candidate = current ? current + " " + word : word;
    if (candidate.length > maxChars && current) {
      lines.push(current);
      current = word;
    } else {
      current = candidate;
    }
  }
  if (current) lines.push(current);
  return lines;
}

function textBlock(x, y, label, options = {}) {
  const lines = wrap(label, options.maxChars || 34);
  const anchor = options.anchor || "middle";
  const size = options.size || 21;
  const weight = options.weight || 400;
  const lineHeight = size + 5;
  return `<text x="${x}" y="${y}" text-anchor="${anchor}" font-family="Arial, sans-serif" font-size="${size}" font-weight="${weight}" fill="${options.color || "#172033"}">` +
    lines.map((line, index) => `<tspan x="${x}" dy="${index === 0 ? 0 : lineHeight}">${xml(line)}</tspan>`).join("") +
    `</text>`;
}

function renderDiagram(diagram) {
  const width = 1800;
  const titleY = 58;
  const participantTop = 105;
  const participantWidth = diagram.participants.length === 5 ? 270 : 330;
  const marginX = Math.max(180, participantWidth / 2 + 35);
  const participantHeight = 84;
  const usableWidth = width - marginX * 2;
  const gap = usableWidth / (diagram.participants.length - 1);
  const xs = diagram.participants.map((_, index) => marginX + gap * index);
  const rowHeight = 116;
  const sectionHeight = 70;
  let cursorY = participantTop + participantHeight + 70;
  const rowPositions = [];

  for (const step of diagram.steps) {
    rowPositions.push(cursorY);
    cursorY += step.kind === "section" ? sectionHeight : rowHeight;
  }

  const height = cursorY + 90;
  const lifelineBottom = height - 55;
  const parts = [];

  parts.push(`<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}">`);
  parts.push(`<defs>
    <marker id="arrow" markerWidth="10" markerHeight="8" refX="9" refY="4" orient="auto" markerUnits="strokeWidth">
      <path d="M0,0 L10,4 L0,8 z" fill="#315c8a"/>
    </marker>
    <filter id="shadow" x="-10%" y="-10%" width="120%" height="130%">
      <feDropShadow dx="0" dy="2" stdDeviation="3" flood-color="#0f172a" flood-opacity="0.16"/>
    </filter>
  </defs>`);
  parts.push(`<rect width="100%" height="100%" fill="#ffffff"/>`);
  parts.push(textBlock(width / 2, titleY, diagram.title, { maxChars: 70, size: 32, weight: 700, color: "#14263d" }));

  diagram.participants.forEach((participant, index) => {
    const x = xs[index];
    parts.push(`<rect x="${x - participantWidth / 2}" y="${participantTop}" width="${participantWidth}" height="${participantHeight}" rx="12" fill="#234d75" filter="url(#shadow)"/>`);
    parts.push(textBlock(x, participantTop + 34, participant, { maxChars: 24, size: 20, weight: 700, color: "#ffffff" }));
    parts.push(`<line x1="${x}" y1="${participantTop + participantHeight}" x2="${x}" y2="${lifelineBottom}" stroke="#91a4b7" stroke-width="2" stroke-dasharray="9 8"/>`);
  });

  diagram.steps.forEach((step, index) => {
    const y = rowPositions[index];
    if (step.kind === "section") {
      parts.push(`<rect x="48" y="${y - 28}" width="${width - 96}" height="46" rx="8" fill="#e8f0f7" stroke="#8aa4bd" stroke-width="1.5"/>`);
      parts.push(textBlock(72, y + 2, step.label, { maxChars: 90, anchor: "start", size: 20, weight: 700, color: "#234d75" }));
      return;
    }

    const fromX = xs[step.from];
    const toX = xs[step.to];
    const lineY = y + 44;

    if (step.from === step.to) {
      const loopWidth = 95;
      const pathData = `M ${fromX} ${lineY - 10} h ${loopWidth} v 42 h -${loopWidth}`;
      parts.push(`<path d="${pathData}" fill="none" stroke="#315c8a" stroke-width="3" marker-end="url(#arrow)"/>`);
      parts.push(textBlock(fromX + loopWidth / 2, y - 2, step.label, { maxChars: 32, size: 19, weight: 500 }));
    } else {
      const direction = toX > fromX ? 1 : -1;
      const startX = fromX + direction * 8;
      const endX = toX - direction * 12;
      const dash = step.dashed ? ` stroke-dasharray="10 7"` : "";
      parts.push(`<line x1="${startX}" y1="${lineY}" x2="${endX}" y2="${lineY}" stroke="#315c8a" stroke-width="3"${dash} marker-end="url(#arrow)"/>`);
      parts.push(textBlock((fromX + toX) / 2, y - 2, step.label, { maxChars: diagram.participants.length === 5 ? 32 : 42, size: 19, weight: 500 }));
    }
  });

  parts.push(`</svg>`);
  return parts.join("\n");
}

async function main() {
  for (const diagram of diagrams) {
    const svg = renderDiagram(diagram);
    const svgPath = path.join(outputDir, diagram.file + ".svg");
    const pngPath = path.join(outputDir, diagram.file + ".png");
    fs.writeFileSync(svgPath, svg, "utf8");
    await sharp(Buffer.from(svg)).png({ compressionLevel: 9 }).toFile(pngPath);
    process.stdout.write(pngPath + "\n");
  }
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
