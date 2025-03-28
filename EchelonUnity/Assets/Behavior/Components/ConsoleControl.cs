using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Text;
using TMPro;
using System;

using System.IO;

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

    public override string ToString() => Message;
    

    public bool IsOutdated(int lineRetentionSeconds) => DateTimeOffset.Now - Captured > TimeSpan.FromSeconds(lineRetentionSeconds);
}

public class ConsoleControl : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    public int maxLines = 10;


    public int charactersPerSecond = 200;


    private float accumulatedSeconds;
    private static List<ConsoleControl> Instances { get; } = new List<ConsoleControl>();

    private static List<Line> StaticLines { get; } = new List<Line>();

    public int lineRetentionSeconds = 30;


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

    public static void WriteException(string whileDoing, Exception ex)
    {
        Debug.LogError($"Caught exception during {whileDoing}: {ex.Message}");
        //Write(ex.StackTrace);
        
        Debug.LogException(ex);
    }


    public static void Write(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        Debug.Log(text);
        var line = new Line(text);
        foreach (var control in Instances)
            control.AddLine(line);
        //if (Instances.Count == 0)
        //    StaticLines.Add(line);

        //Directory.CreateDirectory(@"C:\Temp\Logs");
        //File.AppendAllText(@"C:\Temp\Logs\log.txt", $"{DateTimeOffset.Now:HH:mm:ss.fff} {text} \r\n");

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
            pendingLines.Dequeue();

        pendingLines.Enqueue(line);

    }


    private Line? NextPendingLine()
    {
        if (pendingLines.Count > 0)
            return pendingLines.Dequeue();
        return null;
    }

    void AnimateNextCharacters(int numCharactersLeft)
    {
        try
        {


            if (currentLine == null || lineAnimationAt + numCharactersLeft >= currentLine.Value.Message.Length)
            {
                bool changed = false;
                if (currentLine != null)
                {
                    Debug.Log($"end line");
                    fullLines.Enqueue(currentLine.Value);
                    var take = currentLine.Value.Message.Length - lineAnimationAt;
                    if (take > 0)
                        numCharactersLeft -= take;
                    changed = true;
                    currentLine = null;
                }

                while (pendingLines.Count > 0)
                {
                    currentLine = pendingLines.Dequeue();
                    lineAnimationAt = 0;
                    var take = currentLine.Value.Message.Length;
                    if (take > numCharactersLeft)
                    {
                        Debug.Log($"end on partial line ({numCharactersLeft})");
                        lineAnimationAt = numCharactersLeft;
                        numCharactersLeft = 0;
                        changed = true;
                        break;
                    }


                    if (take == numCharactersLeft)
                    {
                        Debug.Log($"end on exact match");

                        numCharactersLeft = 0;
                        fullLines.Enqueue(currentLine.Value);
                        currentLine = NextPendingLine();
                        lineAnimationAt = 0;
                        changed = true;
                        break;
                    }

                    numCharactersLeft -= take;
                    fullLines.Enqueue(currentLine.Value);
                    currentLine = null;
                    changed = true;
                }

                if (currentLine == null)
                {
                    lineAnimationAt = 0;
                    if (changed)
                    {
                        Rebuild();
                    }
                    return;
                }
            }
            else
            {
                Debug.Log("normal increment by " + numCharactersLeft);
                lineAnimationAt += numCharactersLeft;
                Rebuild();
            }
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
        accumulatedSeconds += Time.fixedDeltaTime;
        float secondsPerCharacter = 1.0f / charactersPerSecond;
        if (accumulatedSeconds > secondsPerCharacter)
        {
            int numCharacters = Mathf.FloorToInt(accumulatedSeconds / secondsPerCharacter);
            accumulatedSeconds -= numCharacters *secondsPerCharacter;
            AnimateNextCharacters(numCharacters);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //tmp.text = textToShow;
        bool changed = false;
        while (fullLines.Count > 1 && fullLines.Peek().IsOutdated(lineRetentionSeconds))
        {
            fullLines.Dequeue();
            changed = true;
        }
        if (changed)
            Rebuild();
    }
}
