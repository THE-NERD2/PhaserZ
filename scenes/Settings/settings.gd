extends Node

func _ready():
	var file = FileAccess.open("user://settings.json", FileAccess.READ)
	var settings = JSON.parse_string(file.get_as_text())
	for setting in settings.keys():
		var setting_path = setting.split(".")
		get_node("%s/VBoxContainer/%s/CheckButton" % [setting_path[0], setting_path[1]]).button_pressed = settings[setting]
	file.close()
func save_settings():
	var settings = {}
	for setting_group in get_children():
		for setting in setting_group.get_node("VBoxContainer").get_children():
			settings["%s.%s" % [setting_group.name, setting.name]] = setting.get_node("CheckButton").button_pressed
	var file = FileAccess.open("user://settings.json", FileAccess.WRITE)
	file.store_string(JSON.stringify(settings))
	file.close()
	SettingsHandler.Update()
