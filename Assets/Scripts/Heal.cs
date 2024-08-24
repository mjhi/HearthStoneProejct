using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Heal : MonoBehaviour
{
    [SerializeField] TMP_Text HealTMP;
    Transform tr;

    public void SetupTransform(Transform tr)
    {
        this.tr = tr;
    }

    void Update()
    {
        if (tr != null)
            transform.position = tr.position;
    }

    public void Damaged(int Heal)
    {
        if (Heal <= 0)
            return;

        GetComponent<Order>().SetOrder(1000);
        HealTMP.text = $"+{Heal}";

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 1.8f, 0.5f).SetEase(Ease.InOutBack))
            .AppendInterval(1.2f)
            .Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack))
            .OnComplete(() => Destroy(gameObject));
    }
}
