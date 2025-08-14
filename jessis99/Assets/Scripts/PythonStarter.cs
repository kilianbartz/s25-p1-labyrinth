using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class PythonStarter : MonoBehaviour
{
    public InputField userInputField;
    public TMP_Text answerText; // Verknuepfe im Inspector!
    private string output = "";
    public ScrollRect scrollRect;

    void Update (){
        if (userInputField.isFocused){
            movement.aktive = false;
        }else{
            movement.aktive = true;
        }
    }

    public void RunScriptWithInput()
    {
        string inputText = userInputField.text;
        output = output + "Du: " + inputText + "\n";
        userInputField.text = "";
        string pythonPath = "/home/jessi/Dokumente/Uni/Master/3.Semester/Spieleprogrammierung/venv/bin/python3";
        string scriptPath = "/home/jessi/Dokumente/Uni/Master/3.Semester/Spieleprogrammierung/s25-p1-labyrinth/jessis99/Assets/Scripts/request.py";

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = pythonPath;
        startInfo.Arguments = $"\"{scriptPath}\" \"{inputText}\"";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;

        Process process = new Process();
        process.StartInfo = startInfo;

        process.OutputDataReceived += (sender, args) => {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output += args.Data + "\n";
            }
        };

        process.ErrorDataReceived += (sender, args) => {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output += "Fehler: " + args.Data + "\n";
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit(); // Wichtig: auf Ergebnis warten!

        answerText.text = output.Trim(); // Ausgabe auf Bildschirm

        // Nach unten scrollen
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}//test
