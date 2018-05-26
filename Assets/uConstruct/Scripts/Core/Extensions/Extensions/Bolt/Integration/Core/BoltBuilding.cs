#if uConstruct_PhotonBolt

using UnityEngine;
using System.Collections;

using uConstruct;

namespace uConstruct.Extensions.BoltExtension
{
    public class BoltBuilding : BaseBuilding
    {
        public int networkedID = -1;
        public int networkedPlacedOnID
        {
            get
            {
                if (placedOn == null) return -1;

                BoltBuilding placedOnNetworked = (BoltBuilding)placedOn;
                return placedOnNetworked.networkedID;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            OnDestroyEvent += () => BoltEntitiesManager.entities.Remove(this);
            OnHealthChangedEvent += OnHealthChanged;
        }

        protected virtual void OnHealthChanged()
        {
            var evnt = UpdateNetworkedBuilding.Create(Bolt.GlobalTargets.OnlyServer, Bolt.ReliabilityModes.Unreliable);
            evnt.buildingUID = networkedID;
            evnt.health = health;
            evnt.Send();
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
            this.placedOn = BoltEntitiesManager.GetEntity(data.placedOnID);

            if (placedOn != null)
                this.SnappedTo = placedOn.ReturnSocket(transform.position, this.buildingType);

            this.PlaceBuilding();

            BoltBuildingPlacer.LocalNetworkedBuildingPlaced(data.requester);

            this.health = data.health;
        }

        public CreateNetworkedBuilding PackData(BoltEntity requester, bool initiateEvent)
        {
            if(initiateEvent)
                CallPack(this);

            var evnt = CreateNetworkedBuilding.Create(Bolt.GlobalTargets.Others, Bolt.ReliabilityModes.ReliableOrdered);
            evnt.pos = transform.position;
            evnt.rot = transform.rotation;

            evnt.id = networkedID;
            evnt.placedOnID = networkedPlacedOnID;
            evnt.prefabID = this.prefabID;

            evnt.health = this.health;

            evnt.requester = requester;

            return evnt;
        }
    }
}

#endif