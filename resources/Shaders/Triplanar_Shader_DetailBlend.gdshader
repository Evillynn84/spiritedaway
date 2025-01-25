shader_type spatial;

//resources:
// https://www.youtube.com/watch?v=WkbVxxcU5d8
// https://www.youtube.com/watch?v=tqv84yq_Dgs
// https://www.shadertoy.com/view/NtVcRw
//https://discourse.selfshadow.com/t/blending-in-detail/21/18

render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
group_uniforms base_blend;
uniform sampler2D albedo : source_color, repeat_enable, filter_linear_mipmap, hint_default_white;
uniform sampler2D orm : repeat_enable,filter_linear_mipmap, hint_default_white;
uniform sampler2D normal : hint_normal,filter_linear_mipmap,repeat_enable;
uniform sampler2D blend_albedo : source_color, repeat_enable,filter_linear_mipmap, hint_default_white;
uniform sampler2D blend_orm: repeat_enable,filter_linear_mipmap, hint_default_white;
uniform sampler2D blend_normal : hint_normal,filter_linear_mipmap,repeat_enable;

uniform float base_blend_normal_scale : hint_range(-5.0, 5.0) = 0.7;
uniform float roughness : hint_range(0.0, 1.0) = 1;
uniform float metal : hint_range(0.0, 1.0) = 0;
uniform float ao_light_effect : hint_range(0.0,1.0);
uniform float tri_blend_factor : hint_range(0.1, 50.0) = 5.0;
uniform float base_and_secondary_maps_blend : hint_range(-3.0, 3.0) = 1.0;
uniform float blend_softness: hint_range(0.0, 2.0) = 0.25;
uniform bool invert_mask  = false;
uniform bool use_xz_mask = false;
uniform bool use_additional_mask_texture = false;
uniform sampler2D additional_mask : repeat_enable,filter_linear_mipmap, hint_default_white;
varying vec3 my_pos;
varying vec3 my_normal;
varying vec3 my_pos_blend;
varying vec3 my_pos_detail;
varying vec3 my_pos_add_mask;

varying mat3 tangent_2_local;
instance uniform vec3 uv_base_scale = vec3(0.45);
//uniform vec3 uv_base_scale = vec3(1.0);
uniform vec3 uv_base_offset = vec3(0.0);
instance uniform vec3 uv_blend_scale = vec3(0.45);
//uniform vec3 uv_blend_scale = vec3(1.0);
uniform vec3 uv_blend_offset = vec3(0.0);
uniform vec3 uv_additional_mask_scale = vec3(1.0);

group_uniforms detail;
uniform bool use_UV_map = false;
uniform bool use_detail_normal = false;
uniform vec3 uv_detail_scale = vec3(1.0);
uniform sampler2D detail_normal_map  : hint_normal,filter_linear_mipmap,repeat_enable;
uniform float detail_normal_scale : hint_range(-5.0,5.0) = 0.5;
uniform vec3 uv_detail_offset = vec3(0.0);

vec3 applyTexture(sampler2D tex, vec3 p, vec3 n, float k) {
     p = 0.5+0.5*p;
	
 // x, y, z axis
vec3 zy = texture(tex, -p.zy).rgb;
vec3 xz = texture(tex, -p.xz).rgb;
vec3 xy = texture(tex, -p.xy).rgb;
     n = pow(n, vec3(k));
     n /= dot(n, vec3(1));
return zy*n.x + xz * n.y + xy*n.z;
}

vec4 applyTextureAlpha(sampler2D tex, vec3 p, vec3 n, float k) {
p = 0.5+0.5*p;

vec4 samp = vec4(0.0);
vec4 zy = texture(tex, -p.zy).rgba;
vec4 xz = texture(tex, -p.xz).rgba;
vec4 xy = texture(tex, -p.xy).rgba;
     n = pow(n, vec3(k));
     n /= dot(n, vec3(1));
return samp = zy*n.x + xz * n.y + xy*n.z;
}





void vertex() 
{
my_pos = VERTEX * uv_base_scale + uv_base_offset;
my_pos *= vec3(-1.0,-1.0, 1.0);
my_pos_blend = VERTEX * uv_blend_scale + uv_blend_offset;
my_pos_blend *= vec3(-1.0,-1.0, 1.0);
my_pos_detail = VERTEX * uv_detail_scale + uv_detail_offset;
my_pos_detail *= vec3(-1.0,-1.0, 1.0);
my_pos_add_mask = VERTEX * uv_additional_mask_scale;
my_pos_add_mask *= vec3(-1.0,-1.0, 1.0);






my_normal = abs(NORMAL);
	//tangent, binormal code from Godot Triplanar code
	TANGENT = vec3(0.0,0.0,-1.0) * abs(my_normal.x);
	TANGENT+= vec3(1.0,0.0,0.0) * abs(my_normal.y);
	TANGENT+= vec3(1.0,0.0,0.0) * abs(my_normal.z);
	TANGENT = normalize(TANGENT);
	BINORMAL = vec3(0.0,1.0,0.0) * abs(my_normal.x);
	BINORMAL+= vec3(0.0,0.0,-1.0) * abs(my_normal.y);
	BINORMAL+= vec3(0.0,1.0,0.0) * abs(my_normal.z);
	BINORMAL = normalize(BINORMAL);

// in local space:
//tangent_2_local = mat3(TANGENT, BINORMAL, NORMAL);
// in world space:

tangent_2_local = MODEL_NORMAL_MATRIX*mat3(TANGENT, BINORMAL, NORMAL);
//normalize all components, so the UVs don't get scaled when scaling mesh
tangent_2_local[0] = normalize(tangent_2_local[0]);
tangent_2_local[1] = normalize(tangent_2_local[1]);
tangent_2_local[2] = normalize(tangent_2_local[2]);

}

void fragment() {
//NORMAL MAPS
vec3 base_normal_texture = applyTexture(normal, my_pos, my_normal, tri_blend_factor);
base_normal_texture = base_normal_texture.xyz;
//mask: unpack in tangent space for normals-based blending support
vec3 base_true_normal = base_normal_texture *2.0 - 1.0;
vec3 base_n = base_normal_texture *2.0 - 1.0;
base_n.x *= 1.0;
base_n.y *= -1.0;

//move normal from tangent space to local space
base_true_normal *= tangent_2_local;

float mask = base_true_normal.y;
if(use_xz_mask)
{
mask = base_true_normal.x + base_true_normal.z;
}

if(invert_mask){
mask = smoothstep(base_and_secondary_maps_blend, base_and_secondary_maps_blend + blend_softness, 1.0-mask);
}
else
{
mask = smoothstep(base_and_secondary_maps_blend, base_and_secondary_maps_blend + blend_softness, mask);
}
vec3 blend_normal_texture = applyTexture(blend_normal, my_pos_blend, my_normal,tri_blend_factor);
blend_normal_texture = blend_normal_texture.rgb;
blend_normal_texture = blend_normal_texture.rgb * 2.0 - 1.0;

vec3 blended_n = vec3(base_n.xy*base_blend_normal_scale + blend_normal_texture.xy*base_blend_normal_scale*mask, base_n.z);


blended_n.z = base_n.z;
blended_n = normalize(blended_n);
NORMAL_MAP = blended_n;



if(use_detail_normal){
	vec3 detail_norm_tex = applyTexture(detail_normal_map, my_pos_detail, my_normal,tri_blend_factor);
	detail_norm_tex = detail_norm_tex.rgb;
	detail_norm_tex = detail_norm_tex*2.0 - 1.0;
	vec3 combo_nm = vec3(NORMAL_MAP.xy + detail_norm_tex.xy*detail_normal_scale, NORMAL_MAP.z);
	combo_nm = normalize(combo_nm);

	NORMAL_MAP = combo_nm;
	NORMAL_MAP = NORMAL_MAP * 0.5 +0.5;
	}
	else
	{
	NORMAL_MAP = NORMAL_MAP * 0.5 +0.5;
	}

//ORM 

// ALEBDO MAPS
vec3 base_albedo_tex = applyTexture(albedo, my_pos, my_normal, tri_blend_factor);
base_albedo_tex = base_albedo_tex.rgb;

vec4 blend_albedo_tex = applyTextureAlpha(blend_albedo, my_pos_blend, my_normal, tri_blend_factor);
blend_albedo_tex = blend_albedo_tex.rgba;
if(use_additional_mask_texture){
vec3 add_mask_tex = applyTexture(additional_mask, my_pos_add_mask, my_normal, tri_blend_factor);
add_mask_tex = add_mask_tex.rgb;
ALBEDO = mix(base_albedo_tex.rgb, blend_albedo_tex.rgb, mask*blend_albedo_tex.a*add_mask_tex.r);
}
else
{
ALBEDO = mix(base_albedo_tex.rgb, blend_albedo_tex.rgb, mask*blend_albedo_tex.a);
}


vec3 orm_texture = applyTexture(orm, my_pos, my_normal, tri_blend_factor).rgb;
vec3 blend_orm_texture = applyTexture(blend_orm, my_pos, my_normal, tri_blend_factor).rgb;
vec3 blended_orm = mix(orm_texture.rgb,blend_orm_texture.rgb, mask*blend_albedo_tex.a);
ROUGHNESS = blended_orm.g;
ROUGHNESS *= roughness;
METALLIC = blended_orm.b;
METALLIC *= metal;
AO = blended_orm.r;
AO_LIGHT_AFFECT = ao_light_effect;


//for testing what the normals look like uncomment the next line
//ALBEDO = my_normal.rgb;
}

