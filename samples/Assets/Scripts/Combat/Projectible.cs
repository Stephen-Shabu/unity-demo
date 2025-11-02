using System;
using UnityEngine;

public interface Projectible
{
    event Action<WeaponData> OnCreated;
    event Action<Vector3, Vector3> OnFired;
    event Action<Projectible> OnCollided;

    void Initialise(WeaponData data);

    void SetProjectile(Vector3 start, Vector3 direction);

    void UpdatePosition();

    bool HasFired();

    Vector3 GetPosition();

    GameObject GetGameObject();
}
