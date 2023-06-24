using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace TMPro
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    [AddComponentMenu("TypingField", 1)]

    public class TypingField : Selectable
    {
        #region Fields

        [Header("Components")]
        [SerializeField] private GameObject textComponentsHolder;

        #region TypingField Settings

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

        private UnityEngine.UI.Image _caretImage;
        private TMP_TextInfo _textInfo;

        private List<TMP_Text> _textComponents = new();

        private float _lineHeight;

        private int _textComponentIndex = 0;

        private int _caretPosition;

        #endregion

        private string text
        {
            get => _textComponents[_textComponentIndex].text;
            set => SetText(value);
        }

        #region Coroutines

        private Coroutine _caretBlinkCoroutine;

        #endregion

        #endregion

        private new void Awake()
        {
            foreach (Transform child in textComponentsHolder.transform)
                _textComponents.Add(child.GetComponent<TMP_Text>());

            _caretImage = caret.gameObject.GetComponent<UnityEngine.UI.Image>();
            _textInfo = _textComponents[_textComponentIndex].textInfo;

            #region Text Height

            TMP_Text textComponent = _textComponents[0];
            _lineHeight = textComponent.GetPreferredValues().y + textComponent.lineSpacing * textComponent.fontSize * .01f;

            float newHeight = _lineHeight * 3f;
            _textComponents[_textComponentIndex].rectTransform.sizeDelta = new Vector2(_textComponents[0].rectTransform.sizeDelta.x, newHeight);

            #endregion
        }

        private new void Start()
        {
            _textComponents[_textComponentIndex].font = fontAsset;
            _textComponents[_textComponentIndex].fontSize = pointSize;

            caret.sizeDelta = new Vector2(caretWidth, caretHeight);
            _caretImage.color = caretColor;

            text = null; // initialize zero-width space and call AlignCaret
        }

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

        private void SetText(string value)
        {
            if (value == null)
                value = "";

            value = value.Replace("\0", string.Empty).Replace("\u200B", string.Empty);

            _caretPosition = value.Length;

            if (value.Length == 0)
                value = "\u200B";

            _textComponents[_textComponentIndex].text = value;

            AlignCaret();
        }

        #region Caret

        private void AlignCaret()
        {
            StartCoroutine(AlignCaretCoroutine());

            IEnumerator AlignCaretCoroutine()
            {
                CaretBlink();

                yield return null;

                _textComponents[_textComponentIndex].ForceMeshUpdate(); // avoid IndexOutOfRangeException when typing quickly

                TMP_CharacterInfo currentCharacter;
                float caretX;

                if (_caretPosition == 0)
                {
                    currentCharacter = _textComponents[_textComponentIndex].textInfo.characterInfo[_caretPosition];
                    caretX = currentCharacter.origin;
                }
                else
                {
                    currentCharacter = _textComponents[_textComponentIndex].textInfo.characterInfo[_caretPosition - 1];
                    caretX = currentCharacter.xAdvance;
                }

                float caretY = (currentCharacter.ascender + currentCharacter.descender) * .5f;

                caret.position = _textComponents[_textComponentIndex].transform.TransformPoint(new(caretX, caretY));
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
    }
}