"""Validate native class/data connector types from audit_ea_uml.ps1, without modifying EA."""
import argparse
import json
from collections import Counter
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('audit', type=Path)
parser.add_argument('--report', required=True, type=Path)
args = parser.parse_args()
raw = args.audit.read_bytes()
lines = raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig').splitlines()
diagrams, counts, abstract, interfaces = [], Counter(), set(), set()
inheritance = {('Usuario', 'Persona'), ('Familia', 'Componente'), ('Patente', 'Componente')}
for line in lines:
    if line.startswith('DIAGRAM '):
        diagram = line.split()[1]
        diagrams.append(diagram)
    elif line.startswith('{'):
        record = json.loads(line)
        if 'element' in record:
            if record['element'] in ('Persona', 'Componente'):
                assert record['abstract'] == '1', record
                abstract.add(record['element'])
            else:
                assert record['type'] == 'Interface' and record['stereotype'] in ('', 'interface'), record
                interfaces.add(record['element'])
            continue
        kind = record['type']
        assert kind in ('Association', 'Dependency', 'Generalization', 'Realisation'), record
        counts[kind] += 1
        if kind == 'Generalization':
            assert (record['source'], record['target']) in inheritance, record
        if kind == 'Realisation':
            assert record['target'].startswith('I') and record['direction'] == 'Source -> Destination', record
        if diagram == 'G08-01' or '-07' in diagram:
            assert kind == 'Association' and record['stereotype'] == 'FK', record
            assert record['sourceCard'] and record['targetCard'], record
            assert record['direction'] == 'Unspecified' and record['targetNav'] != 'Navigable', record
            assert record['sourceAggregation'] == record['targetAggregation'] == 0, record
assert abstract == {'Persona', 'Componente'} and len(interfaces) == 7
report = {'native_diagrams_reviewed': diagrams, 'connector_counts': counts,
          'abstract_classes': sorted(abstract), 'native_interfaces': sorted(interfaces),
          'data_relationships': 'FK associations with multiplicities; no inheritance or aggregation',
          'inheritance': 'subclass to superclass', 'realizations': 'implementation to interface'}
args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', 'utf-8')
print(json.dumps(report, ensure_ascii=True))
