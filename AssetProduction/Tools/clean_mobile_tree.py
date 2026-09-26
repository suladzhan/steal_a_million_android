import bpy,bmesh,json
from pathlib import Path
root=Path.cwd();path=root/'Assets/Art/Major/tree.fbx'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.fbx(filepath=str(path));obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');bpy.context.view_layer.objects.active=obj
bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001);unseen=set(bm.verts);components=[]
while unseen:
 first=unseen.pop();group={first};pending=[first]
 while pending:
  v=pending.pop()
  for e in v.link_edges:
   other=e.other_vert(v)
   if other in unseen:unseen.remove(other);group.add(other);pending.append(other)
 components.append(group)
removed=[v for group in components if len(group)<20 for v in group];bmesh.ops.delete(bm,geom=removed,context='VERTS');bm.to_mesh(obj.data);bm.free()
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(path),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='STRIP')
obj.data.calc_loop_triangles();(root/'AssetProduction/Reviews/Major/tree_cleanup.json').write_text(json.dumps({'components':[len(g) for g in components],'removed_vertices':len(removed),'triangles':len(obj.data.loop_triangles)}))
print('TREE_CLEANUP_PASS',len(removed),len(obj.data.loop_triangles))
