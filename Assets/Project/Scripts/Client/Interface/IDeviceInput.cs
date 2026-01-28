using UnityEngine;

namespace GameClient
{
    public interface IDeviceInput
    {
        public float GetHorizontal();
        public float GetVertical();
    }

    public class DesktopDeviceInput : IDeviceInput
    {
        public float GetHorizontal()
        {
           return Input.GetAxisRaw("Horizontal");
        }

        public float GetVertical()
        {
            return Input.GetAxisRaw("Vertical");
        }
    }
}
