#if uConstruct_PhotonBolt

using UnityEngine;
using System.Collections;

namespace uConstruct.Extensions.BoltExtension
{
    public class Player
    {
        public BoltEntity entity;
        public BoltConnection cn;

        public Player()
        {
            entity = null;
            cn = null;
        }

        public Player(BoltConnection cn)
        {
            entity = BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector3(20, 0, 20), Quaternion.identity);

            this.cn = cn;

            if (cn == null)
            {
                entity.TakeControl();
            }
            else
            {
                entity.AssignControl(cn);
            }

        }
    }
}

#endif
