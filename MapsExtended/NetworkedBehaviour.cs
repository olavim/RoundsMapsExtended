using Photon.Pun;
using UnboundLib;

namespace MapsExtended
{
    public class NetworkedBehaviour : MonoBehaviourPunCallbacks
    {
        private const string RequestViewID = "MapsExtended:NetworkedBehaviour:RequestViewID";
        private const string AllocateViewID = "MapsExtended:NetworkedBehaviour:AllocateViewID";

        /// <summary>
        ///     A NetworkID should be unique between a single client's NetworkedBehaviours. If two clients have a
        ///     NetworkedBehaviour with the same NetworkID, they will be allocated the same Photon View ID.
        /// </summary>
        public string NetworkID { get; set; }

        public void Start()
        {
            if (this.gameObject.GetComponent<PhotonView>() == null)
            {
                this.gameObject.AddComponent<PhotonView>();
            }
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RegisterEvent(NetworkedBehaviour.RequestViewID, this.HandleViewIDRequest);
                PhotonNetwork.AllocateViewID(this.photonView);
            }
            else
            {
                NetworkingManager.RegisterEvent(NetworkedBehaviour.AllocateViewID, this.HandleViewIDAllocation);
                NetworkingManager.RaiseEvent(NetworkedBehaviour.RequestViewID, this.NetworkID);
            }
        }

        private void HandleViewIDRequest(object[] args)
        {
            string requesterInstanceID = (string) args[0];

            if (this.NetworkID == requesterInstanceID)
            {
                NetworkingManager.RaiseEvent(NetworkedBehaviour.AllocateViewID, this.NetworkID, this.photonView.ViewID);
            }
        }

        private void HandleViewIDAllocation(object[] args)
        {
            string recipientInstanceID = (string) args[0];

            if (this.NetworkID == recipientInstanceID)
            {
                int viewID = (int) args[1];
                this.photonView.ViewID = viewID;
            }
        }
    }
}
