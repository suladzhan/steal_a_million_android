"""Optimize paid textured Meshy models in Blender, retain originals and editable sources."""
import bpy,json,math
from mathutils.kdtree import KDTree
from pathlib import Path
from mathutils import Vector
root=Path.cwd();base=root/'AssetProduction/02_Generated/Major';out=root/'Assets/Art/Major';out.mkdir(parents=True,exist_ok=True)
specs={'townhouse':(3500,4.6,6,5),'tower':(3500,4.4,11,5),'villa':(3500,5.5,5.2,5.5),'future':(3500,4.5,11,4.5),'palm2':(1500,3.5,4.7,3.5),'tree':(1500,3.2,4.1,3.2),'car':(2500,1.5,1.4,2.4),'barrier':(2500,1.7,1.4,.65)}
report=[]
for name,(budget,width,height,depth) in specs.items():
    state=json.loads((base/(name+'.json')).read_text(encoding='utf-8-sig'));source=Path(state['project'])
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    if (out/(name+'.fbx')).exists():
        bpy.ops.import_scene.fbx(filepath=str(out/(name+'.fbx')))
        existing=[o for o in bpy.context.scene.objects if o.type=='MESH']
        count=0
        for o in existing:o.data.calc_loop_triangles();count+=len(o.data.loop_triangles)
        if count>budget+20:raise RuntimeError('Existing export exceeds budget: '+name)
        materials=[{'name':p.stem,'texture':p.name} for p in sorted(out.glob(name+'_M*.png'))]
        report.append({'id':name,'source':str(source.relative_to(root)),'original_triangles':None,'triangles':count,'materials':materials,'budget':budget})
        continue
    bpy.ops.import_scene.gltf(filepath=str(source/'model.glb'))
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();obj=bpy.context.object;obj.name='Major_'+name;obj.parent=None
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    if name=='car' and obj.dimensions.x>obj.dimensions.y:obj.rotation_euler.z=math.pi/2;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    obj.data.calc_loop_triangles();original=len(obj.data.loop_triangles)
    dec=obj.modifiers.new('Mobile LOD0','DECIMATE');dec.ratio=min(1,budget/original);dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name)
    points=[obj.matrix_world@Vector(c) for c in obj.bound_box];low=Vector([min(p[i] for p in points) for i in range(3)]);high=Vector([max(p[i] for p in points) for i in range(3)]);size=high-low
    centre=(low+high)/2;scale=min(width/max(size.x,.001),depth/max(size.y,.001),height/max(size.z,.001))
    for v in obj.data.vertices:
        p=obj.matrix_world@v.co;v.co=Vector(((p.x-centre.x)*scale,(p.y-centre.y)*scale,(p.z-low.z)*scale))
    obj.matrix_world.identity()
    obj.data.calc_loop_triangles()
    if len(obj.data.loop_triangles)>budget+20:
        # Disconnected leaves resist collapse. Merge their silhouette, then transfer
        # the original painted colour through a sampled vertex colour bake.
        mesh=obj.data;uv=mesh.uv_layers.active.data
        tex=next(n.image for m in mesh.materials for n in m.node_tree.nodes if n.type=='TEX_IMAGE')
        pixels=list(tex.pixels);tw,th=tex.size;colours={}
        for loop in mesh.loops:
            t=uv[loop.index].uv;x=max(0,min(tw-1,int(t.x*tw)));y=max(0,min(th-1,int(t.y*th)))
            k=(y*tw+x)*4;colours[loop.vertex_index]=pixels[k:k+4]
        kd=KDTree(len(mesh.vertices))
        for v in mesh.vertices:kd.insert(v.co,v.index)
        kd.balance()
        remesh=obj.modifiers.new('Continuous mobile foliage','REMESH');remesh.mode='VOXEL';remesh.voxel_size=height/32
        bpy.ops.object.modifier_apply(modifier=remesh.name)
        obj.data.calc_loop_triangles();dec=obj.modifiers.new('Final foliage budget','DECIMATE');dec.ratio=min(1,(budget-10)/len(obj.data.loop_triangles));dec.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=dec.name)
        attribute=obj.data.color_attributes.new(name='TransferredPaint',type='FLOAT_COLOR',domain='CORNER')
        for loop in obj.data.loops:
            _,index,_=kd.find(obj.data.vertices[loop.vertex_index].co);attribute.data[loop.index].color=colours.get(index,(.2,.5,.15,1))
        bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.03);bpy.ops.object.mode_set(mode='OBJECT')
        mat=bpy.data.materials.new(name+'_Paint');mat.use_nodes=True;nodes=mat.node_tree.nodes;nodes.clear()
        colour=nodes.new('ShaderNodeVertexColor');colour.layer_name='TransferredPaint';emit=nodes.new('ShaderNodeEmission');output=nodes.new('ShaderNodeOutputMaterial');mat.node_tree.links.new(colour.outputs['Color'],emit.inputs['Color']);mat.node_tree.links.new(emit.outputs[0],output.inputs[0])
        obj.data.materials.clear();obj.data.materials.append(mat)
        image=bpy.data.images.new(name+'_MobilePaint',width=1024,height=1024);target=nodes.new('ShaderNodeTexImage');target.image=image;nodes.active=target
        bpy.context.scene.render.engine='CYCLES';bpy.context.scene.cycles.samples=1;bpy.context.scene.render.bake.margin=8;bpy.ops.object.bake(type='EMIT')
        nodes.remove(colour);nodes.remove(emit);bsdf=nodes.new('ShaderNodeBsdfPrincipled');mat.node_tree.links.new(target.outputs['Color'],bsdf.inputs['Base Color']);mat.node_tree.links.new(bsdf.outputs[0],output.inputs[0])
    materials=[]
    for index,mat in enumerate(obj.data.materials):
        mat.name=f'{name}_M{index}';image=None
        if mat.use_nodes:
            node=next((n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None)
            if node:
                node.inputs['Roughness'].default_value=.75;node.inputs['Metallic'].default_value=0
                links=node.inputs['Base Color'].links
                image=links[0].from_node.image if links and links[0].from_node.type=='TEX_IMAGE' else None
        if image:
            image.scale(1024,1024);image.filepath_raw=str(out/f'{name}_M{index}.png');image.file_format='PNG';image.save()
        materials.append({'name':mat.name,'texture':f'{name}_M{index}.png' if image else None})
    for p in obj.data.polygons:p.use_smooth=True
    obj.data.calc_loop_triangles();triangles=len(obj.data.loop_triangles)
    if triangles>budget+20:raise RuntimeError(f'Triangle budget failed: {name}, {triangles}')
    bpy.ops.wm.save_as_mainfile(filepath=str(source/(name+'__mobile.blend')))
    bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=False,bake_anim=False,path_mode='STRIP')
    report.append({'id':name,'source':str(source.relative_to(root)),'original_triangles':original,'triangles':triangles,'materials':materials,'size':list(obj.dimensions),'budget':budget})
reviews=root/'AssetProduction/Reviews/Major';reviews.mkdir(parents=True,exist_ok=True);(reviews/'mobile_geometry.json').write_text(json.dumps({'assets':report},indent=2))
print('MAJOR_MOBILE_EXPORTED',[(r['id'],r['triangles']) for r in report])
