use godot::global::{floori, randf};
use godot::prelude::*;
use godot::classes::{
    mesh::{PrimitiveType, ArrayType},
    fast_noise_lite::{FractalType, NoiseType},
    ArrayMesh,
    Node3D, INode3D,
    MeshInstance3D,
    Shader,
    ShaderMaterial,
    FastNoiseLite,
    ImageTexture
};

#[derive(GodotClass)]
#[class(base=Node3D)]
struct Terrain {
    #[export]
    divisions: i32,

    points: PackedVector3Array,
    indices: PackedInt32Array,
    max_point_width: i32,

    base: Base<Node3D>
}
#[godot_api]
impl INode3D for Terrain {
    fn init(base: Base<Node3D>) -> Self {
        Self {
            divisions: 8,
            points: PackedVector3Array::new(),
            indices: PackedInt32Array::new(),
            max_point_width: 0,
            base
        }
    }
    fn ready(&mut self) {
        let mut vertex_indices: Vec<Vec<i32>> = Vec::new();
        // Create vertices
        let min_x_num = (2 as i32).pow(self.divisions as u32) + 1;
        let max_x_num = (2 as i32).pow(self.divisions as u32 + 1) + 1;
        let z_num = max_x_num;
        let l = 128.0 / (max_x_num - 1) as f32;
        let h = 0.5 * l * f32::sqrt(3.0);
        let mut points_array: Vec<Vector3> = Vec::new();
        let mut index = 0;
        for z in 0..z_num {
            let x_num: i32;
            let second_half: bool;
            let second_half_z = z - (z_num / 2 + 1) + 1;
            if second_half_z >= 0 {
                x_num = max_x_num - second_half_z;
                second_half = true;
            } else {
                x_num = min_x_num + z;
                second_half = false;
            }
            let mut row_indices: Vec<i32> = Vec::new();
            for x in 0..x_num {
                let current_x: f32;
                if second_half {
                    current_x = -64.0 + 0.5 * l * second_half_z as f32 + l * x as f32;
                } else {
                    current_x = -32.0 - 0.5 * l * z as f32 + l * x as f32;
                }
                let current_z = 32.0 * f32::sqrt(3.0) - h * z as f32;
                points_array.push(Vector3::new(current_x, 0.0, current_z));
                row_indices.push(index);
                index += 1;
            }
            vertex_indices.push(row_indices);
        }
        self.points = points_array.into();
        self.max_point_width = max_x_num;
        // Create indices
        let mut indices: Vec<i32> = Vec::new();
        for z in 0..vertex_indices.len() - 1 {
            let top_row = &vertex_indices[z];
            let bottom_row = &vertex_indices[z + 1];
            let top_row_len = top_row.len();
            let bottom_row_len = bottom_row.len();
            if bottom_row_len > top_row_len {
                for x in 0..bottom_row_len + top_row_len - 2 {
                    let triangle: [i32; 3];
                    if x % 2 == 0 {
                        triangle = [bottom_row[x / 2], bottom_row[x / 2 + 1], top_row[x / 2]];
                    } else {
                        triangle = [top_row[x / 2], bottom_row[x / 2 + 1], top_row[x / 2 + 1]];
                    }
                    indices.extend(triangle);
                }
            } else {
                for x in 0..bottom_row_len + top_row_len - 2 {
                    let triangle: [i32; 3];
                    if x % 2 == 0 {
                        triangle = [top_row[x / 2], bottom_row[x / 2], top_row[x / 2 + 1]];
                    } else {
                        triangle = [bottom_row[x / 2], bottom_row[x / 2 + 1], top_row[x / 2 + 1]];
                    }
                    indices.extend(triangle);
                }
            }
        }
        self.indices = indices.into();
        // Create origin cell
        self.add_cell(Vector2i::ZERO);
    }
}
#[godot_api]
impl Terrain {
    fn add_cell(&mut self, position: Vector2i) {
        let mut arrays: Array<Variant> = Array::new();
        arrays.resize(ArrayType::MAX.ord() as usize, &Variant::nil());
        arrays.set(ArrayType::VERTEX.ord() as usize, &self.points.to_variant());
        arrays.set(ArrayType::INDEX.ord() as usize, &self.indices.to_variant());
        
        let mut material = ShaderMaterial::new_gd();
        material.set_shader(&(load("res://shaders/terrain.gdshader") as Gd<Shader>));
        let mut noise = FastNoiseLite::new_gd();
        noise.set_noise_type(NoiseType::SIMPLEX);
        noise.set_seed(floori(randf()) as i32);
        noise.set_frequency(0.01);
        noise.set_fractal_type(FractalType::FBM);
        noise.set_fractal_octaves(5);
        noise.set_fractal_lacunarity(2.0);
        noise.set_fractal_gain(0.5);
        let heightmap_img = noise.get_image(self.max_point_width, self.max_point_width).unwrap();
        let heightmap_tex = ImageTexture::create_from_image(&heightmap_img).unwrap();
        material.set_shader_parameter("heightmap", &heightmap_tex.to_variant());

        let mut mesh = ArrayMesh::new_gd();
        mesh.add_surface_from_arrays(PrimitiveType::TRIANGLES, &arrays);
        mesh.surface_set_material(0, &material);

        let mut mesh_instance = MeshInstance3D::new_alloc();
        mesh_instance.set_mesh(&mesh);

        // Give mesh instance shader material, add it to coordinate system, etc.
        self.base_mut().add_child(&mesh_instance);
    }
}