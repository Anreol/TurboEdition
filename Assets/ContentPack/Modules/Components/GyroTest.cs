//Attach this script to a GameObject in your Scene.
using UnityEngine;
using UnityEngine.UI;

namespace TurboEdition.Components
{
    public class GyroTest : MonoBehaviour
    {
        Gyroscope m_Gyro;

        void Start()
        {
            //Set up and enable the gyroscope (check your device has one)
            m_Gyro = Input.gyro;
            m_Gyro.enabled = true;
        }

        //This is a legacy function, check out the UI section for other ways to create your UI
        void OnGUI()
        {
            //Output the rotation rate, attitude and the enabled state of the gyroscope as a Label
            GUI.Label(new Rect(500, 400, 200, 40), "Gyro rotation rate " + m_Gyro.rotationRate);
            GUI.Label(new Rect(500, 450, 200, 40), "Gyro attitude" + m_Gyro.attitude);
            GUI.Label(new Rect(500, 500, 200, 40), "input.gyro.attitude: " + Input.gyro.attitude);
            GUI.Label(new Rect(500, 550, 200, 40), "Gyro enabled : " + m_Gyro.enabled);
            GUI.Label(new Rect(500, 600, 200, 40), "Input Gyro enabled : " + Input.gyro.enabled);
        }
    }
}