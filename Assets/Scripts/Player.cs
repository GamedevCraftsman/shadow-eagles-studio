using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	[Header("Values")]
	[SerializeField] private float _movementSpeed;
	[SerializeField] private float _rotationSpeed = 1000;
	[SerializeField] private float _timeToOnDoubleAttack = 2;
	[SerializeField] private int _doubleDamage = 2;
	public float Hp;
	public float Damage;
	public float AtackSpeed;
	public float AttackRange = 2;

	[Header("Other")]
	[SerializeField] private Button _doubleAttackbutton;
	public Animator AnimatorController;

	private GameObject _closestEnemy;
	private bool isDead = false;
	private bool _canStartTimer = false;
	private bool _allowDoubleHit = true;
	private bool _canMove = true;
	private bool _canAttack = true;
	private string _simpleAttackLabel = "SimpleAttack";
	private string _doubleAttackLabel = "DoubleAttack";
	private float _saveTimeToDoubleAttack;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		_saveTimeToDoubleAttack = _timeToOnDoubleAttack;
	}

	private void Update()
	{
		PlayerMovement();

		if (isDead)
		{
			return;
		}

		if (Hp <= 0)
		{
			Die();
			return;
		}

		CountDistanceToEnemy();
		SetDoubleAttackButtonState();
	}

	private void CountDistanceToEnemy()
	{
		var enemies = SceneManager.Instance.Enemies;

		for (int i = 0; i < enemies.Count; i++)
		{
			var enemie = enemies[i];
			if (enemie == null)
			{
				continue;
			}

			if (_closestEnemy == null)
			{
				_closestEnemy = enemie;
				continue;
			}

			var distance = Vector3.Distance(transform.position, enemie.transform.position);
			var closestDistance = Vector3.Distance(transform.position, _closestEnemy.transform.position);

			if (distance < closestDistance)
			{
				_closestEnemy = enemie;
			}
		}
	}

	private void SetDoubleAttackButtonState()
	{
		if (_closestEnemy != null)
		{
			float distanceToClose = Vector3.Distance(transform.position, _closestEnemy.transform.position);
			if (IsEnemyInAttackRange(distanceToClose) && _allowDoubleHit)
			{
				_doubleAttackbutton.interactable = true;
			}
			else
			{
				_doubleAttackbutton.interactable = false;
			}

			StartReloadTimer();
		}
	}

	private bool IsEnemyInAttackRange(float distanceToClose)
	{
		return distanceToClose <= AttackRange;
	}

	private void StartReloadTimer()
	{
		if (_canStartTimer)
		{
			DoubleAttackTimer();
		}
	}

	private void Die()
	{
		isDead = true;
		AnimatorController.SetTrigger("Die");
		_canAttack = false;
		SceneManager.Instance.GameOver();
	}

	private void PlayerMovement()
	{
		if (_canMove && !isDead)
		{
			float _xDir = Input.GetAxis("Horizontal");
			float _zDir = Input.GetAxis("Vertical");
			SetPlayerState(_xDir, _zDir);
		}
	}

	private void SetPlayerState(float xDir, float zDir)
	{
		if (IsPlayerMove(xDir, zDir))
		{
			Vector3 _movementDir = new Vector3(xDir, 0, zDir); // set movement direction.
			Move(_movementDir);
			RotateToRunDirection(_movementDir);
			AnimatorController.SetFloat("Speed", 1);
		}
		else
		{
			AnimatorController.SetFloat("Speed", 0);
		}
	}

	private bool IsPlayerMove(float xDir, float zDir)
	{
		return xDir != 0 || zDir != 0;
	}

	private void Move(Vector3 movementDir)
	{
		movementDir.Normalize();
		transform.Translate(movementDir * _movementSpeed * Time.deltaTime, Space.World);
	}

	private void RotateToRunDirection(Vector3 movementDir)
	{
		Quaternion _toRotation = Quaternion.LookRotation(movementDir, Vector3.up);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, _toRotation, _rotationSpeed * Time.deltaTime);
	}

	public void Attack(string _attackName)
	{
		if (_canAttack && !isDead && !IsPlayerWin())
		{
			var _distance = Vector3.Distance(transform.position, _closestEnemy.transform.position);
			if (_attackName == _doubleAttackLabel)
			{
				MakeAttack(_doubleAttackLabel, _closestEnemy, _distance, _doubleDamage);
			}
			if (_attackName == _simpleAttackLabel)
			{
				MakeAttack(_simpleAttackLabel, _closestEnemy, _distance, 1);
			}
		}
	}

	private bool IsPlayerWin()
	{
		return SceneManager.Instance.Win.activeSelf;
	}

	private void MakeAttack(string attackStyle, GameObject closestEnemie, float distance, int damageMultiplier)
	{
		if (_canAttack && !isDead)
		{
			StartAttack(attackStyle, distance);
		}
	}

	private void StartAttack(string attackStyle, float distance)
	{
		RuntimeAnimatorController _animController = AnimatorController.runtimeAnimatorController;
		float _animationDuration = 0;
		_canMove = false;

		DecreaseEnemyHP(distance, _closestEnemy, _doubleDamage);
		DoAttackAnimation(attackStyle, _animController, _animationDuration);
	}

	private void DecreaseEnemyHP(float distance, GameObject closestEnemy, int damageMultiplier)
	{
		if (IsEnemyNearPlayer(distance))
		{
			CheckEnemyType(damageMultiplier, closestEnemy);
		}
	}

	private bool IsEnemyNearPlayer(float distance)
	{
		return distance <= AttackRange;
	}

	private void CheckEnemyType(int damageMultiplier, GameObject closestEnemy)
	{
		if (closestEnemy.GetComponent<Enemie>() != null)
		{
			closestEnemy.GetComponent<Enemie>().Hp -= Damage * damageMultiplier;
		}
		else if (closestEnemy.GetComponent<HolemGoblin>() != null)
		{
			closestEnemy.GetComponent<HolemGoblin>().Hp -= Damage * damageMultiplier;
		}
	}

	private void DoAttackAnimation(string attackStyle, RuntimeAnimatorController animController, float animationDuration)
	{
		transform.rotation = Quaternion.LookRotation(_closestEnemy.transform.position - transform.position);
		if (attackStyle == _doubleAttackLabel)
		{
			StartCoroutine(SetDoubleAttack(attackStyle, animController, animationDuration, _timeToOnDoubleAttack));
		}
		else if (attackStyle == _simpleAttackLabel)
		{
			StartCoroutine(SetSimpleAttack(attackStyle, animController, animationDuration));
		}
	}

	private IEnumerator SetDoubleAttack(string attackStyle, RuntimeAnimatorController animController, float animationDuration, float reloadTime)
	{
		foreach (AnimationClip clip in animController.animationClips)
		{
			if (clip.name == "sword double attack")
			{
				AnimatorController.SetTrigger("DoubleAttack");
				animationDuration = clip.length;

				_allowDoubleHit = false;
				_doubleAttackbutton.interactable = false;
				_canStartTimer = true;
				_canAttack = false;

				yield return new WaitForSeconds(animationDuration);
				_canMove = true;
				_canAttack = true;

				yield return new WaitForSeconds(reloadTime - animationDuration);
				_allowDoubleHit = true;
			}
		}

	}

	private IEnumerator SetSimpleAttack(string attackStyle, RuntimeAnimatorController animController, float animationDuration)
	{
		foreach (AnimationClip clip in animController.animationClips)
		{
			if (clip.name == "sword attack")
			{
				AnimatorController.SetTrigger("Attack");
				animationDuration = clip.length;

				_canAttack = false;

				yield return new WaitForSeconds(animationDuration);
				_canMove = true;
				_canAttack = true;
			}
		}

	}

	private void DoubleAttackTimer()
	{
		Text _buttonText = _doubleAttackbutton.GetComponentInChildren<Text>();
		_timeToOnDoubleAttack -= Time.deltaTime;
		_buttonText.text = Mathf.RoundToInt(_timeToOnDoubleAttack).ToString();

		CheckTimerEnd(_buttonText);
	}

	private void CheckTimerEnd(Text buttonText)
	{
		if (isTimerEnd())
		{
			_canStartTimer = false;
			_timeToOnDoubleAttack = _saveTimeToDoubleAttack;
			buttonText.text = "Double\nattack";
		}
	}

	private bool isTimerEnd()
	{
		return _timeToOnDoubleAttack <= 0;
	}

	public void ResetClosestEnemy()
	{
		_closestEnemy = null;
	}
}
