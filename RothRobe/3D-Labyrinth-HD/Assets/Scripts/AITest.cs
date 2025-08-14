using System.Linq;
using OpenAI;
using OpenAI.Models;
using OpenAI.Responses;
using UnityEngine;

public class AITest : MonoBehaviour 
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TestMethod();
    }

    async void TestMethod()
    {
        var api = new OpenAIClient("Hier API Key einfügen :)");
        var request = new CreateResponseRequest("Tell me a three sentence bedtime story about a unicorn.", Model.GPT4_1_Nano);
        var response = await api.ResponsesEndpoint.CreateModelResponseAsync(request);
        var responseItem = response.Output.LastOrDefault();
        if (responseItem != null)
            Debug.Log($"{responseItem.Id}, {responseItem.Status}, {responseItem.Type}: {responseItem}");
        response.PrintUsage();
    }
}
