using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public enum EventPhase : ushort
    {
        None,
        CapturingPhase,
        AtTarget,
        BubblingPhase
    }

    public class EventInit
    {
        public bool Bubbles { get; set; }

        public bool Cancelable { get; set; }
    }

    public class Event
    {
        /// <summary>
        /// Returns the type of <see cref="Event"/>.
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Returns the object to which <see cref="Event"/> is dispatched. 
        /// </summary>
        public EventTarget Target { get; internal set; }
        /// <summary>
        /// Returns the object whose event listener's callback is currently being invoked. 
        /// </summary>
        public EventTarget CurrentTarget { get; internal set; }

        /// <summary>
        /// Returns the event's phase.
        /// </summary>
        public EventPhase EventPhase { get; internal set; }

        internal bool StopPropagationFlag;
        internal bool StopImmediatePropagationFlag;

        /// <summary>
        /// When dispatched in a tree, invoking this method prevents <see cref="Event"/> from
        /// reaching any objects other than the current object. 
        /// </summary>
        public void StopPropagation()
        {
            StopPropagationFlag = true;
        }
        /// <summary>
        /// Invoking this method prevents <see cref="Event"/> from reaching any registered event listeners
        /// after the current one finishes running and, when dispatched in a tree,
        /// also prevents <see cref="Event"/> from reaching any other objects. 
        /// </summary>
        public void StopImmediatePropagation()
        {
            StopPropagationFlag = true;
            StopImmediatePropagationFlag = true;
        }

        internal bool CanceledFlag;

        /// <summary>
        /// Returns <c>true</c> or <c>false</c> depending on how event was initialized.
        /// <c>true</c> if <see cref="Event"/>'s goes through its <see cref="Target"/> attribute value's ancestors
        /// in reverse tree order, and <c>false</c> otherwise. 
        /// </summary>
        public bool Bubbles { get; }
        /// <summary>
        /// Returns <c>true</c> or <c>false</c> depending on how <see cref="Event"/> was initialized.
        /// Its return value does not always carry meaning,
        /// but <c>true</c> can indicate that part of the operation during which event was dispatched,
        /// can be canceled by invoking the <see cref="PreventDefault"/> method. 
        /// </summary>
        public bool Cancelable { get; }
        /// <summary>
        /// If invoked when the <see cref="Cancelable"/> attribute value is <c>true</c>,
        /// signals to the operation that caused <see cref="Event"/> to be dispatched that it needs to be canceled. 
        /// </summary>
        public void PreventDefault()
        {
            if (Cancelable)
                CanceledFlag = true;
        }
        /// <summary>
        /// Returns <c>true</c> if <see cref="PreventDefault"/> was invoked
        /// while the <see cref="Cancelable"/> attribute value is <c>true</c>, and <c>false</c> otherwise. 
        /// </summary>
        public bool DefaultPrevented => CanceledFlag;

        /// <summary>
        /// Returns <c>true</c> if <see cref="Event"/> was dispatched by the user agent, and <c>false</c> otherwise. 
        /// </summary>
        public bool IsTrusted { get; internal set; }
        /// <summary>
        /// Returns the creation time of <see cref="Event"/> as the number of milliseconds that passed since 00:00:00 UTC on 1 January 1970. 
        /// </summary>
        public DateTimeOffset TimeStamp { get; }

        internal bool InitializedFlag;
        internal bool DispatchFlag;

        [Obsolete]
        public void InitEvent(string type, bool bubbles, bool cancelable) { throw new NotImplementedException(); }
        /// <summary>
        /// Returns a new <see cref="Event"/> whose type attribute value is set to type.
        /// </summary>
        /// <param name="type">The type of <see cref="Event"/></param>
        /// <param name="eventInitDict"> The optional eventInitDict argument allows for setting the bubbles and cancelable attributes via object members of the same name.</param>
        public Event(string type, EventInit eventInitDict = null)
        {
            InitializedFlag = true;

            Type = type;

            if (eventInitDict != null)
            {
                Bubbles = eventInitDict.Bubbles;
                Cancelable = eventInitDict.Cancelable;
            }

            TimeStamp = DateTimeOffset.Now;
        }
    }

    public delegate void EventListener(Event @event);

    public class EventTarget
    {
        internal class EventListenerRecord
        {
            public string Type { get; }

            public EventListener Listener { get; }

            public bool Capture { get; }

            public EventListenerRecord(string type, EventListener listener, bool capture)
            {
                Type = type;
                Listener = listener;
                Capture = capture;
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode() ^ Listener.GetHashCode() ^ Capture.GetHashCode();
            }
        }

        internal HashSet<EventListenerRecord> EventListeners { get; } = new HashSet<EventListenerRecord>();

        /// <summary>
        /// Appends an event listener for events whose type attribute value is type.
        /// </summary>
        /// <param name="type">The type of <see cref="Event"/></param>
        /// <param name="callback">The callback argument sets the callback that will be invoked when the <see cref="Event"/> is dispatched.</param>
        /// <param name="capture">
        /// When set to <c>true</c>, the capture argument prevents callback
        /// from being invoked if the <see cref="Event"/>'s <see cref="Event.EventPhase"/> attribute value is
        /// <see cref="EventPhase.BubblingPhase"/>. When <c>false</c>, callback will not be invoked
        /// when <see cref="Event"/>'s <see cref="Event.EventPhase"/> attribute value is
        /// <see cref="EventPhase.CapturingPhase"/>. Either way, callback will be invoked
        /// when <see cref="Event"/>'s <see cref="Event.EventPhase"/> attribute value is <see cref="EventPhase.AtTarget"/>.
        /// </param>
        public void AddEventListener(string type, EventListener callback, bool capture = false)
        {
            if (callback == null)
                return;

            EventListeners.Add(new EventListenerRecord(type, callback, capture));
        }
        /// <summary>
        /// Remove the event listener in target's list of event listeners with the same type, callback, and capture. 
        /// </summary>
        /// <param name="type">The type of <see cref="Event"/></param>
        /// <param name="callback">The callback.</param>
        /// <param name="capture">The capture parameter.</param>
        public void RemoveEventListener(string type, EventListener callback, bool capture = false)
        {
            EventListeners.Remove(new EventListenerRecord(type, callback, capture));
        }
        /// <summary>
        /// Dispatches a synthetic event event to target.
        /// </summary>
        /// <param name="event"></param>
        /// <returns>
        /// <c>true</c> if either <see cref="Event"/>'s <see cref="Event.Cancelable"/> attribute value
        /// is <c>false</c> or its <see cref="Event.PreventDefault"/> method was not invoked,
        /// and <c>false</c> otherwise.
        /// </returns>
        public bool DispatchEvent(Event @event)
        {
            if (@event.DispatchFlag || !@event.InitializedFlag)
                throw new Exception();

            @event.IsTrusted = false;
            return Dispatch(@event);
        }

        internal bool Dispatch(Event @event, EventTarget targetOverride = null)
        {
            var target = targetOverride ?? this;

            @event.DispatchFlag = true;
            @event.Target = target;
            @event.EventPhase = EventPhase.CapturingPhase;

            var eventPath = new List<EventTarget>();
            var node = (this as Node)?.ParentNode;
            while (node != null)
            {
                eventPath.Add(node);
                node = node.ParentNode;
            }
            eventPath.Reverse();

            foreach (var item in eventPath)
            {
                if (@event.StopPropagationFlag)
                    break;

                item.Invoke(@event);
            }

            @event.EventPhase = EventPhase.AtTarget;
            if (!@event.StopPropagationFlag)
                Invoke(@event);

            if (@event.Bubbles)
            {
                eventPath.Reverse();

                foreach (var item in eventPath)
                {
                    if (@event.StopPropagationFlag)
                        break;

                    item.Invoke(@event);
                }
            }

            @event.DispatchFlag = false;
            @event.EventPhase = EventPhase.None;
            @event.CurrentTarget = null;

            return !@event.CanceledFlag;
        }

        internal void Invoke(Event @event)
        {
            @event.CurrentTarget = this;

            foreach (var record in EventListeners)
            {
                if (@event.StopImmediatePropagationFlag)
                    return;

                if (record.Type != @event.Type)
                    continue;

                if (@event.EventPhase == EventPhase.CapturingPhase &&
                    !record.Capture)
                    continue;

                if (@event.EventPhase == EventPhase.BubblingPhase &&
                    record.Capture)
                    continue;

                try
                {
                    record.Listener(@event);
                }
                catch (Exception)
                {

                }
            }
        }

        internal void Fire(string type)
        {
            var @event = new Event(type);
            @event.IsTrusted = true;
            Dispatch(@event);
        }
    }

    [Flags]
    public enum DocumentPosition : ushort
    {
        /// <summary>
        /// Set when node and other are not in the same tree. 
        /// </summary>
        Disconnected = 0x01,
        /// <summary>
        /// Set when other is preceding node. 
        /// </summary>
        Preceding = 0x02,
        /// <summary>
        /// Set when other is following node. 
        /// </summary>
        Following = 0x04,
        /// <summary>
        /// Set when other is an ancestor of node. 
        /// </summary>
        Contains = 0x08,
        /// <summary>
        /// Set when other is a descendant of node. 
        /// </summary>
        ContainedBy = 0x10,
        ImplementationSpecific = 0x20
    }

    public abstract class DomImplementation
    {
        public abstract DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId);
        public abstract XmlDocument CreateDocument(string @namespace, string qualifiedName, DocumentType docType = null);
        public abstract Document CreateHtmlDocument(string title = null);

        public bool HasFeature => true;
    }

    public class DomTokenList : IEnumerable<string>
    {
        private readonly Attr Attr;
        private readonly List<string> InnerList;

        internal DomTokenList(Attr attr)
        {
            Attr = attr;

            var parts = attr.Value.Split(' ');
            InnerList = new List<string>(parts.Length);
            foreach (var item in parts)
                if (!InnerList.Contains(item))
                    InnerList.Add(item);
        }

        public uint Length => (uint)InnerList.Count;
        public string this[uint index] => InnerList[(int)index];
        public bool Contains(string token) => InnerList.Contains(token);

        private string Serialize() => string.Join(" ", InnerList);
        private void Update() => Attr.Value = Serialize();

        public void Add(params string[] tokens)
        {
            foreach (var item in tokens)
            {
                if (string.IsNullOrEmpty(item))
                    throw new DomException(DomExceptionCode.SyntaxError);

                if (item.Contains(' '))
                    throw new DomException(DomExceptionCode.InvalidCharacterError);

                if (!InnerList.Contains(item))
                    InnerList.Add(item);
            }

            Update();
        }
        public void Remove(params string[] tokens)
        {
            foreach (var item in tokens)
            {
                if (string.IsNullOrEmpty(item))
                    throw new DomException(DomExceptionCode.SyntaxError);

                if (item.Contains(' '))
                    throw new DomException(DomExceptionCode.InvalidCharacterError);

                InnerList.Remove(item);
            }

            Update();
        }
        public bool Toggle(string token, bool? force = null)
        {
            if (force == true || !InnerList.Contains(token))
            {
                Add(token);
                return true;
            }
            else
            {
                Remove(token);
                return false;
            }
        }
        public void Replace(string token, string newToken)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newToken))
                throw new DomException(DomExceptionCode.SyntaxError);

            if (token.Contains(' ') || newToken.Contains(' '))
                throw new DomException(DomExceptionCode.InvalidCharacterError);

            if (!InnerList.Contains(token))
                return;

            InnerList.Remove(token);
            InnerList.Add(newToken);

            Update();
        }

        public IEnumerator<string> GetEnumerator() => InnerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => InnerList.GetEnumerator();
    }

    public abstract class NamedDomNodeMap
    {
        public abstract Node GetNamedItem(string name);
        public abstract Node SetNamedItem(Node arg);
        public abstract Node RemoveNamedItem(string name);
        public abstract Node this[uint index] { get; }
        public abstract uint Length { get; }

        public abstract Node GetNamedItemNS(string namespaceUri, string localName);
        public abstract Node SetNamedItemNS(Node arg);
        public abstract Node RemoveNamedItemNS(string namespaceUri, string localName);
    }

    public class HtmlCollection : IEnumerable<Element>
    {
        public static readonly List<Element> EmptyList = new List<Element>();

        internal static HtmlCollection Empty { get; } = new HtmlCollection(EmptyList);

        internal List<Element> InnerList { get; }

        internal HtmlCollection(List<Element> innerList)
        {
            InnerList = innerList;
        }

        public virtual uint Length => (uint)InnerList.Count;
        public virtual Element this[uint index] => index < InnerList.Count ? InnerList[(int)index] : null;
        public virtual Element this[string name] => !string.IsNullOrEmpty(name) ? InnerList.FirstOrDefault(x => IsNameMatches(x, name)) : null;

        protected bool IsNameMatches(Element element, string name)
        {
            if (element.Id == name)
                return true;

            if (element.NamespaceUri != HtmlElement.HtmlNamespace)
                return false;

            if (element.GetAttribute("name") == name)
                return true;

            return false;
        }

        protected virtual IEnumerator<Element> GetEnumerator() => InnerList.GetEnumerator();

        IEnumerator<Element> IEnumerable<Element>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class LazyHtmlCollection : HtmlCollection
    {
        private readonly NodeList Nodes;
        private readonly Func<Element, bool> Matcher;

        internal int EvaluatedCount { get; set; }

        internal void Insert(int index, Element element)
        {
            if (Matcher == null || Matcher(element))
                InnerList.Insert(index, element);
        }

        public LazyHtmlCollection(NodeList nodes, Func<Element, bool> matcher = null)
            : base(EmptyList)
        {
            nodes.HtmlCollections.Add(new WeakReference<LazyHtmlCollection>(this));

            Nodes = nodes;
            Matcher = matcher;
        }

        public override uint Length
        {
            get
            {
                while (EvaluatedCount < Nodes.Length)
                {
                    if (Nodes[EvaluatedCount] is Element element)
                    {
                        if (Matcher == null || Matcher(element))
                            InnerList.Add(element);
                    }

                    EvaluatedCount++;
                }

                return (uint)InnerList.Count;
            }
        }

        public override Element this[uint index]
        {
            get
            {
                if (index >= int.MaxValue)
                    throw new IndexOutOfRangeException();

                var i = (int)index;

                if (InnerList.Count > i)
                    return InnerList[i];

                while (EvaluatedCount < Nodes.Length)
                {
                    if (Nodes[EvaluatedCount] is Element element)
                    {
                        if (Matcher == null || Matcher(element))
                        {
                            InnerList.Add(element);

                            if (InnerList.Count > i)
                                return element;
                        }
                    }

                    EvaluatedCount++;
                }

                return null;
            }
        }

        public override Element this[string name]
        {
            get
            {
                foreach (var item in InnerList)
                    if (IsNameMatches(item, name))
                        return item;

                while (EvaluatedCount < Nodes.Length)
                {
                    if (Nodes[EvaluatedCount] is Element element)
                    {
                        if (Matcher == null || Matcher(element))
                        {
                            InnerList.Add(element);

                            if (IsNameMatches(element, name))
                                return element;
                        }
                    }

                    EvaluatedCount++;
                }

                return null;
            }
        }

        protected override IEnumerator<Element> GetEnumerator()
        {
            foreach (var item in InnerList)
                yield return item;

            while (EvaluatedCount < Nodes.Length)
            {
                if (Nodes[EvaluatedCount] is Element element)
                {
                    if (Matcher == null || Matcher(element))
                    {
                        InnerList.Add(element);
                        yield return element;
                    }
                }

                EvaluatedCount++;
            }
        }
    }
}
