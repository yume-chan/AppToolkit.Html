using AppToolkit.Html.Tokens;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AppToolkit.Html
{
    /// <summary>
    /// 8.2.4 Tokenization
    /// </summary>
    enum TokenizerStates
    {
        /// <summary>
        /// 8.2.4.1 Data state
        /// </summary>
        Data = 1,
        /// <summary>
        /// 8.2.4.3 RCDATA state
        /// </summary>
        RcData = 3,
        /// <summary>
        /// 8.2.4.5 RAWTEXT state
        /// </summary>
        RawText = 5,
        /// <summary>
        /// 8.2.4.8 Tag open state
        /// </summary>
        TagOpen = 8,
        /// <summary>
        /// 8.2.4.9 End tag open state
        /// </summary>
        EndTagOpen,
        /// <summary>
        /// 8.2.4.10 Tag name state
        /// </summary>
        TagName,
        /// <summary>
        /// 8.2.4.11 RCDATA less-than sign state
        /// </summary>
        RcDataLessThanSign,
        /// <summary>
        /// 8.2.4.12 RCDATA end tag open state
        /// </summary>
        RcDataEndTagOpen,
        /// <summary>
        /// 8.2.4.13 RCDATA end tag name state
        /// </summary>
        RcDataEndTagName,
        /// <summary>
        /// 8.2.4.14 RAWTEXT less-than sign state
        /// </summary>
        RawTextLessThanSign,
        /// <summary>
        /// 8.2.4.15 RAWTEXT end tag open state
        /// </summary>
        RawTextEndTagOpen,
        /// <summary>
        /// 8.2.4.16 RAWTEXT end tag name state
        /// </summary>
        RawTextEndTagName,
        /// <summary>
        /// 8.2.4.34 Before attribute name state
        /// </summary>
        BeforeAttributeName = 34,
        /// <summary>
        /// 8.2.4.35 Attribute name state
        /// </summary>
        AttributeName,
        /// <summary>
        /// 8.2.4.36 After attribute name state
        /// </summary>
        AfterAttributeName,
        /// <summary>
        /// 8.2.4.37 Before attribute value state
        /// </summary>
        BeforeAttributeValue,
        /// <summary>
        /// 8.2.4.38 Attribute value (double-quoted) state
        /// </summary>
        AttributeValueDoubleQuoted,
        /// <summary>
        /// 8.2.4.39 Attribute value (single-quoted) state
        /// </summary>
        AttributeValueSingleQuoted,
        /// <summary>
        /// 8.2.4.40 Attribute value (unquoted) state
        /// </summary>
        AttributeValueUnquoted,
        /// <summary>
        /// 8.2.4.42 After attribute value (quoted) state
        /// </summary>
        AfterAttributeValueQuoted = 42,
        /// <summary>
        /// 8.2.4.43 Self-closing start tag state
        /// </summary>
        SelfClosingStartTag,
        /// <summary>
        /// 8.2.4.46 Comment start state
        /// </summary>
        CommentStart = 46,
        /// <summary>
        /// 8.2.4.47 Comment start dash state
        /// </summary>
        CommentStartDash,
        /// <summary>
        /// 8.2.4.48 Comment state
        /// </summary>
        Comment,
        /// <summary>
        /// 8.2.4.49 Comment end dash state
        /// </summary>
        CommentEndDash,
        /// <summary>
        /// 8.2.4.50 Comment end state
        /// </summary>
        CommentEnd,
        /// <summary>
        /// 8.2.4.51 Comment end bang state
        /// </summary>
        CommentEndBang,
        /// <summary>
        /// 8.2.4.52 DOCTYPE state
        /// </summary>
        Doctype,
    }

    class HtmlTokenizer : IEnumerable<Token>
    {
        public string Html { get; }

        public TokenizerStates State { get; set; }

        public HtmlTokenizer(string html)
        {
            Html = html;
            State = TokenizerStates.Data;
        }

        string LastTagName = null;

        static readonly Regex AsciiDigitsRegex = new Regex("[0-9]*");
        static readonly Regex AsciiHexDigitsRegex = new Regex("[0-9a-fA-F]*");

        static readonly Dictionary<int, char> MalformedCharacterReferenceMap = new Dictionary<int, char>()
        {
            [0x00] = '\xFFFD', // REPLACEMENT CHARACTER
            [0x80] = '\x20AC', // EURO SIGN (€)
            [0x82] = '\x201A', // SINGLE LOW-9 QUOTATION MARK (‚) 
            [0x83] = '\x0192', // LATIN SMALL LETTER F WITH HOOK (ƒ) 
            [0x84] = '\x201E', // DOUBLE LOW-9 QUOTATION MARK („) 
            [0x85] = '\x2026', // HORIZONTAL ELLIPSIS (…) 
            [0x86] = '\x2020', // DAGGER (†) 
            [0x87] = '\x2021', // DOUBLE DAGGER (‡) 
            [0x88] = '\x02C6', // MODIFIER LETTER CIRCUMFLEX ACCENT (ˆ) 
            [0x89] = '\x2030', // PER MILLE SIGN (‰) 
            [0x8A] = '\x0160', // LATIN CAPITAL LETTER S WITH CARON (Š) 
            [0x8B] = '\x2039', // SINGLE LEFT-POINTING ANGLE QUOTATION MARK (‹)
            [0x8C] = '\x0152', // LATIN CAPITAL LIGATURE OE (Œ) 
            [0x8E] = '\x017D', // LATIN CAPITAL LETTER Z WITH CARON (Ž) 
            [0x91] = '\x2018', // LEFT SINGLE QUOTATION MARK (‘) 
            [0x92] = '\x2019', // RIGHT SINGLE QUOTATION MARK (’) 
            [0x93] = '\x201C', // LEFT DOUBLE QUOTATION MARK (“) 
            [0x94] = '\x201D', // RIGHT DOUBLE QUOTATION MARK (”) 
            [0x95] = '\x2022', // BULLET (•) 
            [0x96] = '\x0113', // EN DASH (–) 
            [0x97] = '\x0114', // EM DASH (—) 
            [0x98] = '\x02DC', // SMALL TILDE (˜) 
            [0x99] = '\x2122', // TRADE MARK SIGN (™) 
            [0x9A] = '\x0161', // LATIN SMALL LETTER S WITH CARON (š) 
            [0x9B] = '\x203A', // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK (›) 
            [0x9C] = '\x0153', // LATIN SMALL LIGATURE OE (œ) 
            [0x9E] = '\x017E', // LATIN SMALL LETTER Z WITH CARON (ž) 
            [0x9F] = '\x0178', // LATIN CAPITAL LETTER Y WITH DIAERESIS (Ÿ) 
        };
        static readonly Regex MalformedCharacterReferenceRegex =
            new Regex("[\x0001-\x0008\x000D-\x001F\x007F-\x009F\xFDD0-\xFDEF\x000B\xFFFE\xFFFF\x1FFFE\x1FFFF\x2FFFE\x2FFFF\x3FFFE\x3FFFF\x4FFFE\x4FFFF\x5FFFE\x5FFFF\x6FFFE\x6FFFF\x7FFFE\x7FFFF\x8FFFE\x8FFFF\x9FFFE\x9FFFF\xAFFFE\xAFFFF\xBFFFE\xBFFFF\xCFFFE\xCFFFF\xDFFFE\xDFFFF\xEFFFE\xEFFFF\xFFFFE\xFFFFF\x10FFFE\x10FFFF]");

        IEnumerable<char> ConsumeCharacterReference(CharReader reader, params char[] additionalAllowedCharacter)
        {
            /// The behavior depends on the identity of the next character
            /// (the one immediately after the U+0026 AMPERSAND character),
            /// as follows:
            switch (reader.Peek())
            {
                /// -> "tab" (U+0009)
                case '\x9':
                /// -> "LF" (U+000A)
                case '\xA':
                /// -> "FF" (U+000C)
                case '\xC':
                /// -> U+0020 SPACE
                case ' ':
                /// -> U+003C LESS-THAN SIGN
                case '<':
                /// -> U+0026 AMPERSAND
                case '&':
                /// -> EOF
                case '\x3':
                    /// Not a character reference.
                    /// No characters are consumed, and nothing is returned.
                    /// (This is not an error, either.)
                    yield return '&';
                    yield break;
                /// -> "#" (U+0023 NUMBER SIGN)
                case '#':
                    /// Consume the U+0023 NUMBER SIGN.
                    reader.Position++;

                    bool hexMode = false;
                    /// The behavior further depends on the character after the U+0023 NUMBER SIGN:
                    switch (reader.Peek())
                    {
                        /// -> U+0078 LATIN SMALL LETTER X
                        case 'x':
                        /// -> U+0058 LATIN CAPITAL LETTER X
                        case 'X':
                            /// Consume the X.
                            reader.Position++;
                            /// Follow the steps below, but using ASCII hex digits.
                            hexMode = true;
                            break;
                    }

                    /// Consume as many characters as match the range of
                    /// characters given above (ASCII hex digits or ASCII digits).
                    var match = (hexMode ? AsciiHexDigitsRegex : AsciiDigitsRegex).Match(reader.Source, reader.Position);
                    /// If no characters match the range,
                    // /^./ doesn't work when startat parameter is set, so test it.
                    if (!match.Success || match.Index != reader.Position)
                    {
                        /// then don't consume any characters
                        /// (and unconsume the U+0023 NUMBER SIGN character and, if appropriate, the X character).
                        reader.Position -= hexMode ? 2 : 1;
                        /// This is a parse error;
                        ParserErrorLogger.Log();

                        /// nothing is returned.
                        yield return '&';
                        yield break;
                    }

                    reader.Position += match.Length;
                    /// Otherwise, if the next character is a U+003B SEMICOLON,
                    if (reader.Peek() == ';')
                        /// consume that too.
                        reader.Position++;
                    ///  If it isn't, 
                    else
                        /// there is a parse error.
                        ParserErrorLogger.Log();

                    /// If one or more characters match the range,
                    /// then take them all and interpret the string of characters as a number
                    /// (either hexadecimal or decimal as appropriate).
                    var number = int.Parse(match.Value, hexMode ? NumberStyles.AllowHexSpecifier : NumberStyles.None);

                    /// If that number is one of the numbers in the first column of the following table,
                    if (MalformedCharacterReferenceMap.ContainsKey(number))
                    {
                        /// then this is a parse error.
                        ParserErrorLogger.Log();
                        /// Find the row with that number in the first column,
                        /// and return a character token for the Unicode character given in
                        /// the second column of that row.
                        yield return MalformedCharacterReferenceMap[number];
                        yield break;
                    }
                    /// Otherwise, if the number is in the range 0xD800 to 0xDFFF or
                    /// is greater than 0x10FFFF,
                    else if ((number >= 0xD800 && number <= 0xDFFF) ||
                              number >= 0x10FFFF)
                    {
                        /// then this is a parse error.
                        ParserErrorLogger.Log();
                        /// Return a U+FFFD REPLACEMENT CHARACTER character token.
                        yield return '\xFFFD';
                        yield break;
                    }
                    /// Otherwise,
                    else
                    {
                        var c = (char)number;
                        /// Additionally, if the number is in the range 0x0001 to 0x0008, 0x000D to 0x001F,
                        /// 0x007F to 0x009F, 0xFDD0 to 0xFDEF, or is one of 0x000B, 0xFFFE, 0xFFFF, 0x1FFFE,
                        /// 0x1FFFF, 0x2FFFE, 0x2FFFF, 0x3FFFE, 0x3FFFF, 0x4FFFE, 0x4FFFF, 0x5FFFE, 0x5FFFF,
                        /// 0x6FFFE, 0x6FFFF, 0x7FFFE, 0x7FFFF, 0x8FFFE, 0x8FFFF, 0x9FFFE, 0x9FFFF, 0xAFFFE,
                        /// 0xAFFFF, 0xBFFFE, 0xBFFFF, 0xCFFFE, 0xCFFFF, 0xDFFFE, 0xDFFFF, 0xEFFFE, 0xEFFFF,
                        /// 0xFFFFE, 0xFFFFF, 0x10FFFE, or 0x10FFFF,
                        if (MalformedCharacterReferenceRegex.IsMatch(c.ToString()))
                            /// then this is a parse error.
                            ParserErrorLogger.Log();

                        /// return a character token for the Unicode character
                        /// whose code point is that number.
                        yield return c;
                        yield break;
                    }
                default:
                    string lastResult = null;
                    TrieNode<string> current = HtmlEntities.Map;

                    reader.SavePosition();

                    /// Consume the maximum number of characters possible,
                    /// with the consumed characters matching one of the identifiers in the first column
                    /// of the named character references table (in a case-sensitive manner).
                    while ((current = current[reader.Peek()]) != null)
                    {
                        reader.Position++;

                        if (current.Value != null)
                            lastResult = current.Value;

                        if (current.IsTerminal)
                            break;
                    }

                    /// If no match can be made, then no characters are consumed, and nothing is returned.
                    if (lastResult == null)
                    {
                        reader.RestorePosition();

                        yield return '&';
                        yield break;

                        /// In this case, if the characters after the U+0026 AMPERSAND character (&) consist of a sequence of
                        /// one or more alphanumeric ASCII characters followed by a U+003B SEMICOLON character (;),
                        /// then this is a parse error.
                        /// NOTE: This behavior is ignored.
                    }

                    /// If the character reference is being consumed as part of an attribute,
                    /// and the last character matched is not a ";" (U+003B) character,
                    /// and the next character is either a "=" (U+003D) character or an alphanumeric ASCII character,
                    /// then, for historical reasons, all the characters that were matched after the U+0026 AMPERSAND character (&)
                    /// must be unconsumed, and nothing is returned.
                    /// However, if this next character is in fact a "=" (U+003D) character,
                    /// then this is a parse error, because some legacy user agents will misinterpret the markup in those cases.
                    /// NOTE: This behavior is ignored.

                    /// Otherwise, a character reference is parsed. If the last character matched is not a ";" (U+003B) character,
                    if (reader.Source[reader.Position - 1] != ';')
                    {
                        /// there is a parse error.
                        ParserErrorLogger.Log();

                        reader.Position--;
                    }

                    /// Return one or two character tokens for the character(s) corresponding to
                    /// the character reference name (as given by the second column of the named character references table).
                    foreach (var c in lastResult)
                        yield return c;
                    yield break;
            }
        }

        IEnumerable<Token> GetEnumeratorInternal()
        {
            TagToken currentTagToken = null;
            AttributeToken currentAttributeToken = null;
            CommentToken currentCommentToken = null;

            var reader = new CharReader(Html);
            while (true)
            {
                var c = reader.Read();

                ReconsumeCurrent:
                switch (State)
                {
                    case TokenizerStates.Data:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> U+0026 AMPERSAND (&)
                            case '&':
                                /// Switch to the character reference in data state.
                                /// Attempt to consume a character reference, with no additional allowed character.
                                /// If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
                                /// Otherwise, emit the character tokens that were returned.
                                foreach (var cc in ConsumeCharacterReference(reader))
                                    yield return new CharacterToken(cc);
                                break;
                            /// -> "&lt;" (U+003C)
                            case '<':
                                /// Switch to the tag open state.
                                State = TokenizerStates.TagOpen;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Emit the current input character as a character token.
                                yield return new CharacterToken(c);
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Emit an end-of-file token.
                                yield return new EndOfFileToken();
                                yield break;
                            /// -> Anything else
                            default:
                                /// Emit the current input character as a character token.
                                yield return new CharacterToken(c);
                                break;
                        }
                        break;
                    case TokenizerStates.RcData:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> U+0026 AMPERSAND (&)
                            case '&':
                                /// Switch to the character reference in data state.
                                /// Attempt to consume a character reference, with no additional allowed character.
                                /// If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
                                /// Otherwise, emit the character tokens that were returned.
                                foreach (var cc in ConsumeCharacterReference(reader))
                                    yield return new CharacterToken(cc);
                                break;
                            /// -> "&lt;" (U+003C)
                            case '<':
                                State = TokenizerStates.RcDataLessThanSign;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Emit a U+FFFD REPLACEMENT CHARACTER character token.
                                yield return new CharacterToken('\xFFFD');
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Emit an end-of-file token.
                                yield return new EndOfFileToken();
                                yield break;
                            /// -> Anything else
                            default:
                                /// Emit the current input character as a character token.
                                yield return new CharacterToken(c);
                                break;
                        }
                        break;
                    case TokenizerStates.RawText:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "&lt;" (U+003C)
                            case '<':
                                /// Switch to the RAWTEXT less-than sign state.
                                State = TokenizerStates.RawTextLessThanSign;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Emit a U+FFFD REPLACEMENT CHARACTER character token.
                                yield return new CharacterToken('\xFFFD');
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Emit an end-of-file token.
                                yield return new EndOfFileToken();
                                yield break;
                            /// -> Anything else
                            default:
                                /// Emit the current input character as a character token.
                                yield return new CharacterToken(c);
                                break;
                        }
                        break;
                    case TokenizerStates.TagOpen:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "!" (U+0021)
                            case '!':
                                // TODO: Follow documentation.
                                if (reader.SquenceMatches("--"))
                                {
                                    currentCommentToken = new CommentToken();
                                    State = TokenizerStates.CommentStart;
                                }
                                else if (reader.SquenceMatches("DOCTYPE"))
                                    State = TokenizerStates.Doctype;
                                else
                                {
                                    State = TokenizerStates.Data;
                                    yield return new CommentToken(reader.ReadUntil('>', false));
                                }
                                break;
                            /// -> "/" (U+002F)
                            case '/':
                                /// Switch to the end tag open state.
                                State = TokenizerStates.EndTagOpen;
                                break;
                            /// -> "?" (U+003F)
                            case '?':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the bogus comment state.
                                // TODO: Follow documentation.
                                State = TokenizerStates.Data;
                                yield return new CommentToken(reader.ReadUntil('>', false));
                                break;
                            default:
                                /// -> Uppercase ASCII letter / Lowercase ASCII letter
                                if (char.IsLetter(c))
                                {
                                    /// Create a new start tag token,
                                    currentTagToken = new StartTagToken();
                                    /// set its tag name to the lowercase version of the current input character
                                    /// (add 0x0020 to the character's code point),
                                    currentTagToken.TagName.Append(char.ToLower(c));
                                    /// then switch to the tag name state.
                                    /// (Don't emit the token yet; further details will be filled in before it is emitted.)
                                    State = TokenizerStates.TagName;
                                }
                                /// -> Anything else
                                else
                                {
                                    /// Parse error.
                                    ParserErrorLogger.Log();
                                    /// Switch to the data state.
                                    State = TokenizerStates.Data;
                                    /// Emit a U+003C LESS-THAN SIGN character token.
                                    yield return new CharacterToken('<');
                                    /// Reconsume the current input character.
                                    goto ReconsumeCurrent;
                                }
                                break;
                        }
                        break;
                    case TokenizerStates.EndTagOpen:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> ">" (U+003E)
                            case '>':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit a U+003C LESS-THAN SIGN character token and a U+002F SOLIDUS character token.
                                yield return new CharacterToken('<');
                                yield return new CharacterToken('/');
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            default:
                                /// -> Uppercase ASCII letter / Lowercase ASCII letter
                                if (char.IsLetter(c))
                                {
                                    /// Create a new end tag token,
                                    currentTagToken = new EndTagToken();
                                    /// set its tag name to the lowercase version of the current input character
                                    /// (add 0x0020 to the character's code point),
                                    currentTagToken.TagName.Append(char.ToLower(c));
                                    ///  then switch to the tag name state.
                                    /// (Don't emit the token yet; further details will be filled in before it is emitted.)
                                    State = TokenizerStates.TagName;
                                }
                                /// -> Anything else
                                else
                                {
                                    /// Parse error.
                                    ParserErrorLogger.Log();
                                    /// Switch to the bogus comment state.
                                    // TODO: Follow documentation.
                                    State = TokenizerStates.Data;
                                    reader.Position--;
                                    yield return new CommentToken(reader.ReadUntil('>', false));
                                }
                                break;
                        }
                        break;
                    case TokenizerStates.TagName:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// Switch to the before attribute name state.
                                State = TokenizerStates.BeforeAttributeName;
                                break;
                            /// -> "/" (U+002F)
                            case '/':
                                /// Switch to the self-closing start tag state.
                                State = TokenizerStates.SelfClosingStartTag;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the current tag token's tag name.
                                currentTagToken.TagName.Append('\xFFFD');
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Uppercase ASCII letter / Anything else
                            default:
                                /// Append the lowercase version of the current input character
                                /// (add 0x0020 to the character's code point) to the current tag token's tag name.
                                currentTagToken.TagName.Append(char.ToLower(c));
                                break;
                        }
                        break;
                    case TokenizerStates.RcDataLessThanSign:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "/" (U+002F)
                            case '/':
                                /// Set the temporary buffer to the empty string.
                                /// NOTE: Not Used.
                                /// Switch to the RCDATA end tag open state.
                                State = TokenizerStates.RcDataEndTagOpen;
                                break;
                            /// -> Anything else
                            default:
                                /// Switch to the RCDATA state.
                                State = TokenizerStates.RcData;
                                /// Emit a U+003C LESS-THAN SIGN character token.
                                yield return new CharacterToken('<');
                                /// Reconsume the current input character.
                                goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.RcDataEndTagOpen:
                        /// Consume the next input character:
                        /// -> Uppercase ASCII letter / Lowercase ASCII letter
                        if (char.IsLetter(c))
                        {
                            /// Create a new end tag token,
                            currentTagToken = new EndTagToken();
                            /// and set its tag name to the lowercase version of the current input character
                            /// (add 0x0020 to the character's code point).
                            currentTagToken.TagName.Append(char.ToLower(c));
                            /// Append the current input character to the temporary buffer.
                            /// NOTE: Not Used.
                            /// Finally, switch to the RCDATA end tag name state.
                            /// (Don't emit the token yet; further details will be filled in before it is emitted.)
                            State = TokenizerStates.RcDataEndTagName;
                        }
                        /// -> Anything else
                        else
                        {
                            /// Switch to the RCDATA state.
                            State = TokenizerStates.RcData;
                            /// Emit a U+003C LESS-THAN SIGN character token
                            yield return new CharacterToken('<');
                            /// and a U+002F SOLIDUS character token.
                            yield return new CharacterToken('/');
                            /// Reconsume the current input character.
                            goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.RcDataEndTagName:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// If the current end tag token is an appropriate end tag token,
                                if (currentTagToken.TagName.ToString() == LastTagName)
                                    /// then switch to the before attribute name state.
                                    State = TokenizerStates.BeforeAttributeName;
                                else
                                    /// Otherwise, treat it as per the "anything else" entry below.
                                    goto EndTagNameDefault;
                                break;
                            /// -> "/" (U+002F)
                            case '/':
                                /// If the current end tag token is an appropriate end tag token,
                                if (currentTagToken.TagName.ToString() == LastTagName)
                                    /// then switch to the self-closing start tag state.
                                    State = TokenizerStates.SelfClosingStartTag;
                                else
                                    /// Otherwise, treat it as per the "anything else" entry below.
                                    goto EndTagNameDefault;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// If the current end tag token is an appropriate end tag token,
                                if (currentTagToken.TagName.ToString() == LastTagName)
                                {
                                    /// then switch to the data state
                                    State = TokenizerStates.RcData;
                                    /// and emit the current tag token.
                                    yield return currentTagToken;
                                    break;
                                }
                                else
                                    /// Otherwise, treat it as per the "anything else" entry below.
                                    goto EndTagNameDefault;
                            default:
                                /// -> Uppercase ASCII letter / Lowercase ASCII letter
                                if (char.IsLetter(c))
                                {
                                    /// Append the lowercase version of the current input character
                                    /// (add 0x0020 to the character's code point) to the current tag token's tag name.
                                    currentTagToken.TagName.Append(char.ToLower(c));
                                    /// Append the current input character to the temporary buffer.
                                    /// NOTE: Not Used.
                                    break;
                                }

                                /// -> Anything else
                                EndTagNameDefault:
                                /// Switch to the RCDATA state.
                                State = TokenizerStates.RcData;
                                /// Emit a U+003C LESS-THAN SIGN character token,
                                yield return new CharacterToken('<');
                                /// a U+002F SOLIDUS character token,
                                yield return new CharacterToken('/');
                                /// and a character token for each of the characters in the temporary buffer
                                /// (in the order they were added to the buffer).
                                foreach (var ch in currentTagToken.TagName.ToString())
                                    yield return new CharacterToken(ch);
                                /// Reconsume the current input character.
                                goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.RawTextLessThanSign:
                        /// Consume the next input character:
                        /// -> "/" (U+002F)
                        if (c == '/')
                        {
                            /// Set the temporary buffer to the empty string.
                            /// NOTE: Not Used.
                            /// Switch to the RAWTEXT end tag open state.
                            State = TokenizerStates.RawTextEndTagOpen;
                            break;
                        }
                        /// -> Anything else
                        else
                        {
                            /// Switch to the RAWTEXT state.
                            State = TokenizerStates.RawText;
                            /// Emit a U+003C LESS-THAN SIGN character token.
                            yield return new CharacterToken('<');
                            /// Reconsume the current input character.
                            goto ReconsumeCurrent;
                        }
                    case TokenizerStates.RawTextEndTagOpen:
                        /// Consume the next input character:
                        /// -> Uppercase ASCII letter / Lowercase ASCII letter
                        if (char.IsLetter(c))
                        {
                            /// Create a new end tag token,
                            currentTagToken = new EndTagToken();
                            /// and set its tag name to the lowercase version of the current input character
                            /// (add 0x0020 to the character's code point).
                            currentTagToken.TagName.Append(char.ToLower(c));
                            /// Append the current input character to the temporary buffer.
                            /// NOTE: Not Used.
                            /// Finally, switch to the RAWTEXT end tag name state.
                            /// (Don't emit the token yet; further details will be filled in before it is emitted.)
                            State = TokenizerStates.RawTextEndTagName;
                        }
                        /// -> Anything else
                        else
                        {
                            /// Switch to the RAWTEXT state.
                            State = TokenizerStates.RawText;
                            /// Emit a U+003C LESS-THAN SIGN character token
                            yield return new CharacterToken('<');
                            /// and a U+002F SOLIDUS character token.
                            yield return new CharacterToken('/');
                            /// Reconsume the current input character.
                            goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.RawTextEndTagName:
                        // Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// If the current end tag token is an appropriate end tag token,
                                if (currentTagToken.TagName.ToString() == LastTagName)
                                    /// then switch to the before attribute name state.
                                    State = TokenizerStates.BeforeAttributeName;
                                else
                                    /// Otherwise, treat it as per the "anything else" entry below.
                                    goto EndTagNameDefault;
                                break;
                            /// -> "/" (U+002F)
                            case '/':
                                /// If the current end tag token is an appropriate end tag token,
                                if (currentTagToken.TagName.ToString() == LastTagName)
                                    /// then switch to the self-closing start tag state.
                                    State = TokenizerStates.SelfClosingStartTag;
                                else
                                    /// Otherwise, treat it as per the "anything else" entry below.
                                    goto EndTagNameDefault;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// If the current end tag token is an appropriate end tag token,
                                if (currentTagToken.TagName.ToString() == LastTagName)
                                {
                                    /// then switch to the data state
                                    State = TokenizerStates.RawText;
                                    /// and emit the current tag token.
                                    yield return currentTagToken;
                                    break;
                                }
                                else
                                    /// Otherwise, treat it as per the "anything else" entry below.
                                    goto EndTagNameDefault;
                            default:
                                /// -> Uppercase ASCII letter / Lowercase ASCII letter
                                if (char.IsLetter(c))
                                {
                                    /// Append the lowercase version of the current input character 
                                    /// (add 0x0020 to the character's code point) to the current tag token's tag name.
                                    currentTagToken.TagName.Append(char.ToLower(c));
                                    /// Append the current input character to the temporary buffer.
                                    /// NOTE: Not Used.
                                    break;
                                }

                                /// -> Anything else
                                EndTagNameDefault:
                                /// Switch to the RAWTEXT state.
                                State = TokenizerStates.RawText;
                                /// Emit a U+003C LESS-THAN SIGN character token,
                                yield return new CharacterToken('<');
                                /// a U+002F SOLIDUS character token,
                                yield return new CharacterToken('/');
                                /// and a character token for each of the characters in the temporary buffer
                                /// (in the order they were added to the buffer).
                                foreach (var ch in currentTagToken.TagName.ToString())
                                    yield return new CharacterToken(ch);
                                /// Reconsume the current input character.
                                goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.BeforeAttributeName:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// Ignore the character.
                                continue;
                            /// -> "/" (U+002F)
                            case '/':
                                /// Switch to the self-closing start tag state.
                                State = TokenizerStates.SelfClosingStartTag;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Start a new attribute in the current tag token.
                                currentAttributeToken = new AttributeToken();
                                currentTagToken.Attributes.Add(currentAttributeToken);
                                /// Set that attribute's name to a U+FFFD REPLACEMENT CHARACTER character,
                                /// and its value to the empty string.
                                currentAttributeToken.Name.Append('\xFFFD');
                                /// Switch to the attribute name state.
                                State = TokenizerStates.AttributeName;
                                break;
                            /// -> U+0022 QUOTATION MARK (")
                            case '"':
                            /// -> "&lt;" (U+003C)
                            case '<':
                            /// -> "=" (U+003D)
                            case '=':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Treat it as per the "anything else" entry below.
                                goto default;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Start a new attribute in the current tag token.
                                currentAttributeToken = new AttributeToken();
                                currentTagToken.Attributes.Add(currentAttributeToken);
                                /// Set that attribute's name to the current input character,
                                /// and its value to the empty string.
                                currentAttributeToken.Name.Append(c);
                                /// Switch to the attribute name state.
                                State = TokenizerStates.AttributeName;
                                break;
                        }
                        break;
                    case TokenizerStates.AttributeName:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// When the user agent leaves the attribute name state
                                /// (and before emitting the tag token, if appropriate),
                                /// the complete attribute's name must be compared to
                                /// the other attributes on the same token;
                                /// if there is already an attribute on the token with the exact same name,
                                /// then this is a parse error and the new attribute must be removed from the token.
                                if (currentTagToken.Attributes.Where(x => x.Name.ToString() == currentAttributeToken.Name.ToString()).Count() > 1)
                                    currentTagToken.Attributes.Remove(currentAttributeToken);
                                /// Switch to the after attribute name state.
                                State = TokenizerStates.AfterAttributeName;
                                break;
                            /// -> "/" (U+002F)
                            case '/':
                                /// When the user agent leaves the attribute name state
                                /// (and before emitting the tag token, if appropriate),
                                /// the complete attribute's name must be compared to
                                /// the other attributes on the same token;
                                /// if there is already an attribute on the token with the exact same name,
                                /// then this is a parse error and the new attribute must be removed from the token.
                                if (currentTagToken.Attributes.Where(x => x.Name.ToString() == currentAttributeToken.Name.ToString()).Count() > 1)
                                    currentTagToken.Attributes.Remove(currentAttributeToken);
                                /// Switch to the self-closing start tag state.
                                State = TokenizerStates.SelfClosingStartTag;
                                break;
                            /// -> "=" (U+003D)
                            case '=':
                                /// When the user agent leaves the attribute name state
                                /// (and before emitting the tag token, if appropriate),
                                /// the complete attribute's name must be compared to
                                /// the other attributes on the same token;
                                /// if there is already an attribute on the token with the exact same name,
                                /// then this is a parse error and the new attribute must be removed from the token.
                                if (currentTagToken.Attributes.Where(x => x.Name.ToString() == currentAttributeToken.Name.ToString()).Count() > 1)
                                    currentTagToken.Attributes.Remove(currentAttributeToken);
                                /// Switch to the before attribute value state.
                                State = TokenizerStates.BeforeAttributeValue;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// When the user agent leaves the attribute name state
                                /// (and before emitting the tag token, if appropriate),
                                /// the complete attribute's name must be compared to
                                /// the other attributes on the same token;
                                /// if there is already an attribute on the token with the exact same name,
                                /// then this is a parse error and the new attribute must be removed from the token.
                                if (currentTagToken.Attributes.Where(x => x.Name.ToString() == currentAttributeToken.Name.ToString()).Count() > 1)
                                    currentTagToken.Attributes.Remove(currentAttributeToken);
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's name.
                                currentAttributeToken.Name.Append('\xFFFD');
                                break;
                            /// -> U+0022 QUOTATION MARK (")
                            case '"':
                            /// -> "'" (U+0027)
                            case '\'':
                            /// -> "&lt;" (U+003C)
                            case '<':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Treat it as per the "anything else" entry below.
                                goto default;
                            /// -> EOF
                            case '\x3':
                                /// When the user agent leaves the attribute name state
                                /// (and before emitting the tag token, if appropriate),
                                /// the complete attribute's name must be compared to
                                /// the other attributes on the same token;
                                /// if there is already an attribute on the token with the exact same name,
                                /// then this is a parse error and the new attribute must be removed from the token.
                                if (currentTagToken.Attributes.Where(x => x.Name.ToString() == currentAttributeToken.Name.ToString()).Count() > 1)
                                    currentTagToken.Attributes.Remove(currentAttributeToken);
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Uppercase ASCII letter / Anything else
                            default:
                                /// Append the lowercase version of the current input character
                                /// (add 0x0020 to the character's code point) to the current attribute's name.
                                currentAttributeToken.Name.Append(char.ToLower(c));
                                break;
                        }
                        break;
                    case TokenizerStates.AfterAttributeName:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// Ignore the character.
                                continue;
                            /// -> "/" (U+002F)
                            case '/':
                                /// Switch to the self-closing start tag state.
                                State = TokenizerStates.SelfClosingStartTag;
                                break;
                            /// -> "=" (U+003D)
                            case '=':
                                /// Switch to the before attribute value state.
                                State = TokenizerStates.BeforeAttributeValue;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Start a new attribute in the current tag token.
                                currentAttributeToken = new AttributeToken();
                                currentTagToken.Attributes.Add(currentAttributeToken);
                                /// Set that attribute's name to a U+FFFD REPLACEMENT CHARACTER character,
                                /// and its value to the empty string.
                                currentAttributeToken.Name.Append('\xFFFD');
                                /// Switch to the attribute name state.
                                State = TokenizerStates.AttributeName;
                                break;
                            /// -> U+0022 QUOTATION MARK (")
                            case '"':
                            /// -> "'" (U+0027)
                            case '\'':
                            /// -> "&lt;" (U+003C)
                            case '<':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Treat it as per the "anything else" entry below.
                                goto default;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Uppercase ASCII letter / Anything else
                            default:
                                /// Start a new attribute in the current tag token.
                                currentAttributeToken = new AttributeToken();
                                currentTagToken.Attributes.Add(currentAttributeToken);
                                /// Set that attribute's name to the current input character,
                                /// and its value to the empty string.
                                currentAttributeToken.Name.Append(char.ToLower(c));
                                /// Switch to the attribute name state.
                                State = TokenizerStates.AttributeName;
                                break;
                        }
                        break;
                    case TokenizerStates.BeforeAttributeValue:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// Ignore the character.
                                continue;
                            /// -> U+0022 QUOTATION MARK (")
                            case '"':
                                /// Switch to the attribute value (double-quoted) state.
                                State = TokenizerStates.AttributeValueDoubleQuoted;
                                break;
                            /// -> U+0026 AMPERSAND (&)
                            case '&':
                                /// Switch to the attribute value (unquoted) state.
                                State = TokenizerStates.AttributeValueUnquoted;
                                /// Reconsume the current input character.
                                goto ReconsumeCurrent;
                            /// -> "'" (U+0027)
                            case '\'':
                                /// Switch to the attribute value (single-quoted) state.
                                State = TokenizerStates.AttributeValueSingleQuoted;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                                currentAttributeToken.Value.Append('\xFFFD');
                                /// Switch to the attribute value (unquoted) state.
                                State = TokenizerStates.AttributeValueUnquoted;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> "&lt;" (U+003C)
                            case '<':
                            /// -> "=" (U+003D)
                            case '=':
                            /// -> "`" (U+0060)
                            case '`':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Treat it as per the "anything else" entry below.
                                goto default;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append the current input character to the current attribute's value.
                                currentAttributeToken.Value.Append(c);
                                /// Switch to the attribute value (unquoted) state.
                                State = TokenizerStates.AttributeValueUnquoted;
                                break;
                        }
                        break;
                    case TokenizerStates.AttributeValueDoubleQuoted:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> U+0022 QUOTATION MARK (")
                            case '"':
                                /// Switch to the after attribute value (quoted) state.
                                State = TokenizerStates.AfterAttributeValueQuoted;
                                break;
                            /// -> U+0026 AMPERSAND (&)
                            case '&':
                                /// Switch to the character reference in attribute value state,
                                /// with the additional allowed character being U+0022 QUOTATION MARK (").

                                /// Attempt to consume a character reference.
                                /// If nothing is returned, append a U+0026 AMPERSAND character (&) to the current attribute's value.
                                /// Otherwise, append the returned character tokens to the current attribute's value.

                                /// Attempt to consume a character reference, with no additional allowed character.
                                /// If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
                                /// Otherwise, emit the character tokens that were returned.
                                foreach (var cc in ConsumeCharacterReference(reader, '"'))
                                    currentAttributeToken.Value.Append(c);
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                                currentAttributeToken.Value.Append('\xFFFD');
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append the current input character to the current attribute's value.
                                currentAttributeToken.Value.Append(c);
                                break;
                        }
                        break;
                    case TokenizerStates.AttributeValueSingleQuoted:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "'" (U+0027)
                            case '\'':
                                /// Switch to the after attribute value (quoted) state.
                                State = TokenizerStates.AfterAttributeValueQuoted;
                                break;
                            /// -> U+0026 AMPERSAND (&)
                            case '&':
                                /// Switch to the character reference in attribute value state,
                                /// with the additional allowed character being "'" (U+0027).
                                /// 
                                /// Attempt to consume a character reference.
                                /// If nothing is returned, append a U+0026 AMPERSAND character (&) to the current attribute's value.
                                /// Otherwise, append the returned character tokens to the current attribute's value.

                                /// Attempt to consume a character reference, with no additional allowed character.
                                /// If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
                                /// Otherwise, emit the character tokens that were returned.
                                foreach (var cc in ConsumeCharacterReference(reader, '\''))
                                    currentAttributeToken.Value.Append(c);
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                                currentAttributeToken.Value.Append('\xFFFD');
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append the current input character to the current attribute's value.
                                currentAttributeToken.Value.Append(c);
                                break;
                        }
                        break;
                    case TokenizerStates.AttributeValueUnquoted:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// Switch to the before attribute name state.
                                State = TokenizerStates.BeforeAttributeName;
                                break;
                            /// -> U+0026 AMPERSAND (&)
                            case '&':
                                /// Switch to the character reference in attribute value state,
                                /// with the additional allowed character being ">" (U+003E).
                                /// 
                                /// Attempt to consume a character reference.
                                /// If nothing is returned, append a U+0026 AMPERSAND character (&) to the current attribute's value.
                                /// Otherwise, append the returned character tokens to the current attribute's value.

                                /// Attempt to consume a character reference, with no additional allowed character.
                                /// If nothing is returned, emit a U+0026 AMPERSAND character (&) token.
                                /// Otherwise, emit the character tokens that were returned.
                                foreach (var cc in ConsumeCharacterReference(reader, '>'))
                                    currentAttributeToken.Value.Append(c);
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the current attribute's value.
                                currentAttributeToken.Value.Append('\xFFFD');
                                break;
                            /// -> U+0022 QUOTATION MARK (")
                            case '"':
                            /// -> "'" (U+0027)
                            case '\'':
                            /// -> "&lt;" (U+003C)
                            case '<':
                            /// -> "=" (U+003D)
                            case '=':
                            /// -> "`" (U+0060)
                            case '`':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Treat it as per the "anything else" entry below.
                                goto default;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append the current input character to the current attribute's value.
                                currentAttributeToken.Value.Append(c);
                                break;
                        }
                        break;
                    case TokenizerStates.AfterAttributeValueQuoted:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "tab" (U+0009)
                            case '\x9':
                            /// -> "LF" (U+000A)
                            case '\xA':
                            /// -> "FF" (U+000C)
                            case '\xC':
                            /// -> U+0020 SPACE
                            case ' ':
                                /// Switch to the before attribute name state.
                                State = TokenizerStates.BeforeAttributeName;
                                break;
                            /// -> "/" (U+002F)
                            case '/':
                                /// Switch to the self-closing start tag state.
                                State = TokenizerStates.SelfClosingStartTag;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the before attribute name state.
                                State = TokenizerStates.BeforeAttributeName;
                                /// Reconsume the character.
                                goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.SelfClosingStartTag:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> ">" (U+003E)
                            case '>':
                                /// Set the self-closing flag of the current tag token.
                                currentTagToken.IsSelfClosing = true;
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the current tag token.
                                yield return currentTagToken;
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the before attribute name state.
                                State = TokenizerStates.BeforeAttributeName;
                                /// Reconsume the character.
                                goto ReconsumeCurrent;
                        }
                        break;
                    case TokenizerStates.CommentStart:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "-" (U+002D)
                            case '-':
                                /// Switch to the comment start dash state.
                                State = TokenizerStates.CommentStartDash;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                                currentCommentToken.Data.Append('\xFFFD');
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append the current input character to the comment token's data.
                                currentCommentToken.Data.Append(c);
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                        }
                        break;
                    case TokenizerStates.CommentStartDash:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "-" (U+002D)
                            case '-':
                                /// Switch to the comment end state
                                State = TokenizerStates.CommentEnd;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a "-" (U+002D) character
                                /// and a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                                currentCommentToken.Data.Append("-\xFFFD");
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append a "-" (U+002D) character
                                /// and the current input character to the comment token's data.
                                currentCommentToken.Data.Append('-').Append(c);
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                        }
                        break;
                    case TokenizerStates.Comment:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "-" (U+002D)
                            case '-':
                                /// Switch to the comment end dash state
                                State = TokenizerStates.CommentEndDash;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                                currentCommentToken.Data.Append('\xFFFD');
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append the current input character to the comment token's data.
                                currentCommentToken.Data.Append(c);
                                break;
                        }
                        break;
                    case TokenizerStates.CommentEndDash:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "-" (U+002D)
                            case '-':
                                /// Switch to the comment end state
                                State = TokenizerStates.CommentEnd;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a "-" (U+002D) character
                                /// and a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                                currentCommentToken.Data.Append("-\xFFFD");
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append a "-" (U+002D) character
                                /// and the current input character to the comment token's data.
                                currentCommentToken.Data.Append('-').Append(c);
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                        }
                        break;
                    case TokenizerStates.CommentEnd:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append two "-" (U+002D) characters and
                                /// a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                                currentCommentToken.Data.Append("--\xFFFD");
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                            /// -> "!" (U+0021)
                            case '!':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the comment end bang state.
                                State = TokenizerStates.CommentEndBang;
                                break;
                            /// -> "-" (U+002D)
                            case '-':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append a "-" (U+002D) character to the comment token's data.
                                currentCommentToken.Data.Append("--");
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append two "-" (U+002D) characters
                                /// and the current input character to the comment token's data.
                                currentCommentToken.Data.Append("--").Append(c);
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                        }
                        break;
                    case TokenizerStates.CommentEndBang:
                        /// Consume the next input character:
                        switch (c)
                        {
                            /// -> "-" (U+002D)
                            case '-':
                                /// Append two "-" (U+002D) characters
                                /// and a "!" (U+0021) character to the comment token's data.
                                currentCommentToken.Data.Append("--!");
                                /// Switch to the comment end dash state.
                                State = TokenizerStates.CommentEndDash;
                                break;
                            /// -> ">" (U+003E)
                            case '>':
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                break;
                            /// -> U+0000 NULL
                            case '\0':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Append two "-" (U+002D) characters,
                                /// a "!" (U+0021) character,
                                /// and a U+FFFD REPLACEMENT CHARACTER character to the comment token's data.
                                currentCommentToken.Data.Append("--!\xFFFD");
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                            /// -> EOF
                            case '\x3':
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Switch to the data state.
                                State = TokenizerStates.Data;
                                /// Emit the comment token.
                                yield return currentCommentToken;
                                /// Reconsume the EOF character.
                                goto ReconsumeCurrent;
                            /// -> Anything else
                            default:
                                /// Append two "-" (U+002D) characters,
                                /// a "!" (U+0021) character,
                                /// and the current input character to the comment token's data.
                                currentCommentToken.Data.Append("--!").Append(c);
                                /// Switch to the comment state.
                                State = TokenizerStates.Comment;
                                break;
                        }
                        break;
                }
            }
        }

        static readonly bool ReassembleText = false;

        public IEnumerator<Token> GetEnumerator()
        {
            var textBuilder = new StringBuilder(10);
            foreach (var token in GetEnumeratorInternal())
                switch (token.Type)
                {
                    case TokenType.Character:
                        if (ReassembleText)
                            textBuilder.Append(((CharacterToken)token).Data);
                        else
                            yield return token;
                        break;
                    case TokenType.StartTag:
                        LastTagName = ((StartTagToken)token).TagName.ToString();
                        goto default;
                    case TokenType.EndTag:
                        LastTagName = null;
                        goto default;
                    default:
                        if (ReassembleText &&
                            textBuilder.Length != 0)
                        {
                            yield return new TextToken(textBuilder.ToString());
                            textBuilder.Clear();
                        }

                        yield return token;
                        break;
                }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
