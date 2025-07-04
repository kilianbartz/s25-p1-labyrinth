class_name ttscon extends HTTPRequest

@onready var npc: npc = $".."

func get_tts(message: String):
	print("Preparing tts for " + message)
	request_completed.connect(_on_tts_ready)
	var request_data = JSON.stringify({"text": message})
	request("http://localhost:8000/tts", PackedStringArray(),HTTPClient.METHOD_POST, request_data)
	
func _on_tts_ready(result, response_code, headers, body):
	var audio = AudioStreamWAV.load_from_buffer(body)
	
	npc.play_tts(audio)
