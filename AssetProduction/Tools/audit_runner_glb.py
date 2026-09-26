"""Read GLB structure without modifying generated assets. Not a visual rig review."""
import json
import struct
from pathlib import Path

root = Path(__file__).resolve().parents[1]
records = []
for path in sorted((root / '02_Generated/CHAR_runner').glob('*.glb')):
    data = path.read_bytes()
    magic, version, length = struct.unpack_from('<4sII', data)
    assert magic == b'glTF' and version == 2 and length == len(data), path
    size, kind = struct.unpack_from('<II', data, 12)
    assert kind == 0x4E4F534A, path
    doc = json.loads(data[20:20 + size])
    accessors = doc.get('accessors', [])
    primitives = [p for m in doc.get('meshes', []) for p in m['primitives']]
    animations = []
    for clip in doc.get('animations', []):
        times = [accessors[s['input']] for s in clip['samplers']]
        animations.append({'name': clip.get('name'), 'channels': len(clip['channels']),
                           'duration': max(a['max'][0] for a in times) - min(a['min'][0] for a in times)})
    records.append({'file': path.name, 'bytes': len(data),
                    'triangles': sum(accessors[p['indices']]['count'] // 3 for p in primitives),
                    'materials': len(doc.get('materials', [])),
                    'joints': [len(s['joints']) for s in doc.get('skins', [])],
                    'weighted_primitives': sum('WEIGHTS_0' in p['attributes'] and 'JOINTS_0' in p['attributes'] for p in primitives),
                    'animations': animations})
output = root / 'Reviews/runner__v001__structure.json'
output.write_text(json.dumps(records, indent=2), encoding='utf-8')
print(json.dumps(records, indent=2))
