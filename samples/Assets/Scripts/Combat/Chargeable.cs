using UnityEngine;

public interface Chargeable
{
    void StartCharge(Vector3 start, Vector3 direction);
    void UpdateCharge(float chargeTimer, Vector3 position, Vector3 direction);
}
