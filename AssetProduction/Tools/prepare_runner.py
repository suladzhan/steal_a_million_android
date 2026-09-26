"""Convert preserved Meshy GLBs into a normalized Unity FBX review set."""
import bpy, json, math, sys
from pathlib import Path
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT/'04_Exports/CHAR_runner'
REV = ROOT/'Reviews/Runner'
OUT.mkdir(parents=True, exist_ok=True)
REV.mkdir(parents=True, exist_ok=True)
SOURCE = ROOT/'02_Generated/CHAR_runner'
BAKE = '--bake' in sys.argv

def load(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.scene.render.fps = 30
    bpy.ops.import_scene.gltf(filepath=str(path))
    return next(o for o in bpy.data.objects if o.type=='ARMATURE'), next(o for o in bpy.data.objects if o.type=='MESH')

def bounds(mesh):
    evaluated=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get())
    points=[evaluated.matrix_world@v.co for v in evaluated.data.vertices]
    return Vector([min(p[i] for p in points) for i in range(3)]),Vector([max(p[i] for p in points) for i in range(3)])

rig, mesh=load(SOURCE/'CHAR_runner__v001__rigged.glb')
rig.animation_data_clear()
for bone in rig.pose.bones: bone.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.update()
low,high=bounds(mesh)
factor=1.8/(high.z-low.z)
offset=Vector(((low.x+high.x)/2,(low.y+high.y)/2,low.z))
skeleton=[{'name':b.name,'parent':b.parent.name if b.parent else None,'rest_matrix':[list(r) for r in b.matrix_local]} for b in rig.data.bones]

def normalize(rig,mesh):
    root=bpy.data.objects.new('VisualRoot',None)
    bpy.context.collection.objects.link(root)
    root.rotation_euler.z=math.pi
    root.scale=(factor,)*3
    root.location=(offset.x*factor,offset.y*factor,-offset.z*factor)
    for obj in (rig,mesh):
        if obj.parent is None: obj.parent=root
    bpy.context.view_layer.update()
    return root

def texture(mesh):
    for material in mesh.data.materials:
        for node in material.node_tree.nodes:
            if node.type=='TEX_IMAGE' and node.image:
                node.image.filepath_raw=str(OUT/'CHAR_runner__v001__basecolor.png')
                node.image.file_format='PNG'
                node.image.save()
                return
    raise RuntimeError('No texture')

def export(name,objects,animated):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects: obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[1]
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'__v001.fbx')),use_selection=True,
      object_types={'MESH','ARMATURE','EMPTY'},axis_forward='-Z',axis_up='Y',
      apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,
      bake_anim=animated,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,
      bake_anim_step=1,bake_anim_simplify_factor=0,path_mode='RELATIVE')

def render_sheet(name,rig,mesh,frames):
    if '--skip-render' in sys.argv:return
    scene=bpy.context.scene
    copies=[]
    for i,frame in enumerate(frames):
        scene.frame_set(int(frame),subframe=frame-int(frame))
        evaluated=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get())
        data=bpy.data.meshes.new_from_object(evaluated)
        obj=bpy.data.objects.new('Pose_'+str(i),data)
        scene.collection.objects.link(obj)
        obj.matrix_world=mesh.matrix_world.copy()
        obj.location.x += (i-(len(frames)-1)/2)*1.65
        copies.append(obj)
    mesh.hide_render=True
    engines=scene.render.bl_rna.properties['engine'].enum_items.keys()
    scene.render.engine='BLENDER_EEVEE_NEXT' if 'BLENDER_EEVEE_NEXT' in engines else 'BLENDER_EEVEE'
    scene.render.resolution_x=320*len(frames) if len(frames)>1 else 540;scene.render.resolution_y=540 if len(frames)>1 else 720
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.world=bpy.data.worlds.new('ReviewWorld');scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.12,.14,.18,1)
    scene.view_settings.view_transform='Standard'
    bpy.ops.object.camera_add(location=(0,12,1.2))
    camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,1.2))-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type='ORTHO';camera.data.ortho_scale=max(2.5,1.65*len(frames))
    scene.camera=camera
    for pos,power in [((-3,4,6),1600),((4,-3,4),1000)]:
        bpy.ops.object.light_add(type='AREA',location=pos)
        lamp=bpy.context.object;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=7
        lamp.rotation_euler=(Vector((0,0,1))-lamp.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(REV/(name+'.png'))
    bpy.ops.render.render(write_still=True)

texture(mesh)
root=normalize(rig,mesh)
bpy.context.scene.frame_start=0;bpy.context.scene.frame_end=1
export('CHAR_runner__model',[root,rig,mesh],False)
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'03_Blender/CHAR_runner__v001.blend'))
render_sheet('rest_front',rig,mesh,[0])
records=[]
for path in sorted(SOURCE.glob('ANIM_*.glb')):
    rig,mesh=load(path)
    for expected,bone in zip(skeleton,rig.data.bones):
        assert expected['name']==bone.name and expected['parent']==(bone.parent.name if bone.parent else None)
        assert max(abs(expected['rest_matrix'][i][j]-bone.matrix_local[i][j]) for i in range(4) for j in range(4))<.001
    root=normalize(rig,mesh)
    action=rig.animation_data.action
    start,end=action.frame_range
    name=path.stem.replace('__v001','')
    if BAKE:
        duration={'ANIM_idle':2,'ANIM_run':19/30,'ANIM_preview':80/30,'ANIM_risk_win':.8,
                  'ANIM_victory_dance':2,'ANIM_victory_jump':1.2,'ANIM_victory_money_rain':1.8}[name]
        source_note=path.name
        if name=='ANIM_victory_jump':
            # The catalog jump contains a long unrelated routine. Derive a short
            # celebratory hop from the arm-raise clip and a local vertical arc.
            rig,mesh=load(SOURCE/'ANIM_victory_money_rain__v001.glb');root=normalize(rig,mesh)
            action=rig.animation_data.action;start,end=action.frame_range
            start+=.3*30
            source_note='Local hop arc + ANIM_victory_money_rain arm raise; original jump not used'
        if name=='ANIM_victory_dance':start+=1.4*30;end=start+2*30
        if name=='ANIM_risk_win':start+=.5*30
        loop=name in ('ANIM_run','ANIM_idle','ANIM_preview','ANIM_victory_dance')
        frames=round(duration*30)
        sampled=[];floor=float('inf');anchor=None
        for i in range(frames+1):
            time=start+(end-start)*i/frames
            bpy.context.scene.frame_set(int(time),subframe=time-int(time))
            hip=rig.pose.bones['Hips'];matrix=hip.matrix.copy()
            if anchor is None:anchor=matrix.translation.copy()
            matrix.translation.x=anchor.x;matrix.translation.y=anchor.y
            if name=='ANIM_victory_jump':matrix.translation.z+=.25*math.sin(math.pi*i/frames)**2/(factor*.01)
            hip.matrix=matrix;bpy.context.view_layer.update()
            floor=min(floor,bounds(mesh)[0].z)
            sampled.append([(b.location.copy(),b.rotation_quaternion.copy(),b.scale.copy()) for b in rig.pose.bones])
        rig.animation_data_clear();action=bpy.data.actions.new(name);rig.animation_data_create();rig.animation_data.action=action
        for i,poses in enumerate(sampled):
            blend=max(0,(i/frames-.85)/.15) if loop else 0
            for index,bone in enumerate(rig.pose.bones):
                loc,rot,scale=poses[index];first=sampled[0][index]
                bone.location=loc.lerp(first[0],blend);bone.rotation_mode='QUATERNION';bone.rotation_quaternion=rot.slerp(first[1],blend);bone.scale=scale.lerp(first[2],blend)
            bpy.context.view_layer.update()
            hip=rig.pose.bones['Hips'];matrix=hip.matrix.copy();matrix.translation.z-=floor/(factor*.01);hip.matrix=matrix
            for bone in rig.pose.bones:
                for channel in ('location','rotation_quaternion','scale'):bone.keyframe_insert(data_path=channel,frame=i,group=bone.name)
        bpy.context.scene.frame_start=0;bpy.context.scene.frame_end=frames
        # Unity preserves this hierarchy and copies the model's Humanoid avatar.
        export(name,[root,rig],True)
        bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'03_Blender'/(name+'__v001.blend')))
        render_sheet(name+'__adapted',rig,mesh,[frames*i/7 for i in range(8)])
        records.append({'id':name,'source':source_note,'source_start':start,'source_end':end,'duration':duration,'loop':loop,'frames':frames,'floor_correction':floor})
        continue
    # Preserve the source timing for motion review before selecting ranges.
    render_sheet(name+'__source',rig,mesh,[start+(end-start)*i/7 for i in range(8)])
    records.append({'id':name,'source_start':start,'source_end':end,'duration':(end-start)/30})
(REV/('adapted_ranges.json' if BAKE else 'source_ranges.json')).write_text(json.dumps({'scale':factor,'offset':list(offset),'skeleton':skeleton,'clips':records},indent=2))
print('RUNNER_SOURCE_REVIEW_READY')
