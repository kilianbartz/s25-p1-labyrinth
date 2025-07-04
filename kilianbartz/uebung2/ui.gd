class_name my_ui extends Control

@onready var SSP: Control = $SSP
@onready var label: RichTextLabel = $SSP/Label
@onready var roundlabel: Label = $"../roundlabel"
@onready var showui_button: Button = $"../ShowUI"
@onready var hp_label: Label = $"../hp"

@export var npc: npc

var ssp_won = 0
var ssp_lost = 0

func _on_ssp_toggle_pressed() -> void:
	visible = true
	SSP.visible = not SSP.visible
	
func show_ssp():
	SSP.visible = true


func _on_hide_ui_pressed() -> void:
	visible = false
	showui_button.visible = true
	npc.exit_dialogue()

func update_ssp_label(won: bool):
	if won:
		ssp_won+=1
	else:
		ssp_lost+=1
	label.text = "gewonnen: %s / verloren: %s" % [ssp_won, ssp_lost]
	
func update_round_label(round: int):
	roundlabel.text = "Runde %s" % round
	
func update_player_hp(hp: int):
	hp_label.text = "HP: %s" % hp

func show_round_label():
	roundlabel.visible = true


func _on_button_2_pressed() -> void:
	visible = true
	showui_button.visible = false
