class_name ollamacon extends HTTPRequest

var options = {}
var url = "http://localhost:11434/v1/chat/completions"
var model = "gemma3:latest"
var chat_messages = [] 
@onready var npc: npc = $".."
var save_message = false
var time: int

const available_actions = "IDLE, BE_SCARED, BE_ANGRY, CHAT_WITH_PLAYER, SCHERE_STEIN_PAPIER, FIGHT_PLAYER"

func make_message(description: String):
	return {"role": 'user', "content": """Du bist ein NPC in einem Videospiel und kommst aus Hessen. Du kannst eine der folgenden Aktionen auswählen: 
		%s. 
		%s.
		Wie reagierst du? Bitte antworte IMMER in folgendem JSON-Format: 
		{'action': <action>, 'content': <...>}.
		content DARF NUR bei CHAT_WITH_PLAYER, SCHERE_STEIN_PAPIER oder FIGHT_PLAYER angegeben werden, action darf NIE FEHLEN. 
		Bei SCHERE_STEIN_PAPIER darf content SCHERE, STEIN, oder PAPIER sein.
		Bei FIGHT_PLAYER sollte content eine Liste von 5 Aktionen sein, die nacheinander ausgeführt werden. Zur Verfügung stehen hier ATTACK, BLOCK und COUNTER.""" % [available_actions, description]}

func read_options():
	var file = FileAccess.open("user://.godot.env", FileAccess.READ)
	var content = file.get_as_text()
	file.close()
	return JSON.parse_string(content)

func _ready():
	options = read_options()
	
func dict_to_headers(dict: Dictionary) -> PackedStringArray:
	var headers = PackedStringArray()
	for key in dict:
		headers.append(str(key) + ": " + str(dict[key]))
	return headers
	
func poll_behavior(trigger: String):
	save_message = false
	request_completed.connect(_on_request_completed)
	var new_msg = make_message(trigger)
	var body = {"model": model, "messages": [new_msg]}
	request(url, dict_to_headers(options), HTTPClient.METHOD_POST, JSON.stringify(body))
	time = Time.get_ticks_msec()
	
func trigger_based_action(context: String, exclude_context: bool = false):
	save_message = true
	request_completed.connect(_on_request_completed)
	var new_msg = make_message(context)
	var messages = []
	if exclude_context:
		messages = [new_msg]
	else :
		chat_messages.append(new_msg)
		messages = chat_messages
	var body = {"model": model, "messages": messages}
	request(url, dict_to_headers(options), HTTPClient.METHOD_POST, JSON.stringify(body))
	time = Time.get_ticks_msec()
	
func _on_request_completed(result, response_code, headers, body):
	print("Request took %f ms" % (Time.get_ticks_msec() - time))
	var json = JSON.parse_string(body.get_string_from_utf8())
	print(json)
	var response: String = json["choices"][0]["message"]["content"]
	if save_message:
		chat_messages.append(json["choices"][0]["message"])
	# extract JSON format
	var start_index = response.find("{")
	var end_index = response.rfind("}")
	var action = response.substr(start_index, end_index - start_index + 1)
	
	npc.react(JSON.parse_string(action))
