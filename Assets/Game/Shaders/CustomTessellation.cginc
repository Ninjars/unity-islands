// Tessellation programs based on this article by Catlike Coding:
// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

struct vertexInput
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 color : COLOR;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
};

struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float3 color : COLOR;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
};

struct TessellationFactors 
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

vertexInput vert(vertexInput v) {
	return v;
}

vertexOutput tessVert(vertexInput v) {
	vertexOutput o;
	// Note that the vertex is NOT transformed to clip
	// space here; this is done in the grass geometry shader.
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.color = v.color;
	o.uv = v.uv;
	o.uv1 = v.uv1;
	o.uv2 = v.uv2;
	return o;
}

float _TessellationUniform;
float _TessellationEdgeLength;

float TessellationEdgeFactor (
	vertexInput cp0, vertexInput cp1
) {
	float3 p0 = mul(unity_ObjectToWorld, float4(cp0.vertex.xyz, 1)).xyz;
	float3 p1 = mul(unity_ObjectToWorld, float4(cp1.vertex.xyz, 1)).xyz;
	float edgeLength = distance(p0, p1);
	return edgeLength / _TessellationEdgeLength;
}

TessellationFactors patchConstantFunction (InputPatch<vertexInput, 3> patch) {
	TessellationFactors f;
	f.edge[0] = TessellationEdgeFactor(patch[1], patch[2]);
	f.edge[1] = TessellationEdgeFactor(patch[0], patch[2]);
	f.edge[2] = TessellationEdgeFactor(patch[1], patch[0]);
	f.inside = _TessellationUniform;
	return f;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("patchConstantFunction")]
vertexInput hull (InputPatch<vertexInput, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[UNITY_domain("tri")]
vertexOutput domain(
	TessellationFactors factors, 
	OutputPatch<vertexInput, 3> patch, 
	float3 barycentricCoordinates : SV_DomainLocation
) {
	vertexInput v;

	#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
	MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
	MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
	MY_DOMAIN_PROGRAM_INTERPOLATE(color)
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv)
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv1)
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv2)

	return tessVert(v);
}