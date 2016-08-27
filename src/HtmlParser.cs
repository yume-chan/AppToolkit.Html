using AppToolkit.Html.Interfaces;
using AppToolkit.Html.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppToolkit.Html
{
    static class Extensions
    {
        public static string TakeAndClear(this StringBuilder @this)
        {
            var result = @this.ToString().Trim();
            @this.Clear();
            return result;
        }

        public static void Push<T>(this List<T> @this, T item) => @this.Add(item);

        public static T Peek<T>(this List<T> @this) => @this[@this.Count - 1];

        public static T Pop<T>(this List<T> @this)
        {
            var index = @this.Count - 1;
            var result = @this[index];
            @this.RemoveAt(index);
            return result;
        }
    }

    public class HtmlParser
    {
        private readonly string Input;

        private readonly bool IsFragment;

        private HtmlParser(string input, bool isFragment)
        {
            Input = input ?? string.Empty;
            IsFragment = isFragment;
        }

        /// <summary>
        /// Complete void elements in HTML 5.
        /// <c>http://www.w3.org/TR/html-markup/syntax.html#void-elements</c>
        /// </summary>
        private static readonly HashSet<string> VoidElements = new HashSet<string>() { "area", "base", "br", "col", "command", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr" };

        /// <summary>
        /// 8.2.5.4 The rules for parsing tokens in HTML content
        /// </summary>
        enum InsertionMode
        {
            /// <summary>
            /// 8.2.5.4.1 The "initial" insertion mode
            /// </summary>
            Initial = 1,
            /// <summary>
            /// 8.2.5.4.2 The "before html" insertion mode
            /// </summary>
            BeforeHtml,
            /// <summary>
            /// 8.2.5.4.3 The "before head" insertion mode
            /// </summary>
            BeforeHead,
            /// <summary>
            /// 8.2.5.4.4 The "in head" insertion mode
            /// </summary>
            InHead,
            /// <summary>
            /// 8.2.5.4.5 The "in head noscript" insertion mode
            /// </summary>
            InHeadNoScript,
            /// <summary>
            /// 8.2.5.4.6 The "after head" insertion mode
            /// </summary>
            AfterHead,
            /// <summary>
            /// 8.2.5.4.7 The "in body" insertion mode
            /// </summary>
            InBody,
            /// <summary>
            /// 8.2.5.4.8 The "text" insertion mode
            /// </summary>
            Text,
            ///// <summary>
            ///// 8.2.5.4.9 The "in table" insertion mode
            ///// </summary>
            //InTable,
            ///// <summary>
            ///// 8.2.5.4.10 The "in table text" insertion mode
            ///// </summary>
            //InTableText,
            ///// <summary>
            ///// 8.2.5.4.11 The "in caption" insertion mode
            ///// </summary>
            //InCaption,
            ///// <summary>
            ///// 
            ///// </summary>
            //InColumnGroup,
            ///// <summary>
            ///// 
            ///// </summary>
            //InTableBody,
            ///// <summary>
            ///// 
            ///// </summary>
            //InRow,
            ///// <summary>
            ///// 
            ///// </summary>
            //InCell,
            ///// <summary>
            ///// 
            ///// </summary>
            //InSelect,
            ///// <summary>
            ///// 
            ///// </summary>
            //InSelectInTable,
            /// <summary>
            /// 8.2.5.4.18 The "in template" insertion mode
            /// </summary>
            InTemplate,
            /// <summary>
            /// 8.2.5.4.19 The "after body" insertion mode
            /// </summary>
            AfterBody = 19,
            /// <summary>
            /// 8.2.5.4.20 The "in frameset" insertion mode
            /// </summary>
            InFrameSet,
            ///// <summary>
            ///// 8.2.5.4.21 The "after frameset" insertion mode
            ///// </summary>
            //AfterFrameSet,
            /// <summary>
            /// 8.2.5.4.22 The "after after body" insertion mode
            /// </summary>
            AfterAfterBody,
            ///// <summary>
            ///// 8.2.5.4.23 The "after after frameset" insertion mode
            ///// </summary>
            //AfterAfterFrameSet,
        }

        private class ModeManager
        {
            private InsertionMode value = InsertionMode.Initial;
            private InsertionMode? temp = null;

            public InsertionMode Value => temp ?? value;

            private InsertionMode save;

            public void Switch(InsertionMode mode)
            {
                value = mode;
            }

            public void SaveAndSwitch(InsertionMode mode)
            {
                save = value;
                value = mode;
            }

            public void Restore()
            {
                value = save;
            }

            public void SwitchForReprocess(InsertionMode mode)
            {
                temp = mode;
            }

            public void ReprocessCompleted()
            {
                temp = null;
            }
        }

        private readonly ModeManager Mode = new ModeManager();

        readonly List<Element> OpenElementsStack = new List<Element>();
        readonly List<Element> ActiveFormattingElements = new List<Element>();
        class Marker : Element { public Marker() : base(null, null) { } }

        readonly HtmlDocument document = (HtmlDocument)new Document();

        bool fosterParenting = false;
        bool scripting = false;
        bool frameSetOk = true;

        class Location
        {
            public Node Parent { get; }

            public Node Child { get; }

            public Location(Node parent, Node child)
            {
                Parent = parent;
                Child = child;
            }

            public Node GetPreviousNode()
            {
                if (Child == null)
                    return Parent.LastChild;
                else
                    return Child.PreviousSibling;
            }

            public void InsertBefore(Node node)
            {
                Parent.InsertBefore(node, Child);
            }
        }

        static readonly HashSet<string> TableTagNames = new HashSet<string>() { "table", "tbody", "tfoot", "thead", "tr" };

        Location FindAppropriatePlaceForInsertingNode(Element overrideTarget = null)
        {
            /// The appropriate place for inserting a node,
            /// optionally using a particular override target,
            /// is the position in an element returned by running the following steps:

            Element target;
            /// 1. If there was an override target specified,
            if (overrideTarget != null)
                /// then let target be the override target.
                target = overrideTarget;
            else
                /// Otherwise, let target be the current node.
                target = OpenElementsStack.Peek();

            /// 2. Determine the adjusted insertion location using the first matching steps from the following list:
            Location adjustedInsertionLocation;
            /// -> If foster parenting is enabled and target is a table, tbody, tfoot, thead, or tr element
            if (fosterParenting &&
                TableTagNames.Contains(target.TagName.ToLower()))
            {
                /// Run these substeps:
                /// 1. Let last template be the last template element in the stack of open elements, if any.

                /// 2. Let last table be the last table element in the stack of open elements, if any.

                /// 3. If there is a last template and either there is no last table, or there is one, but last template is lower (more recently added) than last table in the stack of open elements, then: let adjusted insertion location be inside last template's template contents, after its last child (if any), and abort these substeps.

                /// 4. If there is no last table, then let adjusted insertion location be inside the first element in the stack of open elements (the html element), after its last child (if any), and abort these substeps. (fragment case)

                /// 5. If last table has a parent element, then let adjusted insertion location be inside last table's parent element, immediately before last table, and abort these substeps.

                /// 6. Let previous element be the element immediately above last table in the stack of open elements.

                /// 7. Let adjusted insertion location be inside previous element, after its last child (if any).
                adjustedInsertionLocation = null;
            }
            /// -> Otherwise
            else
                /// Let adjusted insertion location be inside target, after its last child (if any).
                adjustedInsertionLocation = new Location(target, null);

            /// 3. If the adjusted insertion location is inside a template element,
            if (adjustedInsertionLocation.Parent is HtmlTemplateElement template)
                /// let it instead be inside the template element's template contents,
                /// after its last child (if any).
                adjustedInsertionLocation = new Location(template.Content, null);

            /// 4. Return the adjusted insertion location.
            return adjustedInsertionLocation;
        }

        Element CreateElement(StartTagToken token, Document intendedParent = null)
        {
            var element = document.CreateElement(token.TagName);
            foreach (var a in token.Attributes)
                element.SetAttribute(a.Name.ToString(), a.Value.ToString());
            return element;
        }

        Element InsertElement(StartTagToken token, Document intendedParent = null)
        {
            var location = FindAppropriatePlaceForInsertingNode();
            var element = CreateElement(token, intendedParent);
            location.Parent.InsertBefore(element, location.Child);
            OpenElementsStack.Push(element);
            return element;
        }

        Element InsertElement(string tagName, Document intendedParent = null)
        {
            var location = FindAppropriatePlaceForInsertingNode();
            var element = (intendedParent ?? document).CreateElement(tagName);
            location.InsertBefore(element);
            OpenElementsStack.Push(element);
            return element;
        }

        void InsertCharacter(char c)
        {
            /// Let the adjusted insertion location be the appropriate place for inserting a node.
            var adjustedInsertionLocation = FindAppropriatePlaceForInsertingNode();

            /// If the adjusted insertion location is in a Document node,
            if (adjustedInsertionLocation.Parent is Document)
                /// then abort these steps. 
                return;

            Text text = adjustedInsertionLocation.GetPreviousNode() as Text;
            /// If there is a Text node immediately before the adjusted insertion location,
            if (text != null)
                /// then append data to that Text node's data.
                text.AppendData(c.ToString());
            else
            {
                /// Otherwise, create a new Text node whose data is data and
                /// whose ownerDocument is the same as that of the element
                /// in which the adjusted insertion location finds itself,
                text = adjustedInsertionLocation.Parent.OwnerDocument.CreateTextNode(c.ToString());
                /// and insert the newly created node at the adjusted insertion location.
                adjustedInsertionLocation.Parent.InsertBefore(text, adjustedInsertionLocation.Child);
            }
        }

        static readonly HashSet<string> ScopeElementType =
            new HashSet<string>() { "applet", "caption", "html", "table", "td", "th", "marquee", "object", "template" };

        bool HasElementInScope(string targetNode, params string[] extraList)
        {
            foreach (var node in Enumerable.Reverse(OpenElementsStack))
            {
                string tagName = node.TagName.ToLower();

                if (tagName == targetNode)
                    return true;

                if (ScopeElementType.Contains(tagName) ||
                    extraList.Contains(tagName))
                    return false;
            }

            throw new Exception("Unreachable");
        }

        bool HasElementsInScope(params string[] targetNodes)
        {
            foreach (var node in Enumerable.Reverse(OpenElementsStack))
            {
                string tagName = node.TagName.ToLower();

                if (targetNodes.Contains(tagName))
                    return true;

                if (ScopeElementType.Contains(tagName))
                    return false;
            }
            return true;
        }

        bool HasElementInButtonScope(string targetNode) =>
            HasElementInScope(targetNode, "button");

        static readonly HashSet<string> ImpiledEndTagList =
            new HashSet<string>() { "dd", "dt", "li", "option", "optgroup", "p", "rb", "rp", "rt", "rtc" };

        void GenerateImpiledEndTags(params string[] exceptFor)
        {
            while (true)
            {
                var tagName = OpenElementsStack.Peek().TagName.ToLower();
                if (ImpiledEndTagList.Contains(tagName) && !exceptFor.Contains(tagName))
                    OpenElementsStack.Pop();
                else
                    break;
            }
        }

        static readonly HashSet<string> BodyEndAllowedUnclosedTagList =
            new HashSet<string>() { "dd", "dt", "li", "option", "optgroup", "p", "rb", "rp", "rt", "rtc",
                "tbody", "td", "tfoot", "th", "thead", "tr", "body", "html" };

        void ClosePElement()
        {
            /// Generate implied end tags, except for p elements.
            GenerateImpiledEndTags(exceptFor: "p");

            /// If the current node is not a p element,
            if (OpenElementsStack.Peek().TagName.ToLower() != "p")
                /// then this is a parse error.
                ParserErrorLogger.Log();

            /// Pop elements from the stack of open elements until a p element has been popped from the stack.
            while (OpenElementsStack.Pop().TagName.ToLower() != "p") ;
        }

        static readonly HashSet<string> HeaderTagList = new HashSet<string>() { "h1", "h2", "h3", "h4", "h5", "h6" };

        static readonly HashSet<string> SpecialTagList = new HashSet<string>() { "address", "applet", "area", "article", "aside", "base", "basefont", "bgsound", "blockquote", "body", "br", "button", "caption", "center", "col", "colgroup", "dd", "details", "dir", "div", "dl", "dt", "embed", "fieldset", "figcaption", "figure", "footer", "form", "frame", "frameset", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hgroup", "hr", "html", "iframe", "img", "input", "isindex", "li", "link", "listing", "main", "marquee", "meta", "nav", "noembed", "noframes", "noscript", "object", "ol", "p", "param", "plaintext", "pre", "script", "section", "select", "source", "style", "summary", "table", "tbody", "td", "template", "textarea", "tfoot", "th", "thead", "title", "tr", "track", "ul", "wbr", "xmp" };

        private HtmlDocument Parse()
        {
            var tokenizer = new HtmlTokenizer(Input);

            foreach (var token in tokenizer)
            {
                Mode.ReprocessCompleted();

                ReprocessCurrent:
                switch (Mode.Value)
                {
                    case InsertionMode.Initial:
                        switch (token.Type)
                        {
                            case TokenType.Character:
                                switch (((CharacterToken)token).Data)
                                {
                                    /// A character token that is one of
                                    /// U+0009 CHARACTER TABULATION,
                                    case '\x9':
                                    /// "LF" (U+000A),
                                    case '\xA':
                                    /// "FF" (U+000C),
                                    case '\xC':
                                    /// "CR" (U+000D),
                                    case '\xD':
                                    /// or U+0020 SPACE
                                    case ' ':
                                        /// Ignore the token.
                                        continue;
                                    default:
                                        goto InitialDefault;
                                }
                            /// -> A comment token
                            case TokenType.Comment:
                                /// Insert a comment as the last child of the Document object.
                                InsertComment((CommentToken)token, new Location(document, null));
                                break;
                            case TokenType.Doctype:
                                // TODO:
                                /// Then, switch the insertion mode to "before html".
                                Mode.Switch(InsertionMode.BeforeHtml);
                                break;
                            /// -> Anything else
                            default:
                                InitialDefault:
                                /// If the document is not an iframe srcdoc document,

                                /// then this is a parse error;
                                ParserErrorLogger.Log();

                                /// set the Document to quirks mode.
                                document.State.Mode = DocumentMode.Quirks;

                                /// In any case, switch the insertion mode to "before html",
                                Mode.Switch(InsertionMode.BeforeHtml);
                                /// then reprocess the token.
                                goto ReprocessCurrent;
                        }
                        break;
                    case InsertionMode.BeforeHtml:
                        switch (token.Type)
                        {
                            /// -> A DOCTYPE token
                            case TokenType.Doctype:
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Ignore the token.
                                continue;
                            /// -> A comment token
                            case TokenType.Comment:
                                /// Insert a comment as the last child of the Document object.
                                InsertComment((CommentToken)token, new Location(document, null));
                                break;
                            case TokenType.Character:
                                switch (((CharacterToken)token).Data)
                                {
                                    /// A character token that is one of
                                    /// U+0009 CHARACTER TABULATION,
                                    case '\x9':
                                    /// "LF" (U+000A),
                                    case '\xA':
                                    /// "FF" (U+000C),
                                    case '\xC':
                                    /// "CR" (U+000D),
                                    case '\xD':
                                    /// or U+0020 SPACE
                                    case ' ':
                                        /// Ignore the token.
                                        continue;
                                    default:
                                        goto BeforeHtmlDefault;
                                }
                            case TokenType.StartTag:
                                /// -> A start tag whose tag name is "html"
                                if (((StartTagToken)token).TagName == "html")
                                {
                                    /// Create an element for the token in the HTML namespace,
                                    /// with the Document as the intended parent.
                                    var html = document.CreateElement("html");

                                    /// Append it to the Document object.
                                    document.AppendChild(html);

                                    /// Put this element in the stack of open elements.
                                    OpenElementsStack.Push(html);

                                    /// If the Document is being loaded as part of navigation of a browsing context,

                                    /// then: if the newly created element has a manifest attribute whose value is not the empty string,

                                    /// then resolve the value of that attribute to an absolute URL, relative to the newly created element,

                                    /// and if that is successful,

                                    /// run the application cache selection algorithm with the result of
                                    /// applying the URL serializer algorithm to the resulting parsed URL with
                                    /// the exclude fragment flag set;

                                    /// otherwise, if there is no such attribute, or its value is the empty string, or resolving its value fails,

                                    /// run the application cache selection algorithm with no manifest.

                                    /// The algorithm must be passed the Document object.

                                    /// Switch the insertion mode to "before head".
                                    Mode.Switch(InsertionMode.BeforeHead);
                                    break;
                                }
                                else
                                    goto BeforeHtmlDefault;
                            case TokenType.EndTag:
                                switch (((EndTagToken)token).TagName)
                                {
                                    /// -> An end tag whose tag name is one of:
                                    /// "head", "body", "html", "br"
                                    case "head":
                                    case "html":
                                    case "body":
                                    case "br":
                                        /// Act as described in the "anything else" entry below.
                                        goto BeforeHtmlDefault;
                                    /// -> Any other end tag
                                    default:
                                        /// Parse error.
                                        ParserErrorLogger.Log();
                                        /// Ignore the token.
                                        continue;
                                }
                            /// -> Anything else
                            default:
                                BeforeHtmlDefault:
                                {
                                    /// Create an html element whose ownerDocument is the Document object.
                                    var html = document.CreateElement("html");
                                    document.DocumentElement = html;

                                    /// Append it to the Document object.
                                    document.AppendChild(html);

                                    /// Put this element in the stack of open elements.
                                    OpenElementsStack.Push(html);

                                    /// If the Document is being loaded as part of navigation of a browsing context,

                                    /// then: run the application cache selection algorithm with no manifest, passing it the Document object.

                                    /// Switch the insertion mode to "before head",
                                    Mode.Switch(InsertionMode.BeforeHead);

                                    /// then reprocess the token.
                                    goto ReprocessCurrent;
                                }
                        }
                        break;
                    case InsertionMode.BeforeHead:
                        switch (token.Type)
                        {
                            case TokenType.Character:
                                switch (((CharacterToken)token).Data)
                                {
                                    /// A character token that is one of
                                    /// U+0009 CHARACTER TABULATION,
                                    case '\x9':
                                    /// "LF" (U+000A),
                                    case '\xA':
                                    /// "FF" (U+000C),
                                    case '\xC':
                                    /// "CR" (U+000D),
                                    case '\xD':
                                    /// or U+0020 SPACE
                                    case ' ':
                                        /// Ignore the token.
                                        continue;
                                    default:
                                        goto BeforeHeadDefault;
                                }
                            /// -> A comment token
                            case TokenType.Comment:
                                /// Insert a comment.
                                InsertComment((CommentToken)token);
                                break;
                            /// -> A DOCTYPE token
                            case TokenType.Doctype:
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Ignore the token.
                                continue;
                            case TokenType.StartTag:
                                switch (((StartTagToken)token).TagName)
                                {
                                    /// -> A start tag whose tag name is "html"
                                    case "html":
                                        /// Process the token using the rules for the "in body" insertion mode.
                                        goto ReprocessCurrent;
                                    /// -> A start tag whose tag name is "head"
                                    case "head":
                                        /// Insert an HTML element for the token.
                                        /// Set the head element pointer to the newly created head element.
                                        InsertElement((StartTagToken)token);

                                        /// Switch the insertion mode to "in head".
                                        Mode.Switch(InsertionMode.InHead);
                                        break;
                                    default:
                                        goto BeforeHeadDefault;
                                }
                                break;
                            case TokenType.EndTag:
                                switch (((EndTagToken)token).TagName.ToString())
                                {
                                    /// -> An end tag whose tag name is one of:
                                    /// "head", "body", "html", "br"
                                    case "head":
                                    case "html":
                                    case "body":
                                    case "br":
                                        /// Act as described in the "anything else" entry below.
                                        goto BeforeHeadDefault;
                                    /// -> Any other end tag
                                    default:
                                        /// Parse error.
                                        ParserErrorLogger.Log();
                                        /// Ignore the token.
                                        continue;
                                }
                            /// -> Anything else
                            default:
                                BeforeHeadDefault:
                                /// Insert an HTML element for a "head" start tag token with no attributes.
                                /// Set the head element pointer to the newly created head element.
                                InsertElement("head");

                                /// Switch the insertion mode to "in head".
                                Mode.Switch(InsertionMode.InHead);

                                /// Reprocess the current token.
                                goto ReprocessCurrent;
                        }
                        break;
                    case InsertionMode.InHead:
                        switch (token)
                        {
                            case CharacterToken characher:
                                switch (characher.Data)
                                {
                                    /// -> A character token that is one of
                                    /// U+0009 CHARACTER TABULATION (tab) 
                                    case '\x9':
                                    /// U+000A LINE FEED (LF)
                                    case '\xA':
                                    /// U+000C FORM FEED (FF)
                                    case '\xC':
                                    /// U+000D CARRIAGE RETURN (CR)
                                    case '\xD':
                                    /// U+0020 SPACE
                                    case ' ':
                                        /// Insert the character.
                                        InsertCharacter(characher.Data);
                                        break;
                                    default:
                                        goto InHeadDefault;
                                }
                                break;
                            /// -> A comment token
                            case CommentToken comment:
                                /// Insert a comment.
                                InsertComment(comment);
                                break;
                            /// -> A DOCTYPE token
                            case DoctypeToken doctype:
                                /// parse error.
                                ParserErrorLogger.Log();

                                /// Ignore the token.
                                continue;
                            case StartTagToken startTag:
                                switch (startTag.TagName)
                                {
                                    /// -> A start tag whose tag name is "html"
                                    case "html":
                                        // TODO:
                                        break;
                                    /// -> A start tag whose tag name is one of: "base", "basefont", "bgsound", "link"
                                    case "base":
                                    case "basefont":
                                    case "bgsound":
                                    case "link":
                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);

                                        /// Immediately pop the current node off the stack of open elements.
                                        OpenElementsStack.Pop();

                                        /// Acknowledge the token's self-closing flag, if it is set.

                                        break;
                                    /// -> A start tag whose tag name is "meta"
                                    case "meta":
                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);

                                        /// Immediately pop the current node off the stack of open elements.
                                        OpenElementsStack.Pop();

                                        /// Acknowledge the token's self-closing flag, if it is set.

                                        /// If the element has a charset attribute,
                                        /// and getting an encoding from its value results
                                        /// in a supported ASCII-compatible character encoding or a UTF-16 encoding,
                                        /// and the confidence is currently tentative,

                                        /// then change the encoding to the resulting encoding.

                                        /// Otherwise, if the element has an http-equiv attribute
                                        /// whose value is an ASCII case-insensitive match for
                                        /// the string "Content-Type", and the element has a content attribute,
                                        /// and applying the algorithm for extracting a character encoding
                                        /// from a meta element to that attribute's value returns
                                        /// a supported ASCII-compatible character encoding or a UTF-16 encoding,
                                        /// and the confidence is currently tentative,

                                        /// then change the encoding to the extracted encoding.

                                        break;
                                    /// -> A start tag whose tag name is "title"
                                    case "title":
                                        /// Follow the generic RCDATA element parsing algorithm.
                                        /// 1. Insert an HTML element for the token.
                                        InsertElement(startTag);

                                        /// 2. Switch the tokenizer to the RCDATA state.
                                        tokenizer.State = TokenizerStates.RcData;

                                        /// 3. Let the original insertion mode be the current insertion mode.
                                        /// 4. Then, switch the insertion mode to "text".
                                        Mode.SaveAndSwitch(InsertionMode.Text);
                                        break;
                                    /// -> A start tag whose tag name is "noscript",
                                    case "noscript":
                                        /// -> If the scripting flag is enabled
                                        if (scripting)
                                            goto case "noframes";
                                        /// -> If the scripting flag is disabled
                                        else
                                        {
                                            /// Insert an HTML element for the token.
                                            InsertElement(startTag);

                                            /// Switch the insertion mode to "in head noscript".
                                            Mode.Switch(InsertionMode.InHeadNoScript);
                                        }
                                        break;
                                    /// -> A start tag whose tag name is one of: "noframes", "style"
                                    case "noframes":
                                    case "style":
                                        /// Follow the generic raw text element parsing algorithm.

                                        break;
                                    /// -> A start tag whose tag name is "script" 
                                    case "script":
                                        /// Run these steps:
                                        /// 1. Let the adjusted insertion location be
                                        /// the appropriate place for inserting a node. 
                                        /// 2. Create an element for the token in the HTML namespace,
                                        /// with the intended parent being the element in which
                                        /// the adjusted insertion location finds itself.
                                        /// 5. Insert the newly created element at the adjusted insertion location. 
                                        /// 6. Push the element onto the stack of open elements so that it is the new current node.
                                        var script = InsertElement(startTag);

                                        /// 3. Mark the element as being "parser-inserted"
                                        /// and unset the element’s "non-blocking" flag.

                                        /// 4. if the parser was originally created for
                                        /// the HTML fragment parsing algorithm,

                                        /// then mark the script element as "already started". (fragment case) 

                                        /// 7. Switch the tokenizer to the §8.2.4.6 Script data state.
                                        tokenizer.State = TokenizerStates.ScriptData;

                                        /// 8. Let the original insertion mode be the current insertion mode.
                                        /// 9. Switch the insertion mode to "text".
                                        Mode.SaveAndSwitch(InsertionMode.Text);
                                        break;
                                    /// -> A start tag whose tag name is "template"
                                    case "template":
                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);

                                        /// Insert a marker at the end of the list of active formatting elements.

                                        /// Set the frameset-ok flag to "not ok".
                                        frameSetOk = false;

                                        /// Switch the insertion mode to "in template".
                                        Mode.Switch(InsertionMode.InTemplate);

                                        /// Push "in template" onto the stack of template insertion modes
                                        /// so that it is the new current template insertion mode.

                                        break;
                                    /// -> A start tag whose tag name is "head"
                                    case "head":
                                        /// Parse error.
                                        ParserErrorLogger.Log();

                                        /// Ignore the token.
                                        continue;
                                    default:
                                        goto InHeadDefault;
                                }
                                break;
                            case EndTagToken endTag:
                                switch (endTag.TagName)
                                {
                                    /// -> An end tag whose tag name is "head"
                                    case "head":
                                        /// Pop the current node (which will be the head element)
                                        /// off the stack of open elements.
                                        OpenElementsStack.Pop();

                                        /// Switch the insertion mode to "after head".
                                        Mode.Switch(InsertionMode.AfterHead);
                                        break;
                                    /// -> An end tag whose tag name is one of: "body", "html", "br"
                                    case "body":
                                    case "html":
                                    case "br":
                                        /// Act as described in the "anything else" entry below.
                                        goto InHeadDefault;
                                    /// -> An end tag whose tag name is "template"
                                    case "template":
                                        // TODO
                                        break;
                                    /// -> Any other end tag
                                    default:
                                        /// Parse error.
                                        ParserErrorLogger.Log();

                                        /// Ignore the token.
                                        continue;
                                }
                                break;
                            default:
                                InHeadDefault:
                                /// Pop the current node (which will be the head element)
                                /// off the stack of open elements.
                                OpenElementsStack.Pop();

                                /// Switch the insertion mode to "after head".
                                Mode.Switch(InsertionMode.AfterHead);

                                /// Reprocess the token.
                                goto ReprocessCurrent;
                        }
                        break;
                    case InsertionMode.AfterHead:
                        switch (token.Type)
                        {
                            case TokenType.Character:
                                switch (((CharacterToken)token).Data)
                                {
                                    /// A character token that is one of
                                    /// U+0009 CHARACTER TABULATION,
                                    case '\x9':
                                    /// "LF" (U+000A),
                                    case '\xA':
                                    /// "FF" (U+000C),
                                    case '\xC':
                                    /// "CR" (U+000D),
                                    case '\xD':
                                    /// or U+0020 SPACE
                                    case ' ':
                                        /// Insert the character.
                                        InsertCharacter(((CharacterToken)token).Data);
                                        break;
                                    default:
                                        goto AfterHeadDefault;
                                }
                                break;
                            /// -> A comment token
                            case TokenType.Comment:
                                /// Insert a comment.
                                InsertComment((CommentToken)token);
                                break;
                            /// -> A DOCTYPE token
                            case TokenType.Doctype:
                                /// Parse error.
                                ParserErrorLogger.Log();
                                /// Ignore the token.
                                continue;
                            case TokenType.StartTag:
                                switch (((StartTagToken)token).TagName)
                                {
                                    /// -> A start tag whose tag name is "html"
                                    case "html":
                                        // TODO:
                                        break;
                                    /// -> A start tag whose tag name is "body"
                                    case "body":
                                        /// Insert an HTML element for the token.
                                        document.Body = (HtmlElement)InsertElement((StartTagToken)token);

                                        /// Set the frameset-ok flag to "not ok".
                                        frameSetOk = false;

                                        /// Switch the insertion mode to "in body".
                                        Mode.Switch(InsertionMode.InBody);
                                        break;
                                    /// -> A start tag whose tag name is "frameset"
                                    case "frameset":
                                        /// Insert an HTML element for the token.
                                        InsertElement((StartTagToken)token);

                                        /// Switch the insertion mode to "in frameset".
                                        Mode.Switch(InsertionMode.InFrameSet);
                                        break;
                                    case "base":
                                    case "basefont":
                                    case "bgsound":
                                    case "link":
                                    case "meta":
                                    case "noframes":
                                    case "script":
                                    case "style":
                                    case "template":
                                    case "title":
                                        /// Parse error.
                                        ParserErrorLogger.Log();

                                        /// Push the node pointed to by the head element pointer onto
                                        /// the stack of open elements.
                                        OpenElementsStack.Push(document.Head);

                                        /// Process the token using the rules for the "in head" insertion mode.

                                        /// Remove the node pointed to by the head element pointer from
                                        /// the stack of open elements.
                                        /// (It might not be the current node at this point.)
                                        while (OpenElementsStack.Pop() != document.Head) ;
                                        break;
                                    case "head":
                                        continue;
                                    default:
                                        goto AfterHeadDefault;
                                }
                                break;
                            case TokenType.EndTag:
                                switch (((EndTagToken)token).TagName)
                                {
                                    case "template":
                                        // TODO:
                                        break;
                                    case "body":
                                    case "html":
                                    case "br":
                                        goto AfterHeadDefault;
                                    default:
                                        continue;
                                }
                                break;
                            /// -> Anything else
                            default:
                                AfterHeadDefault:
                                /// Insert an HTML element for a "body" start tag token with no attributes.
                                document.Body = (HtmlElement)InsertElement("body");

                                /// Switch the insertion mode to "in body".
                                Mode.Switch(InsertionMode.InBody);

                                /// Reprocess the current token.
                                goto ReprocessCurrent;
                        }
                        break;
                    case InsertionMode.InBody:
                        switch (token)
                        {
                            case CharacterToken character:
                                switch (character.Data)
                                {
                                    /// -> A character token that is U+0000 NULL
                                    case '\0':
                                        /// Parse error.
                                        ParserErrorLogger.Log();
                                        /// Ignore the token.
                                        continue;
                                    /// A character token that is one of
                                    /// U+0009 CHARACTER TABULATION,
                                    case '\x9':
                                    /// "LF" (U+000A),
                                    case '\xA':
                                    /// "FF" (U+000C),
                                    case '\xC':
                                    /// "CR" (U+000D),
                                    case '\xD':
                                    /// or U+0020 SPACE
                                    case ' ':
                                        /// Reconstruct the active formatting elements, if any.

                                        /// Insert the token's character.
                                        InsertCharacter(character.Data);
                                        break;
                                    /// -> Any other character token
                                    default:
                                        /// Reconstruct the active formatting elements, if any.

                                        /// Insert the token's character.
                                        InsertCharacter(character.Data);

                                        /// Set the frameset-ok flag to "not ok".
                                        frameSetOk = false;
                                        break;
                                }
                                break;
                            /// -> A comment token
                            case CommentToken comment:
                                /// Insert a comment.
                                InsertComment(comment);
                                break;
                            /// -> A DOCTYPE token
                            case DoctypeToken doctype:
                                /// parse error.
                                ParserErrorLogger.Log();

                                /// Ignore the token.
                                continue;
                            case StartTagToken startTag:
                                switch (startTag.TagName)
                                {
                                    /// -> A start tag whose tag name is "html"
                                    case "html":

                                        break;
                                    /// -> A start tag whose tag name is one of: "base", "basefont", 
                                    /// "bgsound", "link", "meta", "noframes", "script", "style", "template", "title"
                                    case "base":
                                    case "basefont":
                                    case "bgsound":
                                    case "link":
                                    case "meta":
                                    case "noframes":
                                    case "script":
                                    case "style":
                                    case "template":
                                    case "title":
                                        /// -> Process the token using the rules for the "in head" insertion mode.
                                        Mode.SwitchForReprocess(InsertionMode.InHead);
                                        goto ReprocessCurrent;
                                    /// -> A start tag whose tag name is "body"
                                    case "body":
                                        /// Parser error.
                                        ParserErrorLogger.Log();

                                        /// If the second element on the stack of open elements is not a body element,
                                        /// if the stack of open elements has only one node on it, 
                                        /// or if there is a template element on the stack of open elements,
                                        if (OpenElementsStack.Count == 1 ||
                                            OpenElementsStack.ElementAt(1).TagName.ToLower() != "body" ||
                                            OpenElementsStack.Any(x => x.TagName.ToLower() == "template"))
                                            /// then ignore the token. (fragment case)
                                            continue;
                                        else
                                            /// Otherwise, set the frameset-ok flag to "not ok";
                                            frameSetOk = false;

                                        /// then, for each attribute on the token,
                                        foreach (var attribute in startTag.Attributes)
                                            /// check to see if the attribute is already present on the
                                            /// body element (the second element) on the stack of open elements,
                                            if (!document.Body.HasAttribute(attribute.Name.ToString()))
                                                /// and if it is not,
                                                /// add the attribute and its corresponding value to that element.
                                                document.Body.SetAttribute(attribute.Name.ToString(), attribute.Value.ToString());
                                        break;
                                    /// -> A start tag whose tag name is "frameset"
                                    case "frameset":
                                        /// Parser error.
                                        ParserErrorLogger.Log();

                                        /// If the stack of open elements has only one node on it,
                                        /// or if the second element on the stack of open elements is not
                                        /// a body element,
                                        if (OpenElementsStack.Count == 1 ||
                                            OpenElementsStack[1].TagName.ToLower() != "body")
                                            /// then ignore the token. (fragment case)
                                            continue;
                                        /// If the frameset-ok flag is set to "not ok",
                                        else if (!frameSetOk)
                                            /// ignore the token.
                                            continue;
                                        else
                                        {
                                            /// Otherwise, run the following steps:
                                            /// 1. Remove the second element on the stack of open elements from
                                            /// its parent node, if it has one.
                                            var node = OpenElementsStack[1];
                                            node.ParentNode.RemoveChild(node);

                                            /// 2. Pop all the nodes from the bottom of the stack of open elements,
                                            /// from the current node up to, but not including, the root html element.
                                            while (OpenElementsStack.Count != 1) OpenElementsStack.Pop();

                                            /// 3. Insert an HTML element for the token.
                                            InsertElement(startTag);

                                            /// 4. Switch the insertion mode to "in frameset".
                                            Mode.Switch(InsertionMode.InFrameSet);
                                        }
                                        break;
                                    /// -> A start tag whose tag name is one of:
                                    /// "address", "article", "aside", "blockquote", "center", "details", "dialog", "dir",
                                    /// "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup", "main",
                                    /// "nav", "ol", "p", "section", "summary", "ul"
                                    case "address":
                                    case "article":
                                    case "aside":
                                    case "blockquote":
                                    case "center":
                                    case "details":
                                    case "dialog":
                                    case "dir":
                                    case "div":
                                    case "dl":
                                    case "fieldset":
                                    case "figcaption":
                                    case "figure":
                                    case "footer":
                                    case "header":
                                    case "hgroup":
                                    case "main":
                                    case "nav":
                                    case "ol":
                                    case "p":
                                    case "section":
                                    case "summary":
                                    case "ul":
                                        /// If the stack of open elements has a p element in button scope,
                                        if (HasElementInButtonScope("p"))
                                            /// then close a p element.
                                            ClosePElement();

                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);
                                        break;
                                    /// -> A start tag whose tag name is one of:
                                    /// "h1", "h2", "h3", "h4", "h5", "h6"
                                    case "h1":
                                    case "h2":
                                    case "h3":
                                    case "h4":
                                    case "h5":
                                    case "h6":
                                        /// If the stack of open elements has a p element in button scope,
                                        if (HasElementInButtonScope("p"))
                                            /// then close a p element.
                                            ClosePElement();

                                        /// If the current node is an HTML element whose tag name is one of
                                        /// "h1", "h2", "h3", "h4", "h5", or "h6",
                                        if (HeaderTagList.Contains(startTag.TagName))
                                        {
                                            /// then this is a parse error;
                                            ParserErrorLogger.Log();
                                            /// pop the current node off the stack of open elements.
                                            OpenElementsStack.Pop();
                                        }

                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);
                                        break;
                                    /// -> A start tag whose tag name is one of:
                                    /// "pre", "listing"
                                    case "pre":
                                    case "listing":
                                        /// If the stack of open elements has a p element in button scope,

                                        /// then close a p element.

                                        /// Insert an HTML element for the token.

                                        /// If the next token is a "LF" (U+000A) character token,

                                        /// then ignore that token and move on to the next one.
                                        /// (Newlines at the start of pre blocks are ignored as an authoring convenience.)

                                        /// Set the frameset-ok flag to "not ok".
                                        break;
                                    /// -> A start tag whose tag name is one of:
                                    /// "area", "br", "embed", "img", "keygen", "wbr"
                                    case "area":
                                    case "br":
                                    case "img":
                                    case "keygen":
                                    case "wbr":
                                        /// Reconstruct the active formatting elements, if any.

                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);

                                        /// Immediately pop the current node off the stack of open elements.
                                        OpenElementsStack.Pop();

                                        /// Acknowledge the token's self-closing flag, if it is set.

                                        /// Set the frameset-ok flag to "not ok".
                                        frameSetOk = false;
                                        break;
                                    /// -> A start tag whose tag name is "hr"
                                    case "hr":
                                        /// If the stack of open elements has a p element in button scope,
                                        if (HasElementInButtonScope("p"))
                                            /// then close a p element.
                                            ClosePElement();

                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);

                                        /// Immediately pop the current node off the stack of open elements.
                                        OpenElementsStack.Pop();

                                        /// Acknowledge the token's self-closing flag, if it is set.

                                        /// Set the frameset-ok flag to "not ok".
                                        frameSetOk = false;
                                        break;
                                    /// -> A start tag whose tag name is "image"
                                    case "image":
                                        /// Parse error.
                                        ParserErrorLogger.Log();

                                        /// Change the token's tag name to "img" and reprocess it. (Don't ask.)
                                        startTag.TagName = "img";
                                        goto case "img";
                                    /// -> A start tag whose tag name is "a"
                                    case "a":
                                        /// If the list of active formatting elements contains an a element
                                        /// between the end of the list and the last marker on the list
                                        /// (or the start of the list if there is no marker on the list),
                                        if (Enumerable.Reverse(ActiveFormattingElements).TakeWhile(x => !(x is Marker)).Any(x => x.TagName.ToLower() == "a"))
                                        {
                                            /// then this is a parse error;
                                            ParserErrorLogger.Log();

                                            /// run the adoption agency algorithm for the tag name "a",

                                            /// then remove that element from the list of active formatting elements


                                            /// and the stack of open elements if the adoption agency algorithm
                                            /// didn't already remove it
                                            /// (it might not have if the element is not in table scope).
                                        }

                                        /// Reconstruct the active formatting elements, if any.

                                        /// Insert an HTML element for the token.
                                        /// Push onto the list of active formatting elements that element.
                                        ActiveFormattingElements.Add(InsertElement(startTag));
                                        break;
                                    /// -> A start tag whose tag name is one of: 
                                    /// "b", "big", "code", "em", "font", "i", "s", "small",
                                    /// "strike", "strong", "tt", "u"
                                    case "b":
                                    case "big":
                                    case "code":
                                    case "em":
                                    case "font":
                                    case "i":
                                    case "s":
                                    case "small":
                                    case "strike":
                                    case "strong":
                                    case "tt":
                                    case "u":
                                        /// Reconstruct the active formatting elements, if any.

                                        /// Insert an HTML element for the token.
                                        /// Push onto the list of active formatting elements that element.
                                        ActiveFormattingElements.Add(InsertElement(startTag));
                                        break;
                                    /// -> Any other start tag
                                    default:
                                        /// Reconstruct the active formatting elements, if any.

                                        /// Insert an HTML element for the token.
                                        InsertElement(startTag);
                                        break;
                                }
                                break;
                            case EndTagToken endTag:
                                switch (endTag.TagName)
                                {
                                    /// -> An end tag whose tag name is "template"
                                    case "template":
                                        // TODO:
                                        break;
                                    /// -> An end tag whose tag name is "body"
                                    case "body":
                                        /// If the stack of open elements does not have a body element in scope,
                                        if (!HasElementInScope("body"))
                                        {
                                            /// this is a parse error;
                                            ParserErrorLogger.Log();
                                            /// ignore the token.
                                            continue;
                                        }
                                        else
                                        {
                                            /// Otherwise, if there is a node in the stack of open elements that
                                            /// is not either a dd element, a dt element, an li element, 
                                            /// an optgroup element, an option element, a p element, an rb element,
                                            /// an rp element, an rt element, an rtc element, a tbody element,
                                            /// a td element, a tfoot element, a th element, a thead element,
                                            /// a tr element, the body element, or the html element,
                                            foreach (var item in OpenElementsStack)
                                                if (!BodyEndAllowedUnclosedTagList.Contains(item.TagName.ToLower()))
                                                    /// then this is a parse error.
                                                    ParserErrorLogger.Log();

                                            /// Switch the insertion mode to "after body".
                                            Mode.Switch(InsertionMode.AfterBody);
                                            break;
                                        }
                                    case "html":
                                        /// If the stack of open elements does not have a body element in scope,
                                        if (!HasElementInScope("body"))
                                        {
                                            /// this is a parse error;
                                            ParserErrorLogger.Log();
                                            /// ignore the token.
                                            continue;
                                        }
                                        else
                                        {
                                            /// Otherwise, if there is a node in the stack of open elements that
                                            /// is not either a dd element, a dt element, an li element, 
                                            /// an optgroup element, an option element, a p element, an rb element,
                                            /// an rp element, an rt element, an rtc element, a tbody element,
                                            /// a td element, a tfoot element, a th element, a thead element,
                                            /// a tr element, the body element, or the html element,
                                            foreach (var item in OpenElementsStack)
                                                if (!BodyEndAllowedUnclosedTagList.Contains(item.TagName.ToLower()))
                                                    /// then this is a parse error.
                                                    ParserErrorLogger.Log();

                                            /// Switch the insertion mode to "after body".
                                            Mode.Switch(InsertionMode.AfterBody);

                                            /// Reprocess the token.
                                            goto ReprocessCurrent;
                                        }
                                    /// -> An end tag whose tag name is one of:
                                    /// "address", "article", "aside", "blockquote", "button", "center", "details", "dialog", 
                                    /// "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "header", "hgroup",
                                    /// "listing", "main", "nav", "ol", "pre", "section", "summary", "ul"
                                    case "address":
                                    case "article":
                                    case "aside":
                                    case "blockquote":
                                    case "button":
                                    case "center":
                                    case "details":
                                    case "dialog":
                                    case "dir":
                                    case "div":
                                    case "dl":
                                    case "fieldset":
                                    case "figcaption":
                                    case "figure":
                                    case "footer":
                                    case "header":
                                    case "hgroup":
                                    case "listing":
                                    case "main":
                                    case "nav":
                                    case "ol":
                                    case "pre":
                                    case "section":
                                    case "summary":
                                    case "ul":
                                        /// If the stack of open elements does not have an element in scope
                                        /// that is an HTML element and with the same tag name as that of the token,
                                        if (!HasElementInScope(endTag.TagName))
                                        {
                                            /// then this is a parse error;
                                            ParserErrorLogger.Log();
                                            /// ignore the token.
                                            continue;
                                        }
                                        else
                                        {
                                            /// Otherwise, run these steps:
                                            /// 1. Generate implied end tags.
                                            GenerateImpiledEndTags();

                                            /// 2. If the current node is not an HTML element with the
                                            /// same tag name as that of the token,
                                            if (OpenElementsStack.Peek().TagName.ToLower() != endTag.TagName)
                                                /// then this is a parse error.
                                                ParserErrorLogger.Log();

                                            /// 3. Pop elements from the stack of open elements until
                                            /// an HTML element with the same tag name as the token has been
                                            /// popped from the stack.
                                            while (OpenElementsStack.Pop().TagName.ToLower() != endTag.TagName) ;
                                            break;
                                        }
                                    /// -> An end tag whose tag name is "p"
                                    case "p":
                                        /// If the stack of open elements does not have a p element in button scope,
                                        if (!HasElementInButtonScope("p"))
                                        {
                                            /// then this is a parse error;
                                            ParserErrorLogger.Log();

                                            /// insert an HTML element for a "p" start tag token with no attributes.
                                            InsertElement("p");
                                        }

                                        /// Close a p element.
                                        ClosePElement();
                                        break;
                                    case "li":
                                        break;
                                    case "dd":
                                    case "dt":
                                        break;
                                    /// -> An end tag whose tag name is one of:
                                    /// "h1", "h2", "h3", "h4", "h5", "h6"
                                    case "h1":
                                    case "h2":
                                    case "h3":
                                    case "h4":
                                    case "h5":
                                    case "h6":
                                        /// If the stack of open elements does not have an element in scope that
                                        /// is an HTML element and whose tag name is one of
                                        /// "h1", "h2", "h3", "h4", "h5", or "h6",
                                        if (!HasElementsInScope("h1", "h2", "h3", "h4", "h5", "h6"))
                                        {
                                            /// then this is a parse error; 
                                            ParserErrorLogger.Log();
                                            /// ignore the token.
                                            continue;
                                        }
                                        else
                                        {
                                            /// Otherwise, run these steps:
                                            /// 1. Generate implied end tags.
                                            GenerateImpiledEndTags();

                                            /// 2. If the current node is not an HTML element with the
                                            /// same tag name as that of the token,
                                            if (OpenElementsStack.Peek().TagName.ToLower() != endTag.TagName)
                                                /// then this is a parse error.
                                                ParserErrorLogger.Log();

                                            /// 3. Pop elements from the stack of open elements until
                                            /// an HTML element whose tag name is one of
                                            /// "h1", "h2", "h3", "h4", "h5", or "h6" has been popped
                                            /// from the stack.
                                            while (!HeaderTagList.Contains(OpenElementsStack.Pop().TagName.ToLower())) ;

                                            break;
                                        }
                                    /// -> Any other end tag
                                    default:
                                        /// Run these steps:
                                        /// 1. Initialize node to be the current node
                                        /// (the bottommost node of the stack).
                                        foreach (var node in Enumerable.Reverse(new List<Element>(OpenElementsStack)))
                                        {
                                            /// 2. Loop: If node is an HTML element with
                                            /// the same tag name as the token, then:
                                            if (node.TagName.ToLower() == endTag.TagName)
                                            {
                                                /// 1. Generate implied end tags, except for
                                                /// HTML elements with the same tag name as the token.
                                                GenerateImpiledEndTags(exceptFor: node.TagName.ToLower());

                                                /// 2. If node is not the current node,
                                                if (node != OpenElementsStack.Peek())
                                                    /// then this is a parse error.
                                                    ParserErrorLogger.Log();

                                                /// 3. Pop all the nodes from the current node up to node,
                                                /// including node, then stop these steps.
                                                while (OpenElementsStack.Pop() != node) ;
                                            }
                                            /// 3. Otherwise, if node is in the special category,
                                            else if (SpecialTagList.Contains(node.TagName.ToLower()))
                                            {
                                                /// then this is a parse error;
                                                ParserErrorLogger.Log();
                                                /// ignore the token, and abort these steps.
                                                goto InBodyDefaultEnd;
                                            }
                                        }

                                        InBodyDefaultEnd:
                                        break;
                                }
                                break;
                            /// -> An end-of-file token
                            case EndOfFileToken eof:
                                /// If there is a node in the stack of open elements that is not either
                                /// a dd element, a dt element, an li element, a p element, a tbody element, a td element,
                                /// a tfoot element, a th element, a thead element, a tr element, the body element, or the html element,
                                /// then this is a parse error.

                                /// If the stack of template insertion modes is not empty,
                                /// then process the token using the rules for the "in template" insertion mode.

                                /// Otherwise, stop parsing.
                                goto StopParsing;
                        }
                        break;
                    case InsertionMode.Text:
                        switch (token)
                        {
                            /// -> A character token
                            case CharacterToken character:
                                /// Insert the token's character.
                                InsertCharacter(character.Data);
                                break;
                            /// -> An end-of-file token
                            case EndOfFileToken eof:
                                /// Parse error.
                                ParserErrorLogger.Log();

                                /// If the current node is a script element,
                                if (OpenElementsStack.Peek().TagName.ToLower() == "script")
                                {
                                    /// mark the script element as "already started".
                                }

                                /// Pop the current node off the stack of open elements.
                                OpenElementsStack.Pop();

                                /// Switch the insertion mode to the original insertion mode
                                Mode.Restore();
                                /// and reprocess the token.
                                goto ReprocessCurrent;
                            case EndTagToken endTag:
                                /// -> An end tag whose tag name is "script"
                                if (endTag.TagName == "script")
                                {
                                    /// If the JavaScript execution context stack is empty,
                                    /// perform a microtask checkpoint.

                                    /// Let script be the current node (which will be a script element).

                                    /// Pop the current node off the stack of open elements.
                                    OpenElementsStack.Pop();

                                    /// Switch the insertion mode to the original insertion mode.
                                    Mode.Restore();

                                    /// Let the old insertion point have the same value as the current insertion point.
                                    /// Let the insertion point be just before the next input character.

                                    /// Increment the parser’s script nesting level by one.

                                    /// Prepare the script.
                                    /// This might cause some script to execute,
                                    /// which might cause new characters to be inserted into the tokenizer,
                                    /// and might cause the tokenizer to output more tokens,
                                    /// resulting in a reentrant invocation of the parser.

                                    /// Decrement the parser’s script nesting level by one.

                                    /// If the parser’s script nesting level is zero,
                                    /// then set the parser pause flag to false.

                                    /// Let the insertion point have the value of the old insertion point.
                                    /// (In other words, restore the insertion point to its previous value.
                                    /// This value might be the "undefined" value.)

                                    /// At this stage, if there is a pending parsing-blocking script, then:

                                    /// -> If the script nesting level is not zero:
                                    /// Set the parser pause flag to true,
                                    /// and abort the processing of any nested invocations of the tokenizer,
                                    /// yielding control back to the caller.
                                    /// (Tokenization will resume when the caller returns to the "outer" tree construction stage.)

                                    /// -> Otherwise: 
                                    /// Run these steps:
                                    /// 1. Let the script be the pending parsing-blocking script.
                                    /// There is no longer a pending parsing-blocking script.

                                    /// 2.Block the tokenizer for this instance of the HTML parser,
                                    /// such that the event loop will not run tasks that invoke the tokenizer. 

                                    /// 3. If the parser’s Document has a style sheet that is blocking scripts or
                                    /// the script’s "ready to be parser-executed" flag is not set:
                                    /// spin the event loop until the parser’s Document has no style sheet that is
                                    /// blocking scripts and the script’s "ready to be parser-executed" flag is set.

                                    /// 4. If this parser has been aborted in the meantime,
                                    /// abort these steps.

                                    /// 5. Unblock the tokenizer for this instance of the HTML parser,
                                    /// such that tasks that invoke the tokenizer can again be run. 

                                    /// 6. Let the insertion point be just before the next input character. 

                                    /// 7. Increment the parser’s script nesting level by one
                                    /// (it should be zero before this step, so this sets it to one).

                                    /// 8. Execute the script.

                                    /// 9. Decrement the parser’s script nesting level by one.
                                    /// If the parser’s script nesting level is zero
                                    /// (which it always should be at this point),
                                    /// then set the parser pause flag to false. 

                                    /// 10. Let the insertion point be undefined again.

                                    /// 11. If there is once again a pending parsing-blocking script,
                                    /// then repeat these steps from step 1. 
                                }
                                /// -> Any other end tag
                                else
                                {
                                    /// Pop the current node off the stack of open elements.
                                    OpenElementsStack.Pop();

                                    /// Switch the insertion mode to the original insertion mode.
                                    Mode.Restore();
                                }
                                break;
                        }
                        break;
                    case InsertionMode.AfterBody:
                        break;
                }
            }

            StopParsing:

            return document;
        }

        private void InsertComment(CommentToken comment, Location position = null)
        {
            /// When the steps below require the user agent to
            /// insert a comment while processing a comment token,
            /// optionally with an explicitly insertion position position,
            /// the user agent must run the following steps:
            /// 1. Let data be the data given in the comment token being processed.

            /// 2. If position was specified, then let the adjusted insertion location be position.
            /// Otherwise, let adjusted insertion location be the appropriate place for inserting a node. 
            if (position == null)
                position = FindAppropriatePlaceForInsertingNode();

            /// 3. Create a Comment node whose data attribute is set to data
            /// and whose node document is the same as that of the node in which the adjusted insertion location finds itself.
            var node = document.CreateComment(comment.Data);

            /// 4. Insert the newly created node at the adjusted insertion location. 
            position.InsertBefore(node);
        }

        public static Document Parse(string input, bool isFragment)
        {
            return new HtmlParser(input, isFragment).Parse();
        }
    }
}
