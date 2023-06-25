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

        private int _caretPosition;

        private int m_textComponentIndex;

        #region Readonly

        private readonly int _textComponentLineCount = 3;

        #endregion

        #endregion

        #region Properties

        private string text
        {
            get => _textComponent.text;
            set
            {
                if (value == null)
                    value = "";

                value = value.Replace("\0", string.Empty).Replace("\u200B", string.Empty);

                _caretPosition = value.Length;

                if (value.Length == 0)
                    value = "\u200B";

                _textComponent.text = value;

                AlignCaret();
            }
        }

        private int textComponentIndex
        {
            get => m_textComponentIndex;
            set
            {
                while (_textComponents.Count <= value)
                {
                    TMP_Text newTextComponent = Instantiate(textComponentPrefab, textComponentsHolder.transform);
                    _textComponents.Add(newTextComponent);
                }

                m_textComponentIndex = value;
                _textComponent = _textComponents[value];
            }
        }

        #endregion

        #region Coroutines

        private Coroutine _caretBlinkCoroutine;

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

            textComponentPrefab.rectTransform.sizeDelta = new Vector2(textComponentPrefab.rectTransform.sizeDelta.x, _textComponentLineHeight * _textComponentLineCount);

            #endregion
        }

        private void Start()
        {
            textComponentIndex = 0; // instantiate textComponent

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

        #endregion

        #region Caret

        private void AlignCaret()
        {
            StartCoroutine(AlignCaretCoroutine());

            IEnumerator AlignCaretCoroutine()
            {
                CaretBlink();

                yield return null; // wait for textComponent to set correctly

                _textComponent.ForceMeshUpdate(); // avoid IndexOutOfRangeException when typing quickly

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