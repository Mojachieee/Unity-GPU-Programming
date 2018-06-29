#ifndef PROCEDURALINSTANCING_INCLUDE
#define PROCEDURALINSTANCING_INCLUDE


#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
struct Fish {
	float3 pos;
	float mass;
	float3 vel;
	float padding;
};

StructuredBuffer<Fish> _SourceFish;
#endif  // UNITY_PROCEDURAL_INSTANCING_ENABLED

void ProceduralSetup() {
    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

    Fish fish = _SourceFish[unity_InstanceID];
    float3 pos = fish.pos;
    float3 forward = fish.vel;
    if (dot(forward, forward) == 0) {
        forward = float3(1 ,0 ,0);
    } else {
        forward = normalize(forward);
    }
    float3 up = float3(0, 1, 0);
    float3 right = cross(up, forward);

    up = cross(forward, right);

    unity_ObjectToWorld._11_21_31_41 = float4(right, 0);    // Rotation
    unity_ObjectToWorld._12_22_32_42 = float4(up, 0);       // Rotation
    unity_ObjectToWorld._13_23_33_43 = float4(forward, 0);  // Rotation
    unity_ObjectToWorld._14_24_34_44 = float4(pos, 1);      // Position
    
    unity_WorldToObject = transpose(unity_ObjectToWorld);
    unity_WorldToObject._14_24_34 = -pos.xyz;
    unity_WorldToObject._41_42_43_44 = float4(0, 0, 0, 1);
    

    #endif  // UNITY_PROCEDURAL_INSTANCING_ENABLED
}

#endif  // PROCEDURALINSTANCING_INCLUDE