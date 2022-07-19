using System;
using TMPro;
using UnityEngine;

namespace IPFS_Uploader
{
    public class AttributeItem : MonoBehaviour
    {
        public static Action<AttributeObject> OnDeleted;

        [HideInInspector] public int id;
        public TextMeshProUGUI display_type;
        public TextMeshProUGUI trait_type;
        public TextMeshProUGUI value;

        private AttributeObject _obj;

        public void Init(AttributeObject obj)
        {
            _obj = obj;
            
            id = _obj.id;
            display_type.text = _obj.display_type;
            trait_type.text = _obj.trait_type;
            value.text = _obj.value;
        }
    
        public void DeleteMyself()
        {
            OnDeleted?.Invoke(_obj);
            Destroy(gameObject);
        }
    }   
}
