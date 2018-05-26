#if uConstruct_PhotonCloud

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;

using uConstruct;
using uConstruct.Core;

namespace uConstruct.Extensions.PCloudExtension
{
    public class PhotonCloudBuilding : BaseBuilding
    {
        public int networkedID = -1;
        public int networkedPlacedOnID
        {
            get
            {
                if (placedOn == null) return -1;

                PhotonCloudBuilding placedOnNetworked = (PhotonCloudBuilding)placedOn;
                return placedOnNetworked.networkedID;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            OnDestroyEvent += () => PhotonCloudEntitiesManager.entities.Remove(this);
            OnHealthChangedEvent += OnHealthChanged;
        }

        protected virtual void OnHealthChanged()
        {
            var evnt = new UpdateNetworkedBuilding();
            evnt.buildingUID = networkedID;
            evnt.health = health;
            evnt.SendToServer();
        }

        public virtual void AssingNetworkedHealth(int health)
        {
            health = Mathf.Clamp(health, 0, maxHealth);

            base._health = health;

            if (health == 0)
                DestroyBuilding();
        }

        public void LoadData(CreateNetworkedBuilding data)
        {
            this.transform.position = (Vector3)data.pos;
            this.transform.rotation = (Quaternion)data.rot;

            CallLoad(this);

            this.networkedID = data.id;
            this.placedOn = PhotonCloudEntitiesManager.GetEntity(data.placedOnID);

            if (placedOn != null)
                this.SnappedTo = placedOn.ReturnSocket(transform.position, this.buildingType);

            this.PlaceBuilding();

            PhotonCloudBuildingPlacer.LocalNetworkedBuildingPlaced(data.requester);

            this.health = data.health;
        }

        public CreateNetworkedBuilding PackData(PhotonView requester, bool initiateEvent)
        {
            if(initiateEvent)
                CallPack(this);

            var evnt = new CreateNetworkedBuilding();
            evnt.pos = transform.position;
            evnt.rot = transform.rotation;

            evnt.id = networkedID;
            evnt.placedOnID = networkedPlacedOnID;
            evnt.prefabID = this.prefabID;

            evnt.health = this.health;

            evnt._requesterID = requester == null ? -1 : requester.viewID;

            return evnt;
        }
    }
}

#endif