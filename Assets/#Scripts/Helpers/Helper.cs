using System;
using UnityEngine;

namespace UnityHelper
{
    #region Structs

    [Serializable]
    public struct ColorfulChar
    {
        public char c { get; private set; }
        public string color { get; private set; }
        public string parsed { get; private set; }

        public ColorfulChar(char c, Color color)
        {
            this.c = c;
            this.color = ColorUtility.ToHtmlStringRGBA(color);

            parsed = $"<color=#{this.color}>{c}</color>";
        }

        public void ChangeColor(Color color)
        {
            this.color = ColorUtility.ToHtmlStringRGBA(color);

            parsed = $"<color=#{this.color}>{c}</color>";
        }
    }

    #endregion
}