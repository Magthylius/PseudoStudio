//created by Jin
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShoot : MonoBehaviour
{
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bullet;
    [SerializeField] float fireRate;
    [SerializeField] float force;

    #region Unity Lifecycle
    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            GameObject projectile = Instantiate(bullet, firePoint.position, firePoint.rotation);
            projectile.GetComponent<Rigidbody>().AddForce(firePoint.forward * force);
        }
    }
    #endregion
}
