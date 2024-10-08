using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Entity : MonoBehaviour
{
    [SerializeField] Item item;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] GameObject sleepParticle;

    public int attack;
    public int health;
    public bool isMine;
    public bool isDie;
    public bool isBossOrEmpty;
    public bool attackable;
    public Vector3 originPos;
    public int liveCount;
    public int skill;

    public int skillCount = 0;


    void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    void OnTurnStarted(bool myTurn)//턴 시작 시.
    {
        if (isBossOrEmpty)
            return;

        if (isMine == myTurn)
            liveCount++;

        sleepParticle.SetActive(liveCount < 1);
    }

    public void Setup(Item item)
    {
        attack = item.attack;
        health = item.health;

        this.item = item;
        character.sprite = this.item.sprite;
        nameTMP.text = this.item.name;
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();
        skill = item.skills;
    }

    void OnMouseDown()
    {
        if (isMine)
            EntityManager.Inst.EntityMouseDown(this);
    }
    void OnMouseOver()
    {
        if (EntityManager.Inst.threeSkillBool)
        {
            EntityManager.Inst.ThreeSkillSelect(this);
        }
        if (skill > 0)
        {
            EntityManager.Inst.SkillInfoTMP.text = SkillsInfo.Inst.skillsInfo[skill].Replace("\\n", "\n"); // 이걸넣으면 \n이 줄바꿈이 된다.;
            EntityManager.Inst.SkillInfoPanel.SetActive(true);

        }
    }
    void OnMouseExit()
    {
        EntityManager.Inst.SkillInfoPanel.SetActive(false);
    }
    void OnMouseUp()
    {
        if (isMine)
            EntityManager.Inst.EntityMouseUp();
        else if (EntityManager.Inst.threeSkillBool)
        {
            EntityManager.Inst.ThreeSkillSelectDone();
        }
    }

    void OnMouseDrag()
    {
        if (isMine)
            EntityManager.Inst.EntityMouseDrag();
    }

    public bool Damaged(int damage)
    {
        health -= damage;
        healthTMP.text = health.ToString();

        if (health <= 0)
        {
            isDie = true;
            return true;
        }
        return false;
    }


    public bool Healing(int Heal)
    {
        health += Heal;
        healthTMP.text = health.ToString();
        return false;
    }
    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
            transform.DOMove(pos, dotweenTime);
        else
            transform.position = pos;
    }
}
