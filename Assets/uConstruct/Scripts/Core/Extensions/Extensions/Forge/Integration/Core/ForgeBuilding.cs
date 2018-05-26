#if uConstruct_ForgeNetworking

using UnityEngine;
using System.Collections;

using uConstruct;

using BeardedManStudios.Network;
using BeardedManStudios.Forge;

namespace uConstruct.Extensions.ForgeExtension
{
    public class ForgeBuilding : BaseBuilding
    {
        public int networkedID = -1;
        public int networkedPlacedOnID
        {
            get
            {
                if (placedOn == null) return -1;

                ForgeBuilding placedOnNetworked = (ForgeBuilding)placedOn;
                return placedOnNetworked.networkedID;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            OnDestroyEvent += () => ForgeEntitiesManager.entities.Remove(this);
            OnHealthChangedEvent += OnHealthChanged;
        }

        protected virtual void OnHealthChanged()
        {
            var evnt = new UpdateNetworkedBuilding();
            evnt.buildingUID = networkedID;
            evnt.health = health;
            evnt.Send(NetworkReceivers.Server);
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
            this.transform.position = data.pos;
            this.transform.rotation = data.rot;

            CallLoad(this);

            this.networkedID = data.id;
            this.placedOn = ForgeEntitiesManager.GetEntity(data.placedOnID);

            if (placedOn != null)
                this.SnappedTo = placedOn.ReturnSocket(transform.position, this.buildingType);

            this.PlaceBuilding();

            if (data.requester != null)
            {
                ForgeBuildingPlacer.LocalNetworkedBuildingPlaced(data.requester);
            }

            this.health = data.health;
        }

        public CreateNetworkedBuilding PackData(SimpleNetworkedMonoBehavior requester, bool initiateEvent)
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

            evnt.requesterID = requester == null ? 99999 : requester.NetworkedId;

            return evnt;

        }
    }
}

#endif