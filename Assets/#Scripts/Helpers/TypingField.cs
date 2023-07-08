using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TMPro
{
    [AddComponentMenu("TypingField", 1)]

    public class TypingField : MonoBehaviour
    {
        #region Members

        #region Serialized

        [Header("Text")]

        [Space(3f)]
        [SerializeField] private GameObject textComponentsHolder;
        [SerializeField] private TMP_Text textComponentPrefab;
        [SerializeField] private int textComponentLineCount;

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

        private int _caretPosition;
        private int _textComponentCurrLine;

        private float _textComponentLineHeight;
        private float _textComponentMaxWidth;

        private Vector2 _textComponentPrevValues;

        private int m_textComponentIndex;

        #region Coroutines

        private Coroutine _caretBlinkCoroutine;

        #endregion

        #endregion

        #region Properties

        private string text
        {
            get => _textComponent.text;
            set => SetText(value, true);
        }

        private int textComponentIndex
        {
            get => m_textComponentIndex;
            set => SetTextComponentIndex(value);
        }

        #endregion

        #endregion

        #region Fields Assignment

        private void Awake()
        {
            _caretImage = caret.gameObject.GetComponent<Image>();

            #region Set TextComponent

            textComponentPrefab.font = fontAsset;
            textComponentPrefab.fontSize = pointSize;

            _textComponentLineHeight = textComponentPrefab.GetPreferredValues().y + textComponentPrefab.lineSpacing * textComponentPrefab.fontSize * .01f;

            _textComponentMaxWidth = textComponentsHolder.GetComponent<RectTransform>().rect.width;

            textComponentPrefab.rectTransform.sizeDelta = new Vector2(textComponentPrefab.rectTransform.sizeDelta.x, _textComponentLineHeight * textComponentLineCount);

            #endregion
        }

        private void Start()
        {
            textComponentIndex = 0; // instantiate textComponent

            caret.sizeDelta = new Vector2(caretWidth, caretHeight);
            _caretImage.color = caretColor;
        }

        #endregion

        #region Setters

        private void SetText(string value, bool updateExternally)
        {
            #region Parse Text

            if (value == null)
                value = string.Empty;

            value = value.Replace("\0", string.Empty).Replace("\u200B", string.Empty);

            _caretPosition = value.Length;

            if (value.Length == 0)
                value = "\u200B";

            _textComponent.text = value;

            #endregion

            if (!updateExternally) 
                return;

            #region Change TextComponent

            _textComponent.ForceMeshUpdate();

            if (_textComponentCurrLine < 1) 
                _textComponentCurrLine = 1;

            Vector2 _textComponentCurrValues = _textComponent.GetRenderedValues();

            int lastCharacterIndex = text.Length - 1;
            float lastCharacterWidth = _textComponent.textInfo.characterInfo[lastCharacterIndex].xAdvance - _textComponent.textInfo.characterInfo[lastCharacterIndex].origin;

            if (text.Length >= 2 && _textComponent.textInfo.characterInfo[lastCharacterIndex - 1].character == ' ')
                lastCharacterWidth *= 2; // ' ' isn't counted in renderedValues

            // print($"textComponent: {textComponentIndex + 1}; character: \"{value[^1]}\"; _textComponentCurrLine: {_textComponentCurrLine}; _textComponentCurrValues: {_textComponentCurrValues}; _textComponentPrevValues: {_textComponentPrevValues}");

            if (_textComponentCurrValues.y > _textComponentPrevValues.y && _textComponentPrevValues.x + lastCharacterWidth > _textComponentMaxWidth)
            {
                if (_textComponentCurrLine == textComponentLineCount)
                {
                    _textComponentCurrLine = 1;
                    _textComponentCurrValues = default; // reset to zero

                    int firstCharacterIndex = _textComponent.textInfo.lineInfo[textComponentLineCount].firstCharacterIndex;

                    string lastLine = text[firstCharacterIndex..];

                    SetText(text[..firstCharacterIndex], false);

                    textComponentIndex += 1;

                    SetText(lastLine, false);

                    _textComponent.ForceMeshUpdate(); // for AlignCaret() to not throw errors

                    // print($"textComponent added; _textComponentCurrValues: {_textComponentCurrValues}; _textComponentPrevValues: {_textComponentPrevValues}");
                }
                else
                    _textComponentCurrLine += 1;
            }
            else if (_textComponentCurrValues.y < _textComponentPrevValues.y)
            {
                if (_textComponentCurrLine > 0) // shouldn't be negative
                    _textComponentCurrLine -= 1;
            }

            _textComponentPrevValues = _textComponentCurrValues; // assign to zero after manipulations

            AlignCaret();

            #endregion
        }

        private void SetTextComponentIndex(int value)
        {
            if (value < 0) return;

            while (_textComponents.Count <= value)
            {
                if (value != 0)
                    m_textComponentIndex += 1;

                TMP_Text newTextComponent = Instantiate(textComponentPrefab, textComponentsHolder.transform);

                newTextComponent.name = $"Text {m_textComponentIndex + 1}";

                _textComponents.Add(newTextComponent);
            }

            _textComponent = _textComponents[value];

            _caretPosition = (text == "\u200B") ? 0 : text.Length;

            AlignCaret();
        }

        #endregion

        #region Caret

        private void AlignCaret()
        {
            CaretBlink();

            TMP_CharacterInfo[] characterInfo = _textComponent.textInfo.characterInfo;

            TMP_CharacterInfo currentCharacter;
            float caretX;

            if (_caretPosition == 0)
            {
                currentCharacter = characterInfo[_caretPosition];
                caretX = currentCharacter.origin;
            }
            else
            {
                currentCharacter = characterInfo[_caretPosition - 1];
                caretX = currentCharacter.xAdvance;
            }

            float caretY = (currentCharacter.ascender + currentCharacter.descender) * .5f;

            caret.position = _textComponent.transform.TransformPoint(new(caretX, caretY));
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

        #region Check User's Input

        private void OnGUI()
        {
            Event evt = Event.current;

            if (evt != null)
                CheckEventType(evt);
        }

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