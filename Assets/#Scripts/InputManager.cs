using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityHelper;

public class InputManager : MonoBehaviour
{
    #region Fields

    [SerializeField] private TMP_InputField typingField;

    private ColorfulChar[] inputText;

    private int caretPos = 0;

    #region Colors

    private readonly Color32 defaultColor = new(100, 102, 105, 255);
    private readonly Color32 correctColor = new(255, 255, 255, 255);
    private readonly Color32 wrongColor = new(202, 71, 84, 255);

    #endregion

    #endregion

    private void Awake()
    {
        print(defaultColor);
        print(ColorUtility.ToHtmlStringRGBA(defaultColor));
    }

    private void Start()
    {
        typingField.onValidateInput += ValidateInput;

        typingField.ActivateInputField(); // activate InputField from the start

        inputText = GenerateText();
    }

    private void Update()
    {
        ChangeText();

        print($"caretPos: {caretPos}; typingField.caretPosition: {typingField.caretPosition}");

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            print("backspace");

            caretPos += 1;
        }
    }

    private char ValidateInput(string text, int index, char c)
    {
        string binary = Convert.ToString(c, 2);

        if (binary == "1001" || binary == "1010") return '\0';

        if (c == inputText[caretPos].c)
        {
            inputText[caretPos].ChangeColor(correctColor);
        }
        else
        {
            inputText[caretPos].ChangeColor(wrongColor);
        }


        // print(binary);

        ChangeText();

        caretPos += 1;
        return '\0';
    }

    private ColorfulChar[] GenerateText()
    {
        string text = "the of and to a in for is on that by this with i you it not or be are from at as your all have new more an was we will home can us about if page my has search free but our one other do no information time they site he up may what which their news out use any there see only so his when contact here business who web also now help get pm view online c e first am been would how were me s services some these click its like service than find date back top people may just software";

        return text.Select(w => new ColorfulChar(w, defaultColor)).ToArray();
    }

    private void ChangeText()
    {
        typingField.caretPosition = caretPos;
        typingField.text = string.Concat(inputText.Select(w => w.parsed));
    }
}