using System.IO;
using MiniMvvm;

namespace VirtualizationImages.ViewModels;

public class PageViewModel : ViewModelBase
{
    private string _prefix;
    private int _index;
    private double _height = double.NaN;

    public PageViewModel(int index, string prefix = "Item")
    {
        _prefix = prefix;
        _index = index;
    }

    public string Header => $"{_prefix} {_index}";

    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }


    public byte[] DisplayImage
    {
        get
        {
            string path = "image.png";
            return File.ReadAllBytes(path);
        }
        set
        {
            
        }
    }
}
