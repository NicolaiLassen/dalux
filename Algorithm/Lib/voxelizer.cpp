            typedef struct _Vector3 
            { 
                float X; 
                float Y; 
                float Z; 
            } Vector3; 

            typedef struct _Bounds 
            { 
                Vector3 Center; 
                Vector3 Extents;
            } Bounds; 

            typedef struct _Triangle 
            { 
                Vector3 A; 
                Vector3 B; 
                Vector3 C; 
                Vector3 Normal;
                Bounds Bounds;
            } Triangle; 
            
            typedef struct _AABB
            {
                Vector3 Max;
                Vector3 Min;
                Vector3 Center;             
            } AABB;

            Vector3 subScalar(Vector3 left, float right) {
                 Vector3 vec;
                 vec.X = left.X - right;
                 vec.Y = left.Y - right;
                 vec.Z = left.Z - right;
                 return vec;
            }

            Vector3 addScalar(Vector3 left, float right) {
                 Vector3 vec;
                 vec.X = left.X + right;
                 vec.Y = left.Y + right;
                 vec.Z = left.Z + right;
                 return vec;
            }

            Vector3 subVec3(Vector3 left, Vector3 right) {
                 Vector3 vec;
                 vec.X = left.X - right.X;
                 vec.Y = left.Y - right.Y;
                 vec.Z = left.Z - right.Z;
                 return vec;
            }

            Vector3 addVec3(Vector3 left, Vector3 right) {
                 Vector3 vec;
                 vec.X = left.X + right.X;
                 vec.Y = left.Y + right.Y;
                 vec.Z = left.Z + right.Z;
                 return vec;
            }

            bool intersects_tri_aabb(Triangle tri, AABB aabb)
            {
                float p0, p1, p2, r;

                return false;
            }

            kernel void voxelize
            (
                global read_only Triangle* triangles,
                float unit,
                int w,
                int h,
                int d,
                Vector3 s,
                Vector3 e,
                global char* dst
            ) 
            {
               int index = get_global_id(0);     
               Triangle tri = triangles[index];

               Vector3 bCenter = tri.Bounds.Center;
               Vector3 bExtents = tri.Bounds.Extents;

               Vector3 bMin = subVec3(bCenter, bExtents);
               Vector3 bMax = addVec3(bCenter, bExtents);
       
               Vector3 fmin = subVec3(bMin, s); 
               Vector3 fmax = subVec3(bMax, s);        
            
               int iMinX = round(fmin.X / unit);
               int iMinY = round(fmin.Y / unit);
               int iMinZ = round(fmin.Z / unit);

               int iMaxX = round(fmax.X / unit);
               int iMaxY = round(fmax.Y / unit);
               int iMaxZ = round(fmax.Z / unit);
               
               iMinX = clamp(iMinX, 0, w - 1);
               iMinY = clamp(iMinY, 0, h - 1);
               iMinZ = clamp(iMinZ, 0, d - 1);

               iMaxX = clamp(iMaxX, 0, w - 1);
               iMaxY = clamp(iMaxY, 0, h - 1);
               iMaxZ = clamp(iMaxZ, 0, d - 1);
                
               for (int z = iMinZ; z < iMaxZ; z++) {
                    for (int y = iMinY; y < iMaxY; y++) {
                        for (int x = iMinZ; x < iMaxY; x++) {  

                            Vector3 center;
                            center.X = x * unit + s.X;
                            center.Y = y * unit + s.Y;
                            center.Z = z * unit + s.Z;
        
                            AABB aabb;
                            aabb.Center = center;

                            float hunit = unit * 0.5;      

                            Vector3 min = subScalar(center, hunit);
                            aabb.Min = min;

                            Vector3 max = addScalar(center, hunit);
                            aabb.Max = max;
                      
                            if(intersects_tri_aabb(tri, aabb)){
                                dst[z + d * (y + h * x)] = 5;
                            }

                        }
                    }                    
                }
            }