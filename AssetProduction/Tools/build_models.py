"""Original measured game geometry. Blender 4.5, no third-party assets.

blender -b --factory-startup -t 4 --python build_models.py -- --root AssetProduction
Coordinates supplied to helpers use the Unity contract (X right, Y up, Z forward).
Draft exports only: Unity, device and integration QA remain separate gates.
"""
import argparse
import json
import math
import sys
from datetime import date
from pathlib import Path

import bpy
import bmesh
from mathutils import Vector

parser = argparse.ArgumentParser()
parser.add_argument('--root', required=True)
parser.add_argument('--only', default='')
parser.add_argument('--replace-drafts', action='store_true')
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
ROOT = Path(args.root).resolve()
EXPORT = ROOT / '04_Exports'
SOURCE = ROOT / '03_Blender'
REVIEWS = ROOT / 'Reviews'
for path in (EXPORT, SOURCE, REVIEWS):
    path.mkdir(parents=True, exist_ok=True)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
scene.render.engine = 'BLENDER_EEVEE' if bpy.app.version >= (5,0,0) else 'BLENDER_EEVEE_NEXT'
scene.render.resolution_x = scene.render.resolution_y = 512
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.render.film_transparent = True
scene.world.color = (.6, .6, .6)
scene.view_settings.view_transform = 'Standard'

WHITE = '#FFFFFF'
BLUE = '#3E78FF'
GOLD = '#FFC928'
GREEN = '#35D06F'
RED = '#FF4D5A'
DARK = '#16202A'
COLORS = [WHITE, BLUE, GOLD, GREEN, RED, DARK, '#F4F6F8', '#D9E1EA',
          '#159447', '#A9FFBE', '#FFF5B1', '#70899F', '#566575', '#AAB7C4',
          '#34D17B', '#E62D42', '#A85CFF', '#7DD8FF', '#82909D', '#E5A521',
          '#FFC77C', '#93B8EC', '#B8D8F1', '#FFEFDD', '#789DE0', '#FBE7A2',
          '#DBE9FA', '#FFE390', '#FF8B70', '#477DC8', '#B873EF', '#FFD064',
          '#EFB32D', '#BF73F6', '#EBAF2B', '#75CE86', '#82CEA7', '#75C9C6',
          '#8CE0BD', '#46CDE0', '#8BD6BF', '#55CFDA', '#86BED6', '#AFE0D3',
          '#A9E9FF', '#B67AFF', '#56D6B0', '#253A70', '#FF7769', '#916044',
          '#C89462', '#EFF7FC', '#BDD3E8', '#38E2D6', '#B257EB']


def rgb(hex_color):
    return tuple(int(hex_color[i:i+2], 16) / 255 for i in (1, 3, 5))


def linear_rgb(hex_color):
    return tuple(v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in rgb(hex_color))


palette_path = EXPORT / 'Shared' / 'SAM_palette__v001.png'
palette_path.parent.mkdir(exist_ok=True)
palette = bpy.data.images.new('SAM_palette__v001', width=256, height=256, alpha=False)
pixels = []
for y in range(256):
    for x in range(256):
        idx = (y // 32) * 8 + x // 32
        pixels.extend((*linear_rgb(COLORS[idx] if idx < len(COLORS) else WHITE), 1))
palette.pixels.foreach_set(pixels)
palette.filepath_raw = str(palette_path)
palette.file_format = 'PNG'
palette.save()
mat = bpy.data.materials.new('SAM_palette_satin')
mat.use_nodes = True
bsdf = mat.node_tree.nodes.get('Principled BSDF')
bsdf.inputs['Roughness'].default_value = .58
tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
tex.image = palette
tex.interpolation = 'Closest'
mat.node_tree.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
asset = None
objects = []
records = []


def co(p):
    # This FBX pipeline maps Blender (X,Y,Z) to Unity (X,Z,Y).
    # Verified with the asymmetric door hinge and the positive-Z track end.
    return (p[0], p[2], p[1])


def start(asset_id):
    global asset, objects
    for obj in list(bpy.data.objects):
        if obj.type not in ('CAMERA', 'LIGHT'):
            bpy.data.objects.remove(obj, do_unlink=True)
    root = bpy.data.objects.new(asset_id, None)
    scene.collection.objects.link(root)
    asset = root
    objects = [root]
    return root


def group(name, pos=(0, 0, 0), parent=None):
    obj = bpy.data.objects.new(name, None)
    scene.collection.objects.link(obj)
    obj.parent = parent or asset
    obj.location = co(pos)
    objects.append(obj)
    return obj


def finish_part(obj, name, pos, color, parent=None, bevel=0):
    obj.name = name
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        modifier = obj.modifiers.new('Broad readable bevel', 'BEVEL')
        modifier.width = bevel
        modifier.segments = 1
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.parent = parent or asset
    obj.location = co(pos)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(obj.data)
    bm.free()
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    obj.color = (*rgb(color), 1)
    idx = COLORS.index(color)
    uv = obj.data.uv_layers.active or obj.data.uv_layers.new(name='PaletteUV')
    uv.name = 'PaletteUV'
    for entry in uv.data:
        entry.uv = ((idx % 8 + .5) / 8, (idx // 8 + .5) / 8)
    objects.append(obj)
    return obj


def box(name, pos, size, color, parent=None, bevel=.035):
    bpy.ops.mesh.primitive_cube_add(size=1)
    obj = bpy.context.object
    obj.scale = (size[0], size[2], size[1])
    return finish_part(obj, name, pos, color, parent, min(bevel, min(size) / 5))


def cylinder(name, pos, radius, depth, color, axis='y', vertices=16, parent=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth)
    obj = bpy.context.object
    if axis == 'z': obj.rotation_euler.x = math.pi / 2
    if axis == 'x': obj.rotation_euler.y = math.pi / 2
    return finish_part(obj, name, pos, color, parent, min(.025, depth / 5))


def sphere(name, pos, size, color, parent=None, segments=12, rings=6):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=.5)
    obj = bpy.context.object
    obj.scale = (size[0], size[2], size[1])
    obj = finish_part(obj, name, pos, color, parent)
    for poly in obj.data.polygons: poly.use_smooth = True
    return obj


def polygon(name, points, depth, color, pos=(0, 0, 0), parent=None):
    # Extruded front silhouette; points are Unity X/Y, face towards Unity -Z.
    n = len(points)
    verts = [co((x, y, z)) for z in (-depth/2, depth/2) for x, y in points]
    faces = [tuple(reversed(range(n))), tuple(range(n, n*2))]
    faces += [(i, (i+1)%n, (i+1)%n+n, i+n) for i in range(n)]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    scene.collection.objects.link(obj)
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    return finish_part(obj, name, pos, color, parent, .012)


def torus(name, pos, major, minor, color, axis='z', parent=None):
    bpy.ops.mesh.primitive_torus_add(major_segments=20, minor_segments=4, major_radius=major, minor_radius=minor)
    obj = bpy.context.object
    if axis == 'z': obj.rotation_euler.x = math.pi / 2
    return finish_part(obj, name, pos, color, parent)


def cash(pos=(0, 0, 0), parent=None):
    x, y, z = pos
    for i in range(3):
        box('Banknote_layer', (x, y+.045+i*.085, z), (.84, .07, .42), '#A9FFBE' if i==2 else GREEN, parent, .018)
    box('Paper_band_top', (x, y+.26, z), (.18, .025, .43), WHITE, parent, .006)
    box('Paper_band_bottom', (x, y+.01, z), (.18, .02, .43), WHITE, parent, .006)
    for side in (-1, 1): box('Paper_band_side', (x, y+.135, z+side*.215), (.18, .25, .014), WHITE, parent, .004)


def star_points(radius, inner=.40, count=4, center=(0,0)):
    return [(center[0]+math.sin(i*math.pi/count)*radius*(1 if i%2==0 else inner),
             center[1]+math.cos(i*math.pi/count)*radius*(1 if i%2==0 else inner)) for i in range(count*2)]


def coin():
    cylinder('Coin_rim', (0,.4,0), .4, .10, GOLD, axis='z', vertices=20)
    cylinder('Coin_face', (0,.4,-.061), .32, .024, '#E5A521', axis='z', vertices=20)
    polygon('Sparkle', star_points(.24, center=(0,.4)), .018, '#FFF5B1', pos=(0,0,-.079))


def key():
    torus('Key_head',(-.12,.61,0),.15,.045,GOLD)
    box('Shaft',(-.12,.24,0),(.09,.46,.11),GOLD)
    for y in (.06,.20):box('Tooth',(-.015,y,0),(.23,.08,.11),GOLD)


def gold_bar():
    verts=[co((x,y,z)) for y,w,d in [(0,.8,.42),(.28,.62,.30)] for x,z in [(-w/2,-d/2),(w/2,-d/2),(w/2,d/2),(-w/2,d/2)]]
    mesh=bpy.data.meshes.new('Ingot');mesh.from_pydata(verts,[],[(3,2,1,0),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]);mesh.update()
    obj=bpy.data.objects.new('Ingot',mesh);scene.collection.objects.link(obj)
    bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
    finish_part(obj,'Ingot',(0,0,0),GOLD,bevel=.025)


def power(kind):
    if kind=='shield':
        outline=[(-.36,.78),(0,.84),(.36,.78),(.31,.35),(0,0),(-.31,.35)]
        polygon('Shield',outline,.15,BLUE)
        polygon('Check',[(-.23,.43),(-.15,.51),(-.04,.39),(.19,.66),(.26,.59),(-.04,.25)],.025,WHITE,pos=(0,0,-.092))
    elif kind=='magnet':
        outline=[(-.38,.78),(-.38,.38)]
        outline += [(.38*math.cos(a),.38+.38*math.sin(a)) for a in [math.pi+i*math.pi/12 for i in range(1,13)]]
        outline += [(.38,.78),(.20,.78),(.20,.38)]
        outline += [(.20*math.cos(a),.38+.20*math.sin(a)) for a in [2*math.pi-i*math.pi/12 for i in range(1,13)]]
        outline += [(-.20,.78)]
        polygon('Magnet',outline,.18,RED)
        for side in (-1,1):box('Blue_tip',(side*.29,.69,-.006),(.18,.18,.20),BLUE)
    elif kind=='luck':
        box('Stem',(0,.15,0),(.07,.3,.10),GREEN)
        for x,y in [(-.17,.40),(.17,.40),(-.17,.66),(.17,.66)]:
            sphere('Gold_rim',(x,y,0),(.38,.36,.14),GOLD)
            sphere('Leaf',(x,y,-.032),(.33,.31,.12),GREEN)
    elif kind=='double_cash':
        cash((-.15,0,-.13));cash((.15,.25,.13));asset.scale=(.7,.7,.7)
    elif kind=='slow_motion':
        cylinder('Timer_rim',(0,.37,0),.34,.16,BLUE,axis='z',vertices=20)
        cylinder('Blank_face',(0,.37,-.09),.28,.03,WHITE,axis='z',vertices=20)
        cylinder('Top_button',(0,.80,0),.075,.12,GOLD)
        box('Stem',(0,.72,0),(.075,.12,.08),GOLD)
        box('Minute_hand',(0,.46,-.117),(.045,.22,.025),DARK,bevel=.005)
        hand=box('Hour_hand',(.065,.335,-.118),(.16,.045,.028),DARK,bevel=.005);hand.rotation_euler.y=.45
        cylinder('Pin',(0,.37,-.14),.045,.025,GOLD,axis='z',vertices=12)


def gate(color=BLUE):
    for side in (-1, 1):
        box('Pillar', (side*1.52,1.5,0), (.28,3,.32), color)
        box('Foot', (side*1.52,.13,0), (.52,.26,.64), WHITE)
    box('Blank_sign', (0,3.5,0), (3.36,2.35,.24), color, bevel=.07)
    box('Top_cap', (0,4.72,0), (3.5,.12,.38), WHITE)
    for label,y in [('Title',4.3),('Value',3.62),('Odds',2.91),('Loss',2.45)]:
        group('Anchor_'+label, (0,y,-.16))


def track(kind):
    if kind=='split':
        for side in (-1,1):box('Bridge', (side*2.6,-.26,9),(3.05,.5,18),'#F4F6F8',bevel=0)
    else:box('Road', (0,-.26,9),(8.4,.5,18),'#F4F6F8',bevel=0)
    for side in (-1,1):
        box('Edge',(side*4.22,-.12,9),(.22,.24,18),GOLD if kind=='bonus' else '#D9E1EA',bevel=0)
        for z in (2,6,10,14):box('Road_stud',(side*4.1,.025,z),(.12,.025,1),GOLD,bevel=0)
    if kind=='finish':
        for x in range(-4,5):box('Finish_marker',(x*.8,.002,3),(.38,.014,.36),BLUE,bevel=0)


def vault(color='#70899F'):
    # Four walls leave an actual cavity visible when DoorHinge opens.
    body_color=GOLD if color==GOLD else WHITE if color=='#B67AFF' else '#566575'
    for x in (-2.2,2.2):box('Vault_side',(x,2,0),(.4,4,2),body_color)
    for y in (.2,3.8):box('Vault_lintel',(0,y,0),(4,.4,2),body_color)
    box('Vault_back',(0,2,.86),(4,3.2,.28),DARK)
    # Closed square-to-circle front frame, with a genuine circular doorway.
    verts=[]
    n=32
    for z in (-1.16,-.96):
        for outer in (True,False):
            for i in range(n):
                a=i*2*math.pi/n
                dx,dy=math.cos(a),math.sin(a)
                r=min(2.2/max(abs(dx),1e-6),1.8/max(abs(dy),1e-6)) if outer else 1.64
                verts.append(co((r*dx,2+r*dy,z)))
    faces=[]
    for i in range(n):
        j=(i+1)%n
        faces += [(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),
                  (i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)]
    mesh=bpy.data.meshes.new('Circular_front_frame');mesh.from_pydata(verts,[],faces);mesh.update()
    frame=bpy.data.objects.new('Circular_front_frame',mesh);scene.collection.objects.link(frame)
    bpy.ops.object.select_all(action='DESELECT');frame.select_set(True);bpy.context.view_layer.objects.active=frame
    finish_part(frame,'Circular_front_frame',(0,0,0),'#FFF5B1' if color==GOLD else '#AAB7C4')
    hinge = group('DoorHinge',(-1.6,2,-1.4))
    cylinder('Door',(1.6,0,0),1.6,.3,color,'z',24,hinge)
    cylinder('Door_inset',(1.6,0,-.17),1.37,.06,color if color!='#B67AFF' else WHITE,'z',12 if color=='#A9E9FF' else 24,hinge)
    torus('Door_rim',(1.6,0,-.19),1.46,.065,color if color=='#B67AFF' else '#FFF5B1',parent=hinge)
    wheel=group('WheelPivot',(1.6,0,-.3),hinge)
    cylinder('Lock_hub',(0,0,0),.22,.23,GOLD,'z',16,wheel)
    torus('Lock_wheel',(0,0,-.14),.60,.075,GOLD,parent=wheel)
    for i in range(3):
        bar=box('Wheel_spoke',(0,0,-.12),(1.14,.12,.12),GOLD,wheel)
        bar.rotation_euler.y=i*math.pi/3
    for x in (-.9,0,.9):cash((x,.55,.02))


def building(kind,color='#FFC77C',accent=BLUE,height=4,width=3.4):
    if kind=='crate':
        box('Crate',(0,.55,0),(1.1,1.1,1.1),'#C89462')
        for x in (-.4,.4):box('Brace',(x,.55,-.58),(.13,1.12,.07),'#916044')
        for y in (.12,.98):box('Brace',(0,y,-.58),(1.12,.13,.07),'#916044')
        return
    box('Plinth',(0,.12,0),(width+.3,.24,2.8),'#D9E1EA')
    box('Facade',(0,height/2+.2,0),(width,height,2.5),color,bevel=.09)
    box('Roof_cap',(0,height+.25,0),(width+.25,.24,2.75),accent)
    floors=max(1,int(height/1.8))
    columns=2 if width<4 else 3
    for row in range(floors):
        for col in range(columns):
            x=(col-(columns-1)/2)*width/(columns+.4)
            y=1.35+row*(height-.7)/floors
            box('Window_frame',(x,y,-1.285),(.85,.96,.10),WHITE,bevel=.025)
            box('Opaque_window',(x,y,-1.35),(.65,.76,.045),'#7DD8FF',bevel=.015)
    box('Door',(0,.76,-1.31),(.65,1.24,.10),'#253A70')
    box('Door_glass',(0,.85,-1.38),(.43,.71,.03),'#7DD8FF',bevel=.005)
    if kind in ('shopfront','cafe','kiosk'):
        box('Awning',(0,2.02,-1.58),(width+.15,.2,1.05),accent)
        for i in range(5):box('Awning_stripe',((i-2)*width/5,2.13,-1.58),(width/10,.026,1.05),WHITE,bevel=0)
        box('Blank_shop_sign',(0,height-.28,-1.34),(width*.67,.48,.16),WHITE)
    elif kind in ('house','waterfront_house','villa','mansion'):
        polygon('Gable',[(-width*.55,0),(0,.9),(width*.55,0)],2.8,accent,(0,height+.28,0))
    elif kind in ('bank','bank_landmark','palace','treasury'):
        for x in (-width*.42,width*.42):cylinder('Column',(x,height*.43,-1.63),.19,height*.75,WHITE,vertices=10)
        polygon('Pediment',[(-width*.55,0),(0,.7),(width*.55,0)],.48,accent,(0,height+.3,-1.4))
    elif kind in ('tower','tower_curved','office','apartment'):
        box('Roof_crown',(0,height+.7,0),(width*.7,.65,1.8),WHITE)


def light_camera(bounds):
    for obj in list(bpy.data.objects):
        if obj.type in ('CAMERA','LIGHT'):bpy.data.objects.remove(obj,do_unlink=True)
    low,high=bounds
    center=(low+high)/2
    size=max(high-low)
    bpy.ops.object.camera_add(location=center+Vector(co((1.3,1.25,-2.0)))*size)
    camera=bpy.context.object
    camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.type='ORTHO'
    camera.data.ortho_scale=size*1.55
    scene.camera=camera
    for offset,power,scale in [((-2,-1,4),450,4),((3,2,2),150,3)]:
        bpy.ops.object.light_add(type='AREA',location=center+Vector(offset)*size)
        lamp=bpy.context.object
        lamp.data.energy=power*size*size
        lamp.data.shape='DISK'
        lamp.data.size=size*scale
        lamp.rotation_euler=(center-lamp.location).to_track_quat('-Z','Y').to_euler()


def export(asset_id,budget=2000,notes=''):
    folder=EXPORT/asset_id
    folder.mkdir(exist_ok=True)
    fbx=folder/(asset_id+'__v001__model.fbx')
    if fbx.exists() and not args.replace_drafts:raise RuntimeError('Refusing overwrite: '+str(fbx))
    # One renderer per independently moving part, sharing one atlas material.
    parent_groups={}
    for obj in objects:
        if obj.type=='MESH':parent_groups.setdefault(obj.parent,[]).append(obj)
    for parent,parts in parent_groups.items():
        if len(parts)<2:continue
        bpy.ops.object.select_all(action='DESELECT')
        for obj in parts:obj.select_set(True)
        bpy.context.view_layer.objects.active=parts[0]
        bpy.ops.object.join()
        parts[0].name=parent.name+'_Mesh'
        # Joined objects have identical material slots; normalize to one slot.
        parts[0].data.materials.clear();parts[0].data.materials.append(mat)
        for poly in parts[0].data.polygons:poly.material_index=0
    objects[:]=[obj for obj in scene.objects if obj.type not in ('CAMERA','LIGHT')]
    bpy.context.view_layer.update()
    meshes=[obj for obj in objects if obj.type=='MESH']
    points=[obj.matrix_world@Vector(corner) for obj in meshes for corner in obj.bound_box]
    low=Vector([min(v[i] for v in points) for i in range(3)])
    high=Vector([max(v[i] for v in points) for i in range(3)])
    triangles=0
    for obj in meshes:
        obj.data.calc_loop_triangles()
        triangles+=len(obj.data.loop_triangles)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:obj.select_set(True)
    bpy.context.view_layer.objects.active=asset
    bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'MESH','EMPTY'},
        axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',
        bake_space_transform=False,add_leaf_bones=False,bake_anim=False,path_mode='RELATIVE',mesh_smooth_type='FACE')
    light_camera((low,high))
    scene.render.filepath=str(folder/(asset_id+'__v001__preview.png'))
    bpy.ops.render.render(write_still=True)
    if asset_id=='PROP_coin':
        scene.render.resolution_x=scene.render.resolution_y=128
        scene.render.filepath=str(EXPORT/'Particles'/'PT_coin__v001.png')
        bpy.ops.render.render(write_still=True)
        scene.render.resolution_x=scene.render.resolution_y=512
    # Save source with preview camera/lights; exporter above explicitly excludes them.
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(asset_id+'__v001.blend')),check_existing=False)
    record={'asset_id':asset_id,'status':'Exported','triangles':triangles,'triangle_budget':budget,
        'within_triangle_budget':triangles<=budget,'materials':1,
        'bounds_unity_metres':{'width':high.x-low.x,'height':high.z-low.z,'depth':high.y-low.y},
        'source':'Original parameterized Blender geometry; build_models.py',
        'palette':'../Shared/SAM_palette__v001.png','blender_version':bpy.app.version_string,
        'date':date.today().isoformat(),'credits':0,'mesh_objects':len(meshes),'notes':notes,
        'qa':'Blender counts only; pending FBX reimport, Unity, Android and visual approval.'}
    (folder/(asset_id+'__v001__record.json')).write_text(json.dumps(record,indent=2),encoding='utf-8')
    records.append(record)
    print('ASSET_EXPORTED '+asset_id+' triangles='+str(triangles),flush=True)


def produce(asset_id,builder,budget=2000,notes=''):
    if args.only and asset_id not in args.only.split(','):return
    start(asset_id)
    builder()
    export(asset_id,budget,notes)


produce('PROP_cash_stack',cash,600)
produce('PROP_coin',coin,600)
produce('PROP_key',key,900)
produce('PROP_gold_bar',gold_bar,300)
for kind in ('shield','magnet','luck','double_cash','slow_motion'):
    produce('PWR_'+kind,lambda k=kind:power(k),2500,'Visual only; no collider or baked gameplay text.')
produce('GATE_frame',gate,1500,'Blank TMP anchors retained. No collider in FBX.')
for kind in ('straight','split','bonus','finish'):
    produce('TRK_'+kind,lambda k=kind:track(k),1500,'18m length; gameplay collider is not exported.')
for name,color in [('classic','#70899F'),('gold',GOLD),('diamond','#A9E9FF'),('neon','#B67AFF')]:
    produce('vault_'+name,lambda c=color:vault(c),3000,'DoorHinge local pivot: Unity (-1.6,2,-1.4); separate door and wheel.')
for kind in ('shopfront','house','kiosk','crate'):
    produce('ENV_streets_'+kind,lambda k=kind:building(k,height=2.5 if k=='kiosk' else 4,width=2.4 if k=='kiosk' else 3.4),2000)
manifest_path=REVIEWS/'models__v001__manifest.json'
previous=json.loads(manifest_path.read_text(encoding='utf-8')) if manifest_path.exists() else []
merged={record['asset_id']:record for record in previous}
merged.update({record['asset_id']:record for record in records})
manifest_path.write_text(json.dumps(list(merged.values()),indent=2),encoding='utf-8')
print('COMPLETE: '+str(len(records))+' original model exports',flush=True)
