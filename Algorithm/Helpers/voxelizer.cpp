typedef struct _Vector3 
{ 
    float X; 
    float Y; 
    float Z; 
} Vector3; 


typedef struct _VoxelGrid 
{ 
    Grid
 
} _VoxelGrid; 

kernel void voxelize
(
    global read_only Vector3* verts
) 
{
    
    printf(""Y: %d\n"", verts[10].X);
}