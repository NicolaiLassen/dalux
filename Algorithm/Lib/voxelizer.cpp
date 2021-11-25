typedef struct _Vector3 
{ 
    float X; 
    float Y; 
    float Z; 
} Vector3; 

kernel void voxelize
(
    global read_only Vector3* verts,
    float unit,
    float hunit,
    int w,
    int h,
    int d,
    global char* dst
) 
{

          for(Triangle triangle in triangles){
                    printf(""Index %d\n"", triangle.A);       
                }

    for (int z = 0; z < d; z++) {
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
          
                dst[z + d * (y + h * x)] = 0;

            }
        }                    
    }

}