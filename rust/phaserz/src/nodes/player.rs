use godot::global::MouseButton;
use num::clamp;
use godot::prelude::*;
use godot::classes::{
    CharacterBody3D, ICharacterBody3D, InputEvent, InputEventMouseMotion, InputEventMouseButton
};
use godot::classes::input::MouseMode;

const SPEED: f32 = 5.0;
const GRAVITY: f32 = 9.8;
const JUMP_IMPULSE: f32 = 4.9;
const SENSITIVITY: f32 = 0.5;
#[derive(GodotClass)]
#[class(base=CharacterBody3D)]
struct Player {
    v: Vector3,
    yaw: f32,
    pitch: f32,

    base: Base<CharacterBody3D>
}
#[godot_api]
impl ICharacterBody3D for Player {
    fn init(base: Base<CharacterBody3D>) -> Self {
        Self {
            v: Vector3::ZERO,
            yaw: 0.0,
            pitch: 0.0,

            base
        }
    }
    fn ready(&mut self) {
        let mut input = Input::singleton();
        input.set_mouse_mode(MouseMode::CAPTURED);
    }
    fn input(&mut self, event: Gd<InputEvent>) {
        if event.is_class("InputEventMouseMotion") {
            let motion_event: Gd<InputEventMouseMotion> = event.cast();

            self.yaw -= motion_event.get_relative().x * SENSITIVITY;
            self.pitch -= motion_event.get_relative().y * SENSITIVITY;
            self.pitch = clamp(self.pitch, -90.0, 90.0);

            let mut visual = self.base_mut().get_node_as::<Node3D>("Visual");
            let mut rot_visual = visual.get_rotation_degrees();
            rot_visual.y = self.yaw;
            visual.set_rotation_degrees(rot_visual);

            let mut camera = visual.get_node_as::<Camera3D>("Camera3D");
            let mut rot_camera = camera.get_rotation_degrees();
            rot_camera.x = self.pitch;
            camera.set_rotation_degrees(rot_camera);
        } else if event.is_class("InputEventMouseButton") {
            let button_event: Gd<InputEventMouseButton> = event.cast();
            if button_event.get_button_index() == MouseButton::RIGHT && button_event.is_pressed() {
                let mut input = Input::singleton();
                if input.get_mouse_mode() == MouseMode::CAPTURED {
                    input.set_mouse_mode(MouseMode::VISIBLE);
                } else {
                    input.set_mouse_mode(MouseMode::CAPTURED);
                }
            }
        }
    }
    fn physics_process(&mut self, dt: f64) {
        let input = Input::singleton();
        
        let mut v = self.v;
        let mut dir = Vector3::ZERO;
        if input.is_action_pressed("move_forward") {
            dir.z -= 1.0;
        }
        if input.is_action_pressed("move_back") {
            dir.z += 1.0;
        }
        if input.is_action_pressed("move_left") {
            dir.x -= 1.0;
        }
        if input.is_action_pressed("move_right") {
            dir.x += 1.0;
        }
        if dir != Vector3::ZERO {
            dir = dir.normalized();

            let basis = self.base().get_node_as::<Node3D>("Visual").get_basis();
            dir = basis.col_a() * dir.x + basis.col_c() * dir.z;
        }
        v.x = dir.x * SPEED;
        v.z = dir.z * SPEED;
        if !self.base().is_on_floor() {
            v.y -= dt as f32 * GRAVITY;
        } else if input.is_action_pressed("jump") {
            v.y = JUMP_IMPULSE;
        } else {
            v.y = 0.0;
        }
        self.base_mut().set_velocity(v);
        self.base_mut().move_and_slide();
        
        self.v = v;
    }
}