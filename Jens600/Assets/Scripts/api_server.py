from flask import Flask, request, jsonify
import requests

app = Flask(__name__)

LM_API_URL = "http://136.199.51.131:1234/v1/chat/completions"
HEADERS = {"Content-Type": "application/json"}
MODEL = "deepseek-coder-v2-lite-instruct"


@app.route("/chat", methods=["POST"])
def chat():
    try:
        # Nachricht aus Unity lesen
        user_input = request.json.get("message", "")
        if not user_input:
            return jsonify({"error": "Leere Nachricht empfangen."}), 400

        payload = {
            "model": "deepseek-coder-v2-lite-instruct",
            "temperature": 0.7,
            "messages": [
                {
                    "role": "system",
                    "content": (
                        "Der Spieler bewegt sich durch ein Labyrinth und kommt dir manchmal näher. " 
                        "Sofern der Spieler schneller als üblich auf dich zuläuft, kannst du Angst verspüren " 
                        "und deine Reaktionen entsprechend anpassen, allerdings kannst auch freundlich " 
                        "gegenüber dem Spieler reagieren, sofern du ihn nicht als Bedrohung wahrnimmst. " 
                        "Du kannst die selbe Aktion öfter nacheinander ausführen, aber es muss nicht sein. " 
                        "Deine Aufgabe ist es, realistisch zu reagieren. " 
                        "Im folgenden eine Liste mit Aktionen die dir zur Auswahl stehen mit den " 
                        "entsprechenden Strings, die du zurückgeben musst. Nichts tun und stehen: idle, " 
                        " winken: wave, weglaufen: run_away, sich ducken: crouch, rückwärts krabbeln: " 
                        "crawl_backwards, traurig herumstehen: sad, glücklich herumstehen: happy. " 
                        "Wichtig: Du darfst in allen Antworten, die du ab jetzt gibst nur die vorher " 
                        "beschriebenen Wörter verwenden und keine anderen Wörter oder Zeichen." 
                        "Merke dir diese Regeln die ganze Zeit und beziehe sie in alle zukünftigen Antworten ein. " 
                        "Als Beispiel: Spieler 3.4m entfernt, Geschwindigkeit: 0.0 m/s, deine Antwort darauf wäre: " 
                        "happy."
                    )
                },
                {"role": "user", "content": user_input}
            ]
        }

        print(f"📤 Sende an LM Studio: {user_input}")
        response = requests.post(LM_API_URL, headers=HEADERS, json=payload, timeout=15)
        data = response.json()
        print("✅ Antwort erhalten:", data)

        # Antwort robust extrahieren
        content = data.get("choices", [{}])[0].get("message", {}).get("content", "⚠️ Keine Antwort erhalten.")

        # Optional: simple Reaktionslogik
        if "idle" in content.lower():
            reaction = "idle"
        elif "wave" in content.lower():
            reaction = "wave"
        elif "run_away" in content.lower():
            reaction = "run_away"
        elif "crawl_backwards" in content.lower():
            reaction = "crawl_backwards"
        elif "crouch" in content.lower():
            reaction = "crouch"
        elif "sad" in content.lower():
            reaction = "sad"
        elif "happy" in content.lower():
            reaction = "happy"
        else:
            reaction = "idle"

        return jsonify({"reply": content, "reaction": reaction})

    except requests.exceptions.Timeout:
        print("❌ Timeout bei Anfrage an LM Studio.")
        return jsonify({"error": "Timeout beim Warten auf LM Studio"}), 504

    except Exception as e:
        print("❌ Fehler im Server:", e)
        return jsonify({"error": str(e)}), 500


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5050)
