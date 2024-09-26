using UnityEngine;
using UnityEngine.AI;

public class Enemie : MonoBehaviour
{
	public float _playerAddHP = 1;
	public float Hp = 2;
	public float Damage = 1;
	public float AtackSpeed = 1;
	public float AttackRange = 2;

	public Animator AnimatorController;
	public NavMeshAgent Agent;

	private float lastAttackTime = 0;
	private bool isDead = false;

	private void Start()
	{
		SceneManager.Instance.AddEnemie(this.gameObject);
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

		var distance = Vector3.Distance(transform.position, SceneManager.Instance.Player.transform.position);

		if (distance <= AttackRange)
		{
			Agent.isStopped = true;
			if (Time.time - lastAttackTime > AtackSpeed)
			{
				lastAttackTime = Time.time;
				SceneManager.Instance.Player.Hp -= Damage;
				AnimatorController.SetTrigger("Attack");
			}
		}
		else
		{
			Agent.isStopped = false;
			Agent.SetDestination(SceneManager.Instance.Player.transform.position);	
		}
		AnimatorController.SetFloat("Speed", Agent.velocity.magnitude);
		Debug.Log(Agent.speed);
	}



	private void Die()
	{
		SceneManager.Instance.RemoveEnemie(this.gameObject);
		SceneManager.Instance.Player.Hp += _playerAddHP;
		SceneManager.Instance.Player.ResetClosestEnemy();
		isDead = true;
		AnimatorController.SetTrigger("Die");
	}
}
