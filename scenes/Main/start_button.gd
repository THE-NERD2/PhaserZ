extends Button

@export var scenes: Array = []

func _gui_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
			var selected = $"../WorldOption".selected
			var scene = scenes[selected]
			get_tree().change_scene_to_file(scene)
