using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HolemGoblin : MonoBehaviour
{
	[Header("Child enemy values")]
	[SerializeField] private GameObject _miniGoblinPrefab;
	[SerializeField] private int _countOfLittleEnemies;
	[Header("Current enemy values")]
	[SerializeField] private GameObject _dieParticles;
	[SerializeField] private float _goblinSpeed = 2f;
	public float Hp = 5;
	public float Damage = 2;
	public float AtackSpeed = 3;
	public float AttackRange = 2.5f;
	[Header("Components")]
	public Animator AnimatorController;
	public NavMeshAgent Agent;

	private float _distanceBetweenMiniGoblins = 1f;
	private float lastAttackTime = 0;
	private bool isDead = false;
	private void Start()
	{
		PreLoadFunctions();
	}

	private void PreLoadFunctions()
	{
		SceneManager.Instance.AddEnemie(gameObject);
		Agent.speed = _goblinSpeed;
		Agent.SetDestination(SceneManager.Instance.Player.transform.position);
	}

	private void Update()
	{
		if (isDead)
		{
			return;
		}

		if (Hp <= 0)
		{
			Die();
			Agent.isStopped = true;
			return;
		}

		CountDistanceToPlayer();
	}

	private void CountDistanceToPlayer()
	{
		var distance = Vector3.Distance(transform.position, SceneManager.Instance.Player.transform.position);

		if (IsPlayerInRange(distance))
		{
			StartHit();
		}
		else
		{
			ContinueFollow();
		}
		AnimatorController.SetFloat("Speed", Agent.velocity.magnitude);
	}

	private bool IsPlayerInRange(float distance)
	{
		return distance <= AttackRange;
	}

	private void StartHit()
	{
		Agent.isStopped = true;
		if (IsAttackEnd())
		{
			Hit();
		}
	}

	private bool IsAttackEnd()
	{
		return Time.time - lastAttackTime > AtackSpeed;
	}

	private void Hit()
	{
		lastAttackTime = Time.time;
		SceneManager.Instance.Player.Hp -= Damage;
		AnimatorController.SetTrigger("Attack");
	}

	private void ContinueFollow()
	{
		Agent.isStopped = false;
		Agent.SetDestination(SceneManager.Instance.Player.transform.position);
	}

	private void Die()
	{
		SceneManager.Instance.Player.ResetClosestEnemy();
		isDead = true;
		StartCoroutine(PlayDeadParticles());
		SpawnGoblinChild();
		StartCoroutine(RemoveBigGoblin());
	}

	private void SpawnGoblinChild()
	{
		Vector3 _spawnPos = SetNavMeshPosition();

		for (int i = 0; i < _countOfLittleEnemies; i++)
		{
			Instantiate(_miniGoblinPrefab, _spawnPos, Quaternion.identity);

			_spawnPos += new Vector3(_distanceBetweenMiniGoblins, 0, _distanceBetweenMiniGoblins);
		}
	}

	private IEnumerator PlayDeadParticles()
	{
		GameObject particles = Instantiate(_dieParticles, transform.position, Quaternion.identity);
		float _particleLifeTime = particles.GetComponent<ParticleSystem>().main.startLifetime.constant;
		yield return new WaitForSeconds(_particleLifeTime);
		Destroy(particles);
	}

	private IEnumerator RemoveBigGoblin()
	{
		yield return new WaitForEndOfFrame();
		SceneManager.Instance.RemoveEnemie(gameObject);
		Destroy(gameObject);
	}

	private Vector3 SetNavMeshPosition()
	{
		Vector3 _enemyPos = Vector3.zero;
		NavMeshHit hit;
		if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
		{
			_enemyPos = new Vector3(transform.position.x, hit.position.y, transform.position.z);
		}
		return _enemyPos;
	}
}