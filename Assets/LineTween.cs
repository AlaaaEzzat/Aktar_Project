using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class LineTween : MonoBehaviour
{
    public List<Transform> waypoints;
    public float segmentDuration = 0.5f;
    private LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 1;
        line.SetPosition(0, waypoints[0].position);
    }

    public void AnimateLine()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 1; i < waypoints.Count; i++)
        {
            int index = i;
            seq.AppendCallback(() =>
            {
                line.positionCount = index + 1;
                line.SetPosition(index, waypoints[index - 1].position);
                DOTween.To(() => line.GetPosition(index),
                           x => line.SetPosition(index, x),
                           waypoints[index].position,
                           segmentDuration);
            });

            seq.AppendInterval(segmentDuration);
        }
    }
}
