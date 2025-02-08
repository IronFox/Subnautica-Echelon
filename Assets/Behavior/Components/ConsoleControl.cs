using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Text;
using TMPro;
using System;

public readonly struct Line {
    public string Input { get; }
    public string Message { get; }
    public DateTimeOffset Captured { get; }


    public Line(string input)
    {
        Input = input;
        Captured = DateTimeOffset.Now;
        Message = $"{Captured:HH:mm:ss} {input}";
    }

    

    public bool IsOutdated => DateTimeOffset.Now - Captured > TimeSpan.FromSeconds(30);
}

public class ConsoleControl : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    public int maxLines = 10;

    public float secondsPerCharacter = 0.01f;
    private int animationProgress = 0;
    private static List<ConsoleControl> Instances { get; } = new List<ConsoleControl>();

    private static List<Line> StaticLines { get; } = new List<Line>();
    // Start is called before the first frame update
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        Instances.Add(this);
        foreach (var line in StaticLines)
            AddLine(line);
    }

    void OnDestroy()
    {
        Instances.Remove(this);
    }


    public static void Write(string text)
    {
        var line = new Line(text);
        foreach (var control in Instances)
            control.AddLine(line);
        if (Instances.Count == 0)
            StaticLines.Add(line);
    }






    private readonly Queue<Line> fullLines = new Queue<Line>();
    private readonly Queue<Line> pendingLines = new Queue<Line>();
    
    private int lineAnimationAt = 0;

    private Line? currentLine;
    private string textToShow = "";

    public void AddLine(Line line)
    {
        Debug.Log("Adding " + line);
        while (fullLines.Count > 0 && fullLines.Count + pendingLines.Count+1 > maxLines)
            fullLines.Dequeue();
        while (pendingLines.Count+1 > maxLines)
            fullLines.Dequeue();

        pendingLines.Enqueue(line);

    }

    void AnimateNextCharacter()
    {
        try
        {
            

            if (currentLine == null || lineAnimationAt >= currentLine.Value.Message.Length)
            {
                bool changed = false;
                if (currentLine != null)
                {
                    fullLines.Enqueue(currentLine.Value);
                    currentLine = null;
                    changed = true;
                }
                if (pendingLines.Count > 0)
                {
                    currentLine = pendingLines.Dequeue();
                    lineAnimationAt = 0;
                }
                else
                {
                    if (changed)
                    {
                        Rebuild();
                    }
                    lineAnimationAt = 0;
                    return;
                }
            }
            lineAnimationAt++;
            Rebuild();
            //tmp.rectTransform.localPosition = tmp.rectTransform.localPosition + new Vector3(0,0,0.1f);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void Rebuild()
    {
        StringBuilder textBuilder = new StringBuilder();
        foreach (var line in fullLines)
            textBuilder.AppendLine(line.Message);
        if (currentLine != null)
        {
            textBuilder.Append(currentLine.Value.Message.Substring(0, lineAnimationAt));
            textBuilder.Append('▄');
        }
        textToShow = textBuilder.ToString();
        tmp.text = textToShow;
    }

    void FixedUpdate()
    {
        animationProgress++;
        if (animationProgress * Time.fixedDeltaTime > secondsPerCharacter)
        {
            animationProgress = 0;
            AnimateNextCharacter();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //tmp.text = textToShow;
        bool changed = false;
        while (fullLines.Count > 1 && fullLines.Peek().IsOutdated)
        {
            fullLines.Dequeue();
            changed = true;
        }
        if (changed)
            Rebuild();
    }
}
