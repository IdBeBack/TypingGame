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

    private string niceText; // test

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

        niceText = "ability able about above absence absolute accept access accident account achieve across act action active activity actor actual add addition address admit adult advance advice affect afford afraid after afternoon again against age agency agent ago agree agreement ahead air airline airport album alcohol alive all alliance allow almost alone along already also alter always amazing ambition among amount analyst ancient anger angle angry animal ankle announce annual another answer anxiety any apart apartment apologize apparent appeal appear apple application apply appoint approval approve area argue argument arise arm armed army around arrange arrival arrive art article artist artistic as ask asleep aspect assembly assert assess assign assist assistance assistant associate assume assurance assure attach attack attempt attend attention attitude attract attraction attractive audience author authority auto automatic available average avoid awake award aware away awesome awful awkward baby back background bad bag bake balance ball ban band bank bar barely barrel base basic basically basis basketball bathroom battery battle be bear beat beautiful beauty because become bed bedroom before begin beginning behavior behind being belief believe bell belong below belt bench bend beneath benefit beside besides best bet better between beyond Bible big bike bill billion bind biological bird birth birthday bit bite black blade blame blanket bless blind block blood blow blue board boat body boil bomb bond bone bonus book boom boot border born borrow boss both bother bottle bottom bounce bound bowl box boy boyfriend brain branch brand brave bread break breakfast breast breath breathe brick bride bridge brief bright bring broad broken brother brown brush build building bullet bunch burden burn burst bus business busy but butter button buy buyer by cabin cabinet cable cake calculate call calm camera camp campaign campus can Canadian cancer candidate candle candy capable capacity capital captain capture car carbon card care career careful carefully carpet carry case cash cast castle cat catch category Catholic cause caution cave ceiling celebrate celebration celebrity cell center central century CEO ceremony certain certainly chain chair chairman challenge chamber champion chance change changing channel chapter character characteristic characterize charge charity chart chase cheap check cheek cheese chef chemical chest chicken chief child childhood Chinese chip chocolate choice choose Christian Christmas church cigarette circle circumstance cite citizen city civil civilian claim class classic classroom clean clear clearly client climate climb clinic clinical clock close closely closer clothes clothing cloud club clue cluster coach coal coalition coast coat code coffee cognitive cold collapse collar colleague collect collection collective college colonial color column combination combine come comedy comfort comfortable command commander comment commercial commission commit commitment committee common communicate communication community company compare comparison compete competition competitive competitor complain complaint complete completely complex compliance complicated component compose composition comprehensive computer concentrate concentration concept concern concerned concert conclude conclusion concrete condition conduct conference confidence confident confirm conflict confront confusion Congress congressional connect connection consciousness consensus consequence conservative consider considerable consideration consist consistent constantly constitute constitutional construct construction consult consume consumer consumption contact contain container contemplate contemporary content contest context continue continued contract contrast contribute contribution control controversial controversy convenient convention conventional conversation convert conviction convince cook cookie cooking cool cooperation cop cope copy core corn corner corporate corporation correct correspondent cost cotton couch could council counselor count counter country county couple courage course court cousin cover coverage cow crack craft crash crazy cream create creation creative creature credit crew crime criminal crisis criteria critic critical criticism criticize crop cross crowd crucial cruel cruise cry cultural culture cup curious current currently curriculum custom customer cut cycle dad daily damage dance danger dangerous dare dark darkness data date daughter day dead deadline deal dealer dear death debate debt decade decide decision deck declare decline decrease deep deeply deer defend defendant defense defensive deficit define definitely definition degree delay deliver delivery demand democracy Democrat democratic demonstrate demonstration deny department depend dependent depending depict depression depth deputy derive describe description desert deserve design designate designer desire desk desperate despite destroy destruction detail detailed detect determine develop developing development device devote dialogue die die difference different differently difficult difficulty dig digital dimension dining dinner direct direction directly director dirt dirty disability disagree disappear disaster discipline discourse discover discovery discrimination discuss discussion disease dish dismiss disorder display dispute distance distant distinct distinction distinguish distribute distribution district diverse diversity divide division divorce DNA do doctor document dog dollar domestic dominant dominate door double doubt down downtown dozen draft drag drama dramatic dramatically draw drawing dream dress drink drive driver drop drug dry due during dust duty";

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