#[compute]
#version 450

layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;

layout(set = 0, binding = 0, std430) restrict buffer InputImage {
    uint[] data;
} inputImage;
layout(set = 0, binding = 1, std430) restrict buffer OutputImage {
    uint[] data;
} outputImage;
layout(set = 0, binding = 2, std430) restrict buffer Size {
    uint size;
} size;
layout(set = 0, binding = 3, std430) restrict buffer Radius {
    uint radius;
} blur_radius;

void main() {
    uint cx = gl_GlobalInvocationID.x;
    uint cy = gl_GlobalInvocationID.y;
    uint minx = clamp(cx - blur_radius.radius, 0, size.size - 1);
    uint maxx = clamp(cx + blur_radius.radius, 0, size.size - 1);
    uint miny = clamp(cy - blur_radius.radius, 0, size.size - 1);
    uint maxy = clamp(cy + blur_radius.radius, 0, size.size - 1);

    uint l = 0;
    uint pixels = 0;
    for(uint y = miny; y <= maxy; y++) {
        for(uint x = minx; x <= maxx; x++) {
            uint idx = y * size.size + x;
            l += inputImage.data[idx];
            pixels++;
        }
    }
    l /= pixels;
    
    uint idx = cy * size.size + cx;
    outputImage.data[idx] = l;
}