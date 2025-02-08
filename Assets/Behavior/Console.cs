using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Text;
using TMPro;

public class Console : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    public int maxLines = 20;

    public float secondsPerCharacter = 0.1f;
    private int animationProgress = 0;
    // Start is called before the first frame update
    void Start()
    {

        AddLine("Hello world");
    }

    private readonly Queue<string> fullLines = new Queue<string>();
    private readonly Queue<string> pendingLines = new Queue<string>();
    
    private int lineAnimationAt = 0;

    private string currentLine;
    private string textToShow = "";

    public void AddLine(string line)
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
            

            if (currentLine == null || lineAnimationAt >= currentLine.Length)
            {
                bool changed = false;
                if (currentLine != null)
                {
                    fullLines.Enqueue(currentLine);
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
                    Debug.Log(changed);
                    if (changed)
                    {
                        StringBuilder textBuilder = new StringBuilder();
                        foreach (string line in fullLines)
                            textBuilder.AppendLine(line);
                        //textBuilder.Append('▄');
                        textToShow = textBuilder.ToString();
                        tmp.text = textToShow;
                    }
                    lineAnimationAt = 0;
                    return;
                }
            }
            lineAnimationAt++;
            {
                StringBuilder textBuilder = new StringBuilder();
                foreach (string line in fullLines)
                    textBuilder.AppendLine(line);
                textBuilder.Append(currentLine.Substring(0, lineAnimationAt));
                textBuilder.Append('▄');
                textToShow = textBuilder.ToString();
                tmp.text = textToShow;
                
            }
            //tmp.rectTransform.localPosition = tmp.rectTransform.localPosition + new Vector3(0,0,0.1f);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
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

    }
}
