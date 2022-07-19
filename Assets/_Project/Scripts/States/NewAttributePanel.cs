using System;
using Pixelplacement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IPFS_Uploader
{
    public class NewAttributePanel : State
    {
        public static Action<AttributeObject> OnSubmittedAttribute;
        
        [SerializeField] private Button submitButton;
            
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField displayType;
        [SerializeField] private TMP_InputField traitType;
        [SerializeField] private TMP_InputField value;

        private void OnEnable()
        {
            // If any of this values is empty, we set the submit button to not interactable
            if (traitType.text == string.Empty || value.text == string.Empty)
            {
                submitButton.interactable = false;   
            }
        }

        public void SubmitButtonPressed()
        {
            // We create a new AttributeObject and set the values. We don't add the id here, we will do that on the Manager.
            AttributeObject newAttributeObj = new AttributeObject
            {
                display_type = displayType.text,
                trait_type = traitType.text,
                value = value.text
            };

            OnSubmittedAttribute?.Invoke(newAttributeObj); // The Manager listens to this event
            
            ClearFields();
            Previous();
        }

        public void InputFieldHandler()
        {
            if (traitType.text == string.Empty || value.text == string.Empty)
            {
                submitButton.interactable = false;   
            }
            else
            {
                submitButton.interactable = true;
            }
        }

        private void ClearFields()
        {
            displayType.text = string.Empty;
            traitType.text = string.Empty;
            value.text = string.Empty;
        }
    }   
}
