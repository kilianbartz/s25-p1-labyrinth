from flask import Flask, request, jsonify
import requests

app = Flask(__name__)

LM_API_URL = "http://136.199.51.131:1234/v1/chat/completions"
HEADERS = {
    "Content-Type": "application/json",
    "Authorization": "Bearer lm-studio"
}
MODEL_NAME = "deepseek-coder-v2-lite-instruct"

def send_prompt(prompt):
    payload = {
        "model": MODEL_NAME,
        "messages": [
            {"role": "user", "content": prompt}
        ],
        "temperature": 0.7
    }

    try:
        response = requests.post(LM_API_URL, headers=HEADERS, json=payload)
        response.raise_for_status()
        result = response.json()
        return result["choices"][0]["message"]["content"].strip()

    except requests.exceptions.RequestException as e:
        print("\n Netzwerk- oder Serverfehler:", e)
        return None
    except KeyError:
        print("\n Antwort konnte nicht gelesen werden.")
        return None

@app.route("/npcstate", methods=["POST"])
def npc_state():
    data = request.get_json()
    prompt = data.get("prompt", "")
    print("Erhalten von Unity:", prompt)
    answer = send_prompt(prompt)
    return jsonify({"response": answer})

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5005)  # für externen Zugriff


