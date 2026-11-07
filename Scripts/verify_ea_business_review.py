"""Read-only validation of native EA exports, UML alternatives and preserved replies."""
import argparse
import json
from collections import Counter
from pathlib import Path


def verify(before, after, source):
    expected = json.loads(source.read_text(encoding='utf-8-sig'))
    previous = {d['code']: d for d in json.loads(before.read_text(encoding='utf-8-sig'))}
    current = {d['code']: d for d in json.loads(after.read_text(encoding='utf-8-sig'))}
    report = []
    for spec in expected:
        d = current[spec['code']]
        assert d['guid'] == previous[spec['code']]['guid'], 'Diagram GUID changed'
        by_id, by_key = {}, {}
        for n in spec['nodes']:
            matches = [e for e in d['nodes'] if (e['type'],e['name'],e['x'],e['y']) == (n['kind'],n['name'],n['x'],n['y'])]
            assert len(matches)==1, (spec['code'],n['key'],matches)
            by_id[matches[0]['id']] = n['key']
            by_key[n['key']] = matches[0]
        actual = Counter((by_id[l['source']],by_id[l['target']],l['kind'],l['name']) for l in d['links'] if not l['hidden'])
        assert actual == Counter((l['source'],l['target'],l['kind'],l['name']) for l in spec['links'])
        merges, objects = [], []
        for n in spec['nodes']:
            incoming = [l for l in spec['links'] if l['target']==n['key']]
            outgoing = [l for l in spec['links'] if l['source']==n['key']]
            if n['kind']=='Decision' and not n['name']:
                assert len(incoming)>=2 and len(outgoing)==1
                assert {l['kind'] for l in incoming+outgoing}=={'ControlFlow'}
                merges.append(n['key'])
            elif n['kind']=='Decision':
                assert len(incoming)==1 and {l['name'] for l in outgoing}=={'[Sí]','[No]'}
            elif n['kind']=='Activity':
                assert len(incoming)<=1, 'Implicit join of mutually exclusive alternatives'
            elif n['kind']=='Object':
                assert incoming and outgoing and {l['kind'] for l in incoming+outgoing}=={'ObjectFlow'}
                owners=[lane['name'] for lane in spec['nodes'] if lane['kind']=='ActivityPartition' and lane['x']<=n['x'] and n['x']+n['w']<=lane['x']+lane['w']]
                assert len(owners)==1
                objects.append({'name':n['name'],'partition':owners[0]})
        report.append({'code':spec['code'],'preserved_guid':True,'nodes':len(d['nodes']),'connectors':sum(actual.values()),'merges':merges,'objects':objects})
    old, new = previous['N03-06'], current['N03-06']
    signature=lambda d: sorted((l['guid'],l['sequence'],l['source'],l['target'],l['name'],l['properties']['IsReturn']) for l in d['links'])
    assert signature(old)==signature(new), 'Original sequence messages changed'
    nodes={n['id']:n for n in new['nodes']}
    stack, returns, actor_requests = [], 0, 0
    for message in sorted(new['links'],key=lambda m:m['sequence']):
        if message['properties']['IsReturn']=='True':
            call=stack.pop()
            assert (message['source'],message['target']) == (call['target'],call['source'])
            returns+=1
        else:
            assert not stack or message['source']==stack[-1]['target']
            stack.append(message)
            if nodes[message['source']]['type']=='Actor':
                assert 'ForceActivation=1;' in message['stateFlags']
                actor_requests+=1
    assert not stack and returns==26 and actor_requests==3
    report.append({'code':'N03-06','preserved_messages':len(new['links']),'paired_returns':returns,'actor_requests_with_extended_activation':actor_requests})
    return report


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('before',type=Path)
    parser.add_argument('after',type=Path)
    parser.add_argument('--source',type=Path,default=Path('Documentacion/EnterpriseArchitect/business-review.json'))
    parser.add_argument('--report',type=Path,required=True)
    args=parser.parse_args()
    result=verify(args.before,args.after,args.source)
    args.report.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(result,ensure_ascii=True))
