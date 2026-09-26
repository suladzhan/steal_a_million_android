"""Normalize and decimate the four paid Meshy crests for mobile Unity use."""
import bpy, json
from pathlib import Path
from mathutils import Vector
root=Path.cwd()
records=[]
for name in ('safe','risk','jackpot','investment'):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    source=next((root/'AssetProduction/02_Generated/GateCrests').glob('*_'+name+'-v001_*'))
    bpy.ops.import_scene.gltf(filepath=str(source/'model.glb'))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join()
    obj=bpy.context.object;obj.name='Crest_'+name
    obj.matrix_world=obj.matrix_world.copy();obj.parent=None
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    obj.data.calc_loop_triangles();original=len(obj.data.loop_triangles)
    dec=obj.modifiers.new('Mobile geometry','DECIMATE');dec.ratio=min(1,1400/original)
    bpy.ops.object.modifier_apply(modifier=dec.name)
    points=[obj.matrix_world@Vector(c) for c in obj.bound_box]
    low=Vector([min(p[i] for p in points) for i in range(3)])
    high=Vector([max(p[i] for p in points) for i in range(3)])
    center=(low+high)/2;size=high-low
    # GLB Y-up imports as Blender Z-up. Front relief faces Blender -Y.
    scale=min(1.05/size.x,.90/size.z)
    for v in obj.data.vertices:
        p=obj.matrix_world@v.co-center
        v.co=(p.x*scale,p.y*.18/max(size.y,.001),p.z*scale)
    obj.matrix_world.identity();obj.data.materials.clear()
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    out=root/'Assets/Art/GateCrests';out.mkdir(parents=True,exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=False,bake_anim=False)
    obj.data.calc_loop_triangles();records.append(dict(id=name,source=str(source.relative_to(root)),original_triangles=original,triangles=len(obj.data.loop_triangles),normalized_height=size.z*scale,width=size.x*scale,depth=.18))
    bpy.ops.wm.save_as_mainfile(filepath=str(source/('Crest_'+name+'__mobile.blend')))
out=root/'AssetProduction/Reviews/Gates';out.mkdir(parents=True,exist_ok=True)
(out/'mobile_geometry.json').write_text(json.dumps(records,indent=2))
print('GATE_CRESTS_EXPORTED',records)
