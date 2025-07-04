extends Area3D

func _ready():
	body_entered.connect(_on_body_enter)
	body_exited.connect(_on_body_exit)
	
func _on_body_enter(body):
	if body.is_in_group("player"):
		body.enter_water()
		
func _on_body_exit(body):
	if body.is_in_group("player"):
		body.exit_water()
