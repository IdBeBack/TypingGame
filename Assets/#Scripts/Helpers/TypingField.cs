using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace TMPro
{
    [AddComponentMenu("TypingField", 1)]

    public class TypingField : MonoBehaviour
    {
        #region Members

        #region Serialized

        [Header("Text")]
        [SerializeField] private GameObject textComponentsHolder;
        [SerializeField] private TMP_Text textComponentPrefab;

        [Header("Typing Field Settings")]

        [Space(3f)]
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private float pointSize;

        [Space(6f)]
        [SerializeField] private RectTransform caret;
        [SerializeField] private float caretWidth;
        [SerializeField] private float caretHeight;
        [SerializeField] private float caretBlinkRate;
        [SerializeField] private Color caretColor;

        #endregion

        #region Private

        private List<TMP_Text> _textComponents = new();

        private Image _caretImage;
        private TMP_Text _textComponent;

        private float _textComponentLineHeight;
        private float _textComponentWidth;

        private int _caretPosition;

        private int m_textComponentIndex;

        #region Readonly

        private readonly int _textComponentLineCount = 2;

        #endregion

        #region Coroutines

        private Coroutine _caretBlinkCoroutine;
        private Coroutine _setTextCoroutine;
        private Coroutine _setTextComponentCoroutine;

        #endregion

        #endregion

        #region Properties

        private string text
        {
            get => _textComponent.text;
            set
            {
                if (_setTextCoroutine != null)
                    return;

                StartCoroutine(SetTextHelper());

                IEnumerator SetTextHelper()
                {
                    _setTextCoroutine = StartCoroutine(SetText());

                    IEnumerator SetText()
                    {
                        if (value == null)
                            value = string.Empty;

                        value = value.Replace("\0", string.Empty).Replace("\u200B", string.Empty);

                        _caretPosition = value.Length;

                        if (value.Length == 0)
                            value = "\u200B";

                        _textComponent.text = value;

                        _textComponent.ForceMeshUpdate();

                        AlignCaret();

                        //

                        TMP_CharacterInfo lastCharacter = _textComponent.textInfo.characterInfo[_textComponent.textInfo.characterCount - 1];
                        float lastCharacterWidth = lastCharacter.xAdvance - lastCharacter.origin;

                        if (_textComponent.GetRenderedValues().x + lastCharacterWidth > _textComponentWidth)
                        {
                            print("I should change textComponent");
                        }

/*
                        if (_textComponent.textInfo.lineCount == _textComponentLineCount + 1)
                        {
                            // print($"textComponent: {textComponentIndex}; lineCount: {_textComponent.textInfo.lineCount}; text: {_textComponent.text}");

                            if (_textComponent.textInfo.lineCount > _textComponentLineCount + 1)
                                ;

                            var textComponentT = _textComponent;
                            var textInfoT = _textComponent.textInfo;
                            var lineCountT = _textComponent.textInfo.lineCount;
                            var lineInfoT = _textComponent.textInfo.lineInfo;


                            int firstCharacterIndex = _textComponent.textInfo.lineInfo[_textComponentLineCount].firstCharacterIndex;

                            string lastLine = text[firstCharacterIndex..];

                            _textComponent.text = text[..firstCharacterIndex];

                            textComponentIndex += 1;

                            _textComponent.text = lastLine;

                            // print($"textComponent: {textComponentIndex} ; Length: {_textComponent.textInfo.lineInfo.Length}; lastLine: {lastLine}");
                        }
*/
                        yield break;
                    }

                    yield break;
                }
            }
        }

        private int textComponentIndex
        {
            get => m_textComponentIndex;
            set
            {
                if (value < 0) return;

                while (_textComponents.Count <= value)
                {
                    if (value != 0)
                        m_textComponentIndex += 1;

                    TMP_Text newTextComponent = Instantiate(textComponentPrefab, textComponentsHolder.transform);

                    //print($"Instantiated textComponent; lineCount: {newTextComponent.textInfo.lineCount};");

                    newTextComponent.name = $"Text {m_textComponentIndex + 1}";

                    _textComponents.Add(newTextComponent);
                }

                _textComponent = _textComponents[value];

                _caretPosition = (text == "\u200B") ? 0 : text.Length;
                AlignCaret();
            }
        }

        #endregion

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _caretImage = caret.gameObject.GetComponent<UnityEngine.UI.Image>();

            #region Set TextComponent

            textComponentPrefab.font = fontAsset;
            textComponentPrefab.fontSize = pointSize;

            _textComponentLineHeight = textComponentPrefab.GetPreferredValues().y + textComponentPrefab.lineSpacing * textComponentPrefab.fontSize * .01f;

            _textComponentWidth = textComponentsHolder.GetComponent<RectTransform>().rect.width;

            textComponentPrefab.rectTransform.sizeDelta = new Vector2(textComponentPrefab.rectTransform.sizeDelta.x, _textComponentLineHeight * _textComponentLineCount);

            #endregion
        }

        private void Start()
        {
            textComponentIndex = 0; // instantiate textComponent

            caret.sizeDelta = new Vector2(caretWidth, caretHeight);
            _caretImage.color = caretColor;

            print(_textComponentWidth);
        }

        private void OnGUI()
        {
            Event evt = Event.current;

            if (evt != null)
                CheckEventType(evt);
        }

        private void Update()
        {
            int lastCharacterIndex = _textComponent.textInfo.characterCount - 1;

            float lastCharacterWidth = _textComponent.textInfo.characterInfo[lastCharacterIndex].xAdvance - _textComponent.textInfo.characterInfo[lastCharacterIndex].origin;

            //print($"{_textComponent.GetRenderedValues()}; lineHeight: {_textComponentLineHeight}");

            print($"renderedValues: {_textComponent.GetRenderedValues()}; _textComponentWidth: {_textComponentWidth}; lastCharacterWidth: {lastCharacterWidth}; currectWidth: {_textComponent.GetRenderedValues().x + lastCharacterWidth}");
        }

        #endregion

        #region Caret

        private void AlignCaret()
        {
            StartCoroutine(AlignCaretCoroutine());

            IEnumerator AlignCaretCoroutine()
            {
                CaretBlink();

                yield return null; // wait for textComponent to set correctly

                TMP_CharacterInfo currentCharacter;
                float caretX;

                if (_caretPosition == 0)
                {
                    currentCharacter = _textComponent.textInfo.characterInfo[_caretPosition];
                    caretX = currentCharacter.origin;
                }
                else
                {
                    currentCharacter = _textComponent.textInfo.characterInfo[_caretPosition - 1];
                    caretX = currentCharacter.xAdvance;
                }

                float caretY = (currentCharacter.ascender + currentCharacter.descender) * .5f;

                caret.position = _textComponent.transform.TransformPoint(new(caretX, caretY));
            }
        }

        private void CaretBlink()
        {
            if (_caretBlinkCoroutine != null)
                StopCoroutine(_caretBlinkCoroutine);

            ChangeAlpha(1f);

            _caretBlinkCoroutine = StartCoroutine(CaretBlinkCoroutine());

            IEnumerator CaretBlinkCoroutine()
            {
                WaitForSeconds _caretBlinkRateWFS = new WaitForSeconds(caretBlinkRate);

                while (true)
                {
                    yield return _caretBlinkRateWFS;

                    ChangeAlpha(0f);

                    yield return _caretBlinkRateWFS;

                    ChangeAlpha(1f);
                }
            }

            void ChangeAlpha(float alpha)
            {
                Color newColor = _caretImage.color;
                newColor.a = alpha;
                _caretImage.color = newColor;
            }
        }

        #endregion

        #region Event

        private void CheckEventType(Event evt)
        {
            switch (evt.type)
            {
                case EventType.KeyDown:
                    KeyDownEvent(evt);
                    break;
            }
        }

        #region KeyDown

        private void KeyDownEvent(Event evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    BackspaceKey();
                    return;

                case KeyCode.Delete:
                    return;
            }

            ValidateInput(evt.character);
        }

        private void ValidateInput(char c)
        {
            if (c == '\0' || c == '\t' || c == '\n')
                return;

            text += c;
        }

        private void BackspaceKey()
        {
            if (text.Length > 0)
                text = text.Remove(text.Length - 1, 1);
        }

        #endregion  

        #endregion
    }
}