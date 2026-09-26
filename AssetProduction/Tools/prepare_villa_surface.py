"""Produce a continuous villa silhouette in staging for review before integration."""
import bpy,json
from pathlib import Path
from mathutils import Vector
root=Path.cwd();state=json.loads((root/'AssetProduction/02_Generated/Major/villa.json').read_text(encoding='utf-8-sig'));folder=Path(state['project'])
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.gltf(filepath=str(folder/'model.glb'));obj=next(o for o in bpy.context.scene.objects if o.type=='MESH');obj.parent=None;bpy.context.view_layer.objects.active=obj;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
points=[obj.matrix_world@Vector(c) for c in obj.bound_box];lo=Vector([min(p[i] for p in points) for i in range(3)]);hi=Vector([max(p[i] for p in points) for i in range(3)]);centre=(lo+hi)/2;size=hi-lo;s=min(5.5/size.x,5.5/size.y,5.2/size.z)
for v in obj.data.vertices:
 p=obj.matrix_world@v.co;v.co=Vector(((p.x-centre.x)*s,(p.y-centre.y)*s,(p.z-lo.z)*s))
obj.matrix_world.identity();remesh=obj.modifiers.new('Continuous villa silhouette','REMESH');remesh.mode='VOXEL';remesh.voxel_size=.045;bpy.ops.object.modifier_apply(modifier=remesh.name);obj.data.calc_loop_triangles();dec=obj.modifiers.new('Mobile villa','DECIMATE');dec.ratio=min(1,5000/len(obj.data.loop_triangles));dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name)
for p in obj.data.polygons:p.use_smooth=True
obj.data.calc_loop_triangles();count=len(obj.data.loop_triangles)
if count>6000:raise RuntimeError('Villa exceeds surface budget')
obj.name='Major_villa';bpy.ops.object.select_all(action='DESELECT');obj.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(folder/'villa__continuous.blend'));bpy.ops.export_scene.fbx(filepath=str(folder/'villa__continuous.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='STRIP')
(folder/'continuous_surface.json').write_text(json.dumps({'id':'villa','triangles':count,'budget':6000,'voxel_size':.045}));print('VILLA_CONTINUOUS_PASS',count)
