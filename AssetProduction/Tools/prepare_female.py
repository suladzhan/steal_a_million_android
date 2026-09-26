import bpy,json
from pathlib import Path
from mathutils import Vector
root=Path.cwd();state=json.loads((root/'AssetProduction/02_Generated/Major/female.json').read_text(encoding='utf-8-sig'));folder=Path(state['project'])
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(folder/'model.glb'))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
bpy.ops.object.select_all(action='DESELECT')
for o in meshes:o.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
if len(meshes)>1:bpy.ops.object.join()
obj=bpy.context.object;obj.parent=None;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.remove_doubles(threshold=.00001);bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
obj.data.calc_loop_triangles();original=len(obj.data.loop_triangles)
dec=obj.modifiers.new('Mobile character','DECIMATE');dec.ratio=min(1,7900/original);dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name)
points=[obj.matrix_world@Vector(c) for c in obj.bound_box];low=Vector([min(p[i] for p in points) for i in range(3)]);high=Vector([max(p[i] for p in points) for i in range(3)]);centre=(low+high)/2;s=1.8/(high.z-low.z)
for v in obj.data.vertices:
 p=obj.matrix_world@v.co;v.co=Vector(((p.x-centre.x)*s,(p.y-centre.y)*s,(p.z-low.z)*s))
obj.matrix_world.identity()
for mat in obj.data.materials:
 for node in mat.node_tree.nodes:
  if node.type=='TEX_IMAGE':node.image.scale(1024,1024)
obj.data.calc_loop_triangles();count=len(obj.data.loop_triangles)
if count>8020:raise RuntimeError('Character triangle budget '+str(count))
bpy.ops.wm.save_as_mainfile(filepath=str(folder/'female__mobile.blend'))
bpy.ops.export_scene.gltf(filepath=str(folder/'female__mobile.glb'),export_format='GLB',use_selection=True)
(folder/'mobile_character.json').write_text(json.dumps({'original_triangles':original,'triangles':count,'height':1.8},indent=2))
print('FEMALE_MOBILE_PASS',count)
