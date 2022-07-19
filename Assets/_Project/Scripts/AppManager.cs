using System;
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
        [HideInInspector] public List<AttributeObject> currentAttributeObjects = new List<AttributeObject>();

        
        #region UNITY_LIFECYCLE

        private void OnEnable()
        {
            mainPanel.UploadButtonPressed += UploadToIpfs;
            ERC721Panel.OnSubmittedAttribute += AddAttributeObject;
            AttributeItem.OnDeleted += DeleteAttributeObject;
        }

        private void OnDisable()
        {
            mainPanel.UploadButtonPressed -= UploadToIpfs;
            ERC721Panel.OnSubmittedAttribute -= AddAttributeObject;
            AttributeItem.OnDeleted -= DeleteAttributeObject;
        }

        #endregion

        
        #region PUBLIC_METHODS

        public void GoToMainState()
        {
            ChangeState("Main");
        }
        
        public void ViewAttributes()
        {
            ChangeState("Attributes");
        }
        
        public void GoToNextState()
        {
            Next();
        }
        
        public void BackToPreviousState()
        {
            Previous();
        }

        #endregion
        
        
        #region EVENT_HANDLERS
        
        private void AddAttributeObject(AttributeObject obj)
        {
            // We add an ID to the object while adding it to the list.
            obj.id = currentAttributeObjects.Count; //Important!!!!
            currentAttributeObjects.Add(obj);
        }

        private void DeleteAttributeObject(AttributeObject obj)
        {
            currentAttributeObjects.Remove(obj);
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
                mainPanel.ResetUploadButton();
                return;
            }
            
            Debug.Log("Image file saved successfully to IPFS:");
            Debug.Log(ipfsImagePath);

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
                mainPanel.ResetUploadButton();
                return;
            }
            
            Debug.Log("Metadata saved successfully to IPFS:");
            Debug.Log(ipfsMetadataPath);
            mainPanel.ResetUploadButton();
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

        private static object BuildMetadata(string name, string desc, string imageUrl)
        {
            object attribute = new
            {
                display_type = "boost_percentage",
                trait_type = "Movement",
                value = "60"
            };
            
            object attribute2 = new
            {
                display_type = "boost_number",
                trait_type = "Duration",
                value = "12"
            };
            
            object[] attributes = new object[2];

            attributes[0] = attribute;
            attributes[1] = attribute2;

            
            object obj = new
            {
                name = name,
                description = desc, 
                image = imageUrl, 
                attributes = attributes
            };

            return obj; 
        }

        #endregion
    }   
}
