extends Button

@export var world_basic_scene: PackedScene

func _gui_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
			var selected = $"../WorldOption".selected
			var currentWorld = $"../../WorldContainer".get_child(0)
			if is_instance_valid(currentWorld):
				currentWorld.queue_free()
			if selected == 0:
				var scene = world_basic_scene.instantiate()
				$"../../WorldContainer".add_child(scene)
