import bpy,json
from pathlib import Path
root=Path.cwd();base=root/'AssetProduction/02_Generated/Major';report=[]
for name in ['townhouse','tower','villa','future','palm2','tree','car','barrier']:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Art/Major'/(name+'.fbx')));meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];triangles=0
 for o in meshes:o.data.calc_loop_triangles();triangles+=len(o.data.loop_triangles)
 marker=Path(json.loads((base/(name+'.json')).read_text(encoding='utf-8-sig'))['project'])/'welded-geometry-pass.json'
 budget=json.loads(marker.read_text(encoding='utf-8-sig'))['budget'] if marker.exists() else 1500
 if triangles>budget+20:raise RuntimeError(name+' exceeds verified budget')
 names=sorted(set(m.name for o in meshes for m in o.data.materials))
 if names!=[name+'_M0']:raise RuntimeError('Material remap mismatch: '+name+' '+str(names))
 state=json.loads((base/(name+'.json')).read_text(encoding='utf-8-sig'))
 report.append({'id':name,'source':str(Path(state['project']).relative_to(root)),'triangles':triangles,'budget':budget,'materials':[{'name':name+'_M0','texture':name+'_M0.png'}]})
(root/'AssetProduction/Reviews/Major/mobile_geometry.json').write_text(json.dumps({'assets':report},indent=2));print('MAJOR_GEOMETRY_REPORT_PASS',[(r['id'],r['triangles']) for r in report])
