// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace Avalonia.Media.TextFormatting.Unicode
{
    internal static class LineBreakPairTable
    {
        /// <summary>
        /// Direct break opportunity
        /// </summary>
        public const byte DIBRK = 0;

        /// <summary>
        /// Indirect break opportunity
        /// </summary>
        public const byte INBRK = 1;

        /// <summary>
        /// Indirect break opportunity for combining marks
        /// </summary>
        public const byte CIBRK = 2;

        /// <summary>
        /// Prohibited break for combining marks
        /// </summary>
        public const byte CPBRK = 3;

        /// <summary>
        /// Prohibited break
        /// </summary>
        public const byte PRBRK = 4;

        // Based on example pair table from https://www.unicode.org/reports/tr14/tr14-37.html#Table2
        // - ZWJ special processing for LB8a
        // - CB manually added as per Rule LB20
        public static byte[][] Table { get; } = new byte[][]
        {
              // .         OP     CL     CP     QU     GL     NS     EX     SY     IS     PR     PO     NU     AL     HL     ID     IN     HY     BA     BB     B2     ZW     CM     WJ     H2     H3     JL     JV     JT     RI     EB     EM     ZWJ    CB
              new byte[] { PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, CPBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK, PRBRK }, // OP
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // CL
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // CP
              new byte[] { PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, PRBRK, CIBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK }, // QU
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, PRBRK, CIBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK }, // GL
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // NS
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // EX
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, INBRK, DIBRK, INBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // SY
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // IS
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK }, // PR
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // PO
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // NU
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // AL
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // HL
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // ID
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // IN
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, DIBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // HY
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, DIBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // BA
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, PRBRK, CIBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK }, // BB
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, PRBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // B2
              new byte[] { DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK }, // ZW
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // CM
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, PRBRK, CIBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK, INBRK }, // WJ
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // H2
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // H3
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // JL
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // JV
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // JT
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK, DIBRK, INBRK, DIBRK }, // RI
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, DIBRK }, // EB
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, DIBRK, INBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // EM
              new byte[] { INBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, PRBRK, PRBRK, PRBRK, INBRK, INBRK, INBRK, INBRK, INBRK, DIBRK, INBRK, INBRK, INBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK }, // ZWJ
              new byte[] { DIBRK, PRBRK, PRBRK, INBRK, INBRK, DIBRK, PRBRK, PRBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, PRBRK, CIBRK, PRBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, DIBRK, INBRK, DIBRK } // CB
        };
    }
}
