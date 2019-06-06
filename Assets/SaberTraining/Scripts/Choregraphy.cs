using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ChoregraphyStep {
    public float delay;
    public string name;
}

public class Choregraphy : MonoBehaviour
{
    public List<ChoregraphyStep> steps = new List<ChoregraphyStep>();
    Animator animator;
    int stepIndex = 0;
    public bool shouldLoop = true;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        NextStep();
    }

    void NextStep()
    {
        if (stepIndex >= steps.Count) {
            if (!shouldLoop) {
                return;
            }
            stepIndex = 0;
        }
        var step = steps[stepIndex];
        StartCoroutine(PlayStep(step));
    }

    IEnumerator PlayStep(ChoregraphyStep step) {
        yield return new WaitForSeconds(step.delay);
        stepIndex++;
        animator.SetTrigger(step.name);
        NextStep();
    }
}
