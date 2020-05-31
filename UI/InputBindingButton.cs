using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class InputBindingButton : MonoBehaviour {
    public InputAction inputAction;
    public int targetBinding;
    Text buttonText;
    public void BindingButtonCallback() {

        // MyControls c = new MyControls();
        // Debug.Log(c.KeyboardMouseScheme.bindingGroup);

        var remapOperation = inputAction.PerformInteractiveRebinding();
        remapOperation.WithCancelingThrough("<Keyboard>/escape");
        remapOperation.WithControlsExcluding("Mouse");
        remapOperation.OnMatchWaitForAnother(0.1f);
        remapOperation.WithBindingGroup("Keyboard&Mouse");
        remapOperation.OnComplete(ctx => {
            ctx.Dispose();
            SetTexts();
        });

        if (targetBinding != -1) {
            remapOperation.WithTargetBinding(targetBinding);
            // remapOperation.WithBindingMask(inputAction.bindings[targetBinding]);
            // Debug.Log(inputAction.bindings[targetBinding]);
            // Debug.Log(inputAction.bindings[targetBinding].groups);
        }
        // remapOperation.WithExpectedControlType("<k")
        // remapOperation.OnPotentialMatch(ctx => Debug.Log("potential"));
        // remapOperation.WithExpectedControlType()

        remapOperation.Start();
    }
    public void Start() {
        // this.targetBinding = targetBinding;
        // this.inputAction = inputAction;
        SetTexts();

        Text myText = transform.Find("Desc/text").GetComponent<Text>();
        myText.text = name;
    }

    public void SetTexts() {
        Debug.Log("set text");

        buttonText = transform.Find("buttons/Button/Text").GetComponent<Text>();

        // string displayName = InputControlPath.ToHumanReadableString(inputAction.bindings[0].effectivePath);

        string displayName = inputAction.GetBindingDisplayString(
            InputBinding.MaskByGroup("Keyboard&Mouse"),
            InputBinding.DisplayStringOptions.DontUseShortDisplayNames);

        if (targetBinding != -1) {
            displayName = inputAction.bindings[targetBinding].ToDisplayString();
        }


        buttonText.text = displayName;
    }

}
