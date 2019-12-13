using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIToaster : MonoBehaviour
{
    [SerializeField] string text = "";
    [SerializeField] float duration = 10.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] float beginFade = 0.8f;
    [SerializeField] float floatDistance = 4.0f;
    [SerializeField] EasingFunction.Ease outEasing = EasingFunction.Ease.OutExpo;

    [SerializeField] TextMeshProUGUI u_Text = null;
    

    float currentTime = 0.0f;
    Vector3 startPosition = Vector3.zero;
    Vector3 endPosition = Vector3.zero;

    public void SetupToaster(string newText, float newDuration, float newBeginFade, EasingFunction.Ease newEaseOut, float newFloatDistance)
    {
        text = newText;
        duration = newDuration;
        beginFade = newBeginFade;
        outEasing = newEaseOut;
        floatDistance = newFloatDistance;

        UpdateAttributes();
    }

    public void SetupToaster(string newText, Color textColor, float newDuration, float newBeginFade, EasingFunction.Ease newEaseOut, float newFloatDistance)
    {
        u_Text.color = textColor;
        text = newText;
        duration = newDuration;
        beginFade = newBeginFade;
        outEasing = newEaseOut;
        floatDistance = newFloatDistance;

        UpdateAttributes();
    }

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + new Vector3(0.0f, floatDistance, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Remove self when time is done
        if (currentTime >= duration)
            Destroy(gameObject);

        // Easing
        float t = currentTime / duration;
        transform.position = Vector3.Lerp(
            startPosition,
            endPosition,
            EasingFunction.GetEasingFunction(outEasing)(0.0f, 1.0f, t));

        // Fade
        float fadeT = Mathf.Clamp01(t - beginFade) / (1.0f - beginFade);
        u_Text.alpha = 1.0f - fadeT;

        currentTime += Time.deltaTime;
    }

    void UpdateAttributes()
    {
        u_Text.text = text;
    }

}
