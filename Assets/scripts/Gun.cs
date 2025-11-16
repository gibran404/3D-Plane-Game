using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
	[Header("Projectile")]
	public GameObject bulletPrefab;
	public float bulletSpeed = 50f;
	public float bulletLifetime = 5f;
	[Tooltip("Transform under which spawned bullets will be parented. If null a fallback collector is created at runtime.")]
	public Transform bulletCollector;

	[Header("Fire")]
	public float fireRate = 10f;

	[Header("Pooling")]
	public bool usePooling = true;
	public int poolSize = 20;
	public bool expandPoolIfNeeded = true;

	private float nextFireTime;
	private Queue<GameObject> pool;

	private void Awake()
	{
		if (bulletPrefab == null)
		{
			Debug.LogError("Gun: bulletPrefab is not assigned.", this);
			return;
		}

		// Ensure we have a collector transform to keep spawned bullets organized in the hierarchy
		if (bulletCollector == null)
		{
			var go = new GameObject(name + "-BulletCollector");
			bulletCollector = go.transform;
		}

		if (usePooling)
		{
			pool = new Queue<GameObject>(poolSize);
			for (int i = 0; i < poolSize; i++) CreatePooledBullet();
		}
	}

	private GameObject CreatePooledBullet()
	{
		var b = Instantiate(bulletPrefab);
		if (bulletCollector != null) b.transform.SetParent(bulletCollector, true);
		b.SetActive(false);
		EnsureBulletComponents(b);
		pool.Enqueue(b);
		return b;
	}

	private void EnsureBulletComponents(GameObject b)
	{
		var rb = b.GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = b.AddComponent<Rigidbody>();
		}
		rb.useGravity = false;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

		var bb = b.GetComponent<BulletBehaviour>();
		if (bb == null)
		{
			bb = b.AddComponent<BulletBehaviour>();
		}
		bb.owner = this;
		bb.lifetime = bulletLifetime;
		bb.usePool = usePooling;
	}

	private void Update()
	{
        TryFire();
		// if (holdToFire)
		// {
		// 	if (Input.GetButton("Fire1")) TryFire();
		// }
		// else
		// {
		// 	if (Input.GetButtonDown("Fire1")) TryFire();
		// }
	}

	public void TryFire()
	{
		if (Time.time < nextFireTime) return;
		nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, fireRate);
		Fire();
	}

	public void Fire()
	{
		if (bulletPrefab == null) return;

		if (usePooling)
		{
			GameObject bullet = null;
			if (pool != null && pool.Count > 0)
			{
				bullet = pool.Dequeue();
			}
			else if (expandPoolIfNeeded)
			{
				bullet = CreatePooledBullet();
				// it's created inactive and enqueued, dequeue the new one
				if (pool != null && pool.Count > 0) bullet = pool.Dequeue();
			}

			if (bullet == null) return;
			SetupAndLaunch(bullet);
		}
		else
		{
			GameObject b;
			if (bulletCollector != null)
			{
				b = Instantiate(bulletPrefab, transform.position, transform.rotation, bulletCollector);
			}
			else
			{
				b = Instantiate(bulletPrefab, transform.position, transform.rotation);
			}
			EnsureBulletComponents(b);
			SetupAndLaunch(b);
		}
	}

	private void SetupAndLaunch(GameObject bullet)
	{
		bullet.transform.position = transform.position;
		bullet.transform.rotation = transform.rotation;
		bullet.SetActive(true);

		var rb = bullet.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.velocity = transform.forward * bulletSpeed;
		}
	}

	public void ReturnToPool(GameObject bullet)
	{
		if (bullet == null) return;

		var rb = bullet.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}

		if (usePooling && pool != null)
		{
			bullet.SetActive(false);
			if (bulletCollector != null) bullet.transform.SetParent(bulletCollector, true);
			pool.Enqueue(bullet);
		}
		else
		{
			Destroy(bullet);
		}
	}
}

public class BulletBehaviour : MonoBehaviour
{
	[HideInInspector] public Gun owner;
	[HideInInspector] public float lifetime = 5f;
	[HideInInspector] public bool usePool = true;

	private Coroutine lifeRoutine;

	private void OnEnable()
	{
		lifeRoutine = StartCoroutine(Life());
	}

	private void OnDisable()
	{
		if (lifeRoutine != null) StopCoroutine(lifeRoutine);
	}

	private IEnumerator Life()
	{
		yield return new WaitForSeconds(lifetime);
		if (usePool && owner != null)
		{
			owner.ReturnToPool(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}

