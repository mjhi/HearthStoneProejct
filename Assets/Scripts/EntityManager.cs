using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;

public class EntityManager : MonoBehaviour
{
	public static EntityManager Inst { get; private set; }
	void Awake() => Inst = this;

	[SerializeField] GameObject entityPrefab;
	[SerializeField] GameObject damagePrefab;
	[SerializeField] GameObject healPrefab;
	[SerializeField] List<Entity> myEntities;
	[SerializeField] List<Entity> otherEntities;
	[SerializeField] GameObject TargetPicker;
	[SerializeField] Entity myEmptyEntity;
	[SerializeField] Entity myBossEntity;
	[SerializeField] Entity otherBossEntity;
	[SerializeField] TMP_Text skillEventTMP; //스킬 사용시 뜨는 텍스트
	[SerializeField] GameObject eventPanel; // 스킬 사용시 활성화 비활성화 패널

	[SerializeField] GameObject ThreeSkillPanel; // 3스킬 사용시 활성화 비활성화 패널

	[SerializeField] public GameObject SkillInfoPanel; // 스킬 인포 패널
	[SerializeField] public TMP_Text SkillInfoTMP; // 스킬 인포 텍스트

	const int MAX_ENTITY_COUNT = 6;
	public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
	bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
	bool ExistTargetPickEntity => targetPickEntity != null;
	bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);
	int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);
	bool CanMouseInput => TurnManager.Inst.myTurn && !TurnManager.Inst.isLoading;

	float OneSkillsPercent = 0.3f;
	float TwoSkillsPercent = 0.2f;
	float ThreeSkillsPercent = 0.1f;

	public bool threeSkillBool = false;

	public Entity selectEntity;
	public Entity targetPickEntity;
	WaitForSeconds delay1 = new WaitForSeconds(1);
	WaitForSeconds delay2 = new WaitForSeconds(2);



	void Start()
	{
		TurnManager.OnTurnStarted += OnTurnStarted;
	}

	void OnDestroy()
	{
		TurnManager.OnTurnStarted -= OnTurnStarted;
	}

	void OnTurnStarted(bool myTurn)
	{
		var targetEntities = myTurn ? myEntities : otherEntities; // 누구 턴인지 체크하기
		StartCoroutine(SkillsCheck(myTurn, targetEntities));
		AttackableReset(myTurn);

		if (!myTurn)
			StartCoroutine(AICo());
	}
	/// <summary>
	/// 스킬사용 여부 체크하는 함수
	/// </summary>
	/// <param name="myTurn">자신의 턴인지 아닌지</param>
	/// /// <param name="targetEntities">타겟 엔티티들.</param>
	IEnumerator SkillsCheck(bool myTurn, List<Entity> targetEntities)
	{
		for (int i = 0; i < targetEntities.Count; i++)
		{
			//첫 번째 스킬 사용
			if (targetEntities[i].skill == 1 && GetPercentChance(OneSkillsPercent))
			{
				targetEntities[i].skillCount++;
				skillEventTMP.text = "리코 스킬 사용";
				eventPanel.SetActive(true);
				var enemyEntities = myTurn ? otherEntities : myEntities; // 누구 턴인지 체크하기
				OneSkill(enemyEntities);

				//스킬 사용 후
				yield return new WaitForSeconds(3);
				eventPanel.SetActive(false);
			}
			//두 번째 스킬 사용
			else if (targetEntities[i].skill == 2 && GetPercentChance(TwoSkillsPercent))
			{
				skillEventTMP.text = "라니엘 스킬 사용";
				targetEntities[i].skillCount++;
				if (targetEntities[i].skillCount <= 2)
				{
					eventPanel.SetActive(true);
					TwoSkill(myTurn);
				}
				//스킬 사용 후
				yield return new WaitForSeconds(3);
				eventPanel.SetActive(false);
			}
			//세 번째 스킬 사용
			else if (targetEntities[i].liveCount > 1 && targetEntities[i].skill == 3 && GetPercentChance(ThreeSkillsPercent))
			{
				skillEventTMP.text = "누모 스킬 사용";
				eventPanel.SetActive(true);
				targetEntities[i].skillCount++;
				StartCoroutine(ThreeSkill(myTurn, targetEntities[i]));
				//스킬 사용 후
				yield return new WaitForSeconds(3);
				eventPanel.SetActive(false);
			}

		}
	}
	void OneSkill(List<Entity> enemyEntities)
	{

		for (int i = 0; i < enemyEntities.Count; i++)
		{
			int randomDemage = Random.Range(1, 4);
			enemyEntities[i].Damaged(randomDemage);
			SpawnDamage(randomDemage, enemyEntities[i].transform);

		}
		AttackCallback(enemyEntities.ToArray());
	}
	void TwoSkill(bool myTurn)
	{
		Entity bossEntity = myTurn ? myBossEntity : otherBossEntity;

		int randomHeal = Random.Range(1, 4);
		bossEntity.Healing(randomHeal);
		SpawnHeal(randomHeal, bossEntity.transform);

	}
	IEnumerator ThreeSkill(bool myTurn, Entity myEntitie)
	{
		// List<Entity> enemyEntities = myTurn ? otherEntities : myEntitie;
		if (myTurn)
		{
			threeSkillBool = true;
			ThreeSkillPanel.SetActive(true);
			while (threeSkillBool)
			{
				yield return delay1;
			}
		}
		else
		{
			var defenders = new List<Entity>(myEntities);
			int rand = Random.Range(0, defenders.Count);
			targetPickEntity = defenders[rand];
		}
		myEntitie.isDie = true;
		targetPickEntity.isDie = true;
		AttackCallback(myEntitie, targetPickEntity);
		targetPickEntity = null;

	}

	public void ThreeSkillSelect(Entity entity)
	{
		bool existTarget = false;
		foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
		{
			Entity entityEnemy = hit.collider?.GetComponent<Entity>();
			if (entityEnemy != null && !entityEnemy.isMine && entityEnemy != otherBossEntity)
			{
				targetPickEntity = entityEnemy;
				existTarget = true;
				break;
			}
		}
		if (!existTarget)
			targetPickEntity = null;

	}
	public void ThreeSkillSelectDone()
	{
		ThreeSkillPanel.SetActive(false);
		threeSkillBool = false;



	}
	/// <summary>
	/// 스킬 확률 계산 함수
	/// </summary>
	/// <param name="percent"></param>
	/// <returns></returns>
	public bool GetPercentChance(float percent)
	{
		// UnityEngine.Random을 사용하여 랜덤 값을 생성합니다.
		// Random.value는 0.0 이상 1.0 미만의 난수를 생성합니다.
		float randomValue = UnityEngine.Random.value;

		// 20% 확률로 true를 리턴할 조건을 설정합니다.
		bool result = randomValue <= percent;
		string str = "퍼센트 루력 : " + randomValue.ToString();
		Debug.Log(str);
		return result;
	}
	void Update()
	{
		ShowTargetPicker(ExistTargetPickEntity);
	}

	IEnumerator AICo()
	{
		CardManager.Inst.TryPutCard(false);
		yield return delay1;

		// attackable이 true인 모든 otherEntites를 가져와 순서를 섞는다
		var attackers = new List<Entity>(otherEntities.FindAll(x => x.attackable == true));
		for (int i = 0; i < attackers.Count; i++)
		{
			int rand = Random.Range(i, attackers.Count);
			Entity temp = attackers[i];
			attackers[i] = attackers[rand];
			attackers[rand] = temp;
		}

		// 보스를 포함한 myEntities를 랜덤하게 시간차 공격한다
		foreach (var attacker in attackers)
		{
			var defenders = new List<Entity>(myEntities);
			defenders.Add(myBossEntity);
			int rand = Random.Range(0, defenders.Count);
			Attack(attacker, defenders[rand]);

			if (TurnManager.Inst.isLoading)
				yield break;

			yield return delay2;
		}
		TurnManager.Inst.EndTurn();
	}


	void EntityAlignment(bool isMine)
	{
		float targetY = isMine ? -4.35f : 4.15f;
		var targetEntities = isMine ? myEntities : otherEntities;

		for (int i = 0; i < targetEntities.Count; i++)
		{
			float targetX = (targetEntities.Count - 1) * -3.4f + i * 6.8f;

			var targetEntity = targetEntities[i];
			targetEntity.originPos = new Vector3(targetX, targetY, 0);
			targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
			targetEntity.GetComponent<Order>()?.SetOriginOrder(i);
		}
	}

	public void InsertMyEmptyEntity(float xPos)
	{
		if (IsFullMyEntities)
			return;

		if (!ExistMyEmptyEntity)
			myEntities.Add(myEmptyEntity);

		Vector3 emptyEntityPos = myEmptyEntity.transform.position;
		emptyEntityPos.x = xPos;
		myEmptyEntity.transform.position = emptyEntityPos;

		int _emptyEntityIndex = MyEmptyEntityIndex;
		myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x));
		if (MyEmptyEntityIndex != _emptyEntityIndex)
			EntityAlignment(true);
	}

	public void RemoveMyEmptyEntity()
	{
		if (!ExistMyEmptyEntity)
			return;

		myEntities.RemoveAt(MyEmptyEntityIndex);
		EntityAlignment(true);
	}

	public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos)
	{
		if (isMine)
		{
			if (IsFullMyEntities || !ExistMyEmptyEntity)
				return false;
		}
		else
		{
			if (IsFullOtherEntities)
				return false;
		}

		var entityObject = Instantiate(entityPrefab, spawnPos, Utils.QI);
		var entity = entityObject.GetComponent<Entity>();

		if (isMine)
			myEntities[MyEmptyEntityIndex] = entity;
		else
			otherEntities.Insert(Random.Range(0, otherEntities.Count), entity);

		entity.isMine = isMine;
		entity.Setup(item);
		EntityAlignment(isMine);


		List<Entity> entityw = new()
		{
			entity
		};
		StartCoroutine(SkillsCheck(isMine, entityw));
		return true;
	}

	public void EntityMouseDown(Entity entity)
	{
		if (!CanMouseInput)
			return;

		selectEntity = entity;
	}

	public void EntityMouseUp()
	{
		if (!CanMouseInput)
			return;

		// selectEntity, targetPickEntity 둘다 존재하면 공격한다. 바로 null, null로 만든다.
		if (selectEntity && targetPickEntity && selectEntity.attackable)
			Attack(selectEntity, targetPickEntity);

		selectEntity = null;
		targetPickEntity = null;
	}

	public void EntityMouseDrag()
	{
		if (!CanMouseInput || selectEntity == null)
			return;

		// other 타겟엔티티 찾기
		bool existTarget = false;
		foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
		{
			Entity entity = hit.collider?.GetComponent<Entity>();
			if (entity != null && !entity.isMine && selectEntity.attackable)
			{
				targetPickEntity = entity;
				existTarget = true;
				break;
			}
		}
		if (!existTarget)
			targetPickEntity = null;
	}

	void Attack(Entity attacker, Entity defender)
	{
		// _attacker가 _defender의 위치로 이동하다 원래 위치로 온다, 이때 order가 높다
		attacker.attackable = false;
		attacker.GetComponent<Order>().SetMostFrontOrder(true);

		Sequence sequence = DOTween.Sequence()
			.Append(attacker.transform.DOMove(defender.originPos, 0.4f)).SetEase(Ease.InSine)
			.AppendCallback(() =>
			{
				attacker.Damaged(defender.attack);
				defender.Damaged(attacker.attack);
				SpawnDamage(defender.attack, attacker.transform);
				SpawnDamage(attacker.attack, defender.transform);
			})
			.Append(attacker.transform.DOMove(attacker.originPos, 0.4f)).SetEase(Ease.OutSine)
			.OnComplete(() => AttackCallback(attacker, defender));
	}

	void AttackCallback(params Entity[] entities)
	{
		// 죽을 사람 골라서 죽음 처리
		entities[0].GetComponent<Order>().SetMostFrontOrder(false);

		foreach (var entity in entities)
		{
			if (!entity.isDie || entity.isBossOrEmpty)
				continue;

			if (entity.isMine)
				myEntities.Remove(entity);
			else
				otherEntities.Remove(entity);

			Sequence sequence = DOTween.Sequence()
				.Append(entity.transform.DOShakePosition(1.3f))
				.Append(entity.transform.DOScale(Vector3.zero, 0.3f)).SetEase(Ease.OutCirc)
				.OnComplete(() =>
				{
					EntityAlignment(entity.isMine);
					Destroy(entity.gameObject);
				});
		}
		StartCoroutine(CheckBossDie());
	}

	IEnumerator CheckBossDie()
	{
		yield return delay2;

		if (myBossEntity.isDie)
			StartCoroutine(GameManager.Inst.GameOver(false));

		if (otherBossEntity.isDie)
			StartCoroutine(GameManager.Inst.GameOver(true));
	}

	public void DamageBoss(bool isMine, int damage)
	{
		var targetBossEntity = isMine ? myBossEntity : otherBossEntity;
		targetBossEntity.Damaged(damage);
		StartCoroutine(CheckBossDie());
	}

	void ShowTargetPicker(bool isShow)
	{
		TargetPicker.SetActive(isShow);
		if (ExistTargetPickEntity)
			TargetPicker.transform.position = targetPickEntity.transform.position;
	}

	void SpawnDamage(int damage, Transform tr)
	{
		if (damage <= 0)
			return;

		var damageComponent = Instantiate(damagePrefab).GetComponent<Damage>();
		damageComponent.SetupTransform(tr);
		damageComponent.Damaged(damage);
	}

	void SpawnHeal(int Heal, Transform tr)
	{
		if (Heal <= 0)
			return;

		var damageComponent = Instantiate(healPrefab).GetComponent<Heal>();
		damageComponent.SetupTransform(tr);
		damageComponent.Damaged(Heal);
	}

	public void AttackableReset(bool isMine)
	{
		var targetEntites = isMine ? myEntities : otherEntities;
		targetEntites.ForEach(x => x.attackable = true);
	}

}
