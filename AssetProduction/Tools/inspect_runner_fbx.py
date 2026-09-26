"""Independent FBX round-trip measurements for the runner export."""
import bpy, json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
records=[]
for path in sorted((root/'04_Exports/CHAR_runner').glob('*.fbx')):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(path))
    rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
    action=rig.animation_data.action if rig.animation_data else None
    samples=[]
    for frame in ([action.frame_range[0],sum(action.frame_range)/2,action.frame_range[1]] if action else [0]):
        bpy.context.scene.frame_set(int(frame),subframe=frame-int(frame))
        samples.append({'frame':frame,'hips':list((rig.matrix_world@rig.pose.bones['Hips'].matrix).translation),
                        'head':list((rig.matrix_world@rig.pose.bones['Head'].matrix).translation)})
    records.append({'file':path.name,'scale':list(rig.scale),'samples':samples})
(root/'Reviews/Runner/fbx_roundtrip.json').write_text(json.dumps(records,indent=2))
print(json.dumps(records))
