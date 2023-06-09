using System;
using UnityEngine;

namespace UnityHelper
{
    #region Structs

    public enum CharCheck
    {
        Default,
        Correct,
        Incorrect,
        Extra
    }

    [Serializable]
    public struct ColorfulChar
    {
        public char c { get; private set; }
        public string color { get; private set; }
        public string parsed { get; private set; }

        public CharCheck check { get; private set; }

        public ColorfulChar(char c, Color color, CharCheck check = CharCheck.Default)
        {
            this.c = c;
            this.color = ColorUtility.ToHtmlStringRGBA(color);
            this.check = check;

            parsed = $"<color=#{this.color}>{c}</color>";
        }

        public void ChangeColor(Color color, CharCheck check = CharCheck.Default)
        {
            this.color = ColorUtility.ToHtmlStringRGBA(color);
            this.check = check;

            parsed = $"<color=#{this.color}>{c}</color>";
        }
    }

    #endregion
}