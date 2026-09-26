"""Read-only verification of a saved Google Docs DOCX export against EA PNGs."""
import argparse
import hashlib
import json
from collections import Counter
from io import BytesIO
from pathlib import Path

from docx import Document
from PIL import Image, ImageChops, ImageStat


def pixels(source):
    with Image.open(source) as image:
        image = image.convert("RGBA")
        return image.size, hashlib.sha256(image.tobytes()).hexdigest()


def tables(document):
    return [[[cell.text for cell in row.cells] for row in table.rows]
            for table in document.tables]


def verify(baseline, current, images):
    before, after = Document(baseline), Document(current)
    marker = "2.5 Diagramas de Secuencia de Negocio"
    cut = next(i for i, p in enumerate(before.paragraphs) if p.text == marker)
    assert [p.text for p in before.paragraphs[:cut]] == [p.text for p in after.paragraphs[:cut]], "Earlier paragraphs changed"
    assert tables(before) == tables(after), "Use-case table text changed"
    expected = {pixels(path): path.stem for path in Path(images).glob("*.png")}
    native = {path.stem: path for path in Path(images).glob("*.png")}
    downsampled = []
    ns = {"a": "http://schemas.openxmlformats.org/drawingml/2006/main"}
    embed = "{http://schemas.openxmlformats.org/officeDocument/2006/relationships}embed"
    hits, placements = [], []
    section, heading = "", ""
    for paragraph in after.paragraphs:
        if paragraph.text.strip():
            heading = paragraph.text
            if heading.startswith(("2.3 ", "2.4 ", "2.5 ", "2.6 ", "G07.", "G08.")):
                section = heading
        for blip in paragraph._p.findall(".//a:blip", ns):
            part = after.part.related_parts[blip.get(embed)]
            code = expected.get(pixels(BytesIO(part.blob)))
            if code is None:
                # Google Docs reduces uploaded images wider than 2048 pixels.
                # Require both matching aspect ratio and near-identical resampled pixels.
                with Image.open(BytesIO(part.blob)) as uploaded:
                    uploaded = uploaded.convert('RGB')
                    candidates = []
                    for name, path in native.items():
                        with Image.open(path) as source:
                            if source.width <= 2048 or uploaded.width != 2048:
                                continue
                            if abs(uploaded.height - source.height * 2048 / source.width) > 1:
                                continue
                            source = source.convert('RGB')
                            error = min(sum(ImageStat.Stat(ImageChops.difference(uploaded,
                                source.resize(uploaded.size, mode))).mean) / 3
                                for mode in (Image.Resampling.LANCZOS, Image.Resampling.HAMMING, Image.Resampling.BILINEAR))
                            if error < 3:
                                candidates.append((name, error))
                    if len(candidates) == 1:
                        code, error = candidates[0]
                        downsampled.append({'code': code, 'width': uploaded.width, 'mean_pixel_error': round(error, 4)})
            if code:
                hits.append(code)
                placements.append({"code": code, "section": section, "heading": heading})
                for crop in paragraph._p.findall(".//a:srcRect", ns):
                    assert all(int(value) == 0 for value in crop.attrib.values()), f"Cropped image: {code}"
                required = ("2.3" if code.endswith("-01") and code.startswith("N") else
                            "2.4" if code.endswith("-02") else
                            "2.5" if "-06" in code else
                            "G08" if "-07" in code or code == "G08-01" else "G07")
                assert section.startswith(required), f"Wrong section for {code}: {section}"
    assert set(hits) == set(expected.values()), f"Missing EA diagrams: {sorted(set(expected.values()) - set(hits))}"
    assert all(count == 1 for count in Counter(hits).values()), "Duplicate EA diagram"
    return {"unchanged_prefix_paragraphs": cut, "unchanged_tables": len(after.tables),
            "total_images": len(after.inline_shapes), "unique_ea_diagrams": len(hits),
            "pixel_exact": not downsampled, "google_docs_downsampled": downsampled,
            "duplicates": [], "placements": placements}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("baseline", type=Path)
    parser.add_argument("current", type=Path)
    parser.add_argument("--images", type=Path, default=Path(__file__).resolve().parents[1] / "Documentacion/EnterpriseArchitect")
    parser.add_argument("--report", type=Path, help="Optional JSON verification report; source documents are never modified")
    args = parser.parse_args()
    report = json.dumps(verify(args.baseline, args.current, args.images), ensure_ascii=True, indent=2)
    if args.report:
        args.report.write_text(report + '\n', encoding='utf-8')
    print(report)
