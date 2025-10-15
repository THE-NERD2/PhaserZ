extends Button

@export var scenes: Array = []

func _pressed():
	var selected = $"../WorldOption".selected
	var scene = scenes[selected]
	get_tree().change_scene_to_file(scene)
