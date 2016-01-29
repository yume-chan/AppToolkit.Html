using System;

namespace AppToolkit.Html.Interfaces
{
    public delegate void EventHandler(Event @event);

    public interface GlobalEventHandlers
    {
        event EventHandler OnAbort;
        event EventHandler OnBlur;
        event EventHandler OnCancel;
        event EventHandler OnCanPlay;
        event EventHandler OnCanPlayThrough;
        event EventHandler OnChange;
        event EventHandler OnClick;
        event EventHandler OnCueChange;
        event EventHandler OnDblClick;
        event EventHandler OnDurationChange;
        event EventHandler OnEmptied;
        event EventHandler OnEnded;
        event EventHandler OnError;
        event EventHandler OnFocus;
        event EventHandler OnInput;
        event EventHandler OnInvalid;
        event EventHandler OnKeyDown;
        event EventHandler OnKeyPress;
        event EventHandler OnKeyUp;
        event EventHandler OnLoad;
        event EventHandler OnLoadedData;
        event EventHandler OnLoadedMetadata;
        event EventHandler OnLoadStart;
        event EventHandler OnMouseDown;
        event EventHandler OnMouseEnter;
        event EventHandler OnMouseLeave;
        event EventHandler OnMouseMove;
        event EventHandler OnMouseOut;
        event EventHandler OnMouseOver;
        event EventHandler OnMouseUp;
        event EventHandler OnMouseWheel;
        event EventHandler OnPause;
        event EventHandler OnPlay;
        event EventHandler OnPlaying;
        event EventHandler OnProgress;
        event EventHandler OnRateChange;
        event EventHandler OnReset;
        event EventHandler OnResize;
        event EventHandler OnScroll;
        event EventHandler OnSeeked;
        event EventHandler OnSeeking;
        event EventHandler OnSelect;
        event EventHandler OnShow;
        event EventHandler OnStalled;
        event EventHandler OnSubmit;
        event EventHandler OnSuspend;
        event EventHandler OnTimeUpdate;
        event EventHandler OnToggle;
        event EventHandler OnVolumeChange;
        event EventHandler OnWaiting;
    }

    public class HtmlElement : Element, GlobalEventHandlers
    {
        public string Title { get; set; }
        public string Lang { get; set; }
        public bool Translate { get; set; }
        public string Dir { get; set; }

        public bool Hidden { get; set; }
        public void Click() { throw new NotImplementedException(); }
        public long TabIndex { get; set; }
        public void Focus() { throw new NotImplementedException(); }
        public void Blur() { throw new NotImplementedException(); }
        public string AccessKey { get; set; }
        public string AccessKeyLabel { get; }
        public string ContentEditable { get; set; }
        public bool IsContentEditable { get; }
        public bool SpellCheck { get; set; }

        public event EventHandler OnAbort;
        public event EventHandler OnBlur;
        public event EventHandler OnCancel;
        public event EventHandler OnCanPlay;
        public event EventHandler OnCanPlayThrough;
        public event EventHandler OnChange;
        public event EventHandler OnClick;
        public event EventHandler OnCueChange;
        public event EventHandler OnDblClick;
        public event EventHandler OnDurationChange;
        public event EventHandler OnEmptied;
        public event EventHandler OnEnded;
        public event EventHandler OnError;
        public event EventHandler OnFocus;
        public event EventHandler OnInput;
        public event EventHandler OnInvalid;
        public event EventHandler OnKeyDown;
        public event EventHandler OnKeyPress;
        public event EventHandler OnKeyUp;
        public event EventHandler OnLoad;
        public event EventHandler OnLoadedData;
        public event EventHandler OnLoadedMetadata;
        public event EventHandler OnLoadStart;
        public event EventHandler OnMouseDown;
        public event EventHandler OnMouseEnter;
        public event EventHandler OnMouseLeave;
        public event EventHandler OnMouseMove;
        public event EventHandler OnMouseOut;
        public event EventHandler OnMouseOver;
        public event EventHandler OnMouseUp;
        public event EventHandler OnMouseWheel;
        public event EventHandler OnPause;
        public event EventHandler OnPlay;
        public event EventHandler OnPlaying;
        public event EventHandler OnProgress;
        public event EventHandler OnRateChange;
        public event EventHandler OnReset;
        public event EventHandler OnResize;
        public event EventHandler OnScroll;
        public event EventHandler OnSeeked;
        public event EventHandler OnSeeking;
        public event EventHandler OnSelect;
        public event EventHandler OnShow;
        public event EventHandler OnStalled;
        public event EventHandler OnSubmit;
        public event EventHandler OnSuspend;
        public event EventHandler OnTimeUpdate;
        public event EventHandler OnToggle;
        public event EventHandler OnVolumeChange;
        public event EventHandler OnWaiting;
    }

}
