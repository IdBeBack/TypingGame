using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityHelper;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    #region Fields

    private List<ColorfulChar> typingText;

    [SerializeField] private TMP_InputField typingField;

    [SerializeField] private int visibleLines;
    private int prevVisibleLines;

    [SerializeField] private int wordsCount;

    private RectTransform typingFieldRect;

    private Coroutine backspaceCoroutine;

    private int caretPos; // caret position that is changed
    private int prevSceenHeight;
    private float lineHeight;

    #region Readonly

    private readonly int maxExtras = 20;

    private readonly WaitForSeconds backspaceHoldDelay = new WaitForSeconds(.4f);
    private readonly WaitForSeconds backspaceHoldInterval = new WaitForSeconds(.02f);

    private readonly string currDatabasePath = @"Z:\Projects\UNITY\TypingGame\Assets\Databases\Languages\english.json";

    #region Colors

    private readonly Color32 defaultColor = new(100, 102, 105, 255);
    private readonly Color32 correctColor = new(255, 255, 255, 255);
    private readonly Color32 incorrectColor = new(202, 71, 84, 255);
    private readonly Color32 extraColor = new(126, 42, 51, 255);

    #endregion

    #endregion

    #endregion

    private void Awake()
    {
        lineHeight = GetLineHeight();
        typingField.lineHeight = lineHeight;

        typingFieldRect = typingField.GetComponent<RectTransform>();

        AlignTypingField();

        float GetLineHeight()
        {
            TMP_Text textComponent = typingField.textComponent;

            textComponent.text = "A";

            return textComponent.GetPreferredValues().y + textComponent.lineSpacing * textComponent.fontSize * .01f;
        }
    }

    private void Start()
    {
        typingField.onValidateInput += ValidateInput;

        typingField.ActivateInputField();

        typingText = TextManager.GenerateText(currDatabasePath, defaultColor, wordsCount);

        ChangeText();
    }

    private void Update()
    {
        #region Caret

        typingField.caretPosition = caretPos;

        #endregion

        #region Backspace

        if (Input.GetKeyDown(KeyCode.Backspace) && caretPos != 0)
            backspaceCoroutine = StartCoroutine(BackspaceCoroutine());

        if (Input.GetKeyUp(KeyCode.Backspace))
            StopCoroutine(backspaceCoroutine);

        #endregion

        #region Screen

        if (prevSceenHeight != Screen.height || visibleLines != prevVisibleLines) 
            AlignTypingField();

        #endregion
    }

    private void AlignTypingField()
    {
        prevVisibleLines = visibleLines;
        typingField.visibleLines = visibleLines;

        prevSceenHeight = Screen.height;

        float rectOffset = .5f * (prevSceenHeight - lineHeight * visibleLines);

        typingFieldRect.offsetMax = new Vector2(typingFieldRect.offsetMax.x, -rectOffset);
        typingFieldRect.offsetMin = new Vector2(typingFieldRect.offsetMin.x, rectOffset);
    }

    private IEnumerator BackspaceCoroutine()
    {
        Backspace();
        yield return backspaceHoldDelay;

        while (true)
        {
            Backspace();
            yield return backspaceHoldInterval;
        }

        void Backspace()
        {
            if (caretPos == 0) return;

            if (typingText[caretPos - 1].c != ' ') BackspaceHelper();
            else
            {
                for (int i = caretPos - 2; i > 0; i--)
                {
                    if (typingText[i].c == ' ') break;

                    if (typingText[i].check == CharCheck.Incorrect || typingText[i].check == CharCheck.Extra)
                    {
                        BackspaceHelper();
                        break;
                    }
                }
            }

            void BackspaceHelper()
            {
                caretPos -= 1;

                typingField.MoveLeft(false, false);

                if (typingText[caretPos].check != CharCheck.Extra)
                    ChangeColor(caretPos, defaultColor, CharCheck.Default);
                else
                    typingText.RemoveAt(caretPos);

                ChangeText();
            }
        }
    }

    private char ValidateInput(string text, int index, char c)
    {
        if (caretPos == typingText.Count) return '\0';

        string binary = Convert.ToString(c, 2);
        if (binary == "1001" || binary == "1010") return '\0';

        if (c == typingText[caretPos].c) ChangeColor(caretPos, correctColor, CharCheck.Correct);
        else
        {
            if (typingText[caretPos].c != ' ')
            {
                if (c != ' ') ChangeColor(caretPos, incorrectColor, CharCheck.Incorrect);
                else
                {
                    if (typingText[caretPos - 1].c == ' ') return '\0';
                    else
                    {
                        for (int i = caretPos; i < typingText.Count; i++)
                        {
                            if (typingText[i].c == ' ' || i == typingText.Count - 1)
                            {
                                caretPos = i;
                                break;
                            }
                            else ChangeColor(i, incorrectColor, CharCheck.Incorrect);
                        }
                    }
                }
            }
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
                    return '\0';
            }
        }

        ChangeText();
        caretPos += 1;
        return '\0';
    }

    private void ChangeText()
    {
        typingField.text = String.Concat(typingText.Select(w => w.parsed));
    }

    private void ChangeColor(int position, Color color, CharCheck check = CharCheck.Default)
    {
        ColorfulChar c = typingText[position];
        c.ChangeColor(color, check);
        typingText[position] = c;
    }
}