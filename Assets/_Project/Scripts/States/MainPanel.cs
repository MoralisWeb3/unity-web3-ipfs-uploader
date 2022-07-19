using System;
using System.IO;
using Pixelplacement;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IPFS_Uploader
{
    public class MainPanel : State
    {
        public Action<string, string, string, byte[]> UploadButtonPressed;

        public AppManager appManager;

        [Header("Metadata Fields")]
        [SerializeField] private Image image;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField descriptionInput;
        
        [Header("Other UI Elements")]
        [SerializeField] private Button uploadButton;

        //Control vars
        private string _imagePath;
        private byte[] _imageData;
        private bool _isImageLoaded;


        private void OnEnable()
        {
            ReloadUploadButton();
        }

        public async void SelectImage()
        {
            _imagePath = EditorUtility.OpenFilePanel("Select a PNG", "", "png");

            if (_imagePath.Length == 0)
            {
                Debug.Log("Image not selected");
                appManager.SetStatusLabelText("Image not selected");
                _isImageLoaded = false;
                return;
            }
            
            Texture2D texture = new Texture2D(0, 0);
            _imageData = await File.ReadAllBytesAsync(_imagePath);

            if (_imageData == null)
            {
                Debug.Log("Failed to read image");
                appManager.SetStatusLabelText("Failed to read image");
                _isImageLoaded = false;
                return;   
            }
                
            var isLoaded = texture.LoadImage(_imageData);

            if (!isLoaded)
            {
                Debug.Log("Image not loaded");
                appManager.SetStatusLabelText("Image not loaded");
                _isImageLoaded = false;
                return;
            }
                
            Debug.Log("Image loaded successfully!");
            appManager.SetStatusLabelText("Image loaded successfully!");
            
            image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            image.preserveAspect = true;
            
            _isImageLoaded = true;
            ReloadUploadButton();
        }

        public void OnUploadButtonPressed()
        {
            if (image.sprite == null || nameInput.text == string.Empty || descriptionInput.text == string.Empty)
            {
                Debug.Log("All fields (image, name and description) need to be filled");
                return;
            }
            
            UploadButtonPressed?.Invoke(nameInput.text, descriptionInput.text, _imagePath, _imageData);
            uploadButton.interactable = false;
        }

        public void EnableUploadButton()
        {
            uploadButton.interactable = true;
        }

        public void ReloadUploadButton() // TODO find better naming for this function
        {
            // With these conditions, all fields are mandatory. You can change them however you desire.
            if (_isImageLoaded == false || nameInput.text == string.Empty || descriptionInput.text == string.Empty)
            {
                uploadButton.interactable = false;
            }
            else
            {
                uploadButton.interactable = true;
            }
        }
    }   
}
