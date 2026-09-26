import bpy, json
from pathlib import Path
root = Path(__file__).resolve().parents[1]
records = []
for path in sorted((root/'02_Generated/CHAR_runner').glob('*.glb')):
    if '__model' in path.name: continue
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(path))
    rig = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
    mesh = next(o for o in bpy.data.objects if o.type == 'MESH')
    action = rig.animation_data.action
    samples = []
    for frame in [action.frame_range[0], sum(action.frame_range)/2, action.frame_range[1]]:
        bpy.context.scene.frame_set(int(frame), subframe=frame-int(frame))
        samples.append({'frame':frame, 'rig':list(rig.location), 'root':list(rig.pose.bones[0].matrix.translation)})
    if 'victory_jump' in path.name:
        airborne=[]
        for frame in range(int(action.frame_range[0]),int(action.frame_range[1])+1):
            bpy.context.scene.frame_set(frame)
            airborne.append({'seconds':(frame-action.frame_range[0])/24,'hips':rig.pose.bones['Hips'].matrix.translation.z*.01,
                'feet':min(rig.pose.bones[n].matrix.translation.z*.01 for n in ('LeftFoot','RightFoot','LeftToeBase','RightToeBase'))})
        (root/'Reviews/Runner/jump_motion.json').write_text(json.dumps(airborne))
    records.append({'file':path.name,'fps':bpy.context.scene.render.fps,'range':list(action.frame_range),
      'rig_matrix':[list(row) for row in rig.matrix_world], 'mesh_dimensions':list(mesh.dimensions),
      'bones':[{'name':b.name,'parent':b.parent.name if b.parent else None,'head':list(b.head_local)} for b in rig.data.bones],
      'samples':samples})
(root/'Reviews/runner__v001__blender_inspection.json').write_text(json.dumps(records,indent=2))
print('RUNNER_INSPECTED',len(records))
