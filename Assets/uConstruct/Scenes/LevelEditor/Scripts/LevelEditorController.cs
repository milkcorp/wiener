using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using uConstruct.Core.Saving;

namespace uConstruct.Demo
{
    public class LevelEditorController : MonoBehaviour
    {
        public static bool canMove;

        public float normalspeed = 10;
        public float speed
        {
            get { return normalspeed * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1); }
        }

        public float sensitivity = 2;

        float yaw;
        float pitch;

        long lastCheckedSaveBytesLength;

        public virtual void Update()
        {
            if (canMove)
            {
                #region HandleMovement
                Vector3 movement = Vector3.zero;

                movement += (transform.forward * Input.GetAxis("Vertical") * speed);
                movement += (transform.right * Input.GetAxis("Horizontal") * speed);

                transform.position += movement * Time.deltaTime;
                #endregion

                #region Mouse
                if (Input.GetMouseButton(1))
                {
                    yaw += (Input.GetAxisRaw("Mouse X") * sensitivity);
                    yaw %= 360;

                    pitch += (-Input.GetAxisRaw("Mouse Y") * sensitivity);
                    pitch = Mathf.Clamp(pitch, -85, +85);

                    transform.eulerAngles = new Vector3(pitch, yaw, 0);
                }
                #endregion
            }
        }

        void Start()
        {
            UCSavingManager.OnLoadingProcessComplete += () => ReadNewLength();
        }

        void OnGUI()
        {
            GUILayout.Label("current level save bytes : " + lastCheckedSaveBytesLength.ToString());
        }

        public void ReadNewLength()
        {
            lastCheckedSaveBytesLength = uConstruct.Core.Saving.UCSavingManager.Serialize(new System.IO.MemoryStream()).Length;
        }

    }
}