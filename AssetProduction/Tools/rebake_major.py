"""Reproject original Meshy paint onto explicit mobile UVs after decimation."""
import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
root=Path.cwd();base=root/'AssetProduction/02_Generated/Major';out=root/'Assets/Art/Major'
specs={'townhouse':(4.6,6,5),'tower':(4.4,11,5),'villa':(5.5,5.2,5.5),'future':(4.5,11,4.5),'palm2':(3.5,4.7,3.5),'car':(1.5,1.4,2.4),'barrier':(1.7,1.4,.65),'female':(10,1.8,10)}
for name,(width,height,depth) in specs.items():
 if '--' in sys.argv and name not in sys.argv[sys.argv.index('--')+1:]:continue
 state=json.loads((base/(name+'.json')).read_text(encoding='utf-8-sig'));folder=Path(state['project'])
 if (folder/'rebake-pass.json').exists():continue
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.gltf(filepath=str(folder/'model.glb'));sources=[o for o in bpy.context.scene.objects if o.type=='MESH']
 if len(sources)!=1:raise RuntimeError('Reprojection expects joined source: '+name)
 source=sources[0];source.parent=None;bpy.context.view_layer.objects.active=source;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 if name=='car' and source.dimensions.x>source.dimensions.y:source.rotation_euler.z=math.pi/2;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 points=[source.matrix_world@Vector(c) for c in source.bound_box];low=Vector([min(p[i] for p in points) for i in range(3)]);high=Vector([max(p[i] for p in points) for i in range(3)]);size=high-low;centre=(low+high)/2;s=min(width/size.x,depth/size.y,height/size.z)
 for v in source.data.vertices:
  p=source.matrix_world@v.co;v.co=Vector(((p.x-centre.x)*s,(p.y-centre.y)*s,(p.z-low.z)*s))
 source.matrix_world.identity()
 for mat in source.data.materials:
  nodes=mat.node_tree.nodes;bsdf=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');colour=bsdf.inputs['Base Color'].links[0].from_socket;emit=nodes.new('ShaderNodeEmission');output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL');mat.node_tree.links.new(colour,emit.inputs[0]);mat.node_tree.links.new(emit.outputs[0],output.inputs[0])
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(out/(name+'.fbx')));target=next(o for o in set(bpy.context.scene.objects)-before if o.type=='MESH')
 bpy.ops.object.select_all(action='DESELECT');target.select_set(True);bpy.context.view_layer.objects.active=target
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.02);bpy.ops.object.mode_set(mode='OBJECT')
 for existing in list(bpy.data.materials):
  if existing.name.startswith(name+'_M0'):existing.name='Source_'+existing.name
 mat=bpy.data.materials.new(name+'_M0');mat.use_nodes=True;target.data.materials.clear();target.data.materials.append(mat)
 image=bpy.data.images.new(name+'_Reprojected',width=1024,height=1024);tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image;mat.node_tree.nodes.active=tex
 source.select_set(True);scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=1;scene.render.bake.use_selected_to_active=True;scene.render.bake.cage_extrusion=.12;scene.render.bake.max_ray_distance=.25;scene.render.bake.margin=8;bpy.ops.object.bake(type='EMIT')
 image.filepath_raw=str(out/(name+'_M0.png'));image.file_format='PNG';image.save();mat.node_tree.links.new(tex.outputs['Color'],mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
 bpy.data.objects.remove(source,do_unlink=True);bpy.ops.object.select_all(action='DESELECT');target.select_set(True);bpy.context.view_layer.objects.active=target;mat.name=name+'_M0'
 bpy.ops.wm.save_as_mainfile(filepath=str(folder/(name+'__rebaked.blend')))
 if name=='female':
  for o in bpy.context.scene.objects:
   if o.type=='ARMATURE':o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_anim=False,path_mode='STRIP')
 (folder/'rebake-pass.json').write_text(json.dumps({'id':name,'method':'original paint selected-to-active emission bake','texture':1024}))
 print('REBAKE_PASS',name,flush=True)
