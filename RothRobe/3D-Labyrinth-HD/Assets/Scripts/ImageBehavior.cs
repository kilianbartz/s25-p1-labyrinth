using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImageBehavior : MonoBehaviour
{
    private const string Imagepath = "Assets/Rendered_Images";
    private Transform _parentTransform;
    private Vector3 _basePosition;

    private Texture2D _loadedTexture;
    private bool _isViewing;
    
    private RawImage _imageUI;
    private PlayerController _playerController;
    
    private AudioSource _ambientAudioSource;  
    private AudioSource _foundAudioSource;    

    void Start()
    {
        _parentTransform = transform.parent;
        _basePosition = _parentTransform.position;
        var canvas = GameObject.Find("ImageCanvas");
        if (canvas != null)
        {
            _imageUI = canvas.GetComponentInChildren<RawImage>(true); // ← `true` bedeutet: auch inaktive
        }
        
        _playerController = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            _ambientAudioSource = sources[0];
            _foundAudioSource = sources[1];
        }
        
        LoadImage();
    }

    void Update()
    {
        if (!_isViewing)
        {
            _parentTransform.Rotate(0f, (50f * Time.deltaTime) % 360, 0f);
            float offsetY = Mathf.Sin(Time.time * 5) * 0.1f;
            _parentTransform.position = _basePosition + new Vector3(0, offsetY, 0);
        }
        else
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                HideImage();
            }
        }
    }

    void LoadImage()
    {
        if (!Directory.Exists(Imagepath))
        {
            Debug.LogError($"Ordner nicht gefunden: {Imagepath}");
            return;
        }

        string[] imagePaths = Directory
            .GetFiles(Imagepath)
            .Where(f => f.ToLower().EndsWith(".png")
                     || f.ToLower().EndsWith(".jpg")
                     || f.ToLower().EndsWith(".jpeg"))
            .ToArray();

        if (imagePaths.Length == 0)
        {
            Debug.LogWarning("Keine Bilddateien im Ordner gefunden.");
            return;
        }

        string selectedPath = imagePaths[Random.Range(0, imagePaths.Length)];
        _loadedTexture = LoadTextureFromPath(selectedPath);

        Renderer imageRenderer = GetComponent<Renderer>();
        imageRenderer.material.mainTexture = _loadedTexture;
    }

    Texture2D LoadTextureFromPath(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(fileData);
        return tex;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_isViewing)
        {
            ShowImage();
        }
    }

    void ShowImage()
    {
        if (_imageUI == null || _loadedTexture == null) return;

        _imageUI.texture = _loadedTexture;
        _imageUI.gameObject.SetActive(true);
        _isViewing = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (_playerController != null)
            _playerController.enabled = false;
        
        if (_ambientAudioSource != null)
            _ambientAudioSource.Stop();
        else
        {
            Debug.Log("AmbientAudio null");
        }
        
        if (_foundAudioSource != null && !_foundAudioSource.isPlaying)
            _foundAudioSource.Play();
        else
        {
            Debug.Log("FoundAudio null");
        }
    }

    void HideImage()
    {
        if (_imageUI == null) return;

        _imageUI.gameObject.SetActive(false);
        _isViewing = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (_playerController != null)
            _playerController.enabled = true;

        gameObject.SetActive(false); // Objekt verschwindet nach Ansicht
    }
}
