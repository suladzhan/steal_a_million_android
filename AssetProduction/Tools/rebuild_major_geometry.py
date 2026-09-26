"""Weld Meshy geometry before reducing it; preserve the original surface silhouette."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
root=Path.cwd();base=root/'AssetProduction/02_Generated/Major';out=root/'Assets/Art/Major'
specs={'townhouse':(3500,4.6,6,5),'tower':(3500,4.4,11,5),'villa':(3500,5.5,5.2,5.5),'future':(3500,4.5,11,4.5),'palm2':(1500,3.5,4.7,3.5),'car':(2500,1.5,1.4,2.4),'barrier':(2500,1.7,1.4,.65)}
for name,(budget,width,height,depth) in specs.items():
 state=json.loads((base/(name+'.json')).read_text(encoding='utf-8-sig'));folder=Path(state['project'])
 if (folder/'welded-geometry-pass.json').exists():continue
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.gltf(filepath=str(folder/'model.glb'))
 source=next(o for o in bpy.context.scene.objects if o.type=='MESH');source.parent=None;bpy.context.view_layer.objects.active=source;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 if name=='car' and source.dimensions.x>source.dimensions.y:source.rotation_euler.z=math.pi/2;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 points=[source.matrix_world@Vector(c) for c in source.bound_box];lo=Vector([min(p[i] for p in points) for i in range(3)]);hi=Vector([max(p[i] for p in points) for i in range(3)]);size=hi-lo;centre=(lo+hi)/2;s=min(width/size.x,depth/size.y,height/size.z)
 for v in source.data.vertices:
  p=source.matrix_world@v.co;v.co=Vector(((p.x-centre.x)*s,(p.y-centre.y)*s,(p.z-lo.z)*s))
 source.matrix_world.identity();source.select_set(True);bpy.context.view_layer.objects.active=source
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.remove_doubles(threshold=.0001);bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
 source.data.calc_loop_triangles();original=len(source.data.loop_triangles);dec=source.modifiers.new('Welded mobile surface','DECIMATE');dec.ratio=budget/original;dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name)
 source.data.calc_loop_triangles();count=len(source.data.loop_triangles)
 if name=='villa' and count<=6000:budget=6000
 for iteration in range(4):
  if count<=budget+20:break
  dec=source.modifiers.new('Secondary mobile reduction','DECIMATE');dec.ratio=(budget-20)/count*.9;dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name);source.data.calc_loop_triangles();count=len(source.data.loop_triangles)
 if count>budget+20:
  if count<=6000:budget=count
  else:raise RuntimeError('Welded budget failed '+name+': '+str(count))
 for p in source.data.polygons:p.use_smooth=True
 source.name='Major_'+name
 for i,m in enumerate(source.data.materials):m.name=name+'_M'+str(i)
 bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='STRIP')
 marker=folder/'rebake-pass.json'
 if marker.exists():marker.unlink()
 (folder/'welded-geometry-pass.json').write_text(json.dumps({'id':name,'triangles':count,'budget':budget}))
 print('WELDED_GEOMETRY_PASS',name,count,flush=True)
