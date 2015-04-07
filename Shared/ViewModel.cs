using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
#if DLL
using Invert.MVVM;
using Invert.uFrame.Editor;
namespace Invert.MVVM
{
#endif
using UniRx;

/// <summary>
///  A data structure that contains information/data needed for a 'View'
/// </summary>
[Serializable]
public abstract class ViewModel
#if !DLL
    :  IUFSerializable, INotifyPropertyChanged , IObservable<IObservableProperty>, IDisposable, IBindable
#else
 : INotifyPropertyChanged
#endif
{
    public event PropertyChangedEventHandler PropertyChanged;
    private Dictionary<int, List<IDisposable>> _bindings;
    private string _identifier;

    protected ViewModel()
    {
#if !UNITY_EDITOR
        BindInternal();
#endif
    }

    protected IEventAggregator Aggregator { get; set; }

    protected ViewModel(IEventAggregator aggregator)
    {
        if (aggregator == null) throw new ArgumentNullException("aggregator");
        Aggregator = aggregator;
        BindInternal();
    }

    public virtual void MethodAccessException()
    {
        
    }
    private void BindInternal()
    {
        if (!_isBound)
        {
            Bind();
            _isBound = true;
        }

    }
    [Obsolete("Use new ViewModel(EventAggregator) instead.")]
    protected ViewModel(Controller controller, bool initialize = true) :this(controller.EventAggregator)
    {
      
    }

    public Dictionary<int, List<IDisposable>> Bindings
    {
        get { return _bindings ?? (_bindings = new Dictionary<int, List<IDisposable>>()); }
        set { _bindings = value; }
    }

    [Obsolete("Controllers are no longer needed on viewmodels.")]
    public Controller Controller {
        get
        {
            throw new Exception("You should not be accessing controllers from the viewmodel.  It also obsolete in 1.6");
        }
        set
        {
            
        } }

    private bool _isBound;
    public virtual void Bind()
    {
        
    }

    public virtual string Identifier
    {
        get { return _identifier; }
        set { _identifier = value; }
    }

    public int References { get; set; }

    public IDisposable AddBinding(IDisposable binding)
    {
        if (!Bindings.ContainsKey(-1))
        {
            Bindings[-1] = new List<IDisposable>();
        }
        Bindings[-1].Add(binding);
        return binding;
    }

    /// <summary>
    /// Reflection-less get of all view-model commands generated by the designer tool.
    /// </summary>
    /// <returns></returns>
    public List<ViewModelCommandInfo> GetViewModelCommands()
    {
        var list = new List<ViewModelCommandInfo>();
        FillCommands(list);
        return list;
    }

    /// <summary>
    /// Reflection-less get of all view-model commands generated by the designer tool.
    /// </summary>
    /// <returns></returns>
    public List<ViewModelPropertyInfo> GetViewModelProperties()
    {
        var list = new List<ViewModelPropertyInfo>();
        FillProperties(list);
        return list;
    }

    /// <summary>
    /// Implementation of Microsoft's INotifyPropertyChanged
    /// </summary>
    /// <param name="propertyName"></param>
    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Implementation of Microsoft's INotifyPropertyChanged
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="propertyName"></param>
    public virtual void OnPropertyChanged(object sender, string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null) handler(sender, new PropertyChangedEventArgs(propertyName));
    }

#if !DLL
    public virtual void Read(ISerializerStream stream)
    {
        Identifier = stream.DeserializeString("Identifier");
    }
    public virtual void Write(ISerializerStream stream)
    {
        stream.SerializeString("Identifier", Identifier);
    }
#endif

    public IDisposable Subscribe(IObserver<IObservableProperty> observer)
    {
        PropertyChangedEventHandler propertyChanged = (sender, args) =>
        {
            var property = sender as IObservableProperty;
            //if (property != null)
                observer.OnNext(property);
        };

        PropertyChanged += propertyChanged;
        return new SimpleDisposable(() => PropertyChanged -= propertyChanged);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public void Dispose()
    {
        if (Aggregator != null)
        Aggregator.Publish(new ViewModelDestroyedEvent()
        {
            ViewModel = this
        });

        Unbind();
    }

    public virtual void Unbind()
    {
        foreach (var binding in Bindings)
        {
            foreach (var binding1 in binding.Value)
            {
                binding1.Dispose();
            }
            binding.Value.Clear();
        }
        Bindings.Clear();
    }


    protected virtual void FillCommands(List<ViewModelCommandInfo> list)
    {
    }

    protected virtual void FillProperties(List<ViewModelPropertyInfo> list)
    {
    }

    [Obsolete]
    protected virtual void WireCommands(Controller controller)
    {
    }

}

#if DLL
}
#endif