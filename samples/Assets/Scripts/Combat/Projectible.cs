using System;
using UnityEngine;

public interface Projectible
{
    Action<Projectible> OnProjectileCollided { get; set; }

    void Initialise();

    void SetProjectile(Vector3 start, Vector3 direction);

    void UpdatePosition();

    void DestroyProjectile();

    bool HasFired();

    Vector3 GetPosition();

}
