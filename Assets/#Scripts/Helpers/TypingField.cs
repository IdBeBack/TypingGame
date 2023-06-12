using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TMPro
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("TypingField", 1)]

    public class TypingField : Selectable
    {
        #region Fields

        [Header("Components")]
        [SerializeField] private TMP_Text textComponent;

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

        private Image _caretImage;
        private TMP_TextInfo _textInfo;

        private string text
        {
            get => textComponent.text;
            set => SetText(value);
        }

        #region Coroutines

        private Coroutine _caretBlinkCoroutine;

        #endregion

        #endregion

        private new void Awake()
        {
            _caretImage = caret.gameObject.GetComponent<Image>();
            _textInfo = textComponent.textInfo;
        }

        private new void Start()
        {
            textComponent.font = fontAsset;
            textComponent.fontSize = pointSize;

            caret.sizeDelta = new Vector2(caretWidth, caretHeight);
            _caretImage.color = caretColor;

            CaretBlink();

            text = " ";
            AlignCaret(true);
            BackspaceKey();
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
            if (text == value)
                return;

            if (value == null)
                value = string.Empty;

            textComponent.text = value;
        }

        #region Caret

        private void AlignCaret(bool startPos = false)
        {
            CaretBlink();

            textComponent.ForceMeshUpdate(); // text should be updated before this method

            if (_textInfo.lineCount == 0) 
                _textInfo.lineCount = 1; // to set caret at the start

            TMP_LineInfo currentLine = _textInfo.lineInfo[_textInfo.lineCount - 1];

            TMP_CharacterInfo charInfo = _textInfo.characterInfo[currentLine.lastCharacterIndex];

            Extents extents = currentLine.lineExtents;

            float middleX = (text.Length == 0 || startPos) ? extents.min.x : charInfo.xAdvance;
            float middleY = (extents.min.y + extents.max.y) / 2f;

            caret.position = textComponent.transform.TransformPoint(new(middleX, middleY));
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

            AlignCaret();
        }

        private void BackspaceKey()
        {
            if (text.Length > 0)
                text = text.Remove(text.Length - 1, 1);

            AlignCaret();
        }

        #endregion  
    }
}