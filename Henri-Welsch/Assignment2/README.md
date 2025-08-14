# 🤖 Exercise 2 – 3D NPC with AI-Controlled Behavior

**👤 Author:** Joé Welsch  
**📅 Semester:** Summer 2025  
**🎓 Course:** Spieleprogrammierung (Prof. Sturm & Tihomir Bicanic)  
**📁 Folder:** `Henri-Welsch/Assignment2`

---

## 🎮 Project Overview

This Unity project is my submission for **Exercise 2**. The goal was to create a **3D non-player character (NPC)** whose behavior is dynamically controlled by a language model (ChatGPT) via REST API integration.  

The NPC can:

- Stand idle
- Walk
- Run
- Crawl

Behavior changes are triggered by **natural language commands** entered by the player during runtime.

🎥 **[Gameplay Video](https://youtu.be/27rN1tsXll8)**

---

## 🎨 Character and Animations

I used **Mixamo** to download a humanoid character model with four animations:

- Idle
- Walk
- Run
- Crawl

These were organized in a Unity Animator Controller, defaulting to Idle. Animations blend automatically during transitions.

👀 **Animation Basics Tutorial:**  
[YouTube Playlist – Creating NPCs and Animation Controllers](https://www.youtube.com/watch?v=-FhvQDqmgmU&list=PLwyUzJb_FNeTQwyGujWRLqnfKpV-cj-eO)

---

## 🧭 Waypoint Movement

The NPC navigates between **waypoints** placed in the scene. Movement includes:

- Smooth interpolation between points
- Automatic rotation to face the next waypoint
- Configurable delays before advancing

Initially, a test script (`AnimationScript.cs`) cycled through all animations to validate blending. Later, the logic was extended (`NPCAnimationController.cs`) to allow dynamic control of speed and movement based on ChatGPT instructions.

🎓 **Waypoint Tutorial:**  
[YouTube – Moving Between Waypoints](https://www.youtube.com/watch?v=40oTxM6cSYw)

---

## 💬 Dialog System

Players interact via an **in-game dialog popup**, which:

- Opens on pressing **E**
- Focuses automatically
- Accepts free-form text input
- Submits commands with **Enter**
- Closes with **Escape**
- Pauses the game while active

Implementation is in `DialogManager.cs`.

🎓 **UI Tutorial:**  
[YouTube – In-Game UI Popups](https://www.youtube.com/watch?v=JivuXdrIHK0)

---

## 🧠 ChatGPT Integration

The project uses the **OpenAI Chat Completion API** to process commands. Prompts were designed to enforce a predictable response format (e.g., `ACTION: WALK`).  

If an unexpected reply is received, the NPC defaults to **Idle** for safety.  

🎓 **API Integration Guide:**  
[YouTube – OpenAI in Unity](https://www.youtube.com/watch?v=qgT-quk3JEo)

---

## ⚙️ Behavior Mapping

ChatGPT outputs map directly to animation and movement logic:

| Action       | Behavior                           |
|--------------|------------------------------------|
| `IDLE`       | Play idle animation; stop moving  |
| `WALK`       | Play walk animation; move slowly  |
| `RUN`        | Play run animation; move faster   |
| `CRAWL`      | Play crawl animation; stop moving |

---

## 📁 Project Structure

The project is organized as follows:

