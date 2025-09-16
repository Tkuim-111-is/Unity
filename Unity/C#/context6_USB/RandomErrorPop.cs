using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RandomErrorPop : MonoBehaviour
{
    public RectTransform errorCanvas; 
    public RectTransform errorPrefab;
    public int maxCount = 200;
    public float interval = 0.02f; 

    private int count = 0;

    IEnumerator SpawnErrors()
    {
        while (count < maxCount)
        {
            SpawnOneError();
            count++;
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnOneError()
    {
        RectTransform newError = Instantiate(errorPrefab, errorCanvas);

        float width = errorCanvas.rect.width;
        float height = errorCanvas.rect.height;

        float x = Random.Range(-width / 2f, width / 2f);
        float y = Random.Range(-height / 2f, height / 2f);

        newError.anchoredPosition = new Vector2(x, y);
        newError.gameObject.SetActive(true);
    }
}
