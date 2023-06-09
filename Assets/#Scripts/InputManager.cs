using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityHelper;
using System.Collections.Generic;
using System.Collections;

public class InputManager : MonoBehaviour
{
    #region Fields

    [SerializeField] private TMP_InputField typingField;

    private List<ColorfulChar> typingText;

    private int caretPos = 0;

    private readonly int maxExtras = 20;

    #region Colors

    private readonly Color32 defaultColor = new(100, 102, 105, 255);
    private readonly Color32 correctColor = new(255, 255, 255, 255);
    private readonly Color32 incorrectColor = new(202, 71, 84, 255);
    private readonly Color32 extraColor = new(126, 42, 51, 255);

    #endregion

    #endregion

    private void Start()
    {
        typingField.onValidateInput += ValidateInput;

        typingField.ActivateInputField(); // activate InputField from the start

        typingText = GenerateText();
        ChangeText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && caretPos != 0)
        {
            if (typingText[caretPos - 1].c != ' ') Backspace();

            else
            {
                for (int i = caretPos - 2; i > 0; i--)
                {
                    if (typingText[i].c == ' ') break;

                    if (typingText[i].check == CharCheck.Incorrect || typingText[i].check == CharCheck.Extra)
                    {
                        Backspace();
                        break;
                    }
                }
            }

            void Backspace()
            {
                caretPos -= 1;

                typingField.MoveLeft(false, false);

                print($"{typingText[caretPos].c}; {typingText[caretPos].check}");

                if (typingText[caretPos].check != CharCheck.Extra)
                    ChangeColor(caretPos, defaultColor, CharCheck.Default);
                else
                    typingText.RemoveAt(caretPos);

                ChangeText();
            }
        }
        else typingField.caretPosition = caretPos;
    }

    private char ValidateInput(string text, int index, char c)
    {
        string binary = Convert.ToString(c, 2);
        if (binary == "1001" || binary == "1010") return '\0';

        bool canType = true;
        
        if (c == typingText[caretPos].c) ChangeColor(caretPos, correctColor, CharCheck.Correct);

        else
        {
            if (typingText[caretPos].c != ' ') 
                ChangeColor(caretPos, incorrectColor, CharCheck.Incorrect);
            else
            {
                // not more than `maxExtras` extra chars

                int currExtras = 0;

                for (int i = caretPos - 1; i >= 0; i--)
                {
                    if (typingText[i].check != CharCheck.Extra) break;
                    currExtras += 1;
                }

                if (currExtras < maxExtras)
                    typingText.Insert(caretPos, new ColorfulChar(c, extraColor, CharCheck.Extra));
                else 
                    canType = false;
            }
        }

        if (canType)
        {
            ChangeText();
            caretPos += 1;
        }
        return '\0';
    }

    private List<ColorfulChar> GenerateText()
    {
        string text = "dog cat house car book table chair computer phone tree flower river city country friend family school student teacher job work music art movie food drink water coffee time year day night morning evening week month year world life love heart mind body child man woman boy girl people friend family team game sport player team home road street park beach mountain sky sun moon star cloud rain snow ice wind fire earth planet space ocean sea lake river food drink water apple banana orange pizza hamburger sandwich salad coffee tea juice water milk soda beer wine tea coffee time year day night morning evening week month year world life love heart mind body child man woman boy girl people";

        return text.Select(w => new ColorfulChar(w, defaultColor)).ToList();
    }

    private void ChangeText() => typingField.text = String.Join("", typingText.Select(w => w.parsed));

    private void ChangeColor(int position, Color color, CharCheck check = CharCheck.Default)
    {
        ColorfulChar c = typingText[position];
        c.ChangeColor(color, check);
        typingText[position] = c;
    }
}