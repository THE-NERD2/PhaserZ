use godot::prelude::{gdextension, ExtensionLibrary};

struct PhaserZ;

#[gdextension]
unsafe impl ExtensionLibrary for PhaserZ {}

pub mod nodes;