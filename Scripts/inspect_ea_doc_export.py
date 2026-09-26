"""Read-only diagnostic of diagram sizes and paragraph pagination in a DOCX export."""
import sys
from io import BytesIO
from docx import Document
from PIL import Image

document = Document(sys.argv[1])
heading = ""
for p in document.paragraphs:
    if p.text:
        heading = p.text
    for b in p._p.findall('.//{http://schemas.openxmlformats.org/drawingml/2006/main}blip'):
        rid = b.get('{http://schemas.openxmlformats.org/officeDocument/2006/relationships}embed')
        part = document.part.related_parts[rid]
        im = Image.open(BytesIO(part.blob))
        print(heading[:75], im.size)
print('Shapes:', len(document.inline_shapes))
for p in document.paragraphs:
    if p.text.startswith(('2.5', '2.6', 'Preparar y enviar', 'Recepcionar mercad', 'N03 · Clases', 'Los casos de uso')):
        print(repr(p.text), p.paragraph_format.page_break_before, p.paragraph_format.keep_with_next)
