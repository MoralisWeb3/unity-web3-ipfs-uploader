using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MoralisUnity;
using MoralisUnity.Web3Api.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Pixelplacement;
using TMPro;

namespace IPFS_Uploader
{
    public class AttributeObject
    {
        public int id;
        [CanBeNull] public string display_type;
        public string trait_type;
        public string value;
    }
    
    public class AppManager : StateMachine
    {
        [SerializeField] private MainPanel mainPanel;
        [SerializeField] private TextMeshProUGUI statusLabel;
        
        [HideInInspector] public List<AttributeObject> currentAttributeObjects = new List<AttributeObject>();

        
        #region UNITY_LIFECYCLE

        private void OnEnable()
        {
            mainPanel.UploadButtonPressed += UploadToIpfs;
            NewAttributePanel.OnSubmittedAttribute += AddAttributeObject;
            AttributeItem.OnDeleted += DeleteAttributeObject;
        }

        private void OnDisable()
        {
            mainPanel.UploadButtonPressed -= UploadToIpfs;
            NewAttributePanel.OnSubmittedAttribute -= AddAttributeObject;
            AttributeItem.OnDeleted -= DeleteAttributeObject;
        }

        #endregion

        
        #region PUBLIC_METHODS

        public void ToMainState()
        {
            ChangeState("Main");
        }
        
        public void ToViewAttributesState()
        {
            ChangeState("ViewAttributes");
        }
        
        public void ToNewAttributeState()
        {
            ChangeState("NewAttribute");
        }

        public void SetStatusLabelText(string newText)
        {
            statusLabel.text = newText;
            StartCoroutine(ClearStatusLabel());
        }

        #endregion
        
        
        #region EVENT_HANDLERS
        
        private void AddAttributeObject(AttributeObject obj)
        {
            // We add an ID to the object while adding it to the list.
            obj.id = currentAttributeObjects.Count; //Important!!!!
            currentAttributeObjects.Add(obj);
            
            SetStatusLabelText("Attribute added!");
        }

        private void DeleteAttributeObject(AttributeObject obj)
        {
            currentAttributeObjects.Remove(obj);
            SetStatusLabelText("Attribute deleted!");
        }

        #endregion
        

        #region PRIVATE_METHODS

        private async void UploadToIpfs(string imgName, string imgDesc, string imgPath, byte[] imgData)
        {
            // We are replacing any space for an empty
            string filteredName = Regex.Replace(imgName, @"\s", "");
            string ipfsImagePath = await SaveImageToIpfs(filteredName, imgData);

            if (string.IsNullOrEmpty(ipfsImagePath))
            {
                Debug.Log("Failed to save image to IPFS");
                SetStatusLabelText("Failed to save image to IPFS");
                
                mainPanel.EnableUploadButton();
                return;
            }
            
            Debug.Log("Image file saved successfully to IPFS:");
            Debug.Log(ipfsImagePath);
            
            SetStatusLabelText("Image file saved successfully to IPFS!");

            // Build Metadata
            object metadata = BuildMetadata(imgName, imgDesc, ipfsImagePath);
            string dateTime = DateTime.Now.Ticks.ToString();
            
            string metadataName = $"{filteredName}" + $"_{dateTime}" + ".json";

            // Store metadata to IPFS
            string json = JsonConvert.SerializeObject(metadata);
            string base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            string ipfsMetadataPath = await SaveToIpfs(metadataName, base64Data);

            if (ipfsMetadataPath == null)
            {
                Debug.Log("Failed to save metadata to IPFS");
                SetStatusLabelText("Failed to save metadata to IPFS");
                
                mainPanel.EnableUploadButton();
                return;
            }
            
            Debug.Log("Metadata saved successfully to IPFS:");
            Debug.Log(ipfsMetadataPath);
            
            SetStatusLabelText("Metadata saved successfully to IPFS!");
            mainPanel.EnableUploadButton();
        }
        
        
        private async UniTask<string> SaveToIpfs(string name, string data)
        {
            string pinPath = null;

            try
            {
                IpfsFileRequest request = new IpfsFileRequest()
                {
                    Path = name,
                    Content = data
                };

                List<IpfsFileRequest> requests = new List<IpfsFileRequest> {request};
                List<IpfsFile> resp = await Moralis.GetClient().Web3Api.Storage.UploadFolder(requests);

                IpfsFile ipfs = resp.FirstOrDefault<IpfsFile>();

                if (ipfs != null)
                {
                    pinPath = ipfs.Path;
                }
            }
            catch (Exception exp)
            {
                Debug.LogError($"IPFS Save failed: {exp.Message}");
            }

            return pinPath;
        }

        private async UniTask<string> SaveImageToIpfs(string name, byte[] imageData)
        {
            return await SaveToIpfs(name, Convert.ToBase64String(imageData));
        }

        private object BuildMetadata(string name, string desc, string imageUrl)
        {
            // V2 Magic culminates here! We create the attributes. We build metadataObj from main fields + attributes :)
            object[] attributes = new object[currentAttributeObjects.Count];

            for (int i = 0; i < attributes.Length; i++)
            {
                object newAttribute;
                
                if (string.IsNullOrEmpty(currentAttributeObjects[i].display_type)) // We don't want to add the field display_type if it's empty or null
                {
                    newAttribute = new
                    {
                        trait_type = currentAttributeObjects[i].trait_type,
                        value = currentAttributeObjects[i].value
                    };   
                }
                else
                {
                    newAttribute = new
                    {
                        display_type = currentAttributeObjects[i].display_type,
                        trait_type = currentAttributeObjects[i].trait_type,
                        value = currentAttributeObjects[i].value
                    };
                }

                attributes[i] = newAttribute;
            }
            
            object metadataObj = new
            {
                name = name,
                description = desc, 
                image = imageUrl, 
                attributes = attributes
            };

            return metadataObj; 
        }

        private IEnumerator ClearStatusLabel()
        {
            yield return new WaitForSeconds(3f);
            statusLabel.text = string.Empty;
        }

        #endregion
    }   
}
