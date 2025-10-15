extends Window

func _notification(notification):
	if notification == NOTIFICATION_WM_CLOSE_REQUEST:
		$Settings.save_settings()
		visible = false
