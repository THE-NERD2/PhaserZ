extends OptionButton

@export var prevSelected = self.selected

func _process(dt):
	if prevSelected != self.selected:
		prevSelected = self.selected
