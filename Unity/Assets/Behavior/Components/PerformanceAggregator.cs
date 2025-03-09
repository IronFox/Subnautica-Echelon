using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class PerformanceAggregator : CommonBoardingListener
{
    private PerformanceAggregate myAggregate = new PerformanceAggregate();
    public TextMeshProUGUI outText;
    public RectTransform parentCanvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void SignalOnboardingBegin()
    {
        outText.enabled = false;
    }

    public override void SignalOffBoardingEnd()
    {
        outText.enabled = false;
    }


    // Update is called once per frame
    protected override void P_Update()
    {
        PerformanceCapture. AggregateAndReset(myAggregate);

        float w = parentCanvas.rect.width;
        float h = parentCanvas.rect.height;
        float myWidth = w / 2 * 0.9f;
        float myHeight = h * 0.9f;
        outText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, myWidth);
        outText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myHeight);

        outText.rectTransform.localPosition = M.V3(w/2 - myWidth/2*1.4f, h/2 - myHeight, 0);
        //outText.rectTransform.localPosition =
        //    M.V3(parentCanvas.rect.width/2 - w*0.65f, 0, 0);


        StringBuilder b = new StringBuilder();
        b.Append("Time Status\n");

        var worst = myAggregate.GetWorst();

        foreach (var s in worst.OrderByDescending(x => x.Total.TotalSeconds))
        {
            b.Append(s.Worst.Type.Name).Append(" := ").Append(s.Worst.Total.TimeSum.TotalMilliseconds).Append("ms /total ").Append(s.Total.TotalMilliseconds).Append("ms \n");
        }

        outText.text = b.ToString();

    }

    internal void SetVisible(bool enabled)
    {
        outText.enabled = enabled;

    }
}
