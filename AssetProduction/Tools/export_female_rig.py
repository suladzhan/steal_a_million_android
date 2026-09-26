import bpy,json
from pathlib import Path
root=Path.cwd();state=json.loads((root/'AssetProduction/02_Generated/Major/female.json').read_text(encoding='utf-8-sig'));folder=Path(state['project']);out=root/'Assets/Art/Major'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);bpy.ops.import_scene.gltf(filepath=str(folder/'female__rigged.glb'))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];armatures=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
if not armatures:raise RuntimeError('Missing rig')
print('FEMALE_BONES',[b.name for b in armatures[0].data.bones])
for obj in meshes:
 for i,mat in enumerate(obj.data.materials):
  mat.name='female_M'+str(i)
  for n in mat.node_tree.nodes:
   if n.type=='TEX_IMAGE':n.image.filepath_raw=str(out/(mat.name+'.png'));n.image.file_format='PNG';n.image.save()
bpy.ops.object.select_all(action='DESELECT')
for o in meshes+armatures:o.select_set(True)
bpy.context.view_layer.objects.active=armatures[0]
bpy.ops.wm.save_as_mainfile(filepath=str(folder/'female__rigged.blend'))
bpy.ops.export_scene.fbx(filepath=str(out/'female.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,bake_anim=False,path_mode='STRIP')
print('FEMALE_RIG_EXPORT_PASS')
