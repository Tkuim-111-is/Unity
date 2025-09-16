using UnityEngine;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;
using static UnityEngine.Rendering.HableCurve;
using System.Threading;
using System;

public class subtitles_4 : MonoBehaviour
{
    private CancellationTokenSource cancellationTokenSource;

    public TextMeshProUGUI Text;
    private Coroutine subtitleCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (Text != null)
        {
            Debug.Log("Text已綁定");
 
        }
        else
        {
            Debug.Log("Text未綁定");
        }
         
        


    }
    private string[] SplitSubtitle(string text)
    {
        // 根據標點符號、換行符或空格分割文本
        return text.Split(new[] {  '!', '?', '。', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
    }
    public async void ShowSubtitlesInSegments(string fullText )
    {
        cancellationTokenSource = new CancellationTokenSource();
        // 將完整的字幕文本分割成多段
        string[] segments = SplitSubtitle(fullText);
        try
        {
            foreach (var segment in segments)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                // 顯示當前段
                show_subtitle(segment);

                // 等待指定時間後清空字幕
                await Task.Delay((segment.Length * 200));

                // 清空字幕
                clear_subtitle();
            }

        }
        catch (OperationCanceledException)
        {
             
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        // 遍歷每一段字幕，逐步顯示


    }
    public void CancelOperation()
    {
        cancellationTokenSource?.Cancel(); 
        clear_subtitle();
    }
    public  void show_subtitle(string text )
    {
        Text.text= text;            
    }
    // Update is called once per frame
    public void clear_subtitle()
    {
        Text.text = "";
    }
     
}
