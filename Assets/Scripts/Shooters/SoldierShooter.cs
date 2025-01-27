using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierShooter : Shooter
{
    // Transform where projectile will spawn
    public Transform m_FirepointTransform;

    // Transform that bullet spawn location rotates around
    public Transform m_FirepointPivot;
    public float m_CircleSize;

    private Vector3 m_SelfToMouseVector;
    private float m_SelfToMouseAngle;

    public void Update()
    {
        RotateBulletSpawn();
    }

    // Keeps bullet spawn location between player character and mouse cursor
    public void RotateBulletSpawn()
    {
        // Turns screen pixels into world coordinates
        m_SelfToMouseVector = Input.mousePosition;
        m_SelfToMouseVector.z = m_FirepointPivot.transform.position.z - Camera.main.transform.position.z;
        m_SelfToMouseVector = Camera.main.ScreenToWorldPoint(m_SelfToMouseVector);

        // Makes sure spawn location stays in line with mouse
        m_SelfToMouseVector = m_SelfToMouseVector - m_FirepointPivot.transform.position;

        // Complicated trigonometry
        m_SelfToMouseAngle = Mathf.Atan2(m_SelfToMouseVector.y, m_SelfToMouseVector.x) * Mathf.Rad2Deg;

        if (m_SelfToMouseAngle < 0.0f)
        {
            m_SelfToMouseAngle += 360.0f;
        }

        // Rotates bullet spawn relative to the object it's attached to
        m_FirepointTransform.transform.localEulerAngles = new Vector3(0, 0, m_SelfToMouseAngle);

        // Bullet spawn will always be CircleSize distance away from the bullet spawn pivot
        float xPos = Mathf.Cos(Mathf.Deg2Rad * m_SelfToMouseAngle) * m_CircleSize;
        float yPos = Mathf.Sin(Mathf.Deg2Rad * m_SelfToMouseAngle) * m_CircleSize;
        m_FirepointTransform.localPosition = new Vector3(xPos, yPos, 0);
    }

    // Creates a new shell object, saves the damage it'll do and which object fired it, applies a force to the rigidbody to make it fly
    // and destroys it after a set period of time
    public override void Shoot(GameObject shellPrefab, float fireForce, float damageDone, float lifeSpan)
    {
        GameObject newShell = Instantiate(shellPrefab, m_FirepointTransform.position, m_FirepointTransform.rotation);
        DamageOnHit doh = newShell.GetComponent<DamageOnHit>();

        // If damage on hit component exists, set its damage and owner to the data saved in the pawn
        if (doh != null)
        {
            doh.m_DamageDone = damageDone;
            doh.m_Owner = GetComponent<Pawn>();
        }

        Rigidbody2D rb = newShell.GetComponent<Rigidbody2D>();

        // If rigidbody exists, apply force in that direction with that much power
        if (rb != null)
        {
            rb.AddForce(m_FirepointTransform.right * fireForce);
        }

        // Gives BounceOffWalls component bullet trajectory and force applied on being instantiated
        if (newShell.GetComponent<BounceOffWalls>() != null)
        {
            newShell.GetComponent<BounceOffWalls>().GetBulletData();
        }

        // If the object exists after 2nd variable in seconds, destroy object
        Destroy(newShell, lifeSpan);
    }
}