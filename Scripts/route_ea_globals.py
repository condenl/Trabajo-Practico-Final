"""Route native EA connectors around global-view element rectangles."""
import json, heapq, math, argparse
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'Documentacion/EnterpriseArchitect'
parser=argparse.ArgumentParser()
parser.add_argument('--input', default='model-reconciliation.json')
parser.add_argument('--output', default='global-routes.json')
parser.add_argument('--all-logical', action='store_true')
args=parser.parse_args()
models=json.loads((out/args.input).read_text('utf-8'))

def n03_paths(diagram):
    """Keep the small N03 views planar, with distinct ports per relationship.

    A shortest-path router can send the BLL use and DAL realization into the
    same interface port. Dedicated straight ports and one outer lane make the
    direction of each relationship visible without changing its UML type.
    """
    nodes = {node['key']: node for node in diagram['nodes']}
    def port(name, side):
        n = nodes[name]
        x, y, w, h = n['x'], n['y'], n['w'], n['h']
        # EA Path stores routing points, not locked endpoint coordinates. Keep
        # these points outside the boxes so EA attaches to their border rather
        # than drawing the final segment through an attribute compartment.
        return {'left': (x-10, y+h/2), 'right': (x+w+10, y+h/2),
                'top': (x+w/2, y-10), 'bottom': (x+w/2, y+h+10)}[side]
    for link in diagram['links']:
        source, target = link['source'], link['target']
        if diagram['code'] == 'N03-04':
            if (source, target) == ('EvaluacionProveedor', 'DatosEvaluacion'):
                points = [port(source, 'bottom'), port(target, 'top')]
            elif (source, target) == ('EvaluacionProveedor', 'ResultadoProveedor'):
                a, b = port(source, 'left'), port(target, 'left')
                points = [a, (a[0]-35, a[1]), (a[0]-35, b[1]), b]
            else:
                points = [port(source, 'right'), port(target, 'left')]
        elif (source, target) == ('EvaluacionProveedorBLL', 'ArchivoEvaluacion'):
            a, b = port(source, 'right'), port(target, 'right')
            points = [a, (a[0]+45, a[1]), (a[0]+45, b[1]), b]
        elif target == 'IEvaluacionProveedorRepository' and source == 'EvaluacionProveedorDAL':
            points = [port(source, 'top'), port(target, 'bottom')]
        else:
            points = [port(source, 'bottom'), port(target, 'top')]
        link['path'] = ''.join(f'{x:g}:{-y:g};' for x, y in points)

for d in models:
    if not (d['kind']=='Logical' if args.all_logical else d['code'].startswith('G0')): continue
    if d['code'] in ('N03-04', 'N03-05'):
        n03_paths(d)
        continue
    rects={n['key']:n for n in d['nodes']}
    blocked=set()
    for n in rects.values():
        for x in range(math.floor((n['x']-10)/10),math.ceil((n['x']+n['w']+10)/10)+1):
            for y in range(math.floor((n['y']-10)/10),math.ceil((n['y']+n['h']+10)/10)+1): blocked.add((x,y))
    used={}
    xmax=math.ceil(max(n['x']+n['w'] for n in rects.values())/10)+15
    ymax=math.ceil(max(n['y']+n['h'] for n in rects.values())/10)+15
    def ports(n):
        x,y,w,h=n['x'],n['y'],n['w'],n['h']
        return [(round((x-30)/10),round((y+h/2)/10)),(round((x+w+30)/10),round((y+h/2)/10)),(round((x+w/2)/10),round((y-30)/10)),(round((x+w/2)/10),round((y+h+30)/10))]
    for l in d['links']:
        starts=ports(rects[l['source']]); ends=set(ports(rects[l['target']]))
        queue=[]; dist={}; prev={}
        def heur(x,y): return min(abs(x-a)+abs(y-b) for a,b in ends)
        for x,y in starts:
            state=(x,y,-1);dist[state]=0; heapq.heappush(queue,(heur(x,y),0,state))
        goal=None
        while queue:
            _,cost,s=heapq.heappop(queue)
            if cost!=dist[s]: continue
            x,y,direction=s
            if (x,y) in ends: goal=s;break
            for dr,(dx,dy) in enumerate(((1,0),(-1,0),(0,1),(0,-1))):
                a,b=x+dx,y+dy
                if not (-12<=a<=xmax and -10<=b<=ymax) or (a,b) in blocked: continue
                c=cost+1+(3 if direction not in (-1,dr) else 0)+used.get((a,b),0)*2
                ns=(a,b,dr)
                if c<dist.get(ns,1e9):
                    dist[ns]=c;prev[ns]=s;heapq.heappush(queue,(c+heur(a,b),c,ns))
        if goal is None: raise RuntimeError(l)
        pts=[];s=goal
        while True:
            pts.append((s[0],s[1]));used[pts[-1]]=used.get(pts[-1],0)+1
            if s not in prev: break
            s=prev[s]
        pts.reverse(); bends=[pts[0]]
        for i in range(1,len(pts)-1):
            if (pts[i][0]-pts[i-1][0],pts[i][1]-pts[i-1][1])!=(pts[i+1][0]-pts[i][0],pts[i+1][1]-pts[i][1]): bends.append(pts[i])
        bends.append(pts[-1]);l['path']=''.join(f'{x*10}:{-y*10};' for x,y in bends)
(out/args.output).write_text(json.dumps(models if args.all_logical else [d for d in models if d['code'].startswith('G0')],ensure_ascii=False,indent=2),'utf-8')
