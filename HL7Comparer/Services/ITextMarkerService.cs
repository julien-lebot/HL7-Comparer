using System;

namespace HL7Comparer.Services
{
    [Flags]
    public enum TextMarkerTypes
    {
        /// <summary>
        /// Use no marker
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Use squiggly underline marker
        /// </summary>
        SquigglyUnderline = 0x001,
        /// <summary>
        /// Normal underline.
        /// </summary>
        NormalUnderline = 0x002,
        /// <summary>
        /// Dotted underline.
        /// </summary>
        DottedUnderline = 0x004,

        Rectangle = 0x008
    }

    public interface ITextMarkerService
    {
        void AddMarker(int offset, int length, string message);
        void Clear();
    }
}