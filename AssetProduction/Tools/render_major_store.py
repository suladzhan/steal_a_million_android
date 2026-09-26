"""Render a store feature graphic from actual imported game models and our logo."""
import bpy,math
from pathlib import Path
from mathutils import Vector
root=Path.cwd();out=root/'Store';out.mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=1024;scene.render.resolution_y=500;scene.render.resolution_percentage=100
scene.world.color=(.45,.72,.9);scene.view_settings.view_transform='Standard'
def material(name,color):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*color,1);m.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.8;return m
def cube(name,pos,size,mat,bevel=.08):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.scale=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
    if bevel:m=o.modifiers.new('Toy bevel','BEVEL');m.width=bevel;m.segments=3;o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    return o
cream=material('Road',(.9,.94,.98));green=material('Cash',(.13,.72,.33));band=material('Band',(1,.99,.89));ground=material('District',(.26,.70,.60))
cube('Ground',(0,5,-.3),(35,30,.5),ground);cube('Road',(0,2,0),(8,24,.3),cream)
def imported(name,pos,scale=1):
    before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Art/Major'/f'{name}.fbx'));objects=set(bpy.context.scene.objects)-before
    group=bpy.data.objects.new(name+'_placement',None);scene.collection.objects.link(group);group.location=pos;group.scale=(scale,)*3
    for o in objects:
        if o.parent is None:o.parent=group
    return group
imported('townhouse',(-7,5,0),1.1);imported('villa',(7,5,0),1.1);imported('tower',(-11,11,0),1);imported('future',(11,11,0),1)
imported('palm2',(-5,1,0),1);imported('tree',(5,1,0),1);imported('car',(3,-3,0),1.2)
for i in range(5):
    x=-2+(i%2)*2;y=-4+i*1.6
    for j in range(3):cube('Cash stack',(x,y,.3+j*.12),(1.4,.6,.10),green,.035)
    cube('Cash band',(x,y,.52),(.24,.64,.38),band,.02)
bpy.ops.object.camera_add(location=(0,-25,16));camera=bpy.context.object;camera.rotation_euler=(Vector((0,3,2))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=24;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(-5,-7,15));lamp=bpy.context.object;lamp.data.energy=2100;lamp.data.shape='DISK';lamp.data.size=10;lamp.rotation_euler=(Vector((0,3,0))-lamp.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.light_add(type='SUN');bpy.context.object.data.energy=1.2;bpy.context.object.rotation_euler=(math.radians(35),math.radians(-20),math.radians(-25))
# Place a transparent unlit logo in front of the camera; no fake gameplay text.
logo=bpy.data.images.load(str(root/'Assets/Resources/Runner/UI/Logo-v001.png'))
m=bpy.data.materials.new('Original game logo');m.use_nodes=True;nodes=m.node_tree.nodes;nodes.clear();tex=nodes.new('ShaderNodeTexImage');tex.image=logo;em=nodes.new('ShaderNodeEmission');tr=nodes.new('ShaderNodeBsdfTransparent');mix=nodes.new('ShaderNodeMixShader');output=nodes.new('ShaderNodeOutputMaterial');links=m.node_tree.links;links.new(tex.outputs['Color'],em.inputs['Color']);links.new(tex.outputs['Alpha'],mix.inputs[0]);links.new(tr.outputs[0],mix.inputs[1]);links.new(em.outputs[0],mix.inputs[2]);links.new(mix.outputs[0],output.inputs['Surface']);m.surface_render_method='DITHERED'
direction=camera.rotation_euler.to_quaternion()@Vector((0,0,-1));bpy.ops.mesh.primitive_plane_add(size=2,location=camera.location+direction*10);plane=bpy.context.object;plane.rotation_euler=camera.rotation_euler;plane.scale=(7.1,7.1*logo.size[1]/logo.size[0],1);plane.data.materials.append(m)
scene.render.filepath=str(out/'feature-graphic-1024x500.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'AssetProduction/Reviews/Major/store-feature.blend'))
print('STORE_FEATURE_RENDERED',scene.render.filepath)
