class_name npc extends CharacterBody3D

@onready var _anim: AnimationPlayer = $npc/AnimationPlayer
@onready var blood = $blood
@onready var llmcon: llmcon = $llmcon
#@onready var llmcon: ollamacon = $ollamacon
@onready var ttscon: ttscon = $ttscon
@onready var textedit: TextEdit = $"../UI/TextEdit"
@onready var audioplayer: AudioStreamPlayer3D = $AudioStreamPlayer3D
@onready var screamplayer: AudioStreamPlayer3D = $ScreamSound
@onready var gaspplayer: AudioStreamPlayer3D = $GaspSound

@export var player: CharacterBody3D
@export var ui: my_ui
@export var starting_hp: int
@export var hut: Node3D
@export var pond: Node3D


var state: State = State.IDLE
var ssp_last_moves = []
var ssp_last_player_move = ""
var fighting_moves = []
var fighting_last_player_moves = []
var current_hp = 0
var round = 0
var round_timer = 0
var fighting = false
var next_polling_interval = 3
var polling_timer = 0
var exited_dialogue = false
var speed = 5
var flying_away = false

func get_player_distance():
	return global_position.distance_to(player.global_position)
	
func _ready():
	current_hp = starting_hp
	
	
func _physics_process(delta: float) -> void:
	if flying_away:
		var dir = (pond.global_position - global_position)
		var dist = dir.length()
		if dist > 2.:
			velocity = dir.normalized() * speed
			move_and_slide()
		else:
			flying_away = false
			state = State.IDLE
			
	if next_polling_interval > 0:
		polling_timer += delta
		if polling_timer >= next_polling_interval:
			llmcon.poll_behavior(describe_situation())
		
	# process fighting round every 1s
	if fighting:
		if round_timer < 1:
			round_timer += delta
		else:
			process_fighting_round()
			
func face(target: Vector3):
	var look_dir = target - global_position
	look_dir.y = 0
	transform = transform.looking_at(global_position - look_dir, Vector3.UP)
			
func flee():
	gaspplayer.play()
	face(player.global_position)
	next_polling_interval = 0
	state = State.SCARED
	var timer = Timer.new()
	add_child(timer)
	timer.wait_time = 2
	timer.one_shot = true
	timer.timeout.connect(start_flying_away)
	timer.start()
	
func start_flying_away():
	screamplayer.play()
	face(pond.global_position)
	flying_away = true
	
func attacked():
	print("player attacked")
	var dist = get_player_distance()
	if dist < 2.1:
		if not fighting:
			current_hp -= 2
			next_polling_interval = 0
			llmcon.trigger_based_action("Der Spieler schlägt dich plötzlich. Du hast noch %s / %s HP. Um dich zu wehren, wähle FIGHT_PLAYER." % [current_hp, starting_hp], true)
			_show_blood()
		
func process_fighting_round():
	round += 1
	print("Processing round %s" % round)
	round_timer = 0
	ui.show_round_label()
	if len(fighting_moves) < 2:
		# as last actions are already included and no further context is needed, start a new thread (forget past messages)
		llmcon.trigger_based_action("Der Spieler schlägt sich mit dir. Deine aktuellen HP: %s / %s Seine letzte Aktionen waren %s" % [current_hp, starting_hp, JSON.stringify(fighting_last_player_moves)], true)
	var action = fighting_moves.pop_front()
	print(action)
	print(player.PlayerState.keys()[player.state])
	match action:
		"ATTACK":
			state = State.ATTACKING
			if player.state == player.PlayerState.ATTACKING:
				current_hp -= 5
				player.update_hp(-5)
			elif player.state == player.PlayerState.COUNTERING:
				current_hp -= 10
			elif player.state == player.PlayerState.BLOCKING:
				player.update_hp(-2)
			else:
				player.update_hp(-5)
		"BLOCK":
			state = State.BLOCKING
			if player.state == player.PlayerState.COUNTERING:
				player.update_hp(-2)
			if player.state == player.PlayerState.ATTACKING:
				current_hp -= 2
		"COUNTER":
			state = State.COUNTERING
			if len(fighting_moves) == 0:
				if player.state == player.PlayerState.ATTACKING:
					player.update_hp(-10)
				else:
					current_hp -= 2
				if player.state == player.PlayerState.COUNTERING:
					player.update_hp(-2)
	print("player", player.current_hp)
	print("npc", current_hp)
	if player.current_hp <= 0:
		state = State.IDLE
		llmcon.trigger_based_action("Du hast gerade den Spieler totgeboxt. Was sagst du dazu?", true)
		fighting = false
	fighting_last_player_moves.append(player.PlayerState.keys()[player.state])
	if current_hp <= 0:
		die()
			
func die():
	state = State.DEAD
	_show_blood()
	
func _show_blood():
	blood.visible = true
	# remove blood after .5s
	var timer = Timer.new()
	add_child(timer)
	timer.wait_time = .5
	timer.one_shot = true
	timer.timeout.connect(_hide_blood)
	timer.start()
	
func _hide_blood():
	blood.visible = false
	
func exit_dialogue():
	if next_polling_interval == 0:
		next_polling_interval = 1
		polling_timer = 0
		exited_dialogue = true
		
func can_see_player() -> bool:
	var space_state = get_world_3d().direct_space_state
	var ray_params = PhysicsRayQueryParameters3D.new()
	
	ray_params.from = global_transform.origin
	ray_params.to = player.global_transform.origin
	ray_params.exclude = [self]
	
	var result = space_state.intersect_ray(ray_params)
	return (result and result.collider == player)
		

func change_state(new_state: State):
	state = new_state
	print("npc is now " + State.keys()[state])
	
func react_to_chat(message: String):
	var trigger = "Der Spieler sagt zu dir: " + message
	llmcon.trigger_based_action(trigger)
	
func react(json_response):
	polling_timer = 0
	if not json_response or not json_response["action"]:
		return
	match json_response['action']:
		"BE_SCARED":
			if state != State.SCARED:
				flee()
			next_polling_interval = 5
			state = State.SCARED
			# currently not handle as chatting as this causes the npc to switch to idle after tts finished
			#if "content" in json_response:
				#var content = json_response["content"]
				#ttscon.get_tts(content)
				#next_polling_interval = 0
		"BE_ANGRY":
			next_polling_interval = 1
			state = State.ANGRY
			# handle as chatting
			if "content" in json_response:
				var content = json_response["content"]
				ttscon.get_tts(content)
				next_polling_interval = 0
		"CHAT_WITH_PLAYER":
			flying_away = false
			face(player.global_position)
			state = State.IDLE
			player.enter_chatting()
			next_polling_interval = 0
			var content = json_response["content"]
			ttscon.get_tts(content)
		"SCHERE_STEIN_PAPIER":
			next_polling_interval = 0
			if ssp_last_player_move and ssp_last_player_move != "":
				ssp_last_moves.append(ssp_last_player_move)
			ssp_compute_win(json_response["content"])
		"FIGHT_PLAYER":
			fighting = true
			next_polling_interval = 0
			var content = json_response["content"]
			fighting_moves += content
			process_fighting_round()
		"IDLE":
			next_polling_interval = 1
			state = State.IDLE
	if next_polling_interval != 0:
		#update next_polling interval based on player's distance + velocity
		next_polling_interval = min(player.SPEED / player.velocity.length(), get_player_distance() / 3. ) * 3
		
func exit_chatting():
	state = State.IDLE
			
func describe_situation() -> String:
	var description = """Du stehst %m vor deiner Hütte am See. Der Spieler, ein blaues Alien, ist %m von dir entfernt. 
	Dein aktueller Zustand ist: %.\n""" % [(hut.global_position - global_position).length(), get_player_distance(), State.keys()[state]]
	if not can_see_player():
		description += "Du kannst den Spieler gerade nicht sehen"
		if get_player_distance() < 50:
			description += ", aber hören"
		description += ".\n"
		
	description += player.describe_state()
	if exited_dialogue:
		description += "\nDer Spieler möchte gerade nicht reden."
	return description
			
		

enum State {IDLE, SCARED, ANGRY, ATTACKING, DEAD, CHATTING, BLOCKING, COUNTERING}

enum Behavior {
	ATTACK, COWER, SHOUT, DIALOG
}
enum Trigger {
	CHARGE_AT, AIM_AT, DIALOG
}
func get_trigger(trigger: Trigger):
	match trigger:
		Trigger.CHARGE_AT:
			return "charges at you"
		Trigger.AIM_AT:
			return "aim his weapon at you"
		Trigger.DIALOG:
			return "says to you: "
			
func play_tts(audio: AudioStream):
	state = State.CHATTING
	audioplayer.stream = audio
	audioplayer.play()
	if audio:
		print("playing audio %s" % audio.get_length())
		var timer = Timer.new()
		add_child(timer)
		timer.wait_time = audio.get_length()
		timer.one_shot = true
		timer.timeout.connect(_on_tts_ended)
		timer.start()
		
		
func _on_tts_ended():
	state = State.IDLE
	
func react_ssp(move: String):
	var trigger = """Der Spieler möchte Schere, Stein, Papier mit dir spielen. 
	Dies sind die letzten Züge des Gegners, versuche möglichst optimal dagegen zu spielen und Regelmäßigkeiten auszunutzen: %s """ % JSON.stringify(ssp_last_moves)
	ssp_last_player_move = move
	# no need for further context as last moves are included
	llmcon.trigger_based_action(trigger, true)
	
func ssp_compute_win(npc_move: String):
	var player_move = ssp_last_player_move.to_lower()
	npc_move = npc_move.to_lower()
	
	ui.show_ssp()
	
	if npc_move not in ["schere", "stein", "papier"]:
		return
	
	var won = null
	var content = "Ich wähle %s. " % npc_move
	
	match npc_move:
		"stein":
			if player_move == "schere":
				won = false
			elif player_move == "papier":
				won = true
		"schere":
			if player_move == "stein":
				won = true
			elif player_move == "papier":
				won = false
		"papier":
			if player_move == "schere":
				won = true
			elif player_move == "stein":
				won = false
	if won != null:
		content += "Hm, nächstes Mal gewinne ich aber" if won else "Ha, ich hab gewonne"
		ui.update_ssp_label(won)
	else:
		content += "Unentschieden."
	
	# in either case, annouce at least npc choice
	ttscon.get_tts(content)


func _chat_sent() -> void:
	var text = textedit.text
	textedit.text = ""
	react_to_chat(text)
	
	
