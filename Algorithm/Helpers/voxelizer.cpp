typedef struct _Vector3 
{ 
    float X; 
    float Y; 
    float Z; 
} Vector3; 

kernel void voxelize
(
    global read_only Vector3* verts
) 
{
    
    printf(""Y: %d\n"", verts[10].X);
}