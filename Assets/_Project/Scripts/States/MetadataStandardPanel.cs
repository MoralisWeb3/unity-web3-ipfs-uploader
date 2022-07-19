using Pixelplacement;

namespace IPFS_Uploader
{
    public class MetadataStandardPanel : State
    {
        public void GoToERC721State()
        {
            ChangeState("ERC721");
        }
    }   
}
