using Pixelplacement;
using UnityEngine;

namespace IPFS_Uploader
{
    public class AttributesPanel : State
    {
        public Transform contentT;

        public AppManager appManager;
        public AttributeItem attributeItemPrefab;

        private void OnEnable()
        {
            // We should check what are the difference but in this case we just delete all items everytime we enable this state/panel.
            foreach (Transform attribute in contentT)
            {
                Destroy(attribute.gameObject);
            }
            
            // Then we instantiate all the current attributes in ...
            foreach (var obj in appManager.currentAttributeObjects)
            {
                var newAttributeItem = Instantiate(attributeItemPrefab, contentT);
                newAttributeItem.Init(obj);
            }
        }
    }     
}
