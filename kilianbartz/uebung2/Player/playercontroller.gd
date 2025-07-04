class_name playercontroller extends CharacterBody3D

@onready var ui = $"../UI"
@onready var textedit = $"../UI/TextEdit"

@export var SPEED = 5.0
@export var JUMP_VELOCITY = 4.5

@export var SPRINTMULTIPLIER = 2.

@export var swim_up_speed = 1.5
@export var swimming_speed = 3.5
@export var hp = 20

@export var npc: npc

var state: PlayerState = PlayerState.IDLE
var current_hp = hp
var attacked = false

func _ready():
	add_to_group("player")


func _physics_process(delta: float) -> void:
	if state == PlayerState.DEAD:
		return
	if state != PlayerState.SWIMMING:
		if Input.is_action_just_pressed("sprint"):
			if state == PlayerState.IDLE:
				state = PlayerState.SPRINTING
			else:
				state = PlayerState.IDLE
		normal_movement(delta)
	else:
		swim_movement(delta)
	
	move_and_slide()
			
		
func _unhandled_input(event: InputEvent) -> void:
	if state == PlayerState.DEAD:
		return
	if event is InputEventMouseButton and event.pressed:
		if event.button_index == MOUSE_BUTTON_LEFT:
			attacked = true
			state = PlayerState.ATTACKING
			npc.attacked()
		elif event.button_index == MOUSE_BUTTON_RIGHT:
			attacked = true
			state = PlayerState.COUNTERING
		
func swim_movement(delta: float) -> void:
	if Input.is_action_pressed("ui_accept"):
		velocity.y = swim_up_speed
	elif Input.is_action_pressed("crouch"): 
		velocity.y = -swim_up_speed
		
	var input_dir := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	rotation.y -= deg_to_rad(input_dir.x)
	
	if direction:
		velocity.x = direction.x * swimming_speed
		velocity.z = direction.z * swimming_speed
	else:
		velocity.x = move_toward(velocity.x, 0, swimming_speed)
		velocity.z = move_toward(velocity.z, 0, swimming_speed)
	
func normal_movement(delta: float):
	# chatting does not move character
	if textedit.has_focus():
		return
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta

	# Handle jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var input_dir := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	var direction := (transform.basis * Vector3(0, 0, input_dir.y)).normalized()
	rotation.y -= deg_to_rad(input_dir.x)
	var multiplier = 1.
	if state == PlayerState.SPRINTING:
		multiplier = SPRINTMULTIPLIER
		
	if direction:
		velocity.x = direction.x * SPEED * multiplier
		velocity.z = direction.z * SPEED * multiplier
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED * multiplier)
		velocity.z = move_toward(velocity.z, 0, SPEED * multiplier)
	if velocity.length() < .2:
		state = PlayerState.IDLE
	
	
func enter_water():
	state = PlayerState.SWIMMING
	
func exit_water():
	state = PlayerState.IDLE
	
func enter_chatting():
	state = PlayerState.CHATTING
	ui.visible = true
	
func update_hp(change: int):
	current_hp += change
	print(current_hp)
	if current_hp <= 0:
		state = PlayerState.DEAD
		print("Player died.")
	ui.update_player_hp(current_hp)
	
func describe_state() -> String:
	var description = "Aktion: %s\n" % (PlayerState.keys()[state])
	description += "Geschwindigkeit: %f\n" % (velocity.length())
	description += "Der Spieler hat seine Waffe gezogen" if attacked else "Der Spieler hat seine Waffe noch nicht gezogen.\n"
	return description
	

enum PlayerState {
	IDLE,
	ATTACKING, 
	SWIMMING,
	SPRINTING,
	CHATTING,
	SSP_IDLE,
	SSP_SCHERE,
	SSP_STEIN,
	SSP_PAPIER,
	BLOCKING,
	COUNTERING,
	DEAD
}


func _on_schere_pressed() -> void:
	state = PlayerState.SSP_SCHERE
	npc.react_ssp("SCHERE")

func _on_stein_pressed() -> void:
	state = PlayerState.SSP_STEIN
	npc.react_ssp("STEIN")

func _on_papier_pressed() -> void:
	state = PlayerState.SSP_PAPIER
	npc.react_ssp("PAPIER")
	
