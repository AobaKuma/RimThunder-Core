using UnityEngine;


namespace Motorization
{
    [System.Serializable]
    public class VehicleDrawData
    {
        public Vector3
            dataEast = Vector3.zero,
            dataWest = Vector3.zero,
            dataSouth = Vector3.zero,
            dataNorth = Vector3.zero,
            dataNorthEast = Vector3.zero,
            dataSouthEast = Vector3.zero,
            dataSouthWest = Vector3.zero,
            dataNorthWest = Vector3.zero;
    }
}
