extends FlowContainer

func _ready():
	# Ensure settings are initialized
	if not FileAccess.file_exists("user://settings.json"):
		$SettingsDialog/Settings.save_settings()
	
	# Access worlds
	var world_folder_names = DirAccess.open("res://scenes/worlds").get_directories()
	for world_folder_name in world_folder_names:
		var world_data_file = FileAccess.open("res://scenes/worlds/%s/meta.json" % world_folder_name, FileAccess.READ)
		var world_data = JSON.parse_string(world_data_file.get_as_text())
		$StartButton.scenes.append("res://scenes/worlds/%s/%s" % [world_folder_name, world_data.scene_name])
		$WorldOption.add_item(world_data.full_name)
		world_data_file.close()
